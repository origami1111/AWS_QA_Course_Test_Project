using Amazon.S3;
using AWS_QA_Course_Test_Project.Base;
using AWS_QA_Course_Test_Project.Utils;

namespace AWS_QA_Course_Test_Project.Tests
{
    [TestFixture]
    public class S3Tests : BaseTest
    {
        // Deployment Validation
        // Instance requirement:
        [Test(Description = "CXQA-S3-01: The application is deployed in the public subnet and should be accessible by HTTP from the internet via an Internet gateway by public IP address and FQDN"), Order(1)]
        public async Task TestApplicationDeployedInPublicSubnetAndAccessibleByHTTPFromInternetViaInternetGatewayByPublicIPAddressAndFQDN()
        {
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);

            string publicIpAddress = publicInstance.PublicIpAddress;
            string fqdn = publicInstance.PublicDnsName;

            string publicIpAddressUrl = $"http://{publicIpAddress}/api/ui";
            string fqdnUrl = $"http://{fqdn}/api/ui";

            await S3Helper.AssertInstanceInPublicSubnet(Ec2Client, publicInstance);

            using (HttpClient client = new HttpClient())
            {
                await S3Helper.AssertHttpAccess(client, publicIpAddressUrl, "public IP address");
                await S3Helper.AssertHttpAccess(client, fqdnUrl, "FQDN");
            }
        }

        [Test(Description = "CXQA-S3-01: The application instance should be accessible by SSH protocol"), Order(2)]
        public async Task TestApplicationInstanceAccessibleBySSHProtocol()
        {
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);

            string publicIpAddress = publicInstance.PublicIpAddress;
            int sshPort = 22;

            await S3Helper.AssertSshConnectivity(publicIpAddress, sshPort);
        }

        [Test(Description = "CXQA-S3-01: The application should have access to the S3 bucket via an IAM role"), Order(3)]
        public async Task TestApplicationHaveAccessToS3BucketViaIAMRole()
        {
            try
            {
                string bucketName = await S3Helper.GetS3BucketName(S3Client);
                await S3Helper.AssertS3BucketAccess(S3Client, bucketName);
            }
            catch (AmazonS3Exception ex)
            {
                Assert.Fail($"Failed to access the S3 bucket via the IAM role: {ex.Message}");
            }
        }

        //  S3 bucket requirements:
        [Test(Description = "CXQA-S3-02: Name: cloudximage-imagestorebucket{unique id}"), Order(4)]
        public async Task TestS3BucketName()
        {
            string bucketName = await S3Helper.GetS3BucketName(S3Client);
        }

        Dictionary<string, string> expectedTags = new Dictionary<string, string>
        {
            { "cloudx", "qa" }
        };

        [Test(Description = "CXQA-S3-02: Tags: cloudx: qa"), Order(5)]
        public async Task TestS3BucketTags()
        {
            try
            {
                string bucketName = await S3Helper.GetS3BucketName(S3Client);
                await S3Helper.AssertS3BucketTags(S3Client, bucketName, expectedTags);
            }
            catch (AmazonS3Exception ex)
            {
                Assert.Fail($"Failed to get tags for the S3 bucket: {ex.Message}");
            }
        }

        [Test(Description = "CXQA-S3-02: Encryption type: SSE-S3"), Order(6)]
        public async Task TestS3BucketEncryptionType()
        {
            try
            {
                string bucketName = await S3Helper.GetS3BucketName(S3Client);
                await S3Helper.AssertS3BucketEncryption(S3Client, bucketName);
            }
            catch (AmazonS3Exception ex)
            {
                Assert.Fail($"Failed to get encryption configuration for the S3 bucket: {ex.Message}");
            }
        }

        [Test(Description = "CXQA-S3-02: Versioning: disabled"), Order(7)]
        public async Task TestS3BucketVersioning()
        {
            try
            {
                string bucketName = await S3Helper.GetS3BucketName(S3Client);
                await S3Helper.AssertS3BucketVersioning(S3Client, bucketName);
            }
            catch (AmazonS3Exception ex)
            {
                Assert.Fail($"Failed to get versioning status for the S3 bucket: {ex.Message}");
            }
        }

        [Test(Description = "CXQA-S3-02: Public access: no"), Order(8)]
        public async Task TestS3BucketPublicAccess()
        {
            try
            {
                string bucketName = await S3Helper.GetS3BucketName(S3Client);
                await S3Helper.AssertS3BucketPublicAccess(S3Client, bucketName);
            }
            catch (AmazonS3Exception ex)
            {
                Assert.Fail($"Failed to get policy status for the S3 bucket: {ex.Message}");
            }
        }

        // Application functional validation
        [Test(Description = "CXQA-S3-03: Upload images to the S3 bucket"), Order(9)]
        public async Task TestUploadImagesToS3Bucket()
        {
            try
            {
                string bucketName = await S3Helper.GetS3BucketName(S3Client);
                string key = "image.jpg";
                string filePath = "C:\\Users\\origami\\Desktop\\CloudX Associate AWS for Testers\\AWS_QA_Course_Test_Project\\Images\\image.jpg"; // just for testing purposes

                await S3Helper.UploadImageToS3Bucket(S3Client, bucketName, key, filePath);
            }
            catch (AmazonS3Exception ex)
            {
                Assert.Fail($"Failed to upload images to the S3 bucket: {ex.Message}");
            }
        }

        [Test(Description = "CXQA-S3-04: Download images from the S3 bucket"), Order(10)]
        public async Task TestDownloadImagesFromS3Bucket()
        {
            try
            {
                string bucketName = await S3Helper.GetS3BucketName(S3Client);
                string key = "image.jpg";
                string filePath = "DownloadedImages/image.jpg";

                await S3Helper.DownloadImageFromS3Bucket(S3Client, bucketName, key, filePath);
            }
            catch (AmazonS3Exception ex)
            {
                Assert.Fail($"Failed to download images from the S3 bucket: {ex.Message}");
            }
        }

        [Test(Description = "CXQA-S3-05: View a list of uploaded images"), Order(11)]
        public async Task TestViewListOfUploadedImagesS3Bucket()
        {
            try
            {
                string bucketName = await S3Helper.GetS3BucketName(S3Client);
                await S3Helper.ViewListOfUploadedImagesS3Bucket(S3Client, bucketName);
            }
            catch (AmazonS3Exception ex)
            {
                Assert.Fail($"Failed to get list of uploaded images from the S3 bucket: {ex.Message}");
            }
        }

        [Test(Description = "CXQA-S3-06: Delete an image from the S3 bucket"), Order(12)]
        public async Task TestDeleteImageFromS3Bucket()
        {
            try
            {
                string bucketName = await S3Helper.GetS3BucketName(S3Client);
                string key = "image.jpg";

                await S3Helper.DeleteImageFromS3Bucket(S3Client, bucketName, key);
            }
            catch (AmazonS3Exception ex)
            {
                Assert.Fail($"Failed to delete image from the S3 bucket: {ex.Message}");
            }
        }
    }
}

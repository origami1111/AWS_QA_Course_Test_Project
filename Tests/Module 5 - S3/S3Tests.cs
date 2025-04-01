//using Amazon.S3;
//using AWS_QA_Course_Test_Project.Base;
//using AWS_QA_Course_Test_Project.Clients;
//using AWS_QA_Course_Test_Project.Utils;
//using System.Net;

//namespace AWS_QA_Course_Test_Project.Tests
//{
//    [TestFixture]
//    public class S3Tests : BaseTest
//    {
//        // Deployment Validation
//        // Instance requirement:
//        [Test(Description = "CXQA-S3-01: The application is deployed in the public subnet and should be accessible by HTTP from the internet via an Internet gateway by public IP address and FQDN"), Order(1)]
//        public async Task TestApplicationDeployedInPublicSubnetAndAccessibleByHTTPFromInternetViaInternetGatewayByPublicIPAddressAndFQDN()
//        {
//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = EC2Helper.GetPublicInstance(instances);

//            string publicIpAddress = publicInstance.PublicIpAddress;
//            string fqdn = publicInstance.PublicDnsName;

//            string publicIpAddressUrl = $"http://{publicIpAddress}/api/ui";
//            string fqdnUrl = $"http://{fqdn}/api/ui";

//            await S3Helper.AssertInstanceInPublicSubnet(Ec2Client, publicInstance);

//            using (HttpClient client = new HttpClient())
//            {
//                await S3Helper.AssertHttpAccess(client, publicIpAddressUrl, "public IP address");
//                await S3Helper.AssertHttpAccess(client, fqdnUrl, "FQDN");
//            }
//        }

//        [Test(Description = "CXQA-S3-01: The application instance should be accessible by SSH protocol"), Order(2)]
//        public async Task TestApplicationInstanceAccessibleBySSHProtocol()
//        {
//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = EC2Helper.GetPublicInstance(instances);

//            string publicIpAddress = publicInstance.PublicIpAddress;
//            int sshPort = 22;

//            await S3Helper.AssertSshConnectivity(publicIpAddress, sshPort);
//        }

//        [Test(Description = "CXQA-S3-01: The application should have access to the S3 bucket via an IAM role"), Order(3)]
//        public async Task TestApplicationHaveAccessToS3BucketViaIAMRole()
//        {
//            try
//            {
//                string bucketName = await S3Helper.GetS3BucketName(S3Client, "cloudximage-imagestorebucket");
//                await S3Helper.AssertS3BucketAccess(S3Client, bucketName);
//            }
//            catch (AmazonS3Exception ex)
//            {
//                Assert.Fail($"Failed to access the S3 bucket via the IAM role: {ex.Message}");
//            }
//        }

//        //  S3 bucket requirements:
//        [Test(Description = "CXQA-S3-02: Name: cloudximage-imagestorebucket{unique id}"), Order(4)]
//        public async Task TestS3BucketName()
//        {
//            string bucketName = await S3Helper.GetS3BucketName(S3Client, "cloudximage-imagestorebucket");
//        }

//        Dictionary<string, string> expectedTags = new Dictionary<string, string>
//        {
//            { "cloudx", "qa" }
//        };

//        [Test(Description = "CXQA-S3-02: Tags: cloudx: qa"), Order(5)]
//        public async Task TestS3BucketTags()
//        {
//            try
//            {
//                string bucketName = await S3Helper.GetS3BucketName(S3Client, "cloudximage-imagestorebucket");
//                await S3Helper.AssertS3BucketTags(S3Client, bucketName, expectedTags);
//            }
//            catch (AmazonS3Exception ex)
//            {
//                Assert.Fail($"Failed to get tags for the S3 bucket: {ex.Message}");
//            }
//        }

//        [Test(Description = "CXQA-S3-02: Encryption type: SSE-S3"), Order(6)]
//        public async Task TestS3BucketEncryptionType()
//        {
//            try
//            {
//                string bucketName = await S3Helper.GetS3BucketName(S3Client, "cloudximage-imagestorebucket");
//                await S3Helper.AssertS3BucketEncryption(S3Client, bucketName);
//            }
//            catch (AmazonS3Exception ex)
//            {
//                Assert.Fail($"Failed to get encryption configuration for the S3 bucket: {ex.Message}");
//            }
//        }

//        [Test(Description = "CXQA-S3-02: Versioning: disabled"), Order(7)]
//        public async Task TestS3BucketVersioning()
//        {
//            try
//            {
//                string bucketName = await S3Helper.GetS3BucketName(S3Client, "cloudximage-imagestorebucket");
//                await S3Helper.AssertS3BucketVersioning(S3Client, bucketName);
//            }
//            catch (AmazonS3Exception ex)
//            {
//                Assert.Fail($"Failed to get versioning status for the S3 bucket: {ex.Message}");
//            }
//        }

//        [Test(Description = "CXQA-S3-02: Public access: no"), Order(8)]
//        public async Task TestS3BucketPublicAccess()
//        {
//            try
//            {
//                string bucketName = await S3Helper.GetS3BucketName(S3Client, "cloudximage-imagestorebucket");
//                await S3Helper.AssertS3BucketPublicAccess(S3Client, bucketName);
//            }
//            catch (AmazonS3Exception ex)
//            {
//                Assert.Fail($"Failed to get policy status for the S3 bucket: {ex.Message}");
//            }
//        }

//        // Application functional validation
//        string filePath = "C:\\Users\\origami\\Desktop\\CloudX Associate AWS for Testers\\AWS_QA_Course_Test_Project\\Images\\image0.jpg";
//        string imageName = "image0.jpg";

//        [Test(Description = "CXQA-S3-03: Upload image to the S3 bucket")]
//        public async Task TestUploadImageToS3Bucket()
//        {
//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = EC2Helper.GetPublicInstance(instances);
//            string publicIpAddress = publicInstance.PublicIpAddress;

//            var restClient = new RestClient(publicIpAddress);

//            // Upload the image
//            var postImageResponse = await restClient.PostImageAsync(filePath);

//            // Get image response
//            var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);

//            // Assert the response
//            Assert.That(getImageResponse.Id, Is.EqualTo(postImageResponse.Id), "Image was not posted, the ids are different");
//            Assert.That(getImageResponse.ObjectKey, Does.Contain(imageName), "Image was not posted, the object_key`s are different");

//            // Delete the image
//            await restClient.DeleteImageAsync(postImageResponse.Id);
//        }

//        string filePathTemplate = "C:\\Users\\origami\\Desktop\\CloudX Associate AWS for Testers\\AWS_QA_Course_Test_Project\\Images\\image{0}.jpg";
//        string imageNameTemplate = "image{0}.jpg";

//        [Test(Description = "CXQA-S3-03: Upload images to the S3 bucket")]
//        public async Task TestUploadImagesToS3Bucket()
//        {
//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = EC2Helper.GetPublicInstance(instances);
//            string publicIpAddress = publicInstance.PublicIpAddress;
//            int countOfImages = 3;
//            Dictionary<string, string> createdImages = new Dictionary<string, string>();

//            var restClient = new RestClient(publicIpAddress);

//            for (int i = 0; i < countOfImages; i++)
//            {
//                string filePath = string.Format(filePathTemplate, i);
//                string imageName = string.Format(imageNameTemplate, i);

//                var postImageResponse = await restClient.PostImageAsync(filePath);
//                var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);
//                createdImages.Add(postImageResponse.Id, getImageResponse.ObjectKey);

//                // Assert the response
//                Assert.That(getImageResponse.Id, Is.EqualTo(postImageResponse.Id), $"Image {i} was not posted, the ids are different");
//                Assert.That(getImageResponse.ObjectKey, Does.Contain(imageName), $"Image {i} was not posted, the object_key`s are different");
//            }

//            var getImagesResponse = await restClient.GetImagesAsync();

//            Assert.That(getImagesResponse.Count, Is.GreaterThanOrEqualTo(countOfImages), "Not all images were uploaded");
//            Assert.That(createdImages.Keys.All(id => getImagesResponse.Any(img => img.Id == id)), Is.True, "Not all images were uploaded");

//            foreach (var createdImage in createdImages)
//            {
//                await restClient.DeleteImageAsync(createdImage.Key);
//            }
//        }

//        [Test(Description = "CXQA-S3-04: Download images from the S3 bucket")]
//        public async Task TestDownloadImagesFromS3Bucket()
//        {
//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = EC2Helper.GetPublicInstance(instances);
//            string publicIpAddress = publicInstance.PublicIpAddress;
//            string downloadPath = "DownloadedImages/image.jpg";

//            var restClient = new RestClient(publicIpAddress);

//            // Upload the image
//            var postImageResponse = await restClient.PostImageAsync(filePath);

//            // Download the image
//            var response = await restClient.GetImageAsync(postImageResponse.Id);

//            Assert.That(response.IsSuccessStatusCode, Is.True, "The image was not downloaded successfully.");

//            // Ensure the directory exists
//            Directory.CreateDirectory(Path.GetDirectoryName(downloadPath));

//            // Save the downloaded image
//            await using (var fileStream = File.Create(downloadPath))
//            {
//                await response.Content.CopyToAsync(fileStream);
//            }

//            // Assert that file was downloaded
//            Assert.That(File.Exists(downloadPath), Is.True, "The image was not downloaded successfully.");

//            // Delete the image
//            await restClient.DeleteImageAsync(postImageResponse.Id);
//        }

//        [Test(Description = "CXQA-S3-05: View a list of uploaded images")]
//        public async Task TestViewListOfUploadedImagesS3Bucket()
//        {
//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = EC2Helper.GetPublicInstance(instances);
//            string publicIpAddress = publicInstance.PublicIpAddress;

//            var restClient = new RestClient(publicIpAddress);

//            // Upload the image
//            var postImageResponse = await restClient.PostImageAsync(filePath);

//            var getImagesResponse = await restClient.GetImagesAsync();

//            // Assert the response
//            Assert.That(getImagesResponse.Count, Is.GreaterThan(0), "No images were found. Expected count: > 0. Actual count: 0");
//            Assert.That(getImagesResponse.Any(i => i.Id == postImageResponse.Id), Is.True, $"No all images retrieved. It is expected images list contains at least one image with {postImageResponse.Id} id");

//            // Delete the image
//            await restClient.DeleteImageAsync(postImageResponse.Id);
//        }

//        [Test(Description = "CXQA-S3-06: Delete an image from the S3 bucket")]
//        public async Task TestDeleteImageFromS3Bucket()
//        {
//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = EC2Helper.GetPublicInstance(instances);
//            string publicIpAddress = publicInstance.PublicIpAddress;

//            var restClient = new RestClient(publicIpAddress);

//            // Upload the image
//            var postImageResponse = await restClient.PostImageAsync(filePath);

//            // Get image response
//            var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);

//            Assert.That(getImageResponse.Id, Is.EqualTo(postImageResponse.Id), "Image was not posted, the ids are different");

//            // Delete the image
//            await restClient.DeleteImageAsync(postImageResponse.Id);

//            // Get deleted image response
//            var getDeletedImageResponse = await restClient.GetImageAsync(postImageResponse.Id);

//            Assert.That(getDeletedImageResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "The image was not deleted successfully.");
//        }
//    }
//}

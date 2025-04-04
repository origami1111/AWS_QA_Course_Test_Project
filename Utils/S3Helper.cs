using Amazon.EC2.Model;
using Amazon.EC2;
using Amazon.S3.Model;
using Amazon.S3;
using System.Net.Sockets;
using System.Net;

namespace AWS_QA_Course_Test_Project.Utils
{
    public static class S3Helper
    {
        public static async Task AssertInstanceInPublicSubnet(AmazonEC2Client ec2Client, Instance instance)
        {
            var vpcs = await VPCHelper.DescribeVpcsAsync(ec2Client);
            var nonDefaultVpc = vpcs.FirstOrDefault(vpc => !vpc.IsDefault);

            Assert.That(nonDefaultVpc, Is.Not.Null, "Non-default VPC not found.");

            var subnets = await VPCHelper.DescribeSubnetsAsync(ec2Client, nonDefaultVpc.VpcId);
            var publicSubnets = subnets.Where(subnet => subnet.MapPublicIpOnLaunch).ToList();

            Assert.That(publicSubnets, Is.Not.Empty, "No public subnets found in the non-default VPC.");
            Assert.That(publicSubnets.Any(subnet => subnet.SubnetId == instance.SubnetId), Is.True, "The instance is not deployed in a public subnet.");
        }

        public static async Task AssertHttpAccess(HttpClient client, string url, string accessType)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                Assert.That(HttpStatusCode.OK, Is.EqualTo(response.StatusCode), $"The application is not accessible via {accessType}.");
            }
            catch (HttpRequestException e)
            {
                Assert.Fail($"HTTP request to {url} failed: {e.Message}");
            }
            catch (Exception e)
            {
                Assert.Fail($"An unexpected error occurred while accessing {url}: {e.Message}");
            }
        }

        public static async Task AssertSshConnectivity(string ipAddress, int port)
        {
            using (TcpClient client = new TcpClient())
            {
                try
                {
                    await client.ConnectAsync(ipAddress, port);
                    Assert.That(client.Connected, Is.True, "The application instance is not accessible via SSH protocol.");
                }
                catch (SocketException ex)
                {
                    Assert.Fail($"Failed to connect to the application instance via SSH protocol: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Assert.Fail($"An unexpected error occurred while connecting via SSH: {ex.Message}");
                }
            }
        }

        public static async Task<string> GetS3BucketName(AmazonS3Client s3Client, string bucketNamePrefix)
        {
            var listBucketsResponse = await s3Client.ListBucketsAsync();
            var bucket = listBucketsResponse.Buckets.FirstOrDefault(b => b.BucketName.Contains(bucketNamePrefix));

            Assert.That(bucket, Is.Not.Null, "No S3 bucket found.");

            return bucket.BucketName;
        }

        public static async Task AssertS3BucketAccess(AmazonS3Client s3Client, string bucketName)
        {
            var listObjectsRequest = new ListObjectsV2Request
            {
                BucketName = bucketName
            };

            var listObjectsResponse = await s3Client.ListObjectsV2Async(listObjectsRequest);

            Assert.That(listObjectsResponse, Is.Not.Null, "The application does not have access to the S3 bucket via the IAM role.");
            Assert.That(HttpStatusCode.OK, Is.EqualTo(listObjectsResponse.HttpStatusCode), "The application does not have access to the S3 bucket via the IAM role.");
        }

        public static async Task AssertS3BucketTags(AmazonS3Client s3Client, string bucketName, Dictionary<string, string> expectedTags)
        {
            var getBucketTaggingRequest = new GetBucketTaggingRequest
            {
                BucketName = bucketName
            };

            var getBucketTaggingResponse = await s3Client.GetBucketTaggingAsync(getBucketTaggingRequest);
            var actualTags = getBucketTaggingResponse.TagSet.ToDictionary(tag => tag.Key, tag => tag.Value);

            foreach (var expectedTag in expectedTags)
            {
                Assert.That(actualTags.ContainsKey(expectedTag.Key), Is.True, $"The S3 bucket does not contain the expected tag key '{expectedTag.Key}'.");
                Assert.That(expectedTag.Value, Is.EqualTo(actualTags[expectedTag.Key]), $"The S3 bucket tag value for key '{expectedTag.Key}' does not match the expected value '{expectedTag.Value}'.");
            }
        }

        public static async Task AssertS3BucketEncryption(AmazonS3Client s3Client, string bucketName)
        {
            var getBucketEncryptionRequest = new GetBucketEncryptionRequest
            {
                BucketName = bucketName
            };

            var getBucketEncryptionResponse = await s3Client.GetBucketEncryptionAsync(getBucketEncryptionRequest);
            var encryptionRules = getBucketEncryptionResponse.ServerSideEncryptionConfiguration.ServerSideEncryptionRules;
            bool hasSseS3Encryption = encryptionRules.Any(rule =>
                rule.ServerSideEncryptionByDefault.ServerSideEncryptionAlgorithm == ServerSideEncryptionMethod.AES256);

            Assert.That(hasSseS3Encryption, Is.True, $"The S3 bucket '{bucketName}' does not have SSE-S3 encryption.");
        }

        public static async Task AssertS3BucketVersioning(AmazonS3Client s3Client, string bucketName)
        {
            var getBucketVersioningRequest = new GetBucketVersioningRequest
            {
                BucketName = bucketName
            };

            var getBucketVersioningResponse = await s3Client.GetBucketVersioningAsync(getBucketVersioningRequest);
            var versioningStatus = getBucketVersioningResponse.VersioningConfig.Status;

            Assert.That(versioningStatus, Is.EqualTo(VersionStatus.Off), $"The S3 bucket '{bucketName}' has versioning not disabled.");
        }

        public static async Task AssertS3BucketPublicAccess(AmazonS3Client s3Client, string bucketName)
        {
            var getBucketPolicyStatusRequest = new GetBucketPolicyStatusRequest
            {
                BucketName = bucketName
            };

            var getBucketPolicyStatusResponse = await s3Client.GetBucketPolicyStatusAsync(getBucketPolicyStatusRequest);
            bool isPublic = getBucketPolicyStatusResponse.PolicyStatus.IsPublic;

            Assert.That(isPublic, Is.False, $"The S3 bucket '{bucketName}' is public.");
        }
    }
}

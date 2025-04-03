using AWS_QA_Course_Test_Project.Base;
using AWS_QA_Course_Test_Project.Clients;
using AWS_QA_Course_Test_Project.DTOs;
using AWS_QA_Course_Test_Project.Utils;

namespace AWS_QA_Course_Test_Project.Tests.Module_8___Serverless_Basics
{
    [TestFixture]
    public class DynamoDBApplicationFunctionalValidationTests : BaseTest
    {
        private const string ExpectedTableName = "cloudxserverless-DatabaseImagesTable";
        private const string FilePath = "C:\\Users\\origami\\Desktop\\CloudX Associate AWS for Testers\\AWS_QA_Course_Test_Project\\Images\\image0.jpg";

        [Test(Description = "CXQA-RDS-03: Check that uploaded image metadata is stored in DynamoDB database")]
        public async Task TestImageMetadataIsStoredInDynamoDB()
        {
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIpAddress = publicInstance.PublicIpAddress;
            string tableName = await DynamoDBHelper.GetDynamoDBTableNameAsync(DynamoDbClient, ExpectedTableName);

            var restClient = new RestClient(publicIpAddress);
            var postImageResponse = await restClient.PostImageAsync(FilePath);
            var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);

            var imageDataFromDynamoDB = await DynamoDBHelper.GetItemFromDynamoDBById(DynamoDbClient, tableName, postImageResponse.Id);

            Assert.That(imageDataFromDynamoDB, Is.Not.Null, "The item with the specified id does not exist in the DynamoDB table");

            Assert.Multiple(() =>
            {
                Assert.That(imageDataFromDynamoDB.Id, Is.EqualTo(getImageResponse.Id), "The id of the item does not match the expected id");
                Assert.That(imageDataFromDynamoDB.CreatedAt, Is.EqualTo(getImageResponse.CreatedAt), "The created_at of the item does not match the expected value");
                Assert.That(imageDataFromDynamoDB.LastModified, Is.EqualTo(getImageResponse.LastModified), "The last_modified of the item does not match the expected value");
                Assert.That(imageDataFromDynamoDB.ObjectKey, Is.EqualTo(getImageResponse.ObjectKey), "The object_key of the item does not match the expected value");
                Assert.That(imageDataFromDynamoDB.ObjectSize, Is.EqualTo(getImageResponse.ObjectSize), "The object_size of the item does not match the expected value");
                Assert.That(imageDataFromDynamoDB.ObjectType, Is.EqualTo(getImageResponse.ObjectType), "The object_type of the item does not match the expected value");

            });
        }

        [Test(Description = "CXQA-RDS-04: The image metadata is returned by http://{INSTANCE PUBLIC IP}/api/image/{image_id} GET request")]
        public async Task TestImageMetadataIsReturnedByGetRequest()
        {
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIpAddress = publicInstance.PublicIpAddress;

            var restClient = new RestClient(publicIpAddress);
            var postImageResponse = await restClient.PostImageAsync(FilePath);
            var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);

            Assert.That(getImageResponse, Is.Not.Null, "Image metadata not returned by GET request.");

            Console.WriteLine($"ID: {getImageResponse.Id}, Object Key: {getImageResponse.ObjectKey}, Object Size: {getImageResponse.ObjectSize}, Object Type: {getImageResponse.ObjectType}, Last Modified: {getImageResponse.LastModified}");

            await restClient.DeleteImageAsync(postImageResponse.Id);
        }

        [Test(Description = "CXQA-RDS-05: The image metadata for the deleted image is also deleted from the DynamoDB")]
        public async Task TestImageMetadataIsDeletedFromDynamoDB()
        {
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIpAddress = publicInstance.PublicIpAddress;
            string tableName = await DynamoDBHelper.GetDynamoDBTableNameAsync(DynamoDbClient, ExpectedTableName);

            var restClient = new RestClient(publicIpAddress);
            var postImageResponse = await restClient.PostImageAsync(FilePath);

            var getImageResponseBeforeDeleting = await restClient.GetImageMetadataAsync(postImageResponse.Id);
            Assert.That(getImageResponseBeforeDeleting, Is.Not.Null, "Image metadata not found");

            var imageDataFromDynamoDBBeforeDeleting = await DynamoDBHelper.GetItemFromDynamoDBById(DynamoDbClient, tableName, postImageResponse.Id);
            Assert.That(imageDataFromDynamoDBBeforeDeleting, Is.Not.Null, "The item with the specified id does not exist in the DynamoDB table");

            await restClient.DeleteImageAsync(postImageResponse.Id);

            var getImageResponseAfterDeleting = await restClient.GetImageMetadataAsync(postImageResponse.Id);
            Assert.That(getImageResponseAfterDeleting, Is.Null, "Image metadata still found after deletion");

            var imageDataFromDynamoDBAfterDeleting = await DynamoDBHelper.GetItemFromDynamoDBById(DynamoDbClient, tableName, postImageResponse.Id);
            Assert.That(imageDataFromDynamoDBAfterDeleting, Is.Null, "The item with the specified id still exists in the DynamoDB table");
        }

        // Additional test cases
        [Test(Description = "The list of image metadata is returned by http://{INSTANCE PUBLIC IP}/api/image GET request")]
        public async Task TestListOfImagesMetadataIsReturnedByGetRequest()
        {
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIpAddress = publicInstance.PublicIpAddress;
            var restClient = new RestClient(publicIpAddress);

            int imageCount = 10;
            List<PostImageResponseDTO> postImageResponses = new List<PostImageResponseDTO>();
            for (int i = 0; i < imageCount; i++)
            {
                postImageResponses.Add(await restClient.PostImageAsync(FilePath));
            }

            Assert.That(postImageResponses, Is.Not.Null, "Images was not uploaded");
            Assert.That(postImageResponses.Count, Is.EqualTo(imageCount), "The number of uploaded images does not match the expected value");

            var getImagesResponse = await restClient.GetImagesAsync();

            Assert.That(getImagesResponse, Is.Not.Null, "List of images metadata not returned by GET request");
            Assert.That(getImagesResponse.Count, Is.EqualTo(imageCount), "The number of images metadata returned does not match the expected value");

            for (int i = 0; i < imageCount; i++)
            {
                await restClient.DeleteImageAsync(postImageResponses[i].Id);
            }
        }

        [Test(Description = "Download image by http://{INSTANCE PUBLIC IP}/api/image/file/{image id} GET request")]
        public async Task TestDownloadImageByGetRequest()
        {
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIpAddress = publicInstance.PublicIpAddress;
            string downloadPath = "DownloadedImages/image.jpg";

            var restClient = new RestClient(publicIpAddress);

            var postImageResponse = await restClient.PostImageAsync(FilePath);

            var response = await restClient.GetImageAsync(postImageResponse.Id);

            Assert.That(response.IsSuccessStatusCode, Is.True, "The image was not downloaded successfully");

            Directory.CreateDirectory(Path.GetDirectoryName(downloadPath));

            await using (var fileStream = File.Create(downloadPath))
            {
                await response.Content.CopyToAsync(fileStream);
            }

            Assert.That(File.Exists(downloadPath), Is.True, "The image was not downloaded successfully");

            await restClient.DeleteImageAsync(postImageResponse.Id);
        }
    }
}

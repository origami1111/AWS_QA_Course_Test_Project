using AWS_QA_Course_Test_Project.Base;
using AWS_QA_Course_Test_Project.Clients;
using AWS_QA_Course_Test_Project.Utils;
using System.Net;

namespace AWS_QA_Course_Test_Project.Tests.Module_8___Serverless_Basics
{
    [TestFixture]
    public class ServerlessBasicsDeploymentValidationTests : BaseTest
    {
        private const string ExpectedTableName = "cloudxserverless-DatabaseImagesTable";
        private const string FunctionNamePrefix = "cloudxserverless-EventHandlerLambda";
        private const string QueueNamePrefix = "cloudxserverless-QueueSQSQueue";
        private const string LogGroupNamePrefix = "/aws/lambda/cloudxserverless-EventHandlerLambda";
        private const string TopicNamePrefix = "cloudxserverless-TopicSNSTopic";
        private const string EmailToSubscribe = "novembertanks@gmail.com";

        private const string FilePath = "C:\\Users\\origami\\Desktop\\CloudX Associate AWS for Testers\\AWS_QA_Course_Test_Project\\Images\\image0.jpg";

        // CXQA-SLESS-01: The application database is replaced with a DynamoDB table.
        [Test(Description = "CXQA-SLESS-01: DynamoDB Table requirements: Name: cloudxserverless-DatabaseImagesTable{unique id}")]
        public async Task TestDynamoDBTableName()
        {
            string actualTableName = await DynamoDBHelper.GetDynamoDBTableNameAsync(DynamoDbClient, ExpectedTableName);
            Assert.That(actualTableName, Is.Not.Null, "There are no any DanamoDB tables");
            Assert.That(actualTableName, Does.Contain(ExpectedTableName), "The DynamoDB table name does not match the expected pattern");
        }

        [Test(Description = "CXQA-SLESS-01: DynamoDB Table requirements: Global secondary indexes: not enabled")]
        public async Task TestDynamoDBTableGlobalSecondaryIndexes()
        {
            string tableName = await DynamoDBHelper.GetDynamoDBTableNameAsync(DynamoDbClient, ExpectedTableName);
            Assert.That(tableName, Is.Not.Null, "There are no any DanamoDB tables");

            var globalSecondaryIndexes = await DynamoDBHelper.GetGlobalSecondaryIndexesAsync(DynamoDbClient, tableName);
            Assert.That(globalSecondaryIndexes, Is.Null.Or.Empty, "Global secondary indexes are enabled for the DynamoDB table");
        }

        [Test(Description = "CXQA-SLESS-01: DynamoDB Table requirements: Provisioned read capacity units: 5 (autoscaling for reads: Off)")]
        public async Task TestDynamoDBTableProvisionedReadCapacityUnits()
        {
            string tableName = await DynamoDBHelper.GetDynamoDBTableNameAsync(DynamoDbClient, ExpectedTableName);
            Assert.That(tableName, Is.Not.Null, "There are no any DanamoDB tables");

            long readCapacityUnits = await DynamoDBHelper.GetProvisionedReadCapacityUnitsAsync(DynamoDbClient, tableName);
            bool isReadAutoscalingEnabled = await DynamoDBHelper.IsReadAutoscalingEnabledAsync(tableName);
            Assert.That(readCapacityUnits, Is.EqualTo(5), "The provisioned read capacity units are not set to 5");
            Assert.That(isReadAutoscalingEnabled, Is.False, "Autoscaling for reads is enabled");
        }

        [Test(Description = "CXQA-SLESS-01: DynamoDB Table requirements: Provisioned write capacity units: 1-5 (autoscaling for writes: On)")]
        public async Task TestDynamoDBTableProvisionedWriteCapacityUnits()
        {
            string tableName = await DynamoDBHelper.GetDynamoDBTableNameAsync(DynamoDbClient, ExpectedTableName);
            Assert.That(tableName, Is.Not.Null, "There are no any DanamoDB tables");

            long writeCapacityUnits = await DynamoDBHelper.GetProvisionedWriteCapacityUnitsAsync(DynamoDbClient, tableName);
            bool isWriteAutoscalingEnabled = await DynamoDBHelper.IsWriteAutoscalingEnabledAsync(tableName);
            Assert.That(writeCapacityUnits, Is.InRange(1, 5), "The provisioned write capacity units are not in the range from 1 to 5");
            Assert.That(isWriteAutoscalingEnabled, Is.True, "Autoscaling for writes is disabled");
        }

        [Test(Description = "CXQA-SLESS-01: DynamoDB Table requirements: Time to Live: disabled")]
        public async Task TestDynamoDBTableTimeToLive()
        {
            string tableName = await DynamoDBHelper.GetDynamoDBTableNameAsync(DynamoDbClient, ExpectedTableName);
            Assert.That(tableName, Is.Not.Null, "There are no any DanamoDB tables");

            bool isTimeToLiveDisabled = await DynamoDBHelper.IsTimeToLiveDisabledAsync(DynamoDbClient, tableName);
            Assert.That(isTimeToLiveDisabled, Is.True, "Time to Live is not disabled for the DynamoDB table");
        }

        [Test(Description = "CXQA-SLESS-01: DynamoDB Table requirements: Tags: cloudx: qa")]
        public async Task TestDynamoDBTableTags()
        {
            var expectedTagKey = "cloudx";
            var expectedTagValue = "qa";

            string tableName = await DynamoDBHelper.GetDynamoDBTableNameAsync(DynamoDbClient, ExpectedTableName);
            Assert.That(tableName, Is.Not.Null, "There are no any DanamoDB tables");

            var tags = await DynamoDBHelper.GetTableTagsAsync(DynamoDbClient, tableName);
            Assert.That(tags, Is.Not.Null.And.Not.Empty, "The DynamoDB table has no tags");

            var hasCorrectTag = tags.Any(tag => tag.Key == expectedTagKey && tag.Value == expectedTagValue);
            Assert.That(hasCorrectTag, Is.True, $"DynamoDB does not have the correct tag. Expected: {expectedTagKey}: {expectedTagValue}");
        }

        [Test(Description = "CXQA-SLESS-02: DynamoDB Table should store image metadata information: object creation-time, object last modification date-time, object key, object size, object type")]
        public async Task TestDynamoDBTableStoreImageMetadataInformation()
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

        // CXQA-SLESS-03: The SNS topic is used to subscribe, unsubscribe users, list existing subscriptions, and send messages to subscribers about upload and delete image events
        [Test(Description = "CXQA-SLESS-03: The SNS topic is used to subscribe, unsubscribe users, list existing subscriptions, and send messages to subscribers about upload and delete image events")]
        public async Task TestSNSTopicIsUsedToSubscribeUnsubscribeUsersListExistingSubscriptionsSendMessagesAboutUploadAndDeleteImageEvents()
        {
            var gmailClient = new GmailClient(EmailToSubscribe);

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIp = publicInstance.PublicIpAddress;
            var restClient = new RestClient(publicIp);

            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, TopicNamePrefix);
            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

            // Verify the subscription
            var actualSubscriptions = await restClient.GetNotificationsAsync();
            Assert.That(actualSubscriptions.Any(sub => sub.Endpoint == EmailToSubscribe), Is.True, $"User {EmailToSubscribe} isn't subscribed to the topic");

            // list existing subscriptions
            var subscribersResponseAfterSubscriptions = await restClient.GetNotificationsAsync();
            Assert.That(subscribersResponseAfterSubscriptions.Count, Is.EqualTo(1), $"Subscribers count isn't expected. Expected: 1. Actual: {subscribersResponseAfterSubscriptions.Count}");

            // upload image event
            var gmailMessagesBeforeUploadingImage = await gmailClient.GetMessagesAsync();
            var postImageResponse = await restClient.PostImageAsync(FilePath);
            await Task.Delay(5000);
            var gmailMessagesAfterUploadingImage = await gmailClient.GetMessagesAsync();
            Assert.That(gmailMessagesAfterUploadingImage.Count, Is.EqualTo(gmailMessagesBeforeUploadingImage.Count + 1), "The user did not receive a notification about the uploaded image, or received duplication.");

            // delete image event
            var gmailMessagesBeforeDeletingImage = await gmailClient.GetMessagesAsync();
            await restClient.DeleteImageAsync(postImageResponse.Id);
            await Task.Delay(3000);
            var gmailMessagesAfterDeletingImage = await gmailClient.GetMessagesAsync();
            Assert.That(gmailMessagesAfterDeletingImage.Count, Is.EqualTo(gmailMessagesBeforeDeletingImage.Count + 1), "The user did not receive a notification about the uploaded image, or received duplicates");

            // Unsubscribe the user
            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Failed to unsubscribe the user");

            await Task.Delay(5000);

            // Verify the unsubscription
            var actualSubscriptionsAfterUnsubscription = await restClient.GetNotificationsAsync();
            Assert.That(actualSubscriptionsAfterUnsubscription.Any(sub => sub.Endpoint == EmailToSubscribe), Is.False, $"User {EmailToSubscribe} is still subscribed to the topic but shouldn't be");
        }

        [Test(Description = "CXQA-SLESS-04: The application uses an SQS queue to publish event messages")]
        public async Task TestApplicationUsesSQSQueueToPublishMessages()
        {
            string message = "Test message";
            string queueUrl = await SQSHelper.GetQueueUrlAsync(SqsClient, QueueNamePrefix);
            Assert.That(queueUrl, Is.Not.Null, "The SQS queue with the specified prefix does not exist");

            var queueAttributes = await SQSHelper.GetQueueAttributesAsync(SqsClient, queueUrl, new List<string> { "All" });
            Assert.That(queueAttributes, Is.Not.Null, "Failed to get SQS queue attributes");

            var response = await SqsClient.SendMessageAsync(queueUrl, message);
            Assert.That(response.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK), "Message were nor published to the SQS queue");
        }

        [Test(Description = "CXQA-SLESS-05: A lambda function is subscribed to the SQS queue to filter and put event messages to the SNS topic")]
        public async Task TestLambdaFunctionSubscribedToSQSQueue()
        {
            string functionName = await LambdaHelper.GetLambdaFunctionNameAsync(LambdaClient, FunctionNamePrefix);
            Assert.That(functionName, Is.Not.Null, "The Lambda function with the specified prefix does not exist");

            string queueUrl = await SQSHelper.GetQueueUrlAsync(SqsClient, QueueNamePrefix);
            Assert.That(queueUrl, Is.Not.Null, "The SQS queue with the specified prefix does not exist");

            var eventSourceMappings = await LambdaHelper.GetEventSourceMappingsAsync(LambdaClient, functionName);
            Assert.That(eventSourceMappings, Is.Not.Null.And.Not.Empty, "The Lambda function has no event source mappings");

            var queueArn = await SQSHelper.GetQueueArnAsync(SqsClient, queueUrl);
            var hasSqsTrigger = eventSourceMappings.Any(mapping => mapping.EventSourceArn == queueArn);
            Assert.That(hasSqsTrigger, Is.True, $"The Lambda function is not triggered by the expected SQS queue. Expected: {queueArn}");
        }

        [Test(Description = "CXQA-SLESS-06: The application should have access to the S3 bucket via IAM roles")]
        public async Task TestApplicationAccessToS3BucketViaIAMRoles()
        {
            var bucketName = await S3Helper.GetS3BucketName(S3Client, "cloudxserverless-imagestorebucket");
            Assert.That(bucketName, Is.Not.Null, "S3 bucket with the expected name pattern not found.");
            await S3Helper.AssertS3BucketAccess(S3Client, bucketName);
        }

        [Test(Description = "CXQA-SLESS-06: The application should have access to the DynamoDB table via IAM roles")]
        public async Task TestApplicationAccessToDynamoDBTableViaIAMRoles()
        {
            string tableName = await DynamoDBHelper.GetDynamoDBTableNameAsync(DynamoDbClient, ExpectedTableName);
            Assert.That(tableName, Is.Not.Null, "DynamoDB table with the expected name pattern not found.");

            try
            {
                var tableAttributes = await DynamoDBHelper.GetTableAttributesAsync(DynamoDbClient, tableName);
                Assert.That(tableAttributes, Is.Not.Null, "Failed to get DynamoDB table attributes.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to access DynamoDB table: {ex.Message}");
            }
        }

        [Test(Description = "CXQA-SLESS-06: The application should have access to the SQS queue via IAM roles")]
        public async Task TestApplicationAccessToSQSQueueViaIAMRoles()
        {
            var queueUrl = await SQSHelper.GetQueueUrlAsync(SqsClient, "cloudxserverless-QueueSQSQueue");
            Assert.That(queueUrl, Is.Not.Null, "SQS queue with the expected name pattern not found.");

            try
            {
                var attributes = await SQSHelper.GetQueueAttributesAsync(SqsClient, queueUrl, new List<string> { "All" });
                Assert.That(attributes, Is.Not.Null, "Failed to get SQS queue attributes.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to access SQS queue: {ex.Message}");
            }
        }

        [Test(Description = "CXQA-SLESS-06: The application should have access to the SNS topic instance via IAM roles")]
        public async Task TestApplicationAccessToSNSTopicInstanceViaIAMRoles()
        {
            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudxserverless-TopicSNSTopic");
            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

            try
            {
                var attributes = await SNSHelper.GetTopicAttributesAsync(SnsClient, topicArn);
                Assert.That(attributes, Is.Not.Null, "Failed to get SNS topic attributes.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to access SNS topic: {ex.Message}");
            }
        }

        // CXQA-SLESS-07: AWS Lambda requirements
        [Test(Description = "CXQA-SLESS-07: AWS Lambda requirements: Lambda Trigger: SQS Queue")]
        public async Task TestLambdaTriggerSQSQueue()
        {
            string functionName = await LambdaHelper.GetLambdaFunctionNameAsync(LambdaClient, FunctionNamePrefix);
            string queueName = await SQSHelper.GetSqsQueueNameAsync(SqsClient, QueueNamePrefix);

            Assert.That(functionName, Is.Not.Null, "The Lambda function with the specified prefix does not exist");
            Assert.That(queueName, Is.Not.Null, "The SQS queue with the specified prefix does not exist");

            string expectedQueueArn = $"arn:aws:sqs:eu-central-1:396913717218:{queueName}";
            var eventSourceMappings = await LambdaHelper.GetEventSourceMappingsAsync(LambdaClient, functionName);
            Assert.That(eventSourceMappings, Is.Not.Null.And.Not.Empty, "The Lambda function has no event source mappings");

            var hasSqsTrigger = eventSourceMappings.Any(mapping => mapping.EventSourceArn == expectedQueueArn);
            Assert.That(hasSqsTrigger, Is.True, $"The Lambda function is not triggered by the expected SQS queue. Expected: {expectedQueueArn}");
        }

        [Test(Description = "CXQA-SLESS-07: AWS Lambda requirements: Lambda application logs are stored in CloudWatch log group (aws/lambda/cloudxserverless-EventHandlerLambda{unique id})")]
        public async Task TestLambdaApplicationLogsStoredInCloudWatchLogGroup()
        {
            string functionName = await LambdaHelper.GetLambdaFunctionNameAsync(LambdaClient, FunctionNamePrefix);
            Assert.That(functionName, Is.Not.Null, "The Lambda function with the specified prefix does not exist");

            string logGroupName = $"{LogGroupNamePrefix}{functionName.Substring(FunctionNamePrefix.Length)}";
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(CloudWatchLogsClient, logGroupName);
            Assert.That(logGroupExists, Is.True, $"The CloudWatch log group does not exist. Expected: {logGroupName}");
        }

        [Test(Description = "CXQA-SLESS-07: AWS Lambda requirements: Memory: 128 MB")]
        public async Task TestLambdaMemory()
        {
            string functionName = await LambdaHelper.GetLambdaFunctionNameAsync(LambdaClient, FunctionNamePrefix);
            Assert.That(functionName, Is.Not.Null, "The Lambda function with the specified prefix does not exist");

            int memorySize = await LambdaHelper.GetLambdaFunctionMemoryAsync(LambdaClient, functionName);
            Assert.That(memorySize, Is.EqualTo(128), "The Lambda function memory size is not set to 128 MB");
        }

        [Test(Description = "CXQA-SLESS-07: AWS Lambda requirements: Ephemeral storage: 512 MB")]
        public async Task TestLambdaEphemeralStorage()
        {
            string functionName = await LambdaHelper.GetLambdaFunctionNameAsync(LambdaClient, FunctionNamePrefix);
            Assert.That(functionName, Is.Not.Null, "The Lambda function with the specified prefix does not exist");

            int ephemeralStorageSize = await LambdaHelper.GetLambdaFunctionEphemeralStorageAsync(LambdaClient, functionName);
            Assert.That(ephemeralStorageSize, Is.EqualTo(512), "The Lambda function ephemeral storage size is not set to 512 MB");
        }

        [Test(Description = "CXQA-SLESS-07: AWS Lambda requirements: Timeout: 3 sec")]
        public async Task TestLambdaTimeout()
        {
            string functionName = await LambdaHelper.GetLambdaFunctionNameAsync(LambdaClient, FunctionNamePrefix);
            Assert.That(functionName, Is.Not.Null, "The Lambda function with the specified prefix does not exist");

            int timeout = await LambdaHelper.GetLambdaFunctionTimeoutAsync(LambdaClient, functionName);
            Assert.That(timeout, Is.EqualTo(3), "The Lambda function timeout is not set to 3 seconds");
        }

        [Test(Description = "CXQA-SLESS-07: AWS Lambda requirements: Tags: cloudx: qa")]
        public async Task TestLambdaTags()
        {
            var expectedTagKey = "cloudx";
            var expectedTagValue = "qa";

            string functionName = await LambdaHelper.GetLambdaFunctionNameAsync(LambdaClient, FunctionNamePrefix);
            Assert.That(functionName, Is.Not.Null, "The Lambda function with the specified prefix does not exist");

            var tags = await LambdaHelper.GetLambdaFunctionTagsAsync(LambdaClient, functionName);
            Assert.That(tags, Is.Not.Null.And.Not.Empty, "The Lambda function has no tags");

            var hasCorrectTag = tags.Any(tag => tag.Key == expectedTagKey && tag.Value == expectedTagValue);
            Assert.That(hasCorrectTag, Is.True, $"The Lambda function does not have the correct tag. Expected: {expectedTagKey}: {expectedTagValue}");
        }
    }
}
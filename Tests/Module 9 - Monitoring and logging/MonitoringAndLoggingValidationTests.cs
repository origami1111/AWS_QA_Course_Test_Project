using AWS_QA_Course_Test_Project.Base;
using AWS_QA_Course_Test_Project.Clients;
using AWS_QA_Course_Test_Project.DTOs;
using AWS_QA_Course_Test_Project.Utils;

namespace AWS_QA_Course_Test_Project.Tests.Module_9___Monitoring_and_logging
{
    [TestFixture]
    public class MonitoringAndLoggingValidationTests : BaseTest
    {
        private const string AppLogGroupName = "/var/log/cloudxserverless-app";
        private const string FunctionNamePrefix = "cloudxserverless-EventHandlerLambda";

        private const string EmailToSubscribe = "novembertanks@gmail.com";
        private const string FilePath = "C:\\Users\\origami\\Desktop\\CloudX Associate AWS for Testers\\AWS_QA_Course_Test_Project\\Images\\image0.jpg";

        [Test(Description = "CXQA-MON-06: Cloudxserverless-EventHandlerLambda{unique_id} log group: 1. POST notification event processed by Event Handler Lambda is logged in the CloudWatch logs; 2. For each notification, the image information (object key, object type, object size, modification date, download link) is logged in the Event Handler Lambda logs in CloudWatch logs")]
        public async Task TestPostImageEventIsLoggedInCloudWatchLogs()
        {
            string functionName = await LambdaHelper.GetLambdaFunctionNameAsync(LambdaClient, FunctionNamePrefix);
            string logGroupName = $"/aws/lambda/{functionName}";

            // Check if the log group exists
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(CloudWatchLogsClient, logGroupName);
            Assert.That(logGroupExists, Is.True, $"LogGroup {logGroupName} does not exist in CloudWatch Logs");

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string fqdn = publicInstance.PublicDnsName;
            string publicIpAddress = publicInstance.PublicIpAddress;
            string logStreamName = publicInstance.InstanceId;
            var restClient = new RestClient(publicIpAddress);

            // Upload an image to generate a log entry
            var postImageResponse = await restClient.PostImageAsync(FilePath);
            Assert.That(postImageResponse, Is.Not.Null, "Failed to upload image");

            // Get image
            var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);

            // Wait for the logs to be available in CloudWatch
            await Task.Delay(30000);

            // Retrieve the log events from the log stream - after
            var allMessages = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, logGroupName);
            Assert.That(allMessages, Is.Not.Empty, "No log events found in the log stream");

            Assert.Multiple(() =>
            {
                Assert.That(allMessages.Any(message => message.Contains("Published message body: event_type: upload")), Is.True, "Message doesn`t contain event_type");
                Assert.That(allMessages.Any(message => message.Contains($"object_key: {getImageResponse.ObjectKey}")), Is.True, "Message doesn`t contain object_key");
                Assert.That(allMessages.Any(message => message.Contains($"object_type: {getImageResponse.ObjectType}")), Is.True, "Message doesn`t contain object_type");
                Assert.That(allMessages.Any(message => message.Contains($"last_modified: {DateUtils.ParseNumericDateFormatT(getImageResponse.LastModified)}")), Is.True, "Message doesn`t contain last_modified");
                Assert.That(allMessages.Any(message => message.Contains($"object_size: {getImageResponse.ObjectSize}")), Is.True, "Message doesn`t contain object_size");
                Assert.That(allMessages.Any(message => message.Contains($"download_link: http://{fqdn}/api/image/file/{getImageResponse.Id}")), Is.True, "Message doesn`t contain download_link");
            });
        }

        [Test(Description = "CXQA-MON-06: Cloudxserverless-EventHandlerLambda{unique_id} log group: 1. POST notification events processed by Event Handler Lambda is logged in the CloudWatch logs; 2. For each notification, the image information (object key, object type, object size, modification date, download link) is logged in the Event Handler Lambda logs in CloudWatch logs")]
        public async Task TestPostImagesEventsIsLoggedInCloudWatchLogs()
        {
            string functionName = await LambdaHelper.GetLambdaFunctionNameAsync(LambdaClient, FunctionNamePrefix);
            string logGroupName = $"/aws/lambda/{functionName}";

            // Check if the log group exists
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(CloudWatchLogsClient, logGroupName);
            Assert.That(logGroupExists, Is.True, $"LogGroup {logGroupName} does not exist in CloudWatch Logs");

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string fqdn = publicInstance.PublicDnsName;
            string publicIpAddress = publicInstance.PublicIpAddress;
            string logStreamName = publicInstance.InstanceId;
            var restClient = new RestClient(publicIpAddress);
            int imagesCount = 10;

            // Upload an images to generate a log entry
            var postImageResponses = new List<PostImageResponseDTO>();
            for (int i = 0; i < imagesCount; i++)
            {
                postImageResponses.Add(await restClient.PostImageAsync(FilePath));
            }

            // Get images
            var getImageResponses = new List<ImageResponseDTO>();
            for (int i = 0; i < imagesCount; i++)
            {
                getImageResponses.Add(await restClient.GetImageMetadataAsync(postImageResponses[i].Id));
            }

            // Wait for the logs to be available in CloudWatch
            await Task.Delay(30000);

            // Retrieve the log events from the log stream - after
            var allMessages = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, logGroupName);
            Assert.That(allMessages, Is.Not.Empty, "No log events found in the log stream");

            Assert.Multiple(() =>
            {
                for (int i = 0; i < imagesCount; i++)
                {
                    Assert.That(allMessages.Any(message => message.Contains($"object_key: {getImageResponses[i].ObjectKey}")), Is.True, "Message doesn`t contain object_key");
                    Assert.That(allMessages.Any(message => message.Contains($"object_type: {getImageResponses[i].ObjectType}")), Is.True, "Message doesn`t contain object_type");
                    Assert.That(allMessages.Any(message => message.Contains($"last_modified: {DateUtils.ParseNumericDateFormatT(getImageResponses[i].LastModified)}")), Is.True, "Message doesn`t contain last_modified");
                    Assert.That(allMessages.Any(message => message.Contains($"object_size: {getImageResponses[i].ObjectSize}")), Is.True, "Message doesn`t contain object_size");
                    Assert.That(allMessages.Any(message => message.Contains($"download_link: http://{fqdn}/api/image/file/{getImageResponses[i].Id}")), Is.True, "Message doesn`t contain download_link");
                }
            });
        }

        [Test(Description = "CXQA-MON-06: Cloudxserverless-EventHandlerLambda{unique_id} log group: 1. DELETE notification event processed by Event Handler Lambda is logged in the CloudWatch logs; 2. For each notification, the image information (object key, object type, object size, modification date, download link) is logged in the Event Handler Lambda logs in CloudWatch logs")]
        public async Task TestDeleteImageEventIsLoggedInCloudWatchLogs()
        {
            string functionName = await LambdaHelper.GetLambdaFunctionNameAsync(LambdaClient, FunctionNamePrefix);
            string logGroupName = $"/aws/lambda/{functionName}";

            // Check if the log group exists
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(CloudWatchLogsClient, logGroupName);
            Assert.That(logGroupExists, Is.True, $"LogGroup {logGroupName} does not exist in CloudWatch Logs");

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string fqdn = publicInstance.PublicDnsName;
            string publicIpAddress = publicInstance.PublicIpAddress;
            string logStreamName = publicInstance.InstanceId;
            var restClient = new RestClient(publicIpAddress);

            // Upload an image to generate a log entry
            var postImageResponse = await restClient.PostImageAsync(FilePath);
            Assert.That(postImageResponse, Is.Not.Null, "Failed to upload image");

            // Get image
            var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);

            // Delete an image
            await restClient.DeleteImageAsync(postImageResponse.Id);

            // Wait for the logs to be available in CloudWatch
            await Task.Delay(30000);

            // Retrieve the log events from the log stream - after
            var allMessages = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, logGroupName);
            Assert.That(allMessages, Is.Not.Empty, "No log events found in the log stream");

            Assert.Multiple(() =>
            {
                Assert.That(allMessages.Any(message => message.Contains("Published message body: event_type: delete")), Is.True, "Message doesn`t contain event_type");
                Assert.That(allMessages.Any(message => message.Contains($"object_key: {getImageResponse.ObjectKey}")), Is.True, "Message doesn`t contain object_key");
                Assert.That(allMessages.Any(message => message.Contains($"object_type: {getImageResponse.ObjectType}")), Is.True, "Message doesn`t contain object_type");
                Assert.That(allMessages.Any(message => message.Contains($"last_modified: {DateUtils.ParseNumericDateFormatT(getImageResponse.LastModified)}")), Is.True, "Message doesn`t contain last_modified");
                Assert.That(allMessages.Any(message => message.Contains($"object_size: {getImageResponse.ObjectSize}")), Is.True, "Message doesn`t contain object_size");
                Assert.That(allMessages.Any(message => message.Contains($"download_link: ")), Is.True, "Message doesn`t contain download_link");
            });
        }

        [Test(Description = "CXQA-MON-07: Cloudxserverless-app log group: GET /image API request processed by the application are logged in the CloudWatch logs")]
        public async Task TestGetImageRequestIsLoggedInCloudWatchLogs()
        {
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(CloudWatchLogsClient, AppLogGroupName);
            Assert.That(logGroupExists, Is.True, $"LogGroup {AppLogGroupName} does not exist in CloudWatch Logs");

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIpAddress = publicInstance.PublicIpAddress;
            string logStreamName = publicInstance.InstanceId;
            var restClient = new RestClient(publicIpAddress);

            // Upload an image to generate a log entry
            var postImageResponse = await restClient.PostImageAsync(FilePath);
            Assert.That(postImageResponse, Is.Not.Null, "Failed to upload image");

            // Retrieve the image metadata to generate a log entry
            var getImageResponse = await restClient.GetImagesAsync();
            Assert.That(getImageResponse, Is.Not.Null, "Failed to retrieve image metadata");

            // Check if the log stream exists
            bool logStreamExists = await CloudWatchLogsHelper.IsLogStreamExistsAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(logStreamExists, Is.True, $"LogStream {logStreamName} does not exist in LogGroup {AppLogGroupName}");

            // Retrieve the log events from the log stream - before
            var allMessagesBefore = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesBefore, Is.Not.Empty, "No log events found in the log stream");

            // Wait for the logs to be available in CloudWatch
            await Task.Delay(60000);

            // Retrieve the log events from the log stream - after
            var allMessagesAfter = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesAfter, Is.Not.Empty, "No log events found in the log stream");

            Assert.That(allMessagesAfter.Count, Is.EqualTo(allMessagesBefore.Count + 2), "No new log events found in the log stream after the GET request");

            // Verify that the log entry for the GET request contains the expected image information
            var lastLogEntry = allMessagesAfter.LastOrDefault();
            Assert.That(lastLogEntry, Does.Contain("GET /api/image HTTP/1.1"), "Log entry for the GET /image request not found in the log stream"); 
        }

        [Test(Description = "CXQA-MON-07: Cloudxserverless-app log group: POST /image API request processed by the application are logged in the CloudWatch logs")]
        public async Task TestPostImageRequestIsLoggedInCloudWatchLogs()
        {
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(CloudWatchLogsClient, AppLogGroupName);
            Assert.That(logGroupExists, Is.True, $"LogGroup {AppLogGroupName} does not exist in CloudWatch Logs");

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIpAddress = publicInstance.PublicIpAddress;
            string logStreamName = publicInstance.InstanceId;
            var restClient = new RestClient(publicIpAddress);

            // Upload an image to generate a log entry
            var postImageResponse = await restClient.PostImageAsync(FilePath);
            Assert.That(postImageResponse, Is.Not.Null, "Failed to upload image");

            // Check if the log stream exists
            bool logStreamExists = await CloudWatchLogsHelper.IsLogStreamExistsAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(logStreamExists, Is.True, $"LogStream {logStreamName} does not exist in LogGroup {AppLogGroupName}");

            // Retrieve the log events from the log stream - before
            var allMessagesBefore = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesBefore, Is.Not.Empty, "No log events found in the log stream");

            // Wait for the logs to be available in CloudWatch
            await Task.Delay(60000);

            // Retrieve the log events from the log stream - after
            var allMessagesAfter = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesAfter, Is.Not.Empty, "No log events found in the log stream");

            Assert.That(allMessagesAfter.Count, Is.EqualTo(allMessagesBefore.Count + 1), "No new log events found in the log stream after the POST request");

            // Verify that the log entry for the POST request contains the expected image information
            var lastLogEntry = allMessagesAfter.LastOrDefault();
            Assert.That(lastLogEntry, Does.Contain("POST /api/image HTTP/1.1"), "Log entry for the POST /image request not found in the log stream");
        }

        // TODO - Run the test
        [Test(Description = "CXQA-MON-07: Cloudxserverless-app log group: POST /image`s API request processed by the application are logged in the CloudWatch logs")]
        public async Task TestPostImagesRequestIsLoggedInCloudWatchLogs()
        {
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(CloudWatchLogsClient, AppLogGroupName);
            Assert.That(logGroupExists, Is.True, $"LogGroup {AppLogGroupName} does not exist in CloudWatch Logs");

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIpAddress = publicInstance.PublicIpAddress;
            string logStreamName = publicInstance.InstanceId;
            var restClient = new RestClient(publicIpAddress);
            int imagesCount = 10;

            // Upload an images to generate a log entry
            for (int i = 0; i < imagesCount; i++)
            {
                await restClient.PostImageAsync(FilePath);
            }

            // Check if the log stream exists
            bool logStreamExists = await CloudWatchLogsHelper.IsLogStreamExistsAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(logStreamExists, Is.True, $"LogStream {logStreamName} does not exist in LogGroup {AppLogGroupName}");

            // Retrieve the log events from the log stream - before
            var allMessagesBefore = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesBefore, Is.Not.Empty, "No log events found in the log stream");

            // Wait for the logs to be available in CloudWatch
            await Task.Delay(60000);

            // Retrieve the log events from the log stream - after
            var allMessagesAfter = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesAfter, Is.Not.Empty, "No log events found in the log stream");

            Assert.That(allMessagesAfter.Count, Is.EqualTo(allMessagesBefore.Count + 10), "No new log events found in the log stream after the POST request");

            // Verify that the log entry for the POST request contains the expected image information
            var lastLogEntries = allMessagesAfter.TakeLast(10);
            Assert.That(lastLogEntries.All(m => m.Contains("POST /api/image HTTP/1.1")), "Log entry for the POST /image request not found in the log stream");
        }

        [Test(Description = "CXQA-MON-07: Cloudxserverless-app log group: GET /image/file/{image_id} API request processed by the application are logged in the CloudWatch logs")]
        public async Task TestGetImageToDownloadRequestIsLoggedInCloudWatchLogs()
        {
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(CloudWatchLogsClient, AppLogGroupName);
            Assert.That(logGroupExists, Is.True, $"LogGroup {AppLogGroupName} does not exist in CloudWatch Logs");

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIpAddress = publicInstance.PublicIpAddress;
            string logStreamName = publicInstance.InstanceId;
            var restClient = new RestClient(publicIpAddress);

            // Upload an image to generate a log entry
            var postImageResponse = await restClient.PostImageAsync(FilePath);
            Assert.That(postImageResponse, Is.Not.Null, "Failed to upload image");

            // Download the image to generate a log entry
            var getImageResponse = await restClient.GetImageAsync(postImageResponse.Id);
            Assert.That(getImageResponse, Is.Not.Null, "Failed to retrieve image metadata");

            // Check if the log stream exists
            bool logStreamExists = await CloudWatchLogsHelper.IsLogStreamExistsAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(logStreamExists, Is.True, $"LogStream {logStreamName} does not exist in LogGroup {AppLogGroupName}");

            // Retrieve the log events from the log stream - before
            var allMessagesBefore = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesBefore, Is.Not.Empty, "No log events found in the log stream");

            // Wait for the logs to be available in CloudWatch
            await Task.Delay(60000);

            // Retrieve the log events from the log stream - after
            var allMessagesAfter = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesAfter, Is.Not.Empty, "No log events found in the log stream");

            Assert.That(allMessagesAfter.Count, Is.EqualTo(allMessagesBefore.Count + 2), "No new log events found in the log stream after the GET request");

            // Verify that the log entry for the GET request contains the expected image information
            var lastLogEntry = allMessagesAfter.LastOrDefault();
            Assert.That(lastLogEntry, Does.Contain($"GET /api/image/file/{postImageResponse.Id} HTTP/1.1"), $"Log entry for the GET /api/image/file/{postImageResponse.Id} request not found in the log stream");
        }

        [Test(Description = "CXQA-MON-07: Cloudxserverless-app log group: DELETE /image/{image_id} API request processed by the application are logged in the CloudWatch logs")]
        public async Task TestDeleteImageRequestIsLoggedInCloudWatchLogs()
        {
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(CloudWatchLogsClient, AppLogGroupName);
            Assert.That(logGroupExists, Is.True, $"LogGroup {AppLogGroupName} does not exist in CloudWatch Logs");

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIpAddress = publicInstance.PublicIpAddress;
            string logStreamName = publicInstance.InstanceId;
            var restClient = new RestClient(publicIpAddress);

            // Upload an image to generate a log entry
            var postImageResponse = await restClient.PostImageAsync(FilePath);
            Assert.That(postImageResponse, Is.Not.Null, "Failed to upload image");

            // Delete the image to generate a log entry
            await restClient.DeleteImageAsync(postImageResponse.Id);

            // Check if the log stream exists
            bool logStreamExists = await CloudWatchLogsHelper.IsLogStreamExistsAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(logStreamExists, Is.True, $"LogStream {logStreamName} does not exist in LogGroup {AppLogGroupName}");

            // Retrieve the log events from the log stream - before
            var allMessagesBefore = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesBefore, Is.Not.Empty, "No log events found in the log stream");

            // Wait for the logs to be available in CloudWatch
            await Task.Delay(60000);

            // Retrieve the log events from the log stream - after
            var allMessagesAfter = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesAfter, Is.Not.Empty, "No log events found in the log stream");

            Assert.That(allMessagesAfter.Count, Is.EqualTo(allMessagesBefore.Count + 2), "No new log events found in the log stream after the DELETE request");

            // Verify that the log entry for the DELETE request contains the expected image information
            var lastLogEntry = allMessagesAfter.LastOrDefault();
            Assert.That(lastLogEntry, Does.Contain($"DELETE /api/image/{postImageResponse.Id} HTTP/1.1"), $"Log entry for the DELETE /api/image/{postImageResponse.Id} request not found in the log stream");
        }

        [Test(Description = "CXQA-MON-07: Cloudxserverless-app log group: GET /image/{image_id} API request processed by the application are logged in the CloudWatch logs")]
        public async Task TestGetImageMetadataRequestIsLoggedInCloudWatchLogs()
        {
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(CloudWatchLogsClient, AppLogGroupName);
            Assert.That(logGroupExists, Is.True, $"LogGroup {AppLogGroupName} does not exist in CloudWatch Logs");

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIpAddress = publicInstance.PublicIpAddress;
            string logStreamName = publicInstance.InstanceId;
            var restClient = new RestClient(publicIpAddress);

            // Upload an image to generate a log entry
            var postImageResponse = await restClient.PostImageAsync(FilePath);
            Assert.That(postImageResponse, Is.Not.Null, "Failed to upload image");

            // Get the image metadata to generate a log entry
            var getImageMetadataResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);
            Assert.That(getImageMetadataResponse, Is.Not.Null, "Failed to retrieve image metadata");

            // Check if the log stream exists
            bool logStreamExists = await CloudWatchLogsHelper.IsLogStreamExistsAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(logStreamExists, Is.True, $"LogStream {logStreamName} does not exist in LogGroup {AppLogGroupName}");

            // Retrieve the log events from the log stream - before
            var allMessagesBefore = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesBefore, Is.Not.Empty, "No log events found in the log stream");

            // Wait for the logs to be available in CloudWatch
            await Task.Delay(60000);

            // Retrieve the log events from the log stream - after
            var allMessagesAfter = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesAfter, Is.Not.Empty, "No log events found in the log stream");

            Assert.That(allMessagesAfter.Count, Is.EqualTo(allMessagesBefore.Count + 2), "No new log events found in the log stream after the GET request");

            // Verify that the log entry for the GET request contains the expected image information
            var lastLogEntry = allMessagesAfter.LastOrDefault();
            Assert.That(lastLogEntry, Does.Contain($"GET /api/image/{postImageResponse.Id} HTTP/1.1"), $"Log entry for the GET /api/image/{postImageResponse.Id} request not found in the log stream");
        }

        [Test(Description = "CXQA-MON-07: Cloudxserverless-app log group: GET /notification API request processed by the application are logged in the CloudWatch logs")]
        public async Task TestGetNotificationRequestIsLoggedInCloudWatchLogs()
        {
            var gmailClient = new GmailClient(EmailToSubscribe);
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIp = publicInstance.PublicIpAddress;
            string logStreamName = publicInstance.InstanceId;
            var restClient = new RestClient(publicIp);
            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudxserverless-TopicSNSTopic");

            // Check if the log group exists
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(CloudWatchLogsClient, AppLogGroupName);
            Assert.That(logGroupExists, Is.True, $"LogGroup {AppLogGroupName} does not exist in CloudWatch Logs");

            // Check if the log stream exists
            bool logStreamExists = await CloudWatchLogsHelper.IsLogStreamExistsAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(logStreamExists, Is.True, $"LogStream {logStreamName} does not exist in LogGroup {AppLogGroupName}");

            // Retrieve the log events from the log stream - before
            var allMessagesBefore = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesBefore, Is.Not.Empty, "No log events found in the log stream");

            // Subscribe the user to the SNS topic and confirm subbscription
            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

            // Get the list of notifications to generate a log entry
            var getNotificationResponse = await restClient.GetNotificationsAsync();

            // Wait for the logs to be available in CloudWatch
            await Task.Delay(40000);

            // Retrieve the log events from the log stream - after
            var allMessagesAfter = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesAfter, Is.Not.Empty, "No log events found in the log stream");

            // Unsubscribe the user
            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

            Assert.That(allMessagesAfter.Count, Is.EqualTo(allMessagesBefore.Count + 2), "No new log events found in the log stream after the GET request");

            // Verify that the log entry for the POST request contains the expected image information
            var lastLogEntry = allMessagesAfter.LastOrDefault();
            Assert.That(lastLogEntry, Does.Contain($"GET /api/notification HTTP/1.1"), $"Log entry for the GET /api/notification request not found in the log stream");
        }

        [Test(Description = "CXQA-MON-07: Cloudxserverless-app log group: DELETE /notification/{email_address} API request processed by the application are logged in the CloudWatch logs")]
        public async Task TestDeleteNotificationRequestIsLoggedInCloudWatchLogs()
        {
            var gmailClient = new GmailClient(EmailToSubscribe);
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIp = publicInstance.PublicIpAddress;
            string logStreamName = publicInstance.InstanceId;
            var restClient = new RestClient(publicIp);
            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudxserverless-TopicSNSTopic");

            // Check if the log group exists
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(CloudWatchLogsClient, AppLogGroupName);
            Assert.That(logGroupExists, Is.True, $"LogGroup {AppLogGroupName} does not exist in CloudWatch Logs");

            // Check if the log stream exists
            bool logStreamExists = await CloudWatchLogsHelper.IsLogStreamExistsAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(logStreamExists, Is.True, $"LogStream {logStreamName} does not exist in LogGroup {AppLogGroupName}");

            // Retrieve the log events from the log stream - before
            var allMessagesBefore = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesBefore, Is.Not.Empty, "No log events found in the log stream");

            // Subscribe the user to the SNS topic and confirm subbscription
            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

            // Unsubscribe the user to generate a log entry
            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

            // Wait for the logs to be available in CloudWatch
            await Task.Delay(40000);

            // Retrieve the log events from the log stream - after
            var allMessagesAfter = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesAfter, Is.Not.Empty, "No log events found in the log stream");

            Assert.That(allMessagesAfter.Count, Is.EqualTo(allMessagesBefore.Count + 2), "No new log events found in the log stream after the DELETE request");

            // Verify that the log entry for the DELETE request contains the expected image information
            var lastLogEntry = allMessagesAfter.LastOrDefault();
            Assert.That(lastLogEntry, Does.Contain($"DELETE /api/notification/{EmailToSubscribe} HTTP/1.1"), $"Log entry for the DELETE /api/notification/{EmailToSubscribe} request not found in the log stream");
        }

        [Test(Description = "CXQA-MON-07: Cloudxserverless-app log group: POST /notification/{email_address} API request processed by the application are logged in the CloudWatch logs")]
        public async Task TestPostNotificationRequestIsLoggedInCloudWatchLogs()
        {
            var gmailClient = new GmailClient(EmailToSubscribe);
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIp = publicInstance.PublicIpAddress;
            string logStreamName = publicInstance.InstanceId;
            var restClient = new RestClient(publicIp);
            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudxserverless-TopicSNSTopic");

            // Check if the log group exists
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(CloudWatchLogsClient, AppLogGroupName);
            Assert.That(logGroupExists, Is.True, $"LogGroup {AppLogGroupName} does not exist in CloudWatch Logs");

            // Check if the log stream exists
            bool logStreamExists = await CloudWatchLogsHelper.IsLogStreamExistsAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(logStreamExists, Is.True, $"LogStream {logStreamName} does not exist in LogGroup {AppLogGroupName}");

            // Retrieve the log events from the log stream - before
            var allMessagesBefore = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesBefore, Is.Not.Empty, "No log events found in the log stream");

            // Subscribe the user to the SNS topic and confirm subbscription
            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

            // Wait for the logs to be available in CloudWatch
            await Task.Delay(30000);

            // Retrieve the log events from the log stream - after
            var allMessagesAfter = await CloudWatchLogsHelper.GetAllLogEventMessagesAsync(CloudWatchLogsClient, AppLogGroupName, logStreamName);
            Assert.That(allMessagesAfter, Is.Not.Empty, "No log events found in the log stream");

            // Unsubscribe the user
            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

            Assert.That(allMessagesAfter.Count, Is.EqualTo(allMessagesBefore.Count + 1), "No new log events found in the log stream after the POST request");

            // Verify that the log entry for the POST request contains the expected image information
            var lastLogEntry = allMessagesAfter.LastOrDefault();
            Assert.That(lastLogEntry, Does.Contain($"POST /api/notification/{EmailToSubscribe} HTTP/1.1"), $"Log entry for the POST /api/notification/{EmailToSubscribe} request not found in the log stream");
        }
    }
}

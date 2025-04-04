//using Amazon.SQS.Model;
//using AWS_QA_Course_Test_Project.Base;
//using AWS_QA_Course_Test_Project.Clients;
//using AWS_QA_Course_Test_Project.Utils;
//using NUnit.Framework.Internal;
//using NUnit.Framework.Legacy;
//using System.Text.Json;

//namespace AWS_QA_Course_Test_Project.Tests
//{
//    [TestFixture]
//    public class SNSTests : BaseTest
//    {
//        // CXQA-SNSSQS-01: Application Instance requirements:
//        [Test(Description = "CXQA-SNSSQS-01: The application uses an SNS topic to subscribe and unsubscribe users, list existing subscriptions, and send e-mail messages to subscribers about upload and delete image events, in a readable format (not JSON)")]
//        public async Task TestSNSCommunicateInAReadableFormatTextType()
//        {
//            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudximage-TopicSNSTopic");
//            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

//            var attributes = await SNSHelper.GetTopicAttributesAsync(SnsClient, topicArn);
//            var topicEffectiveDeliveryPolicy = attributes.ContainsKey("EffectiveDeliveryPolicy") ? attributes["EffectiveDeliveryPolicy"] : null;

//            Assert.That(topicEffectiveDeliveryPolicy, Is.Not.Null, "EffectiveDeliveryPolicy attribute not found.");

//            var jsonDocument = JsonDocument.Parse(topicEffectiveDeliveryPolicy);
//            var headerContentType = jsonDocument.RootElement
//                .GetProperty("http")
//                .GetProperty("defaultRequestPolicy")
//                .GetProperty("headerContentType")
//                .GetString();

//            Assert.That(headerContentType, Is.EqualTo("text/plain; charset=UTF-8"), "The headerContentType is not 'text/plain; charset=UTF-8'.");
//        }

//        [Test(Description = "CXQA-SNSSQS-01: The application uses an SQS queue to publish event messages")]
//        public async Task TestSQSQueueUsedToPublishEventMessages()
//        {
//            var queueUrl = await SQSHelper.GetQueueUrlAsync(SqsClient, "cloudximage-QueueSQSQueue");
//            Assert.That(queueUrl, Is.Not.Null, "SQS queue with the expected name pattern not found.");

//            var messageBody = "This is a test event message.";
//            var sendMessageRequest = new SendMessageRequest
//            {
//                QueueUrl = queueUrl,
//                MessageBody = messageBody
//            };

//            var sendMessageResponse = await SqsClient.SendMessageAsync(sendMessageRequest);
//            Assert.That(sendMessageResponse.HttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to send message to SQS queue.");

//            // Retrieve the message from the SQS queue
//            var receiveMessageRequest = new ReceiveMessageRequest
//            {
//                QueueUrl = queueUrl,
//                MaxNumberOfMessages = 1,
//                WaitTimeSeconds = 10
//            };

//            var receiveMessageResponse = await SqsClient.ReceiveMessageAsync(receiveMessageRequest);
//            var receivedMessage = receiveMessageResponse.Messages.FirstOrDefault();

//            Assert.That(receivedMessage, Is.Not.Null, "No message received from SQS queue.");
//            Assert.That(receivedMessage.Body, Is.EqualTo(messageBody), "The message content does not match the expected event message.");
//        }

//        [Test(Description = "CXQA-SNSSQS-01: The application should have access to the SQS queue and the SNS topic via IAM roles")]
//        public async Task TestApplicationAccessToSQSAndSNSTopicViaIAMRoles()
//        {
//            // Check access to SNS topic
//            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudximage-TopicSNSTopic");
//            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

//            try
//            {
//                var attributes = await SNSHelper.GetTopicAttributesAsync(SnsClient, topicArn);
//                Assert.That(attributes, Is.Not.Null, "Failed to get SNS topic attributes.");
//            }
//            catch (Exception ex)
//            {
//                Assert.Fail($"Failed to access SNS topic: {ex.Message}");
//            }

//            // Check access to SQS queue
//            var queueUrl = await SQSHelper.GetQueueUrlAsync(SqsClient, "cloudximage-QueueSQSQueue");
//            Assert.That(queueUrl, Is.Not.Null, "SQS queue with the expected name pattern not found.");

//            try
//            {
//                var attributes = await SQSHelper.GetQueueAttributesAsync(SqsClient, queueUrl, new List<string> { "All" });
//                Assert.That(attributes, Is.Not.Null, "Failed to get SQS queue attributes.");
//            }
//            catch (Exception ex)
//            {
//                Assert.Fail($"Failed to access SQS queue: {ex.Message}");
//            }
//        }

//        // CXQA-SNSSQS-02: SNS topic requirements:
//        [Test(Description = "CXQA-SNSSQS-02: Name: cloudximage-TopicSNSTopic{unique id}")]
//        public async Task TestSNSName()
//        {
//            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudximage-TopicSNSTopic");
//            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

//            var topicName = topicArn.Split(':').Last();
//            StringAssert.StartsWith("cloudximage-TopicSNSTopic", topicName, "SNS topic name does not start with the expected prefix.");

//            var uniqueId = topicName.Replace("cloudximage-TopicSNSTopic", "");
//            Assert.That(uniqueId, Is.Not.Empty, "SNS topic name does not contain a unique ID.");
//        }

//        [Test(Description = "CXQA-SNSSQS-02: Type: standard")]
//        public async Task TestSNSType()
//        {
//            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudximage-TopicSNSTopic");
//            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

//            var attributes = await SNSHelper.GetTopicAttributesAsync(SnsClient, topicArn);
//            var topicType = attributes.ContainsKey("FifoTopic") && attributes["FifoTopic"] == "true" ? "fifo" : "standard";

//            Assert.That(topicType, Is.EqualTo("standard"), "SNS topic is not of type standard.");
//        }

//        [Test(Description = "CXQA-SNSSQS-02: Encryption: disabled")]
//        public async Task TestSNSEncryption()
//        {
//            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudximage-TopicSNSTopic");
//            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

//            var attributes = await SNSHelper.GetTopicAttributesAsync(SnsClient, topicArn);
//            var kmsMasterKeyId = attributes.ContainsKey("KmsMasterKeyId") ? attributes["KmsMasterKeyId"] : null;

//            Assert.That(kmsMasterKeyId, Is.Null, "SNS topic encryption is enabled.");
//        }

//        [Test(Description = "CXQA-SNSSQS-02: Tags: cloudx: qa")]
//        public async Task TestSNSTags()
//        {
//            var expectedTagKey = "cloudx";
//            var expectedTagValue = "qa";

//            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudximage-TopicSNSTopic");
//            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

//            var tags = await SNSHelper.GetTopicTagsAsync(SnsClient, topicArn);

//            Assert.That(tags.ContainsKey(expectedTagKey), Is.True, $"SNS topic does not contain the expected tag key '{expectedTagKey}'.");
//            Assert.That(tags[expectedTagKey], Is.EqualTo(expectedTagValue), $"SNS topic tag value for key '{expectedTagKey}' does not match the expected value '{expectedTagValue}'.");
//        }

//        // Application functional validation:
//        private const string EmailToSubscribe = "novembertanks@gmail.com";

//        [Test(Description = "CXQA-SNSSQS-04: The user can subscribe to notifications about application events via a provided email address")]
//        public async Task TestUserCanSubscribeToNotificationsViaEmail()
//        {
//            var gmailClient = new GmailClient(EmailToSubscribe);

//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = EC2Helper.GetPublicInstance(instances);
//            string publicIp = publicInstance.PublicIpAddress;
//            var restClient = new RestClient(publicIp);

//            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudximage-TopicSNSTopic");
//            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

//            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

//            // Verify the subscription
//            var actualSubscriptions = await restClient.GetNotificationsAsync();
//            Assert.That(actualSubscriptions.Any(sub => sub.Endpoint == EmailToSubscribe), Is.True, $"User {EmailToSubscribe} isn't subscribed to the topic");

//            // Unsubscribe the user
//            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
//            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

//            // Wait for the unsubscription to take effect
//            await Task.Delay(3000);

//            // Verify the unsubscription
//            var actualSubscriptionsAfterUnsubscription = await restClient.GetNotificationsAsync();
//            Assert.That(actualSubscriptionsAfterUnsubscription.Any(sub => sub.Endpoint == EmailToSubscribe), Is.False, $"User {EmailToSubscribe} is still subscribed to the topic but shouldn't be");
//        }

//        [Test(Description = "CXQA-SNSSQS-05: The user has to confirm the subscription after receiving the confirmation email")]
//        public async Task TestUserHasToConfirmSubscriptionAfterReceivingConfirmationEmail()
//        {
//            var gmailClient = new GmailClient(EmailToSubscribe);

//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = EC2Helper.GetPublicInstance(instances);
//            string publicIp = publicInstance.PublicIpAddress;
//            var restClient = new RestClient(publicIp);

//            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudximage-TopicSNSTopic");
//            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

//            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

//            // Verify the subscription
//            var actualSubscriptions = await restClient.GetNotificationsAsync();
//            Assert.That(actualSubscriptions.Any(sub => sub.Endpoint == EmailToSubscribe), Is.True, $"User {EmailToSubscribe} isn't subscribed to the topic");

//            // Unsubscribe the user
//            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
//            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

//            // Wait for the unsubscription to take effect
//            await Task.Delay(3000);

//            // Verify the unsubscription
//            var actualSubscriptionsAfterUnsubscription = await restClient.GetNotificationsAsync();
//            Assert.That(actualSubscriptionsAfterUnsubscription.Any(sub => sub.Endpoint == EmailToSubscribe), Is.False, $"User {EmailToSubscribe} is still subscribed to the topic but shouldn't be");
//        }

//        string filePath = "C:\\Users\\origami\\Desktop\\CloudX Associate AWS for Testers\\AWS_QA_Course_Test_Project\\Images\\image0.jpg";
//        string imageName = "image0.jpg";

//        [Test(Description = "CXQA-SNSSQS-06: The subscribed user receives notifications about images events (image is uploaded)")]
//        public async Task TestSubscribedUserReceivesNotificationsAboutUploadedImage()
//        {
//            var gmailClient = new GmailClient(EmailToSubscribe);

//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = EC2Helper.GetPublicInstance(instances);
//            string publicIp = publicInstance.PublicIpAddress;
//            string fqdn = publicInstance.PublicDnsName;
//            var restClient = new RestClient(publicIp);

//            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudximage-TopicSNSTopic");
//            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

//            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

//            var gmailMessagesBeforeUploadingImage = await gmailClient.GetMessagesAsync();

//            // Upload the image
//            var postImageResponse = await restClient.PostImageAsync(filePath);

//            // Get image response
//            var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);

//            await Task.Delay(4000);

//            var gmailMessagesAfterUploadingImage = await gmailClient.GetMessagesAsync();

//            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
//            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

//            Assert.That(gmailMessagesAfterUploadingImage.Count, Is.EqualTo(gmailMessagesBeforeUploadingImage.Count + 1), "The user did not receive a notification about the uploaded image, or received duplication.");

//            var lastMessage = await gmailClient.GetLastMessageAsync();

//            Assert.That(lastMessage.Body, Does.Contain("event_type: upload"), "The notification does not contain the 'event_type: upload'.");
//            Assert.That(lastMessage.Body, Does.Contain($"object_key: {getImageResponse.ObjectKey}"), "The notification does not contain the image object key.");
//            Assert.That(lastMessage.Body, Does.Contain($"object_type: {getImageResponse.ObjectType}"), "The notification does not contain the image object type.");
//            Assert.That(lastMessage.Body, Does.Contain($"last_modified: {DateUtils.FormatDateForUploadedImage(getImageResponse.LastModified)}"), "The notification does not contain the image last modified.");
//            Assert.That(lastMessage.Body, Does.Contain($"object_size: {getImageResponse.ObjectSize}"), "The notification does not contain the image object size.");
//            Assert.That(lastMessage.Body, Does.Contain($"download_link: http://{fqdn}/api/image/file/{getImageResponse.Id}"), "The notification does not contain the download link.");
//        }

//        [Test(Description = "CXQA-SNSSQS-06: The subscribed user receives notifications about images events (image is deleted)")]
//        public async Task TestSubscribedUserReceivesNotificationsAboutDeletedImage()
//        {
//            var gmailClient = new GmailClient(EmailToSubscribe);

//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = EC2Helper.GetPublicInstance(instances);
//            string publicIp = publicInstance.PublicIpAddress;
//            var restClient = new RestClient(publicIp);

//            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudximage-TopicSNSTopic");
//            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

//            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

//            // Upload the image
//            var postImageResponse = await restClient.PostImageAsync(filePath);

//            // Get image response
//            var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);

//            var gmailMessagesBeforeDeletingImage = await gmailClient.GetMessagesAsync();

//            // Delete the image
//            await restClient.DeleteImageAsync(postImageResponse.Id);

//            await Task.Delay(4000);

//            var gmailMessagesAfterDeletingImage = await gmailClient.GetMessagesAsync();
//            var lastMessage = await gmailClient.GetLastMessageAsync();

//            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
//            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

//            Assert.That(gmailMessagesAfterDeletingImage.Count, Is.EqualTo(gmailMessagesBeforeDeletingImage.Count + 1), "The user did not receive a notification about the uploaded image, or received duplicates");

//            Assert.That(lastMessage.Body, Does.Contain("event_type: delete"), "The notification does not contain the 'event_type: delete'.");
//            Assert.That(lastMessage.Body, Does.Contain($"object_key: {getImageResponse.ObjectKey}"), "The notification does not contain the image object key.");
//            Assert.That(lastMessage.Body, Does.Contain($"object_type: {getImageResponse.ObjectType}"), "The notification does not contain the image object type.");
//            Assert.That(lastMessage.Body, Does.Contain($"last_modified: {DateUtils.FormatDateForDeletedImage(getImageResponse.LastModified)}"), "The notification does not contain the image last modified.");
//            Assert.That(lastMessage.Body, Does.Contain($"object_size: {getImageResponse.ObjectSize}"), "The notification does not contain the image object size.");
//            Assert.That(lastMessage.Body, Does.Contain("download_link:"), "The notification does not contain the download link.");
//        }

//        [Test(Description = "CXQA-SNSSQS-07: The notification contains the correct image metadata information and a download link")]
//        public async Task TestNotificationContainsCorrectImageMetadataAndDownloadLink()
//        {
//            var gmailClient = new GmailClient(EmailToSubscribe);

//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = EC2Helper.GetPublicInstance(instances);
//            string publicIp = publicInstance.PublicIpAddress;
//            string fqdn = publicInstance.PublicDnsName;
//            var restClient = new RestClient(publicIp);

//            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudximage-TopicSNSTopic");
//            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

//            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

//            var gmailMessagesBeforeUploadingImage = await gmailClient.GetMessagesAsync();

//            // Upload the image
//            var postImageResponse = await restClient.PostImageAsync(filePath);

//            // Get image response
//            var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);

//            await Task.Delay(4000);

//            var gmailMessagesAfterUploadingImage = await gmailClient.GetMessagesAsync();
//            var lastMessage = await gmailClient.GetLastMessageAsync();

//            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
//            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

//            Assert.That(gmailMessagesAfterUploadingImage.Count, Is.GreaterThan(gmailMessagesBeforeUploadingImage.Count), "The user did not receive a notification about the uploaded image.");

//            Assert.That(lastMessage.Body, Does.Contain("event_type: upload"), "The notification does not contain the 'event_type: upload'.");
//            Assert.That(lastMessage.Body, Does.Contain($"object_key: {getImageResponse.ObjectKey}"), "The notification does not contain the image object key.");
//            Assert.That(lastMessage.Body, Does.Contain($"object_type: {getImageResponse.ObjectType}"), "The notification does not contain the image object type.");
//            Assert.That(lastMessage.Body, Does.Contain($"last_modified: {DateUtils.FormatDateForUploadedImage(getImageResponse.LastModified)}"), "The notification does not contain the image last modified.");
//            Assert.That(lastMessage.Body, Does.Contain($"object_size: {getImageResponse.ObjectSize}"), "The notification does not contain the image object size.");
//            Assert.That(lastMessage.Body, Does.Contain($"download_link: http://{fqdn}/api/image/file/{getImageResponse.Id}"), "The notification does not contain the download link.");
//        }

//        [Test(Description = "CXQA-SNSSQS-08: The user can download the image using the download link from the notification")]
//        public async Task TestUserCanDownloadImageUsingDownloadLinkFromNotification()
//        {
//            var gmailClient = new GmailClient(EmailToSubscribe);

//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = EC2Helper.GetPublicInstance(instances);
//            string publicIp = publicInstance.PublicIpAddress;
//            string fqdn = publicInstance.PublicDnsName;
//            var restClient = new RestClient(publicIp);

//            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudximage-TopicSNSTopic");
//            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

//            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

//            var gmailMessagesBeforeUploadingImage = await gmailClient.GetMessagesAsync();

//            // Upload the image
//            var postImageResponse = await restClient.PostImageAsync(filePath);

//            // Get image response
//            var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);

//            await Task.Delay(4000);

//            var gmailMessagesAfterUploadingImage = await gmailClient.GetMessagesAsync();

//            Assert.That(gmailMessagesAfterUploadingImage.Count, Is.GreaterThan(gmailMessagesBeforeUploadingImage.Count), "The user did not receive a notification about the uploaded image.");

//            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
//            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

//            var lastMessage = await gmailClient.GetLastMessageAsync();
//            string downloadImageLink = $"http://{fqdn}/api/image/file/{getImageResponse.Id}";
//            Assert.That(lastMessage.Body, Does.Contain($"download_link: {downloadImageLink}"), "The notification does not contain the download link.");

//            // Download the image
//            var downloadPath = Path.Combine("C:\\Users\\origami\\Desktop\\CloudX Associate AWS for Testers\\AWS_QA_Course_Test_Project\\DownloadedImage", imageName);
//            using (var httpClient = new HttpClient())
//            {
//                var response = await httpClient.GetAsync(downloadImageLink);
//                response.EnsureSuccessStatusCode();
//                var imageBytes = await response.Content.ReadAsByteArrayAsync();
//                await File.WriteAllBytesAsync(downloadPath, imageBytes);
//            }

//            // Verify that the image was downloaded
//            Assert.That(File.Exists(downloadPath), Is.True, "The image was not downloaded successfully.");

//            // Clean up the downloaded image
//            File.Delete(downloadPath);
//        }

//        [Test(Description = "CXQA-SNSSQS-09: The user can unsubscribe from the notifications")]
//        public async Task TestUserCanUnsubscribeFromNotifications()
//        {
//            var gmailClient = new GmailClient(EmailToSubscribe);

//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = EC2Helper.GetPublicInstance(instances);
//            string publicIp = publicInstance.PublicIpAddress;
//            var restClient = new RestClient(publicIp);

//            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudximage-TopicSNSTopic");
//            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

//            var subscribersResponseBeforeSubscriptions = await restClient.GetNotificationsAsync();
//            Assert.That(subscribersResponseBeforeSubscriptions.Count, Is.EqualTo(0), "Subscribers list is not empty");

//            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

//            var subscribersResponseAfterSubscriptions = await restClient.GetNotificationsAsync();
//            Assert.That(subscribersResponseAfterSubscriptions.Count, Is.EqualTo(1), $"Subscribers count isn't expected. Expected: 1. Actual: {subscribersResponseAfterSubscriptions.Count}");

//            Assert.That(subscribersResponseAfterSubscriptions.Any(sub => sub.Endpoint == EmailToSubscribe), Is.True, $"User {EmailToSubscribe} isn't subscribed to the topic");

//            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
//            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

//            await Task.Delay(3000);

//            var subscribersResponseAfterUnsubscription = await restClient.GetNotificationsAsync();
//            Assert.That(subscribersResponseBeforeSubscriptions.Count, Is.EqualTo(0), "Subscribers list is not empty");
//        }

//        [Test(Description = "CXQA-SNSSQS-10: The unsubscribed user does not receive further notifications")]
//        public async Task TestUnsubscribedUserDoesNotReceiveFurtherNotifications()
//        {
//            var gmailClient = new GmailClient(EmailToSubscribe);

//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = EC2Helper.GetPublicInstance(instances);
//            string publicIp = publicInstance.PublicIpAddress;
//            var restClient = new RestClient(publicIp);

//            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudximage-TopicSNSTopic");
//            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

//            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

//            await Task.Delay(9000);

//            var gmailMessagesBeforeUnsubscribing = await gmailClient.GetMessagesAsync();

//            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
//            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

//            await Task.Delay(3000);

//            // Upload the image
//            var postImageResponse = await restClient.PostImageAsync(filePath);

//            await Task.Delay(4000);

//            var gmailMessagesAfterUnsubscribing = await gmailClient.GetMessagesAsync();

//            Assert.That(gmailMessagesAfterUnsubscribing.Count, Is.EqualTo(gmailMessagesBeforeUnsubscribing.Count), "The user received a notification after unsubscribing.");
//        }

//        [Test(Description = "CXQA-SNSSQS-11: It is possible to view all existing subscriptions using http://{INSTANCE PUBLIC IP}/api/notification GET API call")]
//        public async Task TestViewAllExistingSubscriptionsUsingGetApiCall()
//        {
//            var gmailClient = new GmailClient(EmailToSubscribe);

//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = EC2Helper.GetPublicInstance(instances);
//            string publicIp = publicInstance.PublicIpAddress;
//            var restClient = new RestClient(publicIp);

//            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, "cloudximage-TopicSNSTopic");
//            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

//            var subscribersResponseBeforeSubscriptions = await restClient.GetNotificationsAsync();
//            Assert.That(subscribersResponseBeforeSubscriptions.Count, Is.EqualTo(0), "Subscribers list is not empty");

//            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

//            var subscribersResponseAfterSubscriptions = await restClient.GetNotificationsAsync();
//            Assert.That(subscribersResponseAfterSubscriptions.Count, Is.EqualTo(1), $"Subscribers count isn't expected. Expected: 1. Actual: {subscribersResponseAfterSubscriptions.Count}");

//            Assert.That(subscribersResponseAfterSubscriptions.Any(sub => sub.Endpoint == EmailToSubscribe), Is.True, $"User {EmailToSubscribe} isn't subscribed to the topic");

//            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
//            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");
//        }
//    }
//}

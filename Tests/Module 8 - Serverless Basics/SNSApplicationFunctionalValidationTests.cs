using AWS_QA_Course_Test_Project.Base;
using AWS_QA_Course_Test_Project.Clients;
using AWS_QA_Course_Test_Project.Utils;

namespace AWS_QA_Course_Test_Project.Tests.Module_8___Serverless_Basics
{
    [TestFixture]
    public class SNSApplicationFunctionalValidationTests : BaseTest
    {
        private const string FilePath = "C:\\Users\\origami\\Desktop\\CloudX Associate AWS for Testers\\AWS_QA_Course_Test_Project\\Images\\image0.jpg";
        private const string ImageName = "image0.jpg";
        private const string TopicNamePrefix = "cloudxserverless-TopicSNSTopic";
        private const string EmailToSubscribe = "novembertanks@gmail.com";

        [Test(Description = "CXQA-SNSSQS-04: The user can subscribe to notifications about application events via a provided email address")]
        public async Task TestUserCanSubscribeToNotificationsViaEmail()
        {
            var gmailClient = new GmailClient(EmailToSubscribe);

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIp = publicInstance.PublicIpAddress;
            var restClient = new RestClient(publicIp);

            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, TopicNamePrefix);
            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

            var actualSubscriptions = await restClient.GetNotificationsAsync();
            Assert.That(actualSubscriptions.Any(sub => sub.Endpoint == EmailToSubscribe), Is.True, $"User {EmailToSubscribe} isn't subscribed to the topic");

            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

            await Task.Delay(5000);

            var actualSubscriptionsAfterUnsubscription = await restClient.GetNotificationsAsync();
            Assert.That(actualSubscriptionsAfterUnsubscription.Any(sub => sub.Endpoint == EmailToSubscribe), Is.False, $"User {EmailToSubscribe} is still subscribed to the topic but shouldn't be");
        }

        [Test(Description = "CXQA-SNSSQS-05: The user has to confirm the subscription after receiving the confirmation email")]
        public async Task TestUserHasToConfirmSubscriptionAfterReceivingConfirmationEmail()
        {
            var gmailClient = new GmailClient(EmailToSubscribe);

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIp = publicInstance.PublicIpAddress;
            var restClient = new RestClient(publicIp);

            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, TopicNamePrefix);
            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

            var actualSubscriptions = await restClient.GetNotificationsAsync();
            Assert.That(actualSubscriptions.Any(sub => sub.Endpoint == EmailToSubscribe), Is.True, $"User {EmailToSubscribe} isn't subscribed to the topic");

            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

            await Task.Delay(5000);

            var actualSubscriptionsAfterUnsubscription = await restClient.GetNotificationsAsync();
            Assert.That(actualSubscriptionsAfterUnsubscription.Any(sub => sub.Endpoint == EmailToSubscribe), Is.False, $"User {EmailToSubscribe} is still subscribed to the topic but shouldn't be");
        }

        [Test(Description = "CXQA-SNSSQS-06: The subscribed user receives notifications about images events (image is uploaded)")]
        public async Task TestSubscribedUserReceivesNotificationsAboutUploadedImage()
        {
            var gmailClient = new GmailClient(EmailToSubscribe);

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIp = publicInstance.PublicIpAddress;
            string fqdn = publicInstance.PublicDnsName;
            var restClient = new RestClient(publicIp);

            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, TopicNamePrefix);
            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

            var gmailMessagesBeforeUploadingImage = await gmailClient.GetMessagesAsync();

            var postImageResponse = await restClient.PostImageAsync(FilePath);
            var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);

            await Task.Delay(5000);

            var gmailMessagesAfterUploadingImage = await gmailClient.GetMessagesAsync();

            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

            Assert.That(gmailMessagesAfterUploadingImage.Count, Is.EqualTo(gmailMessagesBeforeUploadingImage.Count + 1), "The user did not receive a notification about the uploaded image, or received duplication.");

            var lastMessage = await gmailClient.GetLastMessageAsync();

            Assert.Multiple(() => 
            {
                Assert.That(lastMessage.Body, Does.Contain("event_type: upload"), "The notification does not contain the 'event_type: upload'.");
                Assert.That(lastMessage.Body, Does.Contain($"object_key: {getImageResponse.ObjectKey}"), "The notification does not contain the image object key.");
                Assert.That(lastMessage.Body, Does.Contain($"object_type: {getImageResponse.ObjectType}"), "The notification does not contain the image object type.");
                Assert.That(lastMessage.Body, Does.Contain($"last_modified: {DateUtils.ParseNumericDateFormatT(getImageResponse.LastModified)}"), "The notification does not contain the image last modified.");
                Assert.That(lastMessage.Body, Does.Contain($"object_size: {getImageResponse.ObjectSize}"), "The notification does not contain the image object size.");
                Assert.That(lastMessage.Body, Does.Contain($"download_link: http://{fqdn}/api/image/file/{getImageResponse.Id}"), "The notification does not contain the download link.");
            });
        }

        [Test(Description = "CXQA-SNSSQS-06: The subscribed user receives notifications about images events (image is deleted)")]
        public async Task TestSubscribedUserReceivesNotificationsAboutDeletedImage()
        {
            var gmailClient = new GmailClient(EmailToSubscribe);

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIp = publicInstance.PublicIpAddress;
            var restClient = new RestClient(publicIp);

            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, TopicNamePrefix);
            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

            var postImageResponse = await restClient.PostImageAsync(FilePath);
            var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);
            var gmailMessagesBeforeDeletingImage = await gmailClient.GetMessagesAsync();

            await restClient.DeleteImageAsync(postImageResponse.Id);
            await Task.Delay(3000);

            var gmailMessagesAfterDeletingImage = await gmailClient.GetMessagesAsync();
            var lastMessage = await gmailClient.GetLastMessageAsync();

            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

            Assert.That(gmailMessagesAfterDeletingImage.Count, Is.EqualTo(gmailMessagesBeforeDeletingImage.Count + 1), "The user did not receive a notification about the deleted image, or received duplicates");

            Assert.Multiple(() =>
            {
                Assert.That(lastMessage.Body, Does.Contain("event_type: delete"), "The notification does not contain the 'event_type: delete'.");
                Assert.That(lastMessage.Body, Does.Contain($"object_key: {getImageResponse.ObjectKey}"), "The notification does not contain the image object key.");
                Assert.That(lastMessage.Body, Does.Contain($"object_type: {getImageResponse.ObjectType}"), "The notification does not contain the image object type.");
                Assert.That(lastMessage.Body, Does.Contain($"last_modified: {DateUtils.ParseNumericDateFormatT(getImageResponse.LastModified)}"), "The notification does not contain the image last modified.");
                Assert.That(lastMessage.Body, Does.Contain($"object_size: {getImageResponse.ObjectSize}"), "The notification does not contain the image object size.");
                Assert.That(lastMessage.Body, Does.Contain("download_link:"), "The notification does not contain the download link.");
            });
        }

        [Test(Description = "CXQA-SNSSQS-07: The notification contains the correct image metadata information and a download link")]
        public async Task TestNotificationContainsCorrectImageMetadataAndDownloadLink()
        {
            var gmailClient = new GmailClient(EmailToSubscribe);

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIp = publicInstance.PublicIpAddress;
            string fqdn = publicInstance.PublicDnsName;
            var restClient = new RestClient(publicIp);

            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, TopicNamePrefix);
            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

            var gmailMessagesBeforeUploadingImage = await gmailClient.GetMessagesAsync();

            var postImageResponse = await restClient.PostImageAsync(FilePath);
            var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);

            await Task.Delay(5000);

            var gmailMessagesAfterUploadingImage = await gmailClient.GetMessagesAsync();
            var lastMessage = await gmailClient.GetLastMessageAsync();

            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

            Assert.That(gmailMessagesAfterUploadingImage.Count, Is.GreaterThan(gmailMessagesBeforeUploadingImage.Count), "The user did not receive a notification about the uploaded image.");

            Assert.Multiple(() =>
            {
                Assert.That(lastMessage.Body, Does.Contain("event_type: upload"), "The notification does not contain the 'event_type: upload'.");
                Assert.That(lastMessage.Body, Does.Contain($"object_key: {getImageResponse.ObjectKey}"), "The notification does not contain the image object key.");
                Assert.That(lastMessage.Body, Does.Contain($"object_type: {getImageResponse.ObjectType}"), "The notification does not contain the image object type.");
                Assert.That(lastMessage.Body, Does.Contain($"last_modified: {DateUtils.ParseNumericDateFormatT(getImageResponse.LastModified)}"), "The notification does not contain the image last modified.");
                Assert.That(lastMessage.Body, Does.Contain($"object_size: {getImageResponse.ObjectSize}"), "The notification does not contain the image object size.");
                Assert.That(lastMessage.Body, Does.Contain($"download_link: http://{fqdn}/api/image/file/{getImageResponse.Id}"), "The notification does not contain the download link.");
            });
        }

        [Test(Description = "CXQA-SNSSQS-08: The user can download the image using the download link from the notification")]
        public async Task TestUserCanDownloadImageUsingDownloadLinkFromNotification()
        {
            var gmailClient = new GmailClient(EmailToSubscribe);

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIp = publicInstance.PublicIpAddress;
            string fqdn = publicInstance.PublicDnsName;
            var restClient = new RestClient(publicIp);

            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, TopicNamePrefix);
            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

            var gmailMessagesBeforeUploadingImage = await gmailClient.GetMessagesAsync();

            var postImageResponse = await restClient.PostImageAsync(FilePath);
            var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);

            await Task.Delay(5000);

            var gmailMessagesAfterUploadingImage = await gmailClient.GetMessagesAsync();

            Assert.That(gmailMessagesAfterUploadingImage.Count, Is.GreaterThan(gmailMessagesBeforeUploadingImage.Count), "The user did not receive a notification about the uploaded image.");

            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

            var lastMessage = await gmailClient.GetLastMessageAsync();
            string downloadImageLink = $"http://{fqdn}/api/image/file/{getImageResponse.Id}";
            Assert.That(lastMessage.Body, Does.Contain($"download_link: {downloadImageLink}"), "The notification does not contain the download link.");

            var downloadPath = Path.Combine("C:\\Users\\origami\\Desktop\\CloudX Associate AWS for Testers\\AWS_QA_Course_Test_Project\\DownloadedImage", ImageName);
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(downloadImageLink);
                response.EnsureSuccessStatusCode();
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(downloadPath, imageBytes);
            }

            Assert.That(File.Exists(downloadPath), Is.True, "The image was not downloaded successfully.");

            File.Delete(downloadPath);
        }

        [Test(Description = "CXQA-SNSSQS-09: The user can unsubscribe from the notifications")]
        public async Task TestUserCanUnsubscribeFromNotifications()
        {
            var gmailClient = new GmailClient(EmailToSubscribe);

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIp = publicInstance.PublicIpAddress;
            var restClient = new RestClient(publicIp);

            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, TopicNamePrefix);
            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

            var subscribersResponseBeforeSubscriptions = await restClient.GetNotificationsAsync();
            Assert.That(subscribersResponseBeforeSubscriptions.Count, Is.EqualTo(0), "Subscribers list is not empty");

            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

            var subscribersResponseAfterSubscriptions = await restClient.GetNotificationsAsync();
            Assert.That(subscribersResponseAfterSubscriptions.Count, Is.EqualTo(1), $"Subscribers count isn't expected. Expected: 1. Actual: {subscribersResponseAfterSubscriptions.Count}");

            Assert.That(subscribersResponseAfterSubscriptions.Any(sub => sub.Endpoint == EmailToSubscribe), Is.True, $"User {EmailToSubscribe} isn't subscribed to the topic");

            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

            await Task.Delay(5000);

            var subscribersResponseAfterUnsubscription = await restClient.GetNotificationsAsync();
            Assert.That(subscribersResponseBeforeSubscriptions.Count, Is.EqualTo(0), "Subscribers list is not empty");
        }

        [Test(Description = "CXQA-SNSSQS-10: The unsubscribed user does not receive further notifications")]
        public async Task TestUnsubscribedUserDoesNotReceiveFurtherNotifications()
        {
            var gmailClient = new GmailClient(EmailToSubscribe);

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIp = publicInstance.PublicIpAddress;
            var restClient = new RestClient(publicIp);

            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, TopicNamePrefix);
            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

            await Task.Delay(9000);

            var gmailMessagesBeforeUnsubscribing = await gmailClient.GetMessagesAsync();

            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");

            await Task.Delay(3000);

            var postImageResponse = await restClient.PostImageAsync(FilePath);

            await Task.Delay(4000);

            var gmailMessagesAfterUnsubscribing = await gmailClient.GetMessagesAsync();

            Assert.That(gmailMessagesAfterUnsubscribing.Count, Is.EqualTo(gmailMessagesBeforeUnsubscribing.Count), "The user received a notification after unsubscribing.");
        }

        [Test(Description = "CXQA-SNSSQS-11: It is possible to view all existing subscriptions using http://{INSTANCE PUBLIC IP}/api/notification GET API call")]
        public async Task TestViewAllExistingSubscriptionsUsingGetApiCall()
        {
            var gmailClient = new GmailClient(EmailToSubscribe);

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIp = publicInstance.PublicIpAddress;
            var restClient = new RestClient(publicIp);

            var topicArn = await SNSHelper.GetTopicArnAsync(SnsClient, TopicNamePrefix);
            Assert.That(topicArn, Is.Not.Null, "SNS topic with the expected name pattern not found.");

            var subscribersResponseBeforeSubscriptions = await restClient.GetNotificationsAsync();
            Assert.That(subscribersResponseBeforeSubscriptions.Count, Is.EqualTo(0), "Subscribers list is not empty");

            await SNSHelper.SubscribeUserWithConfirmationAsync(SnsClient, publicIp, gmailClient, topicArn);

            var subscribersResponseAfterSubscriptions = await restClient.GetNotificationsAsync();
            Assert.That(subscribersResponseAfterSubscriptions.Count, Is.EqualTo(1), $"Subscribers count isn't expected. Expected: 1. Actual: {subscribersResponseAfterSubscriptions.Count}");

            Assert.That(subscribersResponseAfterSubscriptions.Any(sub => sub.Endpoint == EmailToSubscribe), Is.True, $"User {EmailToSubscribe} isn't subscribed to the topic");

            var unsubscribedResponse = await restClient.DeleteNotificationAsync(EmailToSubscribe);
            Assert.That(unsubscribedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to unsubscribe the user");
        }
    }
}
using Amazon.SimpleNotificationService.Model;
using Amazon.SimpleNotificationService;
using AWS_QA_Course_Test_Project.Clients;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;

namespace AWS_QA_Course_Test_Project.Utils
{
    public static class SNSHelper
    {
        public static async Task<string> GetTopicArnAsync(AmazonSimpleNotificationServiceClient snsClient, string topicNamePattern)
        {
            var listTopicsResponse = await snsClient.ListTopicsAsync(new ListTopicsRequest());
            return listTopicsResponse.Topics.FirstOrDefault(topic => topic.TopicArn.Contains(topicNamePattern))?.TopicArn;
        }

        public static async Task<Dictionary<string, string>> GetTopicAttributesAsync(AmazonSimpleNotificationServiceClient snsClient, string topicArn)
        {
            var getTopicAttributesRequest = new GetTopicAttributesRequest
            {
                TopicArn = topicArn
            };

            var getTopicAttributesResponse = await snsClient.GetTopicAttributesAsync(getTopicAttributesRequest);
            return getTopicAttributesResponse.Attributes;
        }

        public static async Task<Dictionary<string, string>> GetTopicTagsAsync(AmazonSimpleNotificationServiceClient snsClient, string topicArn)
        {
            var listTagsForResourceRequest = new ListTagsForResourceRequest
            {
                ResourceArn = topicArn
            };

            var listTagsForResourceResponse = await snsClient.ListTagsForResourceAsync(listTagsForResourceRequest);
            return listTagsForResourceResponse.Tags.ToDictionary(tag => tag.Key, tag => tag.Value);
        }
        private const string EmailToSubscribe = "novembertanks@gmail.com";


        public static async Task ConfirmUserSubscriptionAsync(AmazonSimpleNotificationServiceClient snsClient, string token, string topicArn)
        {
            var request = new ConfirmSubscriptionRequest
            {
                Token = token,
                TopicArn = topicArn
            };
            await snsClient.ConfirmSubscriptionAsync(request);
        }

        public static async Task SubscribeUserWithConfirmationAsync(AmazonSimpleNotificationServiceClient snsClient, string url, GmailClient gmailClient, string topicArn)
        {
            var messagesBeforeSubscription = await gmailClient.GetMessagesAsync();
            var restClient = new RestClient(url);
            await restClient.PostNotificationAsync(EmailToSubscribe);

            await Task.Delay(9000);

            var messagesAfterSubscription = await gmailClient.GetMessagesAsync();

            if (messagesAfterSubscription.Count == messagesBeforeSubscription.Count)
            {
                throw new InvalidOperationException("No confirmation message received.");
            }

            var newMessage = messagesAfterSubscription.Except(messagesBeforeSubscription).FirstOrDefault();
            var confirmationLink = ExtractConfirmationLinkFromMessage(newMessage);
            var token = ExtractTokenFromLink(confirmationLink);
            await ConfirmUserSubscriptionAsync(snsClient, token, topicArn);
        }

        public static async Task SubscribeUserWithoutConfirmationAsync(string url)
        {
            var restClient = new RestClient(url);
            await restClient.PostNotificationAsync(EmailToSubscribe);
        }

        public static async Task ConfirmUserSubscription(AmazonSimpleNotificationServiceClient snsClient, string topicArn, MailMessage message)
        {
            var confirmationLink = ExtractConfirmationLinkFromMessage(message);
            var token = ExtractTokenFromLink(confirmationLink);
            await ConfirmUserSubscriptionAsync(snsClient, token, topicArn);
        }

        public static string ExtractConfirmationLinkFromMessage(MailMessage message)
        {
            var body = message.Body;
            var linkMatch = Regex.Match(body, @"https://sns\.[^""]+");
            if (linkMatch.Success)
            {
                return linkMatch.Value;
            }
            throw new InvalidOperationException("Confirmation link not found in the message body.");
        }

        public static string ExtractTokenFromLink(string link)
        {
            var uri = new Uri(link);
            var query = HttpUtility.ParseQueryString(uri.Query);
            return query["Token"];
        }
    }
}

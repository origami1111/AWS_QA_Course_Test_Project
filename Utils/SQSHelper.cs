using Amazon.SQS.Model;
using Amazon.SQS;

namespace AWS_QA_Course_Test_Project.Utils
{
    public static class SQSHelper
    {
        public static async Task<string> GetQueueUrlAsync(AmazonSQSClient sqsClient, string queueNamePattern)
        {
            var listQueuesResponse = await sqsClient.ListQueuesAsync(new ListQueuesRequest());
            return listQueuesResponse.QueueUrls.FirstOrDefault(url => url.Contains(queueNamePattern));
        }

        public static async Task<Dictionary<string, string>> GetQueueAttributesAsync(AmazonSQSClient sqsClient, string queueUrl, List<string> attributeNames)
        {
            var getQueueAttributesRequest = new GetQueueAttributesRequest
            {
                QueueUrl = queueUrl,
                AttributeNames = attributeNames
            };

            var getQueueAttributesResponse = await sqsClient.GetQueueAttributesAsync(getQueueAttributesRequest);
            return getQueueAttributesResponse.Attributes;
        }

        public static async Task<Dictionary<string, string>> GetQueueTagsAsync(AmazonSQSClient sqsClient, string queueUrl)
        {
            var listQueueTagsRequest = new ListQueueTagsRequest
            {
                QueueUrl = queueUrl
            };

            var listQueueTagsResponse = await sqsClient.ListQueueTagsAsync(listQueueTagsRequest);
            return listQueueTagsResponse.Tags;
        }
    }
}

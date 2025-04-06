using Amazon.CloudWatchLogs.Model;
using Amazon.CloudWatchLogs;

namespace AWS_QA_Course_Test_Project.Utils
{
    public static class CloudWatchLogsHelper
    {
        public static async Task<bool> IsLogGroupExistsAsync(AmazonCloudWatchLogsClient cloudWatchLogsClient, string logGroupName)
        {
            var request = new DescribeLogGroupsRequest
            {
                LogGroupNamePrefix = logGroupName
            };
            var response = await cloudWatchLogsClient.DescribeLogGroupsAsync(request);

            return response.LogGroups.Any(logGroup => logGroup.LogGroupName == logGroupName);
        }

        public static async Task<bool> IsLogStreamExistsAsync(AmazonCloudWatchLogsClient cloudWatchLogsClient, string logGroupName)
        {
            var request = new DescribeLogStreamsRequest
            {
                LogGroupName = logGroupName
            };
            var response = await cloudWatchLogsClient.DescribeLogStreamsAsync(request);
            return response.LogStreams.Any();
        }

        public static async Task<bool> IsLogStreamExistsAsync(AmazonCloudWatchLogsClient cloudWatchLogsClient, string logGroupName, string logStreamName)
        {
            var request = new DescribeLogStreamsRequest
            {
                LogGroupName = logGroupName,
                LogStreamNamePrefix = logStreamName
            };
            var response = await cloudWatchLogsClient.DescribeLogStreamsAsync(request);
            return response.LogStreams.Any(ls => ls.LogStreamName == logStreamName);
        }

        public static async Task<List<OutputLogEvent>> GetLogEventsAsync(AmazonCloudWatchLogsClient cloudWatchLogsClient, string logGroupName, string logStreamName)
        {
            var request = new GetLogEventsRequest
            {
                LogGroupName = logGroupName,
                LogStreamName = logStreamName
            };
            var response = await cloudWatchLogsClient.GetLogEventsAsync(request);
            return response.Events;
        }

        public static async Task<List<string>> GetAllLogEventMessagesAsync(AmazonCloudWatchLogsClient cloudWatchLogsClient, string logGroupName, string logStreamName)
        {
            var request = new GetLogEventsRequest
            {
                LogGroupName = logGroupName,
                LogStreamName = logStreamName
            };
            var response = await cloudWatchLogsClient.GetLogEventsAsync(request);
            return response.Events.Select(m => m.Message).ToList();
        }

        public static async Task<List<string>> GetAllLogEventMessagesAsync(AmazonCloudWatchLogsClient cloudWatchLogsClient, string logGroupName)
        {
            var describeLogStreamsRequest = new DescribeLogStreamsRequest
            {
                LogGroupName = logGroupName
            };
            var describeLogStreamsResponse = await cloudWatchLogsClient.DescribeLogStreamsAsync(describeLogStreamsRequest);
            var logStreamNames = describeLogStreamsResponse.LogStreams.Select(ls => ls.LogStreamName).ToList();

            var messages = new List<string>();

            foreach (var logStreamName in logStreamNames)
            {
                var getLogEventsRequest = new GetLogEventsRequest
                {
                    LogGroupName = logGroupName,
                    LogStreamName = logStreamName
                };
                var getLogEventsResponse = await cloudWatchLogsClient.GetLogEventsAsync(getLogEventsRequest);
                messages.AddRange(getLogEventsResponse.Events.Select(m => m.Message));
            }

            return messages;
        }
    }
}

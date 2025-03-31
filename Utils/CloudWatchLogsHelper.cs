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
    }
}

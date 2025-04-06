using Amazon.CloudTrail.Model;
using Amazon.CloudTrail;

namespace AWS_QA_Course_Test_Project.Utils
{
    public static class CloudTrailHelper
    {
        public static async Task<string> GetTrailNameAsync(AmazonCloudTrailClient cloudTrailClient, string trailNamePrefix)
        {
            var request = new DescribeTrailsRequest();
            var response = await cloudTrailClient.DescribeTrailsAsync(request);
            var trail = response.TrailList.FirstOrDefault(t => t.Name.StartsWith(trailNamePrefix));
            return trail?.Name;
        }

        public static async Task<bool> IsMultiRegionTrailAsync(AmazonCloudTrailClient cloudTrailClient, string trailNamePrefix)
        {
            var request = new DescribeTrailsRequest();
            var response = await cloudTrailClient.DescribeTrailsAsync(request);
            var trail = response.TrailList.FirstOrDefault(t => t.Name.StartsWith(trailNamePrefix));
            return trail?.IsMultiRegionTrail ?? false;
        }

        public static async Task<bool> IsLogFileValidationEnabledAsync(AmazonCloudTrailClient cloudTrailClient, string trailNamePrefix)
        {
            var request = new DescribeTrailsRequest();
            var response = await cloudTrailClient.DescribeTrailsAsync(request);
            var trail = response.TrailList.FirstOrDefault(t => t.Name.StartsWith(trailNamePrefix));
            return trail?.LogFileValidationEnabled ?? false;
        }

        public static async Task<bool> IsSSEKMSEncryptionEnabledAsync(AmazonCloudTrailClient cloudTrailClient, string trailNamePrefix)
        {
            var request = new DescribeTrailsRequest();
            var response = await cloudTrailClient.DescribeTrailsAsync(request);
            var trail = response.TrailList.FirstOrDefault(t => t.Name.StartsWith(trailNamePrefix));
            return trail?.KmsKeyId != null;
        }

        public static async Task<Dictionary<string, string>> GetTrailTagsAsync(AmazonCloudTrailClient cloudTrailClient, string region, string accountId, string trailName)
        {
            string trailArn = $"arn:aws:cloudtrail:{region}:{accountId}:trail/{trailName}";
            var request = new ListTagsRequest
            {
                ResourceIdList = new List<string> { trailArn }
            };
            var response = await cloudTrailClient.ListTagsAsync(request);
            var tags = response.ResourceTagList.FirstOrDefault()?.TagsList;
            return tags?.ToDictionary(tag => tag.Key, tag => tag.Value) ?? new Dictionary<string, string>();
        }

        public static async Task<bool> IsLoggingAsync(AmazonCloudTrailClient cloudTrailClient, string trailName)
        {
            var request = new GetTrailStatusRequest
            {
                Name = trailName
            };
            var response = await cloudTrailClient.GetTrailStatusAsync(request);
            return response.IsLogging;
        }
    }
}

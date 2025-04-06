using Amazon;
using Amazon.CloudWatchLogs;
using AWS_QA_Course_Test_Project.Base;
using AWS_QA_Course_Test_Project.Clients;
using AWS_QA_Course_Test_Project.Utils;

namespace AWS_QA_Course_Test_Project.Tests.Module_9___Monitoring_and_logging
{
    [TestFixture]
    public class DeploymentValidationTests : BaseTest
    {
        private const string FunctionNamePrefix = "cloudxserverless-EventHandlerLambda";
        private const string TrailNamePrefix = "cloudxserverless-Trail";

        private const string FilePath = "C:\\Users\\origami\\Desktop\\CloudX Associate AWS for Testers\\AWS_QA_Course_Test_Project\\Images\\image0.jpg";

        [Test(Description = "CXQA-MON-01: The application EC2 instance has CloudWatch integration")]
        public async Task TestApplicationEC2InstanceHasCloudWatchIntegration()
        {
            string logGroupName = "/var/log/cloudxserverless-app";
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(CloudWatchLogsClient, logGroupName);
            Assert.That(logGroupExists, Is.True, $"LogGroup {logGroupName} does not exist in CloudWatch Logs");

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string logStreamName = publicInstance.InstanceId;

            bool logStreamExists = await CloudWatchLogsHelper.IsLogStreamExistsAsync(CloudWatchLogsClient, logGroupName, logStreamName);
            Assert.That(logStreamExists, Is.True, $"LogStream {logStreamName} does not exist in LogGroup {logGroupName}");
        }

        [Test(Description = "CXQA-MON-02: CloudInit logs should be collected in CloudWatch logs. LogGroup /var/log/cloud-init: for the cloud-init logs of the EC2 instance (LogStreams by instance ID), in the us-east-1 region")]
        public async Task TestCloudInitLogsCollectedInCloudWatchLogs()
        {
            var cloudWatchLogsClient = new AmazonCloudWatchLogsClient(RegionEndpoint.USEast1);
            string logGroupName = "/var/log/cloud-init";
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(cloudWatchLogsClient, logGroupName);
            Assert.That(logGroupExists, Is.True, $"LogGroup {logGroupName} does not exist in CloudWatch Logs in the us-east-1 region");

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string logStreamName = publicInstance.InstanceId;

            bool logStreamExists = await CloudWatchLogsHelper.IsLogStreamExistsAsync(cloudWatchLogsClient, logGroupName, logStreamName);
            Assert.That(logStreamExists, Is.True, $"LogStream {logStreamName} does not exist in LogGroup {logGroupName}");
        }

        [Test(Description = "CXQA-MON-03: The application messages should be collected in CloudWatch logs. LogGroup /var/log/cloudxserverless-app: for the application deployed on the EC2 instance (LogStreams by instance ID), in the same region as the stack0")]
        public async Task TestApplicationMessagesCollectedInCloudWatchLogs()
        {
            string logGroupName = "/var/log/cloudxserverless-app";
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(CloudWatchLogsClient, logGroupName);
            Assert.That(logGroupExists, Is.True, $"LogGroup {logGroupName} does not exist in CloudWatch Logs");

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string logStreamName = publicInstance.InstanceId;

            bool logStreamExists = await CloudWatchLogsHelper.IsLogStreamExistsAsync(CloudWatchLogsClient, logGroupName, logStreamName);
            Assert.That(logStreamExists, Is.True, $"LogStream {logStreamName} does not exist in LogGroup {logGroupName}");
        }

        [Test(Description = "CXQA-MON-04: The event handler logs should be collected in CloudWatch logs. LogGroup /aws/lambda/cloudxserverless-EventHandlerLambda{unique id}: for the event handler lambda function (LogStreams as default), in the same region as the stack")]
        public async Task TestEventHandlerLogsCollectedInCloudWatchLogs()
        {
            string functionName = await LambdaHelper.GetLambdaFunctionNameAsync(LambdaClient, FunctionNamePrefix);
            string logGroupName = $"/aws/lambda/{functionName}";
            bool logGroupExists = await CloudWatchLogsHelper.IsLogGroupExistsAsync(CloudWatchLogsClient, logGroupName);
            Assert.That(logGroupExists, Is.True, $"LogGroup {logGroupName} does not exist in CloudWatch Logs");

            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIpAddress = publicInstance.PublicIpAddress;
            var restClient = new RestClient(publicIpAddress);
            await restClient.PostImageAsync(FilePath);

            await Task.Delay(10000);

            bool logStreamExists = await CloudWatchLogsHelper.IsLogStreamExistsAsync(CloudWatchLogsClient, logGroupName);
            Assert.That(logStreamExists, Is.True, $"LogStream does not exist in LogGroup {logGroupName}");
        }

        // CloudTrail trail requirements:
        [Test(Description = "CXQA-MON-05: CloudTrail is enabled for Serverless stack and collects logs about AWS services access")]
        public async Task TestCloudTrailEnabledForServerlessStack()
        {
            string trailName = await CloudTrailHelper.GetTrailNameAsync(CloudTrailClient, TrailNamePrefix);
            Assert.That(trailName, Is.Not.Null, "CloudTrail with the specified prefix does not exist");

            var isLogging = await CloudTrailHelper.IsLoggingAsync(CloudTrailClient, trailName);
            Assert.That(isLogging, Is.True, "CloudTrail is not logging");
        }

        [Test(Description = "CXQA-MON-05: CloudTrail trail requirements: Name: cloudxserverless-Trail{unique id}")]
        public async Task TestCloudTrailName()
        {
            string trailName = await CloudTrailHelper.GetTrailNameAsync(CloudTrailClient, TrailNamePrefix);
            Assert.That(trailName, Is.Not.Null,"CloudTrail with the specified prefix does not exist");
            Assert.That(trailName.StartsWith(TrailNamePrefix), Is.True, "CloudTrail name does not match the expected prefix");
        }

        [Test(Description = "CXQA-MON-05: CloudTrail trail requirements: Multi-region: yes")]
        public async Task TestCloudTrailMultiRegion()
        {
            bool isMultiRegion = await CloudTrailHelper.IsMultiRegionTrailAsync(CloudTrailClient, TrailNamePrefix);
            Assert.That(isMultiRegion, Is.True, "CloudTrail is not configured as multi-region");
        }

        [Test(Description = "CXQA-MON-05: CloudTrail trail requirements: Log file validation: enabled")]
        public async Task TestCloudTrailLogFileValidation()
        {
            bool isLogFileValidationEnabled = await CloudTrailHelper.IsLogFileValidationEnabledAsync(CloudTrailClient, TrailNamePrefix);
            Assert.That(isLogFileValidationEnabled, Is.True, "CloudTrail log file validation is not enabled");
        }

        [Test(Description = "CXQA-MON-05: CloudTrail trail requirements: SSE-KMS encryption: not enabled")]
        public async Task TestCloudTrailSSEKMSEncryption()
        {
            bool isSSEKMSEncryptionEnabled = await CloudTrailHelper.IsSSEKMSEncryptionEnabledAsync(CloudTrailClient, TrailNamePrefix);
            Assert.That(isSSEKMSEncryptionEnabled, Is.False, "CloudTrail SSE-KMS encryption is enabled");
        }

        [Test(Description = "CXQA-MON-05: CloudTrail trail requirements: Tags: cloudx: qa")]
        public async Task TestCloudTrailTags()
        {
            string trailName = await CloudTrailHelper.GetTrailNameAsync(CloudTrailClient, TrailNamePrefix);
            Assert.That(trailName, Is.Not.Null, "CloudTrail with the specified prefix does not exist");

            var tags = await CloudTrailHelper.GetTrailTagsAsync(CloudTrailClient, Region, AccountId, trailName);
            Assert.That(tags.ContainsKey("cloudx"), Is.True, "Tag 'cloudx' is missing");
            Assert.That(tags["cloudx"], Is.EqualTo("qa"), "Tag 'cloudx' does not have the expected value 'qa'");
        }
    }
}

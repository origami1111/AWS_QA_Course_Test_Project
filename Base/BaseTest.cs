using Amazon.IdentityManagement;
using Amazon.EC2;
using Microsoft.Extensions.Configuration;
using Amazon.S3;
using Amazon.RDS;
using Amazon.DynamoDBv2;
using Amazon.Lambda;
using Amazon.CloudWatchLogs;

namespace AWS_QA_Course_Test_Project.Base
{
    [TestFixture]
    public class BaseTest
    {
        protected AmazonIdentityManagementServiceClient IamClient;
        protected AmazonEC2Client Ec2Client;
        protected AmazonS3Client S3Client;
        protected AmazonRDSClient RdsClient;
        protected AmazonDynamoDBClient DynamoDbClient;
        protected AmazonLambdaClient LambdaClient;
        protected AmazonCloudWatchLogsClient CloudWatchLogsClient;
        protected string Region;

        [SetUp]
        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("Config/appsettings.json")
                .Build();

            Region = config["AWS:Region"];
            IamClient = new AmazonIdentityManagementServiceClient();
            Ec2Client = new AmazonEC2Client();
            S3Client = new AmazonS3Client();
            RdsClient = new AmazonRDSClient();
            DynamoDbClient = new AmazonDynamoDBClient();
            LambdaClient = new AmazonLambdaClient();
            CloudWatchLogsClient = new AmazonCloudWatchLogsClient();
        }

        [TearDown]
        public void TearDown()
        {
            IamClient.Dispose();
            Ec2Client.Dispose();
            S3Client.Dispose();
            RdsClient.Dispose();
            DynamoDbClient.Dispose();
            LambdaClient.Dispose();
            CloudWatchLogsClient.Dispose();
        }
    }
}

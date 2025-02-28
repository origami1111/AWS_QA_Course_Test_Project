using Amazon.IdentityManagement;
using Amazon.EC2;
using Microsoft.Extensions.Configuration;

namespace AWS_QA_Course_Test_Project.Base
{
    [TestFixture]
    public class BaseTest
    {
        protected AmazonIdentityManagementServiceClient IamClient;
        protected AmazonEC2Client Ec2Client;
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
        }

        [TearDown]
        public void TearDown()
        {
            IamClient.Dispose();
            Ec2Client.Dispose();
        }
    }
}

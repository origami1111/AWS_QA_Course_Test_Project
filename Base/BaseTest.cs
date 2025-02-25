using Amazon.IdentityManagement;
using Microsoft.Extensions.Configuration;

namespace AWS_QA_Course_Test_Project.Base
{
    [TestFixture]
    public class BaseTest
    {
        protected AmazonIdentityManagementServiceClient IamClient;

        [SetUp]
        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("Config/appsettings.json")
                .Build();

            string region = config["AWS:Region"];
            IamClient = new AmazonIdentityManagementServiceClient();
        }

        [TearDown]
        public void TearDown()
        {
            IamClient.Dispose();
        }
    }
}

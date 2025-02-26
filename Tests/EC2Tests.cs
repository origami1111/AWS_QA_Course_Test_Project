using Amazon.EC2.Model;
using AWS_QA_Course_Test_Project.Base;
using AWS_QA_Course_Test_Project.Utils;
using Newtonsoft.Json;

namespace AWS_QA_Course_Test_Project.Tests
{
    [TestFixture]
    public class EC2Tests : BaseTest
    {
        // Deployment Validation
        [Test(Description = "CXQA-EC2-01: Check that public application instance deployed")]
        public async Task TestHasPublicInstance()
        {
            string expectedInstance = "public";
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);

            bool instanceFound = false;

            instanceFound = EC2Helper.HasPublicInstance(instances);

            Assert.That(instanceFound, Is.True, $"Expected {expectedInstance} instance not found.");
        }

        [Test(Description = "CXQA-EC2-01: Check that private application instance deployed")]
        public async Task TestPrivateInstanceInstances()
        {
            string expectedInstance = "private";
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);

            bool instanceFound = false;

            instanceFound = EC2Helper.HasPrivateInstance(instances);

            Assert.That(instanceFound, Is.True, $"Expected {expectedInstance} instance not found.");
        }

        string expectedInstanceType = "t2.micro";
        Dictionary<string, string> expectedInstanceTags = new Dictionary<string, string>
        {
            { "cloudx", "qa" }
        };
        int expectedRootBlockDeviceSize = 8; // GiB
        string expectedInstanceOSDescription = "Amazon Linux 2";

        [Test(Description = "CXQA-EC2-02: Check that EC2 public instance have configuration and have public IP assigned")]
        public async Task TestPublicInstanceConfiguration()
        {
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);

            AssertPublicInstance(publicInstance, expectedInstanceType, expectedInstanceTags, expectedRootBlockDeviceSize, expectedInstanceOSDescription);
        }

        [Test(Description = "CXQA-EC2-02: Check that EC2 private instance have configuration and do not have public IP assigned")]
        public async Task TestPrivateInstanceConfiguration()
        {
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var privateInstance = EC2Helper.GetPrivateInstance(instances);

            AssertPrivateInstance(privateInstance, expectedInstanceType, expectedInstanceTags, expectedRootBlockDeviceSize, expectedInstanceOSDescription);
        }

        // This should check, if the public instance should be accessible from the internet by SSH (port 22) and HTTP (port 80) only
        // and if the private instance should be accessible only from the public instance by SSH and HTTP protocols only
        // and if Both private and public instances should have access to the internet

        [Test(Description = "CXQA-EC2-03: Check the security groups configuration")]
        public async Task TestSecurityGroupsConfiguration()
        {
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            var privateInstance = EC2Helper.GetPrivateInstance(instances);

            AssertSecurityGroupConfiguration(publicInstance, privateInstance);
        }

        // Application functional validation
        [Test(Description = "CXQA-EC2-04: Check that application API endpoint respond with the correct instance information from EC2 metadata")]
        public async Task TestApplicationAPI()
        {
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);

            string expectedAvailabilityZone = publicInstance.Placement.AvailabilityZone;
            string expectedPrivateIpv4 = publicInstance.PrivateIpAddress;
            string expectedRegion = Region;
            string ipv4 = publicInstance.PublicIpAddress;

            string apiUrl = $"http://{ipv4}/";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseBody);

                Assert.That(apiResponse.AvailabilityZone, Is.EqualTo(expectedAvailabilityZone), "Availability zone does not match.");
                Assert.That(apiResponse.PrivateIpv4, Is.EqualTo(expectedPrivateIpv4), "Private IPv4 does not match.");
                Assert.That(apiResponse.Region, Is.EqualTo(expectedRegion), "Region does not match.");
            }
        }

        private async Task AssertSecurityGroupConfiguration(Instance publicInstance, Instance privateInstance)
        {
            var publicSecurityGroup = await EC2Helper.DescribeSecurityGroupAsync(Ec2Client, publicInstance.SecurityGroups.First().GroupId);
            var privateSecurityGroup = await EC2Helper.DescribeSecurityGroupAsync(Ec2Client, privateInstance.SecurityGroups.First().GroupId);

            // Check public instance security group inbound rules
            Assert.That(publicSecurityGroup.IpPermissions.Any(p => p.FromPort == 22 && p.ToPort == 22 && p.IpProtocol == "tcp"), Is.True, "Public instance should be accessible by SSH (port 22) from the internet.");
            Assert.That(publicSecurityGroup.IpPermissions.Any(p => p.FromPort == 80 && p.ToPort == 80 && p.IpProtocol == "tcp"), Is.True, "Public instance should be accessible by HTTP (port 80) from the internet.");

            // Check private instance security group inbound rules
            Assert.That(privateSecurityGroup.IpPermissions.Any(p => p.FromPort == 22 && p.ToPort == 22 && p.IpProtocol == "tcp" && p.UserIdGroupPairs.Any(g => g.GroupId == publicSecurityGroup.GroupId)), Is.True, "Private instance should be accessible by SSH (port 22) from the public instance.");
            Assert.That(privateSecurityGroup.IpPermissions.Any(p => p.FromPort == 80 && p.ToPort == 80 && p.IpProtocol == "tcp" && p.UserIdGroupPairs.Any(g => g.GroupId == publicSecurityGroup.GroupId)), Is.True, "Private instance should be accessible by HTTP (port 80) from the public instance.");

            // Check public instance security group outbound rules
            Assert.That(publicSecurityGroup.IpPermissionsEgress.Any(p => p.IpProtocol == "-1"), Is.True, "Public instance should have access to the internet.");

            // Check private instance security group outbound rules
            Assert.That(privateSecurityGroup.IpPermissionsEgress.Any(p => p.IpProtocol == "-1"), Is.True, "Private instance should have access to the internet.");
        }

        private void AssertPrivateInstance(Instance privateInstance, string expectedInstanceType, Dictionary<string, string> expectedInstanceTags, int expectedRootBlockDeviceSize, string expectedInstanceOSDescription)
        {
            Assert.That(privateInstance, Is.Not.Null, "Private instance not found.");
            Assert.That(privateInstance.InstanceType.ToString(), Is.EqualTo(expectedInstanceType), "Instance type does not match.");
            AssertTags(privateInstance, expectedInstanceTags);
            AssertRootBlockDeviceSize(privateInstance, expectedRootBlockDeviceSize);
            AssertInstanceOS(privateInstance, expectedInstanceOSDescription);
            Assert.That(privateInstance.PublicIpAddress, Is.Null, "Private instance should not have a public IP assigned.");
        }

        private void AssertPublicInstance(Instance publicInstance, string expectedInstanceType, Dictionary<string, string> expectedInstanceTags, int expectedRootBlockDeviceSize, string expectedInstanceOSDescription)
        {
            Assert.That(publicInstance, Is.Not.Null, "Public instance not found.");
            Assert.That(publicInstance.InstanceType.ToString(), Is.EqualTo(expectedInstanceType), "Instance type does not match.");
            AssertTags(publicInstance, expectedInstanceTags);
            AssertRootBlockDeviceSize(publicInstance, expectedRootBlockDeviceSize);
            AssertInstanceOS(publicInstance, expectedInstanceOSDescription);
            Assert.That(publicInstance.PublicIpAddress, Is.Not.Null, "Public instance does not have a public IP assigned.");
        }

        private void AssertTags(Instance publicInstance, Dictionary<string, string> expectedInstanceTags)
        {
            Assert.That(expectedInstanceTags.All(tag =>
                publicInstance.Tags.Any(instanceTag =>
                instanceTag.Key == tag.Key && instanceTag.Value == tag.Value)),
            Is.True, "Instance tag does not match.");
        }

        private async Task AssertRootBlockDeviceSize(Instance publicInstance, int expectedRootBlockDeviceSize)
        {
            var volumeId = publicInstance.BlockDeviceMappings.First().Ebs.VolumeId;
            var volume = await EC2Helper.DescribeVolumeAsync(Ec2Client, volumeId);
            Assert.That(volume.Size, Is.EqualTo(expectedRootBlockDeviceSize), "Root block device size does not match.");
        }

        private async Task AssertInstanceOS(Instance publicInstance, string expectedInstanceOSDescription)
        {
            var image = await EC2Helper.DescribeImageAsync(Ec2Client, publicInstance.ImageId);
            Assert.That(image.Description.Contains(expectedInstanceOSDescription), Is.True, "Instance OS does not match.");
        }
    }
}

//using Amazon.EC2;
//using AWS_QA_Course_Test_Project.Base;
//using AWS_QA_Course_Test_Project.Utils;

//namespace AWS_QA_Course_Test_Project.Tests
//{
//    [TestFixture]
//    public class VPCTests : BaseTest
//    {
//        // Deployment Validation: VPC configuration
//        [Test(Description = "CXQA-VPC-01: The application should be deployed in non-default VPC which has 2 subnets: public and private")]
//        public async Task TestDeployedApplicationHasPublicAndPrivateSubnetsInNonDefaultVPC()
//        {
//            var vpcs = await VPCHelper.DescribeVpcsAsync(Ec2Client);
//            var nonDefaultVpc = vpcs.FirstOrDefault(vpc => !vpc.IsDefault);

//            Assert.That(nonDefaultVpc, Is.Not.Null, "Non-default VPC not found.");

//            var subnets = await VPCHelper.DescribeSubnetsAsync(Ec2Client, nonDefaultVpc.VpcId);
//            var publicSubnets = subnets.Where(subnet => subnet.MapPublicIpOnLaunch).ToList();
//            var privateSubnets = subnets.Where(subnet => !subnet.MapPublicIpOnLaunch).ToList();

//            Assert.That(publicSubnets, Is.Not.Empty, "No public subnets found in the non-default VPC.");
//            Assert.That(privateSubnets, Is.Not.Empty, "No private subnets found in the non-default VPC.");
//        }

//        [Test(Description = "CXQA-VPC-01: VPC CIDR Block: 10.0.0.0/16")]
//        public async Task TestVPCCIDRBlock()
//        {
//            var vpcs = await VPCHelper.DescribeVpcsAsync(Ec2Client);
//            var nonDefaultVpc = vpcs.FirstOrDefault(vpc => !vpc.IsDefault);

//            Assert.That(nonDefaultVpc, Is.Not.Null, "Non-default VPC not found.");
//            Assert.That(nonDefaultVpc.CidrBlock, Is.EqualTo("10.0.0.0/16"), "VPC CIDR block does not match the expected value.");
//        }

//        Dictionary<string, string> expectedVPCTags = new Dictionary<string, string>
//        {
//            { "cloudx", "qa" }
//        };

//        [Test(Description = "CXQA-VPC-01: VPC tags: cloudx: qa")]
//        public async Task TestVPCTags()
//        {
//            var vpcs = await VPCHelper.DescribeVpcsAsync(Ec2Client);
//            var nonDefaultVpc = vpcs.FirstOrDefault(vpc => !vpc.IsDefault);

//            Assert.That(nonDefaultVpc, Is.Not.Null, "Non-default VPC not found.");

//            Assert.That(expectedVPCTags.All(tag =>
//                nonDefaultVpc.Tags.Any(vpcTag =>
//                vpcTag.Key == tag.Key && vpcTag.Value == tag.Value)),
//            Is.True, "VPC tag does not match.");
//        }

//        // Subnets and routing configuration
//        [Test(Description = "CXQA-VPC-02: The public instance should be accessible from the public internet")]
//        public async Task TestPublicInstanceAccessibleFromInternet()
//        {
//            var vpcs = await VPCHelper.DescribeVpcsAsync(Ec2Client);
//            var nonDefaultVpc = vpcs.FirstOrDefault(vpc => !vpc.IsDefault);

//            Assert.That(nonDefaultVpc, Is.Not.Null, "Non-default VPC not found.");

//            var subnets = await VPCHelper.DescribeSubnetsAsync(Ec2Client, nonDefaultVpc.VpcId);
//            var publicSubnet = subnets.FirstOrDefault(subnet => subnet.MapPublicIpOnLaunch);

//            Assert.That(publicSubnet, Is.Not.Null, "Public subnet not found in the non-default VPC.");

//            var routeTables = await VPCHelper.DescribeRouteTablesAsync(Ec2Client, publicSubnet.SubnetId);
//            var routeTable = routeTables.FirstOrDefault();

//            Assert.That(routeTable, Is.Not.Null, "Route table not found for the public subnet.");
//            Assert.That(routeTable.Routes.Any(route => route.DestinationCidrBlock == "0.0.0.0/0" && route.GatewayId.StartsWith("igw-")), Is.True, "Route table does not have a route for 0.0.0.0/0 with the target internet gateway.");
//        }

//        [Test(Description = "CXQA-VPC-02: The private instance should not be accessible from the public internet")]
//        public async Task TestPrivateInstanceNotAccessibleFromInternet()
//        {
//            var vpcs = await VPCHelper.DescribeVpcsAsync(Ec2Client);
//            var nonDefaultVpc = vpcs.FirstOrDefault(vpc => !vpc.IsDefault);

//            Assert.That(nonDefaultVpc, Is.Not.Null, "Non-default VPC not found.");

//            var subnets = await VPCHelper.DescribeSubnetsAsync(Ec2Client, nonDefaultVpc.VpcId);
//            var privateSubnet = subnets.FirstOrDefault(subnet => !subnet.MapPublicIpOnLaunch);

//            Assert.That(privateSubnet, Is.Not.Null, "Private subnet not found in the non-default VPC.");

//            var routeTables = await VPCHelper.DescribeRouteTablesAsync(Ec2Client, privateSubnet.SubnetId);
//            var routeTable = routeTables.FirstOrDefault();

//            Assert.That(routeTable, Is.Not.Null, "Route table not found for the private subnet.");
//            Assert.That(routeTable.Routes.All(route => route.DestinationCidrBlock != "0.0.0.0/0" || route.GatewayId == null || !route.GatewayId.StartsWith("igw-")), Is.True, "Route table should not have a route for 0.0.0.0/0 with the target internet gateway.");
//        }

//        [Test(Description = "CXQA-VPC-02: The public instance should have access to the private instance")]
//        public async Task TestPublicInstanceHasAccessToPrivateInstance()
//        {
//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = instances.FirstOrDefault(i => i.PublicIpAddress != null);
//            var privateInstance = instances.FirstOrDefault(i => i.PublicIpAddress == null);

//            Assert.That(publicInstance, Is.Not.Null, "Public instance not found.");
//            Assert.That(privateInstance, Is.Not.Null, "Private instance not found.");

//            var subnets = await VPCHelper.DescribeSubnetsAsync(Ec2Client, privateInstance.VpcId);
//            var privateSubnet = subnets.FirstOrDefault(subnet => !subnet.MapPublicIpOnLaunch);

//            Assert.That(privateSubnet, Is.Not.Null, "Private subnet not found in the non-default VPC.");

//            var routeTables = await VPCHelper.DescribeRouteTablesAsync(Ec2Client, privateSubnet.SubnetId);
//            var routeTable = routeTables.FirstOrDefault();

//            Assert.That(routeTable, Is.Not.Null, "Route table not found for the private subnet.");
//            Assert.That(routeTable.Routes.Any(route => route.DestinationCidrBlock == "10.0.0.0/16"), Is.True, "Route table does not have a route for 10.0.0.0/16 with the target local.");
//        }

//        [Test(Description = "CXQA-VPC-02: Both the public and the private instances should have access to the public internet")]
//        public async Task TestPublicAndPrivateInstancesHaveAccessToInternet()
//        {
//            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
//            var publicInstance = instances.FirstOrDefault(i => i.PublicIpAddress != null);
//            var privateInstance = instances.FirstOrDefault(i => i.PublicIpAddress == null);

//            Assert.That(publicInstance, Is.Not.Null, "Public instance not found.");
//            Assert.That(privateInstance, Is.Not.Null, "Private instance not found.");

//            var subnets = await VPCHelper.DescribeSubnetsAsync(Ec2Client, publicInstance.VpcId);
//            var publicSubnet = subnets.FirstOrDefault(subnet => subnet.MapPublicIpOnLaunch);
//            var privateSubnet = subnets.FirstOrDefault(subnet => !subnet.MapPublicIpOnLaunch);

//            Assert.That(publicSubnet, Is.Not.Null, "Public subnet not found in the non-default VPC.");
//            Assert.That(privateSubnet, Is.Not.Null, "Private subnet not found in the non-default VPC.");

//            var publicNetworkAcl = await VPCHelper.DescribeNetworkAclsAsync(Ec2Client, publicSubnet.SubnetId);
//            var privateNetworkAcl = await VPCHelper.DescribeNetworkAclsAsync(Ec2Client, privateSubnet.SubnetId);

//            Assert.That(publicNetworkAcl, Is.Not.Null, "Network ACL not found for the public subnet.");
//            Assert.That(privateNetworkAcl, Is.Not.Null, "Network ACL not found for the private subnet.");

//            Assert.That(publicNetworkAcl.Entries.Any(e => e.RuleAction == RuleAction.Allow && e.CidrBlock == "0.0.0.0/0" && e.Protocol == "-1"), Is.True, "Public subnet should have a Network ACL allowing all inbound and outbound traffic from 0.0.0.0/0.");
//            Assert.That(privateNetworkAcl.Entries.Any(e => e.RuleAction == RuleAction.Allow && e.CidrBlock == "0.0.0.0/0" && e.Protocol == "-1"), Is.True, "Private subnet should have a Network ACL allowing all inbound and outbound traffic from 0.0.0.0/0.");
//        }
//    }
//}

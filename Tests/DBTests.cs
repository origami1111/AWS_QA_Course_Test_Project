using Amazon.RDS.Model;
using AWS_QA_Course_Test_Project.Base;
using AWS_QA_Course_Test_Project.Clients;
using AWS_QA_Course_Test_Project.Utils;
using MySql.Data.MySqlClient;
using Renci.SshNet;
using System.Data;

namespace AWS_QA_Course_Test_Project.Tests
{
    [TestFixture]
    public class DBTests : BaseTest
    {
        // CXQA-RDS-01: Application Instance requirements
        [Test(Description = "CXQA-RDS-01: Check that DB is deployed to the private subnet")]
        public async Task TestDBIsDeployedToThePrivateSubnet()
        {
            var dbInstances = await RDSHelper.DescribeDBInstancesAsync(RdsClient);
            var dbInstance = dbInstances.FirstOrDefault();

            Assert.That(dbInstance, Is.Not.Null, "DB instance not found.");

            var subnets = await VPCHelper.DescribeSubnetsAsync(Ec2Client, dbInstance.DBSubnetGroup.VpcId);
            var privateSubnet = subnets.FirstOrDefault(subnet => subnet.SubnetId == dbInstance.DBSubnetGroup.Subnets.First().SubnetIdentifier && !subnet.MapPublicIpOnLaunch);

            Assert.That(privateSubnet, Is.Not.Null, "DB instance is not deployed to a private subnet.");
        }

        [Test(Description = "CXQA-RDS-01: Check that DB is accessible from the application's public subnet")]
        public async Task TestDBIsAccessibleFromApplicationsPublicSubnet()
        {
            var dbInstances = await RDSHelper.DescribeDBInstancesAsync(RdsClient);
            var dbInstance = dbInstances.FirstOrDefault();

            Assert.That(dbInstance, Is.Not.Null, "DB instance not found.");

            var ec2Instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var ec2Instance = ec2Instances.FirstOrDefault(instance => instance.PublicIpAddress != null);

            Assert.That(ec2Instance, Is.Not.Null, "EC2 instance with public IP not found.");

            var dbSecurityGroups = await EC2Helper.DescribeSecurityGroupsAsync(Ec2Client, dbInstance.VpcSecurityGroups.Select(sg => sg.VpcSecurityGroupId).ToList());
            var ec2SecurityGroups = await EC2Helper.DescribeSecurityGroupsAsync(Ec2Client, ec2Instance.SecurityGroups.Select(sg => sg.GroupId).ToList());

            var isAccessible = false;
            var dbPort = dbInstance.Endpoint.Port;

            foreach (var dbSecurityGroup in dbSecurityGroups)
            {
                foreach (var ec2SecurityGroup in ec2SecurityGroups)
                {
                    var ingressRules = dbSecurityGroup.IpPermissions;

                    isAccessible = ingressRules.Any(rule =>
                        rule.FromPort <= dbPort && rule.ToPort >= dbPort &&
                        rule.UserIdGroupPairs.Any(pair => pair.GroupId == ec2SecurityGroup.GroupId));

                    if (isAccessible)
                        break;
                }

                if (isAccessible)
                    break;
            }

            Assert.That(isAccessible, Is.True, "DB instance is not accessible from the application's public subnet.");
        }

        [Test(Description = "CXQA-RDS-01: Check that DB is not accessible from the public internet")]
        public async Task TestDBIsNotAccessibleFromPublicInternet()
        {
            var dbInstances = await RDSHelper.DescribeDBInstancesAsync(RdsClient);
            var dbInstance = dbInstances.FirstOrDefault();

            Assert.That(dbInstance, Is.Not.Null, "DB instance not found.");

            var securityGroups = await EC2Helper.DescribeSecurityGroupsAsync(Ec2Client, dbInstance.VpcSecurityGroups.Select(sg => sg.VpcSecurityGroupId).ToList());

            foreach (var securityGroup in securityGroups)
            {
                var ingressRules = securityGroup.IpPermissions;
                var dbPort = dbInstance.Endpoint.Port;

                var hasPublicAccess = ingressRules.Any(rule =>
                    rule.FromPort <= dbPort && rule.ToPort >= dbPort &&
                    rule.Ipv4Ranges.Any(ipRange => ipRange.CidrIp == "0.0.0.0/0"));

                Assert.That(hasPublicAccess, Is.False, $"DB instance is accessible from the public internet via security group {securityGroup.GroupId}.");
            }
        }

        [Test(Description = "CXQA-RDS-01: Check that EC2 application can access to DB via security group")]
        public async Task TestEC2ApplicationCanAccessToDBViaSecurityGroup()
        {
            var dbInstances = await RDSHelper.DescribeDBInstancesAsync(RdsClient);
            var dbInstance = dbInstances.FirstOrDefault();

            Assert.That(dbInstance, Is.Not.Null, "DB instance not found.");

            var ec2Instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var ec2Instance = ec2Instances.FirstOrDefault();

            Assert.That(ec2Instance, Is.Not.Null, "EC2 instance not found.");

            var dbSecurityGroups = await EC2Helper.DescribeSecurityGroupsAsync(Ec2Client, dbInstance.VpcSecurityGroups.Select(sg => sg.VpcSecurityGroupId).ToList());
            var ec2SecurityGroups = await EC2Helper.DescribeSecurityGroupsAsync(Ec2Client, ec2Instance.SecurityGroups.Select(sg => sg.GroupId).ToList());

            var isAccessible = false;
            var dbPort = dbInstance.Endpoint.Port;

            foreach (var dbSecurityGroup in dbSecurityGroups)
            {
                foreach (var ec2SecurityGroup in ec2SecurityGroups)
                {
                    var ingressRules = dbSecurityGroup.IpPermissions;

                    isAccessible = ingressRules.Any(rule =>
                        rule.FromPort <= dbPort && rule.ToPort >= dbPort &&
                        rule.UserIdGroupPairs.Any(pair => pair.GroupId == ec2SecurityGroup.GroupId));

                    if (isAccessible)
                        break;
                }

                if (isAccessible)
                    break;
            }

            Assert.That(isAccessible, Is.True, "EC2 application cannot access the DB via security group.");
        }

        // CXQA-RDS-02: RDS Instance requirements
        [Test(Description = "CXQA-RDS-02: Check that RDS instance type is correct: db.t3.micro")]
        public async Task TestRdsInstanceTypeIsCorrect()
        {
            var expectedInstanceType = "db.t3.micro";

            var dbInstances = await RDSHelper.DescribeDBInstancesAsync(RdsClient);
            var dbInstance = dbInstances.FirstOrDefault();

            Assert.That(dbInstance, Is.Not.Null, "DB instance not found.");
            Assert.That(dbInstance.DBInstanceClass, Is.EqualTo(expectedInstanceType), $"RDS instance type is not correct. Expected: {expectedInstanceType}, Actual: {dbInstance.DBInstanceClass}");
        }

        [Test(Description = "CXQA-RDS-02: Check that RDS instance Multi-AZ is no")]
        public async Task TestRdsInstanceMultiAZIsNo()
        {
            var dbInstances = await RDSHelper.DescribeDBInstancesAsync(RdsClient);
            var dbInstance = dbInstances.FirstOrDefault();

            Assert.That(dbInstance, Is.Not.Null, "DB instance not found.");
            Assert.That(dbInstance.MultiAZ, Is.False, "RDS instance is configured for Multi-AZ, but it should not be.");
        }

        [Test(Description = "CXQA-RDS-02: Check that RDS instance storage size is correct: 100 GiB")]
        public async Task TestRdsInstanceStorageSizeIsCorrect()
        {
            var expectedStorageSize = 100;

            var dbInstances = await RDSHelper.DescribeDBInstancesAsync(RdsClient);
            var dbInstance = dbInstances.FirstOrDefault();

            Assert.That(dbInstance, Is.Not.Null, "DB instance not found.");
            Assert.That(dbInstance.AllocatedStorage, Is.EqualTo(expectedStorageSize), $"RDS instance storage size is not correct. Expected: {expectedStorageSize} GiB, Actual: {dbInstance.AllocatedStorage} GiB");
        }

        [Test(Description = "CXQA-RDS-02: Check that RDS instance storage type is correct: General Purpose SSD (gp2)")]
        public async Task TestRdsInstanceStorageTypeIsCorrect()
        {
            var expectedStorageType = "gp2";

            var dbInstances = await RDSHelper.DescribeDBInstancesAsync(RdsClient);
            var dbInstance = dbInstances.FirstOrDefault();

            Assert.That(dbInstance, Is.Not.Null, "DB instance not found.");

            var storageType = dbInstance.StorageType;
            Assert.That(storageType, Is.EqualTo(expectedStorageType), $"RDS instance storage type is not correct. Expected: {expectedStorageType}, Actual: {storageType}");
        }

        [Test(Description = "CXQA-RDS-02: Check that RDS instance encryption not enabled")]
        public async Task TestRdsInstanceEncryptionNotEnabled()
        {
            var dbInstances = await RDSHelper.DescribeDBInstancesAsync(RdsClient);
            var dbInstance = dbInstances.FirstOrDefault();

            Assert.That(dbInstance, Is.Not.Null, "DB instance not found.");
            Assert.That(dbInstance.StorageEncrypted, Is.False, "RDS instance encryption is enabled, but it should not be.");
        }

        [Test(Description = "CXQA-RDS-02: Check that RDS instance has tag: cloudx: qa")]
        public async Task TestRdsInstanceHasCorrectTags()
        {
            var expectedTagKey = "cloudx";
            var expectedTagValue = "qa";

            var dbInstances = await RDSHelper.DescribeDBInstancesAsync(RdsClient);
            var dbInstance = dbInstances.FirstOrDefault();

            Assert.That(dbInstance, Is.Not.Null, "DB instance not found.");

            var describeTagsRequest = new ListTagsForResourceRequest
            {
                ResourceName = dbInstance.DBInstanceArn
            };
            var describeTagsResponse = await RdsClient.ListTagsForResourceAsync(describeTagsRequest);
            var tags = describeTagsResponse.TagList;

            var hasCorrectTag = tags.Any(tag => tag.Key == expectedTagKey && tag.Value == expectedTagValue);

            Assert.That(hasCorrectTag, Is.True, $"RDS instance does not have the correct tag. Expected: {expectedTagKey}: {expectedTagValue}");
        }

        [Test(Description = "CXQA-RDS-03: Check that DB is MySQL")]
        public async Task TestDBIsMySQL()
        {
            var expectedEngine = "mysql";

            var dbInstances = await RDSHelper.DescribeDBInstancesAsync(RdsClient);
            var dbInstance = dbInstances.FirstOrDefault();

            Assert.That(dbInstance, Is.Not.Null, "DB instance not found.");
            Assert.That(dbInstance.Engine, Is.EqualTo(expectedEngine), $"DB engine is not MySQL. Expected: {expectedEngine}, Actual: {dbInstance.Engine}");
        }

        [Test(Description = "CXQA-RDS-03: Check that DB version is correct: 8.0.32")]
        public async Task TestDataBaseVersionIsCorrect()
        {
            var expectedVersion = "8.0.32";

            var dbInstances = await RDSHelper.DescribeDBInstancesAsync(RdsClient);
            var dbInstance = dbInstances.FirstOrDefault();

            Assert.That(dbInstance, Is.Not.Null, "DB instance not found.");
            Assert.That(dbInstance.EngineVersion, Is.EqualTo(expectedVersion), $"DB version is not correct. Expected: {expectedVersion}, Actual: {dbInstance.EngineVersion}");
        }

        // Application functional validation
        string filePath = "C:\\Users\\origami\\Desktop\\CloudX Associate AWS for Testers\\AWS_QA_Course_Test_Project\\Images\\image0.jpg";
        string imageName = "image0.jpg";

        [Test(Description = "CXQA-RDS-03: Check that uploaded image metadata is stored in MySQL RDS database")]
        public async Task TestImageMetadataIsStoredInDB()
        {
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIpAddress = publicInstance.PublicIpAddress;

            var restClient = new RestClient(publicIpAddress);
            var postImageResponse = await restClient.PostImageAsync(filePath);
            var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);

            using (var dbManager = new DatabaseConnectionManager())
            {
                try
                {
                    var connection = await dbManager.ConnectToDatabaseAsync();
                    var images = await DBUtilsHandler.GetImagesAsync(connection);

                    Assert.That(images.Count, Is.EqualTo(1), "Several images was returned, expected 1");

                    var image = await DBUtilsHandler.GetImageAsync(connection, getImageResponse.Id);

                    Assert.That(image, Is.Not.Null, "Image metadata not found in the database.");
                    Assert.That(image.Id, Is.EqualTo(int.Parse(getImageResponse.Id)), "Image ID is not correct.");
                    Assert.That(image.ObjectKey, Is.EqualTo(getImageResponse.ObjectKey), "Object key is not correct.");
                    Assert.That(image.ObjectSize, Is.EqualTo(getImageResponse.ObjectSize), "Object size is not correct.");
                    Assert.That(image.ObjectType, Is.EqualTo(getImageResponse.ObjectType), "Object type is not correct.");
                    Assert.That(image.LastModified, Is.EqualTo(getImageResponse.LastModified), "Last modified date is not correct.");

                    Console.WriteLine($"ID: {image.Id}, Object Key: {image.ObjectKey}, Object Size: {image.ObjectSize}, Object Type: {image.ObjectType}, Last Modified: {image.LastModified}");
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"MySQL exception occurred: {ex.Message}");
                    Assert.Fail($"MySQL exception occurred while connecting to the database: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occurred: {ex.Message}");
                    Assert.Fail($"Exception occurred while connecting to the database: {ex.Message}");
                }
                finally
                {
                    await restClient.DeleteImageAsync(postImageResponse.Id);
                    dbManager.Dispose();
                }
            }
        }

        [Test(Description = "CXQA-RDS-04: The image metadata is returned by http://{INSTANCE PUBLIC IP}/api/image/{image_id} GET request")]
        public async Task TestImageMetadataIsReturnedByGetRequest()
        {
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIpAddress = publicInstance.PublicIpAddress;

            var restClient = new RestClient(publicIpAddress);
            var postImageResponse = await restClient.PostImageAsync(filePath);
            var getImageResponse = await restClient.GetImageMetadataAsync(postImageResponse.Id);

            Assert.That(getImageResponse, Is.Not.Null, "Image metadata not returned by GET request.");

            Console.WriteLine($"ID: {getImageResponse.Id}, Object Key: {getImageResponse.ObjectKey}, Object Size: {getImageResponse.ObjectSize}, Object Type: {getImageResponse.ObjectType}, Last Modified: {getImageResponse.LastModified}");

            await restClient.DeleteImageAsync(postImageResponse.Id);
        }

        [Test(Description = "CXQA-RDS-05: The image metadata for the deleted image is also deleted from the database")]
        public async Task TestImageMetadataIsDeletedFromDB()
        {
            var instances = await EC2Helper.DescribeInstancesAsync(Ec2Client);
            var publicInstance = EC2Helper.GetPublicInstance(instances);
            string publicIpAddress = publicInstance.PublicIpAddress;

            var restClient = new RestClient(publicIpAddress);
            var postImageResponse = await restClient.PostImageAsync(filePath);

            using (var dbManager = new DatabaseConnectionManager())
            {
                try
                {
                    var connection = await dbManager.ConnectToDatabaseAsync();
                    var image = await DBUtilsHandler.GetImageAsync(connection, postImageResponse.Id);

                    Assert.That(image, Is.Not.Null, "Image metadata not found in the database before deletion.");

                    await restClient.DeleteImageAsync(postImageResponse.Id);

                    image = await DBUtilsHandler.GetImageAsync(connection, postImageResponse.Id);

                    Assert.That(image, Is.Null, "Image metadata still found in the database after deletion.");
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"MySQL exception occurred: {ex.Message}");
                    Assert.Fail($"MySQL exception occurred while connecting to the database: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occurred: {ex.Message}");
                    Assert.Fail($"Exception occurred while connecting to the database: {ex.Message}");
                }
                finally
                {
                    dbManager.Dispose();
                }
            }
        }
    }
}

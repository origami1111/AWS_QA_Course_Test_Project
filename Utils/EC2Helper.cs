using Amazon.EC2.Model;
using Amazon.EC2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace AWS_QA_Course_Test_Project.Utils
{
    public static class EC2Helper
    {
        public static async Task<List<Instance>> DescribeInstancesAsync(AmazonEC2Client ec2Client)
        {
            var describeInstancesRequest = new DescribeInstancesRequest();
            var describeInstancesResponse = await ec2Client.DescribeInstancesAsync(describeInstancesRequest);

            return describeInstancesResponse.Reservations
                .SelectMany(r => r.Instances)
                .Where(i => i.State.Name == InstanceStateName.Running)
                .ToList();
        }

        public static bool HasPublicInstance(List<Instance> instances)
        {
            return instances.Any(i => i.PublicIpAddress != null);
        }

        public static bool HasPrivateInstance(List<Instance> instances)
        {
            return instances.Any(i => i.PublicIpAddress == null);
        }

        public static Instance GetPublicInstance(List<Instance> instances)
        {
            return instances.FirstOrDefault(i => i.PublicIpAddress != null);
        }

        public static Instance GetPrivateInstance(List<Instance> instances)
        {
            return instances.FirstOrDefault(i => i.PublicIpAddress == null);
        }

        public static async Task<Volume> DescribeVolumeAsync(AmazonEC2Client ec2Client, string volumeId)
        {
            var describeVolumesRequest = new DescribeVolumesRequest
            {
                VolumeIds = new List<string> { volumeId }
            };
            var describeVolumesResponse = await ec2Client.DescribeVolumesAsync(describeVolumesRequest);
            return describeVolumesResponse.Volumes.FirstOrDefault();
        }

        public static async Task<Image> DescribeImageAsync(AmazonEC2Client ec2Client, string imageId)
        {
            var describeImagesRequest = new DescribeImagesRequest
            {
                ImageIds = new List<string> { imageId }
            };
            var describeImagesResponse = await ec2Client.DescribeImagesAsync(describeImagesRequest);
            return describeImagesResponse.Images.FirstOrDefault();
        }

        public static async Task<SecurityGroup> DescribeSecurityGroupAsync(AmazonEC2Client ec2Client, string groupId)
        {
            var describeSecurityGroupsRequest = new DescribeSecurityGroupsRequest
            {
                GroupIds = new List<string> { groupId }
            };
            var describeSecurityGroupsResponse = await ec2Client.DescribeSecurityGroupsAsync(describeSecurityGroupsRequest);
            return describeSecurityGroupsResponse.SecurityGroups.FirstOrDefault();
        }
    }
}

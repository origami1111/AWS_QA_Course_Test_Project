using Amazon.EC2.Model;
using Amazon.EC2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS_QA_Course_Test_Project.Utils
{
    public static class VPCHelper
    {
        public static async Task<List<Vpc>> DescribeVpcsAsync(AmazonEC2Client ec2Client)
        {
            var describeVpcsRequest = new DescribeVpcsRequest();
            var describeVpcsResponse = await ec2Client.DescribeVpcsAsync(describeVpcsRequest);
            return describeVpcsResponse.Vpcs;
        }

        public static async Task<List<Subnet>> DescribeSubnetsAsync(AmazonEC2Client ec2Client, string vpcId)
        {
            var describeSubnetsRequest = new DescribeSubnetsRequest
            {
                Filters = new List<Filter>
            {
                new Filter("vpc-id", new List<string> { vpcId })
            }
            };
            var describeSubnetsResponse = await ec2Client.DescribeSubnetsAsync(describeSubnetsRequest);
            return describeSubnetsResponse.Subnets;
        }

        public static async Task<List<RouteTable>> DescribeRouteTablesAsync(AmazonEC2Client ec2Client, string subnetId)
        {
            var describeRouteTablesRequest = new DescribeRouteTablesRequest
            {
                Filters = new List<Filter>
                {
                    new Filter("association.subnet-id", new List<string> { subnetId })
                }
            };
            var describeRouteTablesResponse = await ec2Client.DescribeRouteTablesAsync(describeRouteTablesRequest);
            return describeRouteTablesResponse.RouteTables;
        }

        public static async Task<NetworkAcl> DescribeNetworkAclsAsync(AmazonEC2Client ec2Client, string subnetId)
        {
            var describeNetworkAclsRequest = new DescribeNetworkAclsRequest
            {
                Filters = new List<Filter>
                {
                    new Filter("association.subnet-id", new List<string> { subnetId })
                }
            };
            var describeNetworkAclsResponse = await ec2Client.DescribeNetworkAclsAsync(describeNetworkAclsRequest);
            return describeNetworkAclsResponse.NetworkAcls.FirstOrDefault();
        }
    }
}

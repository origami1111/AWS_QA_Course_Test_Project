using Amazon.RDS;
using Amazon.RDS.Model;

namespace AWS_QA_Course_Test_Project.Utils
{
    public static class RDSHelper
    {
        public static async Task<List<DBInstance>> DescribeDBInstancesAsync(AmazonRDSClient rdsClient)
        {
            var describeDBInstancesRequest = new DescribeDBInstancesRequest();
            var describeDBInstancesResponse = await rdsClient.DescribeDBInstancesAsync(describeDBInstancesRequest);

            return describeDBInstancesResponse.DBInstances;
        }
    }
}

using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.ApplicationAutoScaling;
using Amazon.ApplicationAutoScaling.Model;
using AWS_QA_Course_Test_Project.DTOs;

namespace AWS_QA_Course_Test_Project.Utils
{
    public static class DynamoDBHelper
    {
        public static async Task<string> GetDynamoDBTableNameAsync(AmazonDynamoDBClient dynamoDbClient, string tableName)
        {
            var request = new ListTablesRequest();
            var response = await dynamoDbClient.ListTablesAsync(request);

            foreach (var table in response.TableNames)
            {
                if (table.StartsWith(tableName))
                {
                    return table;
                }
            }

            return null;
        }

        public static async Task<List<GlobalSecondaryIndexDescription>> GetGlobalSecondaryIndexesAsync(AmazonDynamoDBClient dynamoDbClient, string tableName)
        {
            var response = await GetDescribeTableResponseAsync(dynamoDbClient, tableName);
            return response.Table.GlobalSecondaryIndexes;
        }

        public static async Task<long> GetProvisionedReadCapacityUnitsAsync(AmazonDynamoDBClient dynamoDbClient, string tableName)
        {
            var provisionedThroughput = await GetProvisionedThroughputAsync(dynamoDbClient, tableName);
            return provisionedThroughput.ReadCapacityUnits;
        }

        public static async Task<long> GetProvisionedWriteCapacityUnitsAsync(AmazonDynamoDBClient dynamoDbClient, string tableName)
        {
            var provisionedThroughput = await GetProvisionedThroughputAsync(dynamoDbClient, tableName);
            return provisionedThroughput.WriteCapacityUnits;
        }

        public static async Task<bool> IsReadAutoscalingEnabledAsync(string tableName)
        {
            var applicationAutoScalingClient = new AmazonApplicationAutoScalingClient();
            var request = new DescribeScalingPoliciesRequest
            {
                ServiceNamespace = ServiceNamespace.Dynamodb,
                ResourceId = $"table/{tableName}",
                ScalableDimension = ScalableDimension.DynamodbTableReadCapacityUnits
            };
            var response = await applicationAutoScalingClient.DescribeScalingPoliciesAsync(request);
            return response.ScalingPolicies.Count > 0;
        }

        public static async Task<bool> IsWriteAutoscalingEnabledAsync(string tableName)
        {
            var applicationAutoScalingClient = new AmazonApplicationAutoScalingClient();
            var request = new DescribeScalingPoliciesRequest
            {
                ServiceNamespace = ServiceNamespace.Dynamodb,
                ResourceId = $"table/{tableName}",
                ScalableDimension = ScalableDimension.DynamodbTableWriteCapacityUnits
            };
            var response = await applicationAutoScalingClient.DescribeScalingPoliciesAsync(request);
            return response.ScalingPolicies.Count > 0;
        }

        public static async Task<bool> IsTimeToLiveDisabledAsync(AmazonDynamoDBClient dynamoDbClient, string tableName)
        {
            var request = new DescribeTimeToLiveRequest
            {
                TableName = tableName
            };
            var response = await dynamoDbClient.DescribeTimeToLiveAsync(request);
            return response.TimeToLiveDescription.TimeToLiveStatus == TimeToLiveStatus.DISABLED;
        }

        public static async Task<List<Tag>> GetTableTagsAsync(AmazonDynamoDBClient dynamoDbClient, string tableName)
        {
            var request = new ListTagsOfResourceRequest
            {
                ResourceArn = $"arn:aws:dynamodb:eu-central-1:396913717218:table/{tableName}"
            };
            var response = await dynamoDbClient.ListTagsOfResourceAsync(request);
            return response.Tags;
        }

        public static async Task<List<string>> GetTableAttributesAsync(AmazonDynamoDBClient client, string tableName)
        {
            var tableDescription = await client.DescribeTableAsync(new DescribeTableRequest
            {
                TableName = tableName
            });

            return tableDescription.Table.AttributeDefinitions.Select(attr => attr.AttributeName).ToList();
        }

        public static async Task<ImageResponseDTO> GetItemFromDynamoDBById(AmazonDynamoDBClient dynamoDbClient, string tableName, string id)
        {
            var key = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } }
            };

            var request = new GetItemRequest
            {
                TableName = tableName,
                Key = key
            };

            var response = await dynamoDbClient.GetItemAsync(request);
            var item = response.Item;

            ImageResponseDTO imageResponse;

            if (item.Count == 0)
            {
                imageResponse = null;
            }
            else
            {
                imageResponse = new ImageResponseDTO
                {
                    Id = item["id"].S,
                    CreatedAt = double.Parse(item["created_at"].N),
                    LastModified = double.Parse(item["last_modified"].N),
                    ObjectKey = item["object_key"].S,
                    ObjectSize = double.Parse(item["object_size"].N),
                    ObjectType = item["object_type"].S
                };
            }

            return imageResponse;
        } 

        private static async Task<DescribeTableResponse> GetDescribeTableResponseAsync(AmazonDynamoDBClient dynamoDbClient, string tableName)
        {
            var request = new DescribeTableRequest
            {
                TableName = tableName
            };
            var response = await dynamoDbClient.DescribeTableAsync(request);
            return await dynamoDbClient.DescribeTableAsync(request);
        }

        private static async Task<ProvisionedThroughputDescription> GetProvisionedThroughputAsync(AmazonDynamoDBClient dynamoDbClient, string tableName)
        {
            var response = await GetDescribeTableResponseAsync(dynamoDbClient, tableName);
            return response.Table.ProvisionedThroughput;
        }
    }
}

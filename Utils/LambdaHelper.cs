using Amazon.Lambda;
using Amazon.Lambda.Model;

namespace AWS_QA_Course_Test_Project.Utils
{
    public class LambdaHelper
    {
        public static async Task<List<EventSourceMappingConfiguration>> GetEventSourceMappingsAsync(AmazonLambdaClient lambdaClient, string functionName)
        {
            var request = new ListEventSourceMappingsRequest
            {
                FunctionName = functionName
            };
            var response = await lambdaClient.ListEventSourceMappingsAsync(request);
            return response.EventSourceMappings;
        }

        public static async Task<string> GetLambdaFunctionNameAsync(AmazonLambdaClient lambdaClient, string functionNamePrefix)
        {
            var request = new ListFunctionsRequest();
            var response = await lambdaClient.ListFunctionsAsync(request);

            foreach (var function in response.Functions)
            {
                if (function.FunctionName.StartsWith(functionNamePrefix))
                {
                    return function.FunctionName;
                }
            }

            return null;
        }

        public static async Task<int> GetLambdaFunctionMemoryAsync(AmazonLambdaClient lambdaClient, string functionName)
        {
            var response = await GetFunctionConfigurationResponseAsync(lambdaClient, functionName);
            return response.MemorySize;
        }

        public static async Task<int> GetLambdaFunctionEphemeralStorageAsync(AmazonLambdaClient lambdaClient, string functionName)
        {
            var response = await GetFunctionConfigurationResponseAsync(lambdaClient, functionName);
            return response.EphemeralStorage?.Size ?? 512;
        }

        public static async Task<int> GetLambdaFunctionTimeoutAsync(AmazonLambdaClient lambdaClient, string functionName)
        {
            var response = await GetFunctionConfigurationResponseAsync(lambdaClient, functionName);
            return response.Timeout;
        }

        public static async Task<Dictionary<string, string>> GetLambdaFunctionTagsAsync(AmazonLambdaClient lambdaClient, string functionName) // Fix CS0246 and IDE0060
        {
            string functionArn = $"arn:aws:lambda:eu-central-1:396913717218:function:{functionName}";
            var request = new ListTagsRequest
            {
                Resource = functionArn
            };
            var response = await lambdaClient.ListTagsAsync(request);
            return response.Tags;
        }

        private static async Task<GetFunctionConfigurationResponse> GetFunctionConfigurationResponseAsync(AmazonLambdaClient lambdaClient, string functionName)
        {
            var request = new GetFunctionConfigurationRequest
            {
                FunctionName = functionName
            };
            return await lambdaClient.GetFunctionConfigurationAsync(request);
        }
    }
}

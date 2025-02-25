using Amazon.Auth.AccessControlPolicy;
using Amazon.CDK.AWS.IAM;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace AWS_QA_Course_Test_Project.Utils
{
    public static class IamHelper
    {
        public static async Task<List<Amazon.IdentityManagement.Model.User>> GetUsers(AmazonIdentityManagementServiceClient client)
        {
            var response = await client.ListUsersAsync(new ListUsersRequest());
            return response.Users;
        }

        public static async Task<List<Amazon.IdentityManagement.Model.Group>> GetGroups(AmazonIdentityManagementServiceClient client)
        {
            var response = await client.ListGroupsAsync(new ListGroupsRequest());
            return response.Groups;
        }

        public static async Task<List<Amazon.IdentityManagement.Model.Role>> GetRoles(AmazonIdentityManagementServiceClient client)
        {
            var response = await client.ListRolesAsync(new ListRolesRequest());
            return response.Roles;
        }

        public static async Task<List<Amazon.IdentityManagement.Model.ManagedPolicy>> GetPolicies(AmazonIdentityManagementServiceClient client)
        {
            var response = await client.ListPoliciesAsync(new ListPoliciesRequest());
            return response.Policies;
        }

        public static async Task<string> GetPolicyDocument(AmazonIdentityManagementServiceClient client, string policyArn)
        {
            var getPolicyResponse = await client.GetPolicyAsync(new GetPolicyRequest
            {
                PolicyArn = policyArn
            });

            var getPolicyVersionResponse = await client.GetPolicyVersionAsync(new GetPolicyVersionRequest
            {
                PolicyArn = policyArn,
                VersionId = getPolicyResponse.Policy.DefaultVersionId
            });

            var policyDocument = getPolicyVersionResponse.PolicyVersion.Document;
            var decodedDocument = WebUtility.UrlDecode(policyDocument);
            var jsonDocument = JsonSerializer.Serialize(JsonDocument.Parse(decodedDocument).RootElement);
            return jsonDocument;
        }

        public static async Task<List<AttachedPolicyType>> GetAttachedRolePolicies(AmazonIdentityManagementServiceClient client, string roleName)
        {
            var response = await client.ListAttachedRolePoliciesAsync(new ListAttachedRolePoliciesRequest
            {
                RoleName = roleName
            });

            return response.AttachedPolicies;
        }

        public static async Task<List<AttachedPolicyType>> GetAttachedGroupPolicies(AmazonIdentityManagementServiceClient iamClient, string groupName)
        {
            var response = await iamClient.ListAttachedGroupPoliciesAsync(new ListAttachedGroupPoliciesRequest
            {
                GroupName = groupName
            });

            return response.AttachedPolicies;
        }

        public static async Task<List<Amazon.IdentityManagement.Model.Group>> GetUserGroups(AmazonIdentityManagementServiceClient client, string userName)
        {
            var response = await client.ListGroupsForUserAsync(new ListGroupsForUserRequest
            {
                UserName = userName
            });

            return response.Groups;
        }

    }
}

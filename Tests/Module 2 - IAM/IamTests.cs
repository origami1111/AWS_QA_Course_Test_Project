//using AWS_QA_Course_Test_Project.Base;
//using AWS_QA_Course_Test_Project.Utils;
//using Newtonsoft.Json.Linq;

//namespace AWS_QA_Course_Test_Project.Tests
//{
//    [TestFixture]
//    public class IamTests : BaseTest
//    {
//        [Test(Description = "CXQA-IAM-01: Check that 3 IAM policies are created according to the requirements")]
//        [TestCase("FullAccessPolicyEC2", "ec2:*", "*", "Allow")]
//        [TestCase("FullAccessPolicyS3", "s3:*", "*", "Allow")]
//        [TestCase("ReadAccessPolicyS3", "s3:Describe*,s3:Get*,s3:List*", "*", "Allow")]
//        public async Task TestPolicies(string expectedPolicy, string expectedActionsAllowed, string expectedResources, string expectedEffect)
//        {
//            var policies = await IamHelper.GetPolicies(IamClient);
//            var policy = policies.FirstOrDefault(p => p.PolicyName == expectedPolicy);
//            Assert.That(policy, Is.Not.Null, $"Policy {expectedPolicy} not found.");

//            var policyDocument = await IamHelper.GetPolicyDocument(IamClient, policy.Arn);
//            Console.WriteLine($"Policy Document: {policyDocument}");
//            var policyJson = JObject.Parse(policyDocument);

//            var statements = policyJson["Statement"]?.ToArray();
//            Assert.That(statements, Is.Not.Null, "Statements not found.");
//            Assert.That(statements.Length, Is.EqualTo(1), "Unexpected number of statements found.");

//            var statement = statements.First();
//            var actions = statement["Action"]?.ToString();
//            Assert.That(actions, Is.Not.Null, "Actions are null.");
//            actions = actions.Replace("\r\n", "").Replace("[", "").Replace("]", "").Replace("\"", "").Replace(" ", "").Trim();

//            var resources = statement["Resource"]?.ToString();
//            Assert.That(resources, Is.Not.Null, "Resources are null.");

//            var effect = statement["Effect"]?.ToString();
//            Assert.That(effect, Is.Not.Null, "Effect is null.");

//            Assert.That(actions, Is.EqualTo(expectedActionsAllowed), "Actions do not match.");
//            Assert.That(resources, Is.EqualTo(expectedResources), "Resources do not match.");
//            Assert.That(effect, Is.EqualTo(expectedEffect), "Effect does not match.");
//        }

//        [Test(Description = "CXQA-IAM-02: Check that 3 IAM roles are created according to the requirements")]
//        [TestCase("FullAccessRoleEC2", "FullAccessPolicyEC2")]
//        [TestCase("FullAccessRoleS3", "FullAccessPolicyS3")]
//        [TestCase("ReadAccessRoleS3", "ReadAccessPolicyS3")]
//        public async Task TestRoles(string expectedRole, string expectedPolicies)
//        {
//            var roles = await IamHelper.GetRoles(IamClient);

//            var role = roles.FirstOrDefault(p => p.RoleName == expectedRole);
//            Assert.That(role, Is.Not.Null, $"Role {expectedRole} not found.");

//            var attachedPolicies = await IamHelper.GetAttachedRolePolicies(IamClient, role.RoleName);
//            var policy = attachedPolicies.FirstOrDefault(p => p.PolicyName == expectedPolicies);
//            Assert.That(policy, Is.Not.Null, $"Policy {expectedPolicies} not attached to role {expectedRole}.");

//            Console.WriteLine($"Role: {role.RoleName}, ARN: {role.Arn}, Attached Policy: {policy.PolicyName}");
//        }


//        [Test(Description = "CXQA-IAM-03: Check that 3 IAM users groups are created according to the requirements")]
//        [TestCase("FullAccessGroupEC2", "FullAccessPolicyEC2")]
//        [TestCase("FullAccessGroupS3", "FullAccessPolicyS3")]
//        [TestCase("ReadAccessGroupS3", "ReadAccessPolicyS3")]
//        public async Task TestUserGroups(string expectedGroup, string expectedPolicies)
//        {
//            var groups = await IamHelper.GetGroups(IamClient);
//            var group = groups.FirstOrDefault(g => g.GroupName == expectedGroup);
//            Assert.That(group, Is.Not.Null, $"Group {expectedGroup} not found.");

//            var attachedPolicies = await IamHelper.GetAttachedGroupPolicies(IamClient, group.GroupName);
//            var policy = attachedPolicies.FirstOrDefault(p => p.PolicyName == expectedPolicies);
//            Assert.That(policy, Is.Not.Null, $"Policy {expectedPolicies} not attached to group {expectedGroup}.");

//            Console.WriteLine($"Group: {group.GroupName}, ARN: {group.Arn}, Attached Policy: {policy.PolicyName}");
//        }


//        [Test(Description = "CXQA-IAM-04: Check that 3 IAM users are created according to the requirements")]
//        [TestCase("FullAccessUserEC2", "FullAccessGroupEC2")]
//        [TestCase("FullAccessUserS3", "FullAccessGroupS3")]
//        [TestCase("ReadAccessUserS3", "ReadAccessGroupS3")]
//        public async Task TestUsers(string expectedUser, string expectedGroup)
//        {
//            var users = await IamHelper.GetUsers(IamClient);
//            var user = users.FirstOrDefault(u => u.UserName == expectedUser);
//            Assert.That(user, Is.Not.Null, $"User {expectedUser} not found.");

//            var userGroups = await IamHelper.GetUserGroups(IamClient, user.UserName);
//            var group = userGroups.FirstOrDefault(g => g.GroupName == expectedGroup);
//            Assert.That(group, Is.Not.Null, $"Group {expectedGroup} not attached to user {expectedUser}.");

//            Console.WriteLine($"User: {user.UserName}, ARN: {user.Arn}, Attached Group: {group.GroupName}");
//        }

//    }
//}

//using AWS_QA_Course_Test_Project.Base;
//using AWS_QA_Course_Test_Project.Utils;
//using NUnit.Framework.Legacy;

//namespace AWS_QA_Course_Test_Project.Tests
//{
//    [TestFixture]
//    public class SQSTests : BaseTest
//    {
//        // CXQA-SNSSQS-03: SQS queue requirements
//        [Test(Description = "CXQA-SNSSQS-03: Name: cloudximage-QueueSQSQueue{unique id}")]
//        public async Task TestSQSName()
//        {
//            var queueUrl = await SQSHelper.GetQueueUrlAsync(SqsClient, "cloudximage-QueueSQSQueue");
//            Assert.That(queueUrl, Is.Not.Null, "SQS queue with the expected name pattern not found.");

//            var queueName = queueUrl.Split('/').Last();
//            StringAssert.StartsWith("cloudximage-QueueSQSQueue", queueName, "SQS queue name does not start with the expected prefix.");

//            var uniqueId = queueName.Replace("cloudximage-QueueSQSQueue", "");
//            Assert.That(uniqueId, Is.Not.Empty, "SQS queue name does not contain a unique ID.");
//        }

//        [Test(Description = "CXQA-SNSSQS-03: Encryption: enabled")]
//        public async Task TestSQSEncryption()
//        {
//            var queueUrl = await SQSHelper.GetQueueUrlAsync(SqsClient, "cloudximage-QueueSQSQueue");
//            Assert.That(queueUrl, Is.Not.Null, "SQS queue with the expected name pattern not found.");

//            var attributes = await SQSHelper.GetQueueAttributesAsync(SqsClient, queueUrl, new List<string> { "SqsManagedSseEnabled" });
//            var encryption = attributes.ContainsKey("SqsManagedSseEnabled") ? attributes["SqsManagedSseEnabled"] : null;

//            Assert.That(encryption, Is.Not.Null.And.Not.Empty, "SQS queue encryption is not enabled.");
//        }

//        [Test(Description = "CXQA-SNSSQS-03: Type: standard")]
//        public async Task TestSQSType()
//        {
//            var queueUrl = await SQSHelper.GetQueueUrlAsync(SqsClient, "cloudximage-QueueSQSQueue");
//            Assert.That(queueUrl, Is.Not.Null, "SQS queue with the expected name pattern not found.");

//            var queueName = queueUrl.Split('/').Last();
//            var queueType = queueName.EndsWith(".fifo") ? "fifo" : "standard";

//            Assert.That(queueType, Is.EqualTo("standard"), "SQS queue is not of type standard.");
//        }

//        [Test(Description = "CXQA-SNSSQS-03: Tags: cloudx: qa")]
//        public async Task TestSQSTags()
//        {
//            var expectedTagKey = "cloudx";
//            var expectedTagValue = "qa";

//            var queueUrl = await SQSHelper.GetQueueUrlAsync(SqsClient, "cloudximage-QueueSQSQueue");
//            Assert.That(queueUrl, Is.Not.Null, "SQS queue with the expected name pattern not found.");

//            var tags = await SQSHelper.GetQueueTagsAsync(SqsClient, queueUrl);

//            Assert.That(tags.ContainsKey(expectedTagKey), Is.True, $"SQS queue does not contain the expected tag key '{expectedTagKey}'.");
//            Assert.That(tags[expectedTagKey], Is.EqualTo(expectedTagValue), $"SQS queue tag value for key '{expectedTagKey}' does not match the expected value '{expectedTagValue}'.");
//        }

//        [Test(Description = "CXQA-SNSSQS-03: Dead-letter queue: no")]
//        public async Task TestSQSDeadLetterQueue()
//        {
//            var queueUrl = await SQSHelper.GetQueueUrlAsync(SqsClient, "cloudximage-QueueSQSQueue");
//            Assert.That(queueUrl, Is.Not.Null, "SQS queue with the expected name pattern not found.");

//            var attributes = await SQSHelper.GetQueueAttributesAsync(SqsClient, queueUrl, new List<string> { "RedrivePolicy" });
//            var redrivePolicy = attributes.ContainsKey("RedrivePolicy") ? attributes["RedrivePolicy"] : null;

//            Assert.That(redrivePolicy, Is.Null, "SQS queue has a dead-letter queue associated with it.");
//        }
//    }
//}

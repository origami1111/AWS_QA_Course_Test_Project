using Newtonsoft.Json;

namespace AWS_QA_Course_Test_Project.DTOs
{
    public class SubscriptionResponseDTO
    {
        [JsonProperty("Endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty("Owner")]
        public string Owner { get; set; }

        [JsonProperty("Protocol")]
        public string Protocol { get; set; }

        [JsonProperty("SubscriptionArn")]
        public string SubscriptionArn { get; set; }

        [JsonProperty("TopicArn")]
        public string TopicArn { get; set; }
    }
}

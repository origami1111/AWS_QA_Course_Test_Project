using Newtonsoft.Json;

namespace AWS_QA_Course_Test_Project.DTOs
{
    public class SecretValuesDTO
    {
        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("dbname")]
        public string DBname { get; set; }

        [JsonProperty("engine")]
        public string Engine { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("dbInstanceIdentifier")]
        public string DBInstanceIdentifier { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }
}

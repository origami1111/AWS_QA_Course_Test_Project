using Newtonsoft.Json;

namespace AWS_QA_Course_Test_Project.DTOs
{
    public class ImageResponseDTO
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created_at")]
        public double CreatedAt { get; set; }

        [JsonProperty("last_modified")]
        public double LastModified { get; set; }

        [JsonProperty("object_key")]
        public string ObjectKey { get; set; }

        [JsonProperty("object_size")]
        public double ObjectSize { get; set; }

        [JsonProperty("object_type")]
        public string ObjectType { get; set; }
    }
}

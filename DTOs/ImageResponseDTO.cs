﻿using Newtonsoft.Json;

namespace AWS_QA_Course_Test_Project.DTOs
{
    public class ImageResponseDTO
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("last_modified")]
        public string LastModified { get; set; }

        [JsonProperty("object_key")]
        public string ObjectKey { get; set; }

        [JsonProperty("object_size")]
        public int ObjectSize { get; set; }

        [JsonProperty("object_type")]
        public string ObjectType { get; set; }
    }
}

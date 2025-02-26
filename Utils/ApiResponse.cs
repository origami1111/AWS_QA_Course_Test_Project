﻿using Newtonsoft.Json;

namespace AWS_QA_Course_Test_Project.Utils
{
    public class ApiResponse
    {
        [JsonProperty("availability_zone")]
        public string AvailabilityZone { get; set; }

        [JsonProperty("private_ipv4")]
        public string PrivateIpv4 { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }
    }
}
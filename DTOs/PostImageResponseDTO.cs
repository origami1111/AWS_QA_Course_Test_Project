using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS_QA_Course_Test_Project.DTOs
{
    public class PostImageResponseDTO
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}

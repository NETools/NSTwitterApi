using System.Text.Json.Serialization;

namespace NSTwitterApi.Models.Json
{
    public class Root
    {
        [JsonPropertyName("variables")]
        public Variables Variables { get; set; }

        [JsonPropertyName("features")]
        public Features Features { get; set; }

        [JsonPropertyName("queryId")]
        public string QueryId { get; set; }
    }
}
using System.Text.Json.Serialization;

namespace NSTwitterApi.Models.Json
{
    public class Variables
    {
        [JsonPropertyName("tweet_text")]
        public string TweetText { get; set; }

        [JsonPropertyName("dark_request")]
        public bool DarkRequest { get; set; }

        [JsonPropertyName("media")]
        public Media Media { get; set; }

        [JsonPropertyName("semantic_annotation_ids")]
        public List<object> SemanticAnnotationIds { get; set; }
    }
}
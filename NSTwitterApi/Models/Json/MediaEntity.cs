using System.Text.Json.Serialization;

namespace NSTwitterApi.Models.Json
{
    public class MediaEntity
    {
        [JsonPropertyName("media_id")]
        public string MediaId { get; set; }

        [JsonPropertyName("tagged_users")]
        public List<object> TaggedUsers { get; set; }
    }

}
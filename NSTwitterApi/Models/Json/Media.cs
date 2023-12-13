using System.Text.Json.Serialization;

namespace NSTwitterApi.Models.Json
{
    public class Media
    {
        [JsonPropertyName("media_entities")]
        public List<MediaEntity> MediaEntities { get; set; }

        [JsonPropertyName("possibly_sensitive")]
        public bool PossiblySensitive { get; set; }
    }

}
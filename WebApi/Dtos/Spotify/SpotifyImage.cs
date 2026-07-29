using System.Text.Json.Serialization;

namespace WebApi.Dtos.Spotify
{
    public class SpotifyImage
    {
        [JsonPropertyName("url")] public string Url { get; set; }
        [JsonPropertyName("height")] public int Height { get; set; }
        [JsonPropertyName("width")] public int Width { get; set; }

    }
}

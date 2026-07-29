using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace WebApi.Dtos.Spotify
{
    public class SpotifyArtistItem
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("images")] public List<SpotifyImage> Images { get; set; }


    }
}

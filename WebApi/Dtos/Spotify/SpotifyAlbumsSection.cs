using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace WebApi.Dtos.Spotify
{
    public class SpotifyAlbumsSection
    {
        [JsonPropertyName("items")] public List<SpotifyAlbumItem> Items { get; set; }
        [JsonPropertyName("next")] public string Next { get; set; }
    }
}

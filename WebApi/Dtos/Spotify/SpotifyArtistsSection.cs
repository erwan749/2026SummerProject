using System.Text.Json.Serialization;
using System.Collections.Generic;
namespace WebApi.Dtos.Spotify
{
    public class SpotifyArtistsSection
    {
        [JsonPropertyName("items")] public List<SpotifyArtistItem> Items { get; set; }
    }
}

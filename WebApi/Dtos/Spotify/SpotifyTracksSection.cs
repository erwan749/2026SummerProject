using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebApi.Dtos.Spotify
{
    public class SpotifyTracksSection
    {
        [JsonPropertyName("items")] public List<SpotifyTrackItem> Items { get; set; }
    }
}

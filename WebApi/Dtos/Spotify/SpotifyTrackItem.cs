using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace WebApi.Dtos.Spotify
{
    public class SpotifyTrackItem
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("duration_ms")] public int DurationMs { get; set; }
        [JsonPropertyName("track_number")] public int TrackNumber { get; set; }
        [JsonPropertyName("preview_url")] public string PreviewUrl { get; set; }
        [JsonPropertyName("artists")] public List<SpotifyArtistItem> Artists { get; set; }
        [JsonPropertyName("album")] public SpotifyAlbumItem Album { get; set; }

    }
}

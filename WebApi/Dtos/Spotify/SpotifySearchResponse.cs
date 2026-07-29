using System.Text.Json.Serialization;
using System.Collections.Generic;
namespace WebApi.Dtos.Spotify
{
    public class SpotifySearchResponse
    {
        [JsonPropertyName("artists")] public SpotifyArtistsSection Artists { get; set; }
        [JsonPropertyName("albums")] public SpotifyAlbumsSection Albums { get; set; }
        [JsonPropertyName("tracks")] public SpotifyTracksSection Tracks { get; set; }


    }
}

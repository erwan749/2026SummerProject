using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace WebApi.Dtos.Spotify
{
    public class SpotifyAlbumItem
    {

        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("total_tracks")] public int TotalTracks { get; set; }
        [JsonPropertyName("images")] public List<SpotifyImage> Images { get; set; }
        [JsonPropertyName("artists")] public List<SpotifyArtistItem> Artists { get; set; }


    }
}

using System.Text.Json.Serialization;
using System.Collections.Generic;
namespace WebApi.Dtos.Deezer
{
    public class DeezerSearchResponse
    {
        [JsonPropertyName("data")] public List<DeezerTrackItem> Data { get; set; }
    }
}
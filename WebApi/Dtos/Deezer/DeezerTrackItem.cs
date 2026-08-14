using System.Text.Json.Serialization;
namespace WebApi.Dtos.Deezer
{
    public class DeezerTrackItem
    {
        [JsonPropertyName("preview")] public string Preview { get; set; }
    }
}
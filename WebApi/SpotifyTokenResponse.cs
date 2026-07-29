using System.Text.Json.Serialization;

namespace WebApi
{
    public class SpotifyTokenResponse
    {
        [JsonPropertyName("access_token")]  public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
        
    }
}

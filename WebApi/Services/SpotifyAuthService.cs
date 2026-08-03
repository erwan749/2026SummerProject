using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Collections.Generic;
namespace WebApi.Services
{
    public class SpotifyAuthService
    {

        private string _accessToken = string.Empty;
        private DateTime _tokenExpiration= DateTime.MinValue;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public SpotifyAuthService( IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        public async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiration)
            {
                return _accessToken;
            }
            string clientId = _configuration["Spotify:ClientId"];
            string clientSecret = _configuration["Spotify:ClientSecret"];
            string encodedCredential = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            HttpClient client = _httpClientFactory.CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedCredential);
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>{ { "grant_type","client_credentials"} });
            var reponse = await client.SendAsync(request);
            SpotifyTokenResponse tokenReponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(await reponse.Content.ReadAsStringAsync());
            _accessToken = tokenReponse.AccessToken;
            _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenReponse.ExpiresIn - 60);
            return _accessToken;
        }
    }
}

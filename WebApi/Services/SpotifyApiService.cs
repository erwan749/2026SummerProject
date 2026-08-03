using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using WebApi.Dtos;
using WebApi.Dtos.Spotify;


namespace WebApi.Services
{
    public class SpotifyApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SpotifyAuthService _spotifyAuthService;

        public SpotifyApiService(IHttpClientFactory httpClientFactory, SpotifyAuthService spotifyAuthService)
        {
            _httpClientFactory = httpClientFactory;
            _spotifyAuthService = spotifyAuthService;
        }

        public async Task<List<SearchResultItem>> SearchAsync(string query)
        {
            string token = await _spotifyAuthService.GetAccessTokenAsync();
            string url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=artist,album,track";
            HttpClient client = _httpClientFactory.CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",token);
            var reponse = await client.SendAsync(request);
            string json = await reponse.Content.ReadAsStringAsync();
            SpotifySearchResponse searchResponse = JsonSerializer.Deserialize<SpotifySearchResponse>(json);
            List<SearchResultItem> results = new List<SearchResultItem>();
            foreach(SpotifyArtistItem aItem in searchResponse.Artists.Items)
            {
                SearchResultItem item = new SearchResultItem();
                item.Type = "artist";
                item.Id = aItem.Id;
                item.Name = aItem.Name;
                item.ImageUrl = aItem.Images != null && aItem.Images.Count > 0 ? aItem.Images[0].Url : "";
                item.Subtitle = null;
                item.ArtistId = aItem.Id;
                results.Add(item);
            }
            foreach (SpotifyAlbumItem aItem in searchResponse.Albums.Items)
            {
                SearchResultItem item = new SearchResultItem();
                item.Type = "album";
                item.Id = aItem.Id;
                item.Name = aItem.Name;
                item.ImageUrl = aItem.Images != null && aItem.Images.Count > 0 ? aItem.Images[0].Url : "";
                item.Subtitle = aItem.Artists != null && aItem.Artists.Count > 0 ? aItem.Artists[0].Name : "";
                item.ArtistId = aItem.Artists != null && aItem.Artists.Count > 0 ? aItem.Artists[0].Id : "";
                results.Add(item);
            }
            foreach (SpotifyTrackItem tItem in searchResponse.Tracks.Items)
            {
                SearchResultItem item = new SearchResultItem();
                item.Type = "track";
                item.Id = tItem.Id;
                item.Name = tItem.Name;
                item.ImageUrl = tItem.Album != null && tItem.Album.Images != null && tItem.Album.Images.Count > 0 ? tItem.Album.Images[0].Url : "";
                string artistName = tItem.Artists != null && tItem.Artists.Count > 0 ? tItem.Artists[0].Name : "";
                string albumName = tItem.Album != null ? tItem.Album.Name : "";
                item.Subtitle = $"par {artistName} · {albumName}";
                item.ArtistId = tItem.Artists != null && tItem.Artists.Count > 0 ? tItem.Artists[0].Id : "";
                results.Add(item);
            }
            return results;
        }
    }
}

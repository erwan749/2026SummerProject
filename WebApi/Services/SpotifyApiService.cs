using Entities;
using Microsoft.JSInterop.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using WebApi.Dtos;
using WebApi.Dtos.Deezer;
using WebApi.Dtos.Spotify;


namespace WebApi.Services
{
    public class SpotifyApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SpotifyAuthService _spotifyAuthService;
        private readonly ArtistManager _artistManager;
        private readonly Dictionary<string, string> _nextAlbumsUrlByArtist = new Dictionary<string, string>();

        public SpotifyApiService(IHttpClientFactory httpClientFactory, SpotifyAuthService spotifyAuthService, ArtistManager artistManager)
        {
            _httpClientFactory = httpClientFactory;
            _spotifyAuthService = spotifyAuthService;
            _artistManager = artistManager;

        }

        public async Task<List<SearchResultItem>> SearchAsync(string query)
        {
            string token = await _spotifyAuthService.GetAccessTokenAsync();
            string url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=artist,album,track";
            HttpClient client = _httpClientFactory.CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var reponse = await client.SendAsync(request);
            string json = await reponse.Content.ReadAsStringAsync();
            SpotifySearchResponse searchResponse = JsonSerializer.Deserialize<SpotifySearchResponse>(json);
            List<SearchResultItem> results = new List<SearchResultItem>();
            foreach (SpotifyArtistItem aItem in searchResponse.Artists.Items)
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
                item.AlbumId = tItem?.Album != null ? tItem?.Album.Id : "";
                results.Add(item);
            }
            return results;
        }
        public async Task<ArtistDetailDto> GetArtistDetailAsync(string artistId)
        {
            Entities.Artist existingArtist = _artistManager.Artists.FirstOrDefault(a => a.ExternalId == artistId);

            if (existingArtist != null)
            {
                bool hasMore = _nextAlbumsUrlByArtist.ContainsKey(artistId) && _nextAlbumsUrlByArtist[artistId] != null;
                return MapToDto(existingArtist, hasMore);
            }

            string token = await _spotifyAuthService.GetAccessTokenAsync();
            HttpClient client = _httpClientFactory.CreateClient();
            string artistUrl = $"https://api.spotify.com/v1/artists/{artistId}";
            HttpRequestMessage artistRequest = new HttpRequestMessage(HttpMethod.Get, artistUrl);
            artistRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var artistResponse = await client.SendAsync(artistRequest);

            if (artistResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                string retryAfter = artistResponse.Headers.RetryAfter?.ToString() ?? "inconnu";
                throw new HttpRequestException($"Spotify a renvoyé 429 Too Many Requests. Retry-After : {retryAfter}");
            }

            artistResponse.EnsureSuccessStatusCode();
            string artistJson = await artistResponse.Content.ReadAsStringAsync();
            SpotifyArtistItem artistDto = JsonSerializer.Deserialize<SpotifyArtistItem>(artistJson);
            Entities.Artist artist = new Entities.Artist();
            artist.ExternalId = artistDto.Id;
            artist.Name = artistDto.Name;
            artist.PictureXl = artistDto.Images != null && artistDto.Images.Count > 0 ? artistDto.Images[0].Url : "";
            artist.PictureBig = artistDto.Images != null && artistDto.Images.Count > 1 ? artistDto.Images[1].Url : "";
            artist.PictureMedium = artistDto.Images != null && artistDto.Images.Count > 2 ? artistDto.Images[2].Url : "";
            _artistManager.Add(artist);

            string albumUrl = $"https://api.spotify.com/v1/artists/{artistId}/albums";
            HttpRequestMessage albumsRequest = new HttpRequestMessage(HttpMethod.Get, albumUrl);
            albumsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var albumsResponse = await client.SendAsync(albumsRequest);

            if (albumsResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                string retryAfter = albumsResponse.Headers.RetryAfter?.ToString() ?? "inconnu";
                throw new HttpRequestException($"Spotify a renvoyé 429 Too Many Requests. Retry-After : {retryAfter}");
            }

            albumsResponse.EnsureSuccessStatusCode();
            string albumsJson = await albumsResponse.Content.ReadAsStringAsync();
            SpotifyAlbumsSection albumsSection = JsonSerializer.Deserialize<SpotifyAlbumsSection>(albumsJson);

            foreach (SpotifyAlbumItem albumItem in albumsSection.Items)
            {
                Entities.Album album = new Entities.Album();
                album.Id = albumItem.Id;
                album.Title = albumItem.Name;
                album.Cover = albumItem.Images != null && albumItem.Images.Count > 0 ? albumItem.Images[0].Url : "";
                _artistManager.AddAlbum(artistId, album);
            }

            _nextAlbumsUrlByArtist[artistId] = albumsSection.Next;
            return MapToDto(artist, albumsSection.Next != null);
        }
        public async Task<AlbumDetailDto> GetAlbumDetailAsync(string artistId, string albumId)
        {
            await GetArtistDetailAsync(artistId);
            Entities.Artist artist = _artistManager.Artists.FirstOrDefault(a => a.ExternalId == artistId);
            Entities.Album album = artist.Albums.FirstOrDefault(al => al.Id == albumId);
            if (album.Tracks.Count == 0) 
            {
                HttpClient client = _httpClientFactory.CreateClient();
                string token = await _spotifyAuthService.GetAccessTokenAsync();
                string nextTracksUrl = $"https://api.spotify.com/v1/albums/{albumId}/tracks";
                while (nextTracksUrl != null)
                {
                    HttpRequestMessage tracksRequest = new HttpRequestMessage(HttpMethod.Get, nextTracksUrl);
                    tracksRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var tracksReponse = await client.SendAsync(tracksRequest);
                    string tracksJson = await tracksReponse.Content.ReadAsStringAsync();
                    SpotifyTracksSection tracksSection = JsonSerializer.Deserialize<SpotifyTracksSection>(tracksJson);

                    foreach (SpotifyTrackItem trackItem in tracksSection.Items)
                    {
                        Entities.Track track = new Entities.Track();
                        track.Id = trackItem.Id;
                        track.Title = trackItem.Name;
                        track.TrackPosition = trackItem.TrackNumber;
                        track.Preview = trackItem.PreviewUrl;
                        track.Duration = trackItem.DurationMs / 1000;
                        _artistManager.AddTrack(artistId,albumId,track);
                    }
                    nextTracksUrl = tracksSection.Next;
                }
            }
            return MapAlbumToDto(album);
        }

        private ArtistDetailDto MapToDto(Entities.Artist artist, bool hasMoreAlbums)
        {
            var dto = new ArtistDetailDto();
            dto.Id = artist.ExternalId;
            dto.Name = artist.Name;
            dto.ImageUrl = artist.PictureXl;
            dto.Albums = new List<AlbumSummaryDto>();
            foreach (Album album in artist.Albums)
            {
                AlbumSummaryDto albumDto = new AlbumSummaryDto();
                albumDto.Id = album.Id;
                albumDto.Name = album.Title;
                albumDto.ImageUrl = album.Cover;
                dto.Albums.Add(albumDto);
            }
            dto.HasMoreAlbums = hasMoreAlbums;
            return dto;
        }
        private AlbumDetailDto MapAlbumToDto(Album album) 
        { 
            AlbumDetailDto dto = new AlbumDetailDto();
            dto.Id = album.Id;
            dto.Name = album.Title;
            dto.ImageUrl = album.Cover;
            dto.ArtistId = album.Artist.ExternalId;
            dto.ArtistName = album.Artist.Name;
            dto.Tracks = new List<TrackSummaryDto>();
            foreach (Track track in album.Tracks)
            {
                TrackSummaryDto trackDto = new TrackSummaryDto();
                trackDto.Id = track.Id;
                trackDto.Title = track.Title;
                trackDto.Duration = track.Duration;
                trackDto.TrackPosition = track.TrackPosition;
                trackDto.PreviewUrl = track.Preview;
                dto.Tracks.Add(trackDto);
            }
            return dto;
        }
        public async Task<MoreAlbumsDto> GetMoreAlbumsAsync(string artistId)
        {
            if (!_nextAlbumsUrlByArtist.ContainsKey(artistId) || _nextAlbumsUrlByArtist[artistId] == null)
            {
                return new MoreAlbumsDto { Albums = new List<AlbumSummaryDto>(), HasMoreAlbums = false };
            }

            string token = await _spotifyAuthService.GetAccessTokenAsync();
            HttpClient client = _httpClientFactory.CreateClient();
            string url = _nextAlbumsUrlByArtist[artistId];

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                string retryAfter = response.Headers.RetryAfter?.ToString() ?? "inconnu";
                throw new HttpRequestException($"Spotify a renvoyé 429 Too Many Requests. Retry-After : {retryAfter}");
            }
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            SpotifyAlbumsSection albumsSection = JsonSerializer.Deserialize<SpotifyAlbumsSection>(json);

            MoreAlbumsDto result = new MoreAlbumsDto();
            result.Albums = new List<AlbumSummaryDto>();
            foreach (SpotifyAlbumItem albumItem in albumsSection.Items)
            {
                Entities.Album album = new Entities.Album();
                album.Id = albumItem.Id;
                album.Title = albumItem.Name;
                album.Cover = albumItem.Images != null && albumItem.Images.Count > 0 ? albumItem.Images[0].Url : "";
                _artistManager.AddAlbum(artistId, album);

                AlbumSummaryDto summary = new AlbumSummaryDto();
                summary.Id = album.Id;
                summary.Name = album.Title;
                summary.ImageUrl = album.Cover;
                result.Albums.Add(summary);
            }

            _nextAlbumsUrlByArtist[artistId] = albumsSection.Next;
            result.HasMoreAlbums = albumsSection.Next != null;

            return result;
        }
        public async Task<TrackDetailDto> GetTrackDetailAsync(string artistId, string albumId, string trackId)
        {
            Entities.Artist artist = _artistManager.Artists.FirstOrDefault(a => a.ExternalId == artistId);
            Entities.Album album = artist.Albums.FirstOrDefault(al => al.Id == albumId);
            Entities.Track track = album.Tracks.FirstOrDefault(t => t.Id == trackId);

            if (string.IsNullOrEmpty(track.Preview))
            {
                string deezerQuery = Uri.EscapeDataString($"{track.Title} {artist.Name}");
                string deezerUrl = $"https://api.deezer.com/search?q={deezerQuery}";
                HttpClient client = _httpClientFactory.CreateClient();
                var deezerResponse = await client.GetAsync(deezerUrl);
                deezerResponse.EnsureSuccessStatusCode();
                string deezerJson = await deezerResponse.Content.ReadAsStringAsync();
                DeezerSearchResponse deezerResult = JsonSerializer.Deserialize<DeezerSearchResponse>(deezerJson);

                if (deezerResult.Data != null && deezerResult.Data.Count > 0)
                {
                    track.Preview = deezerResult.Data[0].Preview;
                }
            }

            TrackDetailDto dto = new TrackDetailDto();
            dto.Id = track.Id;
            dto.Title = track.Title;
            dto.AlbumImageUrl = album.Cover;
            dto.AlbumId = album.Id;
            dto.AlbumName = album.Title;
            dto.ArtistId = artist.ExternalId;
            dto.ArtistName = artist.Name;
            dto.PreviewUrl = track.Preview;
            return dto;
        }
    }
}

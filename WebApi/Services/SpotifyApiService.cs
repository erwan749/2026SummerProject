using Entities;
using Microsoft.EntityFrameworkCore;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApi.Data;
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
        private readonly BlindTestDbContext _dbContext;
        private readonly AlbumPaginationCache _paginationCache;

        private readonly Dictionary<string, string> _nextAlbumsUrlByArtist = new Dictionary<string, string>();


        public SpotifyApiService(IHttpClientFactory httpClientFactory, SpotifyAuthService spotifyAuthService, ArtistManager artistManager, BlindTestDbContext blindTestDbContext, AlbumPaginationCache paginationCache)
        {
            _httpClientFactory = httpClientFactory;
            _spotifyAuthService = spotifyAuthService;
            _artistManager = artistManager;
            _dbContext = blindTestDbContext;
            _paginationCache = paginationCache;
        }
        private async Task<Entities.Artist> EnsureArtistExistsAsync(string artistId)
        {
            Entities.Artist artist = await _dbContext.Artists.FirstOrDefaultAsync(a => a.ExternalId == artistId);
            if (artist != null)
            {
                if (!_artistManager.Artists.Any(a => a.ExternalId == artistId))
                {
                    _artistManager.Add(artist);
                }
                return artist;
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

            artist = new Entities.Artist();
            artist.ExternalId = artistDto.Id;
            artist.Name = artistDto.Name;
            artist.PictureXl = artistDto.Images != null && artistDto.Images.Count > 0 ? artistDto.Images[0].Url : "";
            artist.PictureBig = artistDto.Images != null && artistDto.Images.Count > 1 ? artistDto.Images[1].Url : "";
            artist.PictureMedium = artistDto.Images != null && artistDto.Images.Count > 2 ? artistDto.Images[2].Url : "";
            artist.PictureSmall = artistDto.Images != null && artistDto.Images.Count > 3 ? artistDto.Images[3].Url : "";

            _artistManager.Add(artist);
            _dbContext.Artists.Add(artist);
            await _dbContext.SaveChangesAsync();
            return artist;
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
                results.Add(MapTrackItemToSearchResult(tItem));
            }
            return results;
        }
        public async Task<ArtistDetailDto> GetArtistDetailAsync(string artistId)
        {
            Entities.Artist artist = await EnsureArtistExistsAsync(artistId);

            string token = await _spotifyAuthService.GetAccessTokenAsync();
            HttpClient client = _httpClientFactory.CreateClient();
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

            List<AlbumSummaryDto> albumDtos = new List<AlbumSummaryDto>();
            foreach (SpotifyAlbumItem albumItem in albumsSection.Items)
            {
                AlbumSummaryDto summary = new AlbumSummaryDto();
                summary.Id = albumItem.Id;
                summary.Name = albumItem.Name;
                summary.ImageUrl = albumItem.Images != null && albumItem.Images.Count > 0 ? albumItem.Images[0].Url : "";
                albumDtos.Add(summary);
            }

            _paginationCache.SetNextUrl(artistId, albumsSection.Next);

            ArtistDetailDto dto = new ArtistDetailDto();
            dto.Id = artist.ExternalId;
            dto.Name = artist.Name;
            dto.ImageUrl = artist.PictureXl;
            dto.Albums = albumDtos;
            dto.HasMoreAlbums = albumsSection.Next != null;
            return dto;
        }
        public async Task<AlbumDetailDto> GetAlbumDetailAsync(string artistId, string albumId)
        {
            Entities.Artist artist = await EnsureArtistExistsAsync(artistId);
            Entities.Album album = await _dbContext.Albums.Include(al => al.Tracks).Include(al => al.Artist).FirstOrDefaultAsync(al => al.Id == albumId);
            if (album == null)
            {
                string token2 = await _spotifyAuthService.GetAccessTokenAsync();
                HttpClient client2 = _httpClientFactory.CreateClient();
                string albumUrl = $"https://api.spotify.com/v1/albums/{albumId}";
                HttpRequestMessage albumRequest = new HttpRequestMessage(HttpMethod.Get, albumUrl);
                albumRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token2);
                var albumResponse = await client2.SendAsync(albumRequest);
                if (albumResponse.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    string retryAfter = albumResponse.Headers.RetryAfter?.ToString() ?? "inconnu";
                    throw new HttpRequestException($"Spotify a renvoyé 429 Too Many Requests. Retry-After : {retryAfter}");
                }
                albumResponse.EnsureSuccessStatusCode();
                string albumJson = await albumResponse.Content.ReadAsStringAsync();
                SpotifyAlbumItem albumItem = JsonSerializer.Deserialize<SpotifyAlbumItem>(albumJson);

                album = new Entities.Album();
                album.Id = albumItem.Id;
                album.Title = albumItem.Name;
                album.Cover = albumItem.Images != null && albumItem.Images.Count > 0 ? albumItem.Images[0].Url : "";
                _artistManager.AddAlbum(artistId, album);
                _dbContext.Albums.Add(album);
                await _dbContext.SaveChangesAsync();
            }
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
                        track.Preview = trackItem.PreviewUrl ?? "";
                        track.Duration = trackItem.DurationMs / 1000;
                        if (string.IsNullOrEmpty(track.Preview))
                        {
                            string deezerQuery = Uri.EscapeDataString($"{track.Title} {artist.Name}");
                            string deezerUrl = $"https://api.deezer.com/search?q={deezerQuery}";
                            HttpClient deezerClient = _httpClientFactory.CreateClient();
                            var deezerResponse = await deezerClient.GetAsync(deezerUrl);
                            if (deezerResponse.IsSuccessStatusCode)
                            {
                                string deezerJson = await deezerResponse.Content.ReadAsStringAsync();
                                DeezerSearchResponse deezerResult = JsonSerializer.Deserialize<DeezerSearchResponse>(deezerJson);
                                if (deezerResult.Data != null && deezerResult.Data.Count > 0)
                                {
                                    track.Preview = deezerResult.Data[0].Preview ?? "";
                                }
                            }
                        }
                        _artistManager.AddTrack(artistId,albumId,track);
                        _dbContext.Tracks.Add(track);
                    }
                    nextTracksUrl = tracksSection.Next;
                }
                await _dbContext.SaveChangesAsync();
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
            if (!_paginationCache.HasMore(artistId))
            {
                return new MoreAlbumsDto { Albums = new List<AlbumSummaryDto>(), HasMoreAlbums = false };
            }
            string token = await _spotifyAuthService.GetAccessTokenAsync();
            HttpClient client = _httpClientFactory.CreateClient();
            string url = _paginationCache.GetNextUrl(artistId);
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
                AlbumSummaryDto summary = new AlbumSummaryDto();
                summary.Id = albumItem.Id;
                summary.Name = albumItem.Name;
                summary.ImageUrl = albumItem.Images != null && albumItem.Images.Count > 0 ? albumItem.Images[0].Url : "";
                result.Albums.Add(summary);
            }

            _paginationCache.SetNextUrl(artistId, albumsSection.Next);
            result.HasMoreAlbums = albumsSection.Next != null;
            return result;
        }
        public async Task<TrackDetailDto> GetTrackDetailAsync(string artistId, string albumId, string trackId)
        {
            await GetAlbumDetailAsync(artistId, albumId);
            Entities.Track track = await _dbContext.Tracks.Include(t => t.Album).ThenInclude(a => a.Artist).FirstOrDefaultAsync(t => t.Id == trackId);
            if (track == null) throw new Exception("Track not found");

            await RefreshPreviewIfNeededAsync(track);

            TrackDetailDto dto = new TrackDetailDto();
            dto.Id = track.Id;
            dto.Title = track.Title;
            dto.AlbumImageUrl = track.Album.Cover;
            dto.AlbumId = track.Album.Id;
            dto.AlbumName = track.Album.Title;
            dto.ArtistId = track.Album.Artist.ExternalId;
            dto.ArtistName = track.Album.Artist.Name;
            dto.PreviewUrl = track.Preview;
            await _dbContext.SaveChangesAsync();
            return dto;
        }
        private SearchResultItem MapTrackItemToSearchResult(SpotifyTrackItem tItem)
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
            return item;
        }
        public async Task<List<SearchResultItem>> SearchTracksOnlyAsync(string query) 
        {

            string token = await _spotifyAuthService.GetAccessTokenAsync();
            string url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=track";
            HttpClient client = _httpClientFactory.CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var reponse = await client.SendAsync(request);
            string json = await reponse.Content.ReadAsStringAsync();
            SpotifySearchResponse searchResponse = JsonSerializer.Deserialize<SpotifySearchResponse>(json);
            List<SearchResultItem> results = new List<SearchResultItem>();
            foreach(SpotifyTrackItem tItem in searchResponse.Tracks.Items)
            {
                results.Add(MapTrackItemToSearchResult(tItem));
            }
            return results;
        }
        public bool IsPreviewExpired(string previewUrl)
        {
            if (string.IsNullOrEmpty(previewUrl)) return true;

            Match match = Regex.Match(previewUrl, @"exp=(\d+)");
            if (!match.Success) return false;

            long expTimestamp = long.Parse(match.Groups[1].Value);
            DateTimeOffset expDate = DateTimeOffset.FromUnixTimeSeconds(expTimestamp);
            return expDate <= DateTimeOffset.UtcNow.AddMinutes(5);
        }
        public async Task RefreshPreviewIfNeededAsync(Track track)
        {
            if (!IsPreviewExpired(track.Preview)) return;

            string deezerQuery = Uri.EscapeDataString($"{track.Title} {track.Album.Artist.Name}");
            string deezerUrl = $"https://api.deezer.com/search?q={deezerQuery}";
            HttpClient client = _httpClientFactory.CreateClient();
            var deezerResponse = await client.GetAsync(deezerUrl);

            if (deezerResponse.IsSuccessStatusCode)
            {
                string deezerJson = await deezerResponse.Content.ReadAsStringAsync();
                DeezerSearchResponse deezerResult = JsonSerializer.Deserialize<DeezerSearchResponse>(deezerJson);
                if (deezerResult.Data != null && deezerResult.Data.Count > 0)
                {
                    track.Preview = deezerResult.Data[0].Preview ?? "";
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
    }
}

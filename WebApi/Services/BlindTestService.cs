using WebApi.Data;
using System;
using System.Threading.Tasks;
using WebApi.Dtos;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Services
{
    public class BlindTestService
    {
        private readonly BlindTestDbContext _blindTestDbContext;
        private readonly SpotifyApiService _spotifyApiService;

        public BlindTestService(BlindTestDbContext blindTestDbContext, SpotifyApiService spotifyApiService)
        {
            _blindTestDbContext = blindTestDbContext;
            _spotifyApiService = spotifyApiService;
        }

        public async Task<Guid> CreateBlindTestAsync(CreateBlindTestDto dto)
        {
            BlindTest blindTest = new BlindTest();
            blindTest.Name = dto.Name;
            blindTest.Category = dto.Category;

            foreach (TrackSelectionDto track in dto.Tracks) 
            {
                await _spotifyApiService.GetTrackDetailAsync(track.ArtistId, track.AlbumId, track.TrackId);
                Track selectedTrack = await _blindTestDbContext.Tracks.FirstOrDefaultAsync(t => t.Id == track.TrackId);
                if (selectedTrack != null)
                {
                    blindTest.Tracks.Add(selectedTrack);
                }
            }
            _blindTestDbContext.BlindTests.Add(blindTest);
            await _blindTestDbContext.SaveChangesAsync();
            return blindTest.Id;
        }
    }
}

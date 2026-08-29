using WebApi.Data;
using System;
using System.Threading.Tasks;
using WebApi.Dtos;
using Entities;
using System.Collections.Generic;
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
        public async Task<List<BlindTestSummaryDto>> GetAllBlindTestsAsync()
        {
            List<BlindTest> blindTests = await _blindTestDbContext.BlindTests.ToListAsync();
            List<BlindTestSummaryDto> results = new List<BlindTestSummaryDto>();
            foreach (BlindTest blindTest in blindTests)
            {
                BlindTestSummaryDto dto = new BlindTestSummaryDto();
                dto.Id = blindTest.Id;
                dto.Name = blindTest.Name;
                dto.Category = blindTest.Category;
                results.Add(dto);
            }
            return results;
        }
        public async Task<Guid> CreateBlindTestAsync(CreateBlindTestDto dto)
        {
            bool isValidKey = await _blindTestDbContext.AdminKeys.AnyAsync(k => k.Key == dto.AdminKey);
            if (!isValidKey) throw new UnauthorizedAccessException("Invalid admin key");
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

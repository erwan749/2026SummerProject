using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using WebApi.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlindTestsController : ControllerBase
    {

        private readonly SpotifyApiService _spotifyApiService;
        private readonly BlindTestService _blindTestService;

        public BlindTestsController(SpotifyApiService spotifyApiService, BlindTestService blindTestService)
        {
            _spotifyApiService = spotifyApiService;
            _blindTestService = blindTestService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<BlindTestSummaryDto> results = await _blindTestService.GetAllBlindTestsAsync();
            return Ok(results);
        }

        [HttpGet("search-tracks")]
        public async Task<IActionResult> SearchTracks([FromQuery(Name = "q")] string query)
        {
            List<SearchResultItem> resultItems = await _spotifyApiService.SearchTracksOnlyAsync(query);
            return Ok(resultItems);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBlindTestDto dto)
        {
            try
            {
                Guid id = await _blindTestService.CreateBlindTestAsync(dto);
                return Ok(id);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Clé admin invalide.");
            }
        }
    }
}

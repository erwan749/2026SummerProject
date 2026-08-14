using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using WebApi.Services;
using System.Threading.Tasks;
namespace WebApi.Controllers
{
    [Route("api/artists/{artistId}/albums/{albumId}/tracks")]
    [ApiController]
    public class TracksController : ControllerBase
    {
        private readonly SpotifyApiService _spotifyApiService;
        public TracksController(SpotifyApiService spotifyApiService)
        {
            _spotifyApiService = spotifyApiService;
        }
        [HttpGet("{trackId}")]
        public async Task<IActionResult> GetTrackDetail(string artistId, string albumId, string trackId)
        {
            TrackDetailDto result = await _spotifyApiService.GetTrackDetailAsync(artistId, albumId, trackId);
            return Ok(result);
        }
    }
}
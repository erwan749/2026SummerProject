using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using WebApi.Services;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/artists/{artistId}/albums")]
    [ApiController]
    public class AlbumsController : ControllerBase
    {
        private readonly SpotifyApiService _spotifyApiService;

        public AlbumsController(SpotifyApiService spotifyApiService)
        {
            _spotifyApiService = spotifyApiService;
        }
        [HttpGet("{albumId}")]
        public async Task<IActionResult> GetAlbumDetail(string artistId , string albumId)
        {
            AlbumDetailDto resultItems = await _spotifyApiService.GetAlbumDetailAsync(artistId, albumId);
            return Ok(resultItems);
        }
    }
}

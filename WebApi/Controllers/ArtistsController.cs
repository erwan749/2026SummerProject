using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using WebApi.Services;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistsController : ControllerBase
    {
        private readonly SpotifyApiService _spotifyApiService;

        public ArtistsController(SpotifyApiService spotifyApiService)
        {
            _spotifyApiService = spotifyApiService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Artists(string id)
        {
            ArtistDetailDto resultItems = await _spotifyApiService.GetArtistDetailAsync(id);
            return Ok(resultItems);
        }
        [HttpGet("{id}/albums")]
        public async Task<IActionResult> GetMoreAlbums(string id)
        {
            MoreAlbumsDto result = await _spotifyApiService.GetMoreAlbumsAsync(id);
            return Ok(result);
        }
    }
}

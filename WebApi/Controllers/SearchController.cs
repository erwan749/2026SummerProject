using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebApi.Dtos;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly SpotifyApiService _spotifyApiService ;
       
        public SearchController (SpotifyApiService spotifyApiService)
        {
            _spotifyApiService = spotifyApiService ;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery(Name = "q")] string query)
        {
            List<SearchResultItem> resultItems = await _spotifyApiService.SearchAsync(query);
            return Ok(resultItems);
        }
    }
}

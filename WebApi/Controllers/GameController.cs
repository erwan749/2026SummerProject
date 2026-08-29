using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using System.Threading.Tasks;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {


        private readonly GameSessionService _gameSessionService;

        public GameController(GameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartGameDto dto)
        {
           GameQuestionDto results = await _gameSessionService.StartGameAsync(dto);
            return Ok(results);
        }

        [HttpPost("answer")]
        public async Task<IActionResult> Answer([FromBody] AnswerDto dto)
        {
           AnswerResultDto answerResultDto = await _gameSessionService.SubmitAnswerAsync(dto);
            return Ok(answerResultDto);
        }

        [HttpGet("{sessionId}/current")]
        public async Task<IActionResult> GetCurrent(Guid sessionId)
        {
            GameQuestionDto result = await _gameSessionService.GetCurrentQuestionAsync(sessionId);
            return Ok(result);
        }
    }
}

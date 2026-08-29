using System;
namespace WebApi.Dtos
{
    public class AnswerResultDto
    {
        public bool IsCorrect { get; set; }
        public string CorrectAnswer { get; set; }
        public TrackDetailDto Track { get; set; }
        public bool IsGameOver { get; set; }
    }
}

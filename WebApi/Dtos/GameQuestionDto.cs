using System;

namespace WebApi.Dtos
{
    public class GameQuestionDto
    {

        public Guid SessionId { get; set; }
        public string QuestionType { get; set; }
        public string PreviewUrl { get; set; }
        public int QuestionNumber { get; set; }
        public int TotalQuestions { get; set; }

    }
}

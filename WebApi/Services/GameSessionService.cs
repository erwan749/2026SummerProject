using Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Dtos;
using WebApi.Dtos.Deezer;
using WebApi.Extensions;

namespace WebApi.Services
{
    public class GameSessionService
    {
        private readonly BlindTestDbContext _blindTestDbContext;
        private readonly SpotifyApiService _spotifyApiService;
        private static Random rng = new Random();

        public GameSessionService(BlindTestDbContext blindTestDbContext, SpotifyApiService spotifyApiService)
        {
            _blindTestDbContext = blindTestDbContext;
            _spotifyApiService = spotifyApiService;
        }

        public async Task<GameQuestionDto> StartGameAsync(StartGameDto dto)
        {
            int numberTrack = 10;
            BlindTest blindTest = await _blindTestDbContext.BlindTests
                .Include(bt => bt.Tracks)
                .ThenInclude(t => t.Album)
                .ThenInclude(a => a.Artist)
                .FirstOrDefaultAsync(bt => bt.Id == dto.BlindTestId);

            if (blindTest == null) throw new Exception("Blind test not found");
            if (blindTest.Tracks.Count == 0) throw new Exception("Blind test has no tracks");
            if (blindTest.Tracks.Count < numberTrack) numberTrack = blindTest.Tracks.Count;
            blindTest.Tracks.Shuffle();
            List<Track> selectedTrack = blindTest.Tracks.Take(numberTrack).ToList();
            GameSession gameSession = new GameSession();
            gameSession.BlindTestId = dto.BlindTestId;
            for (int i = 0; i < numberTrack; i++)
            {
                GameSessionQuestion question = new GameSessionQuestion();
                question.GameSessionId = gameSession.Id;
                question.TrackId = selectedTrack[i].Id;
                question.Track = selectedTrack[i];
                question.Order = i;
                question.QuestionType = rng.Next(2) != 0 ? "artist" : "title";
                gameSession.Questions.Add(question);
            }
            _blindTestDbContext.GameSessions.Add(gameSession);
            await _blindTestDbContext.SaveChangesAsync();
            return await BuildQuestionDto(gameSession, gameSession.Questions[0]);
        }
        public async Task<AnswerResultDto> SubmitAnswerAsync(AnswerDto dto)
        {
            GameSession gameSession = await LoadSessionWithCurrentQuestionAsync(dto.SessionId);
            GameSessionQuestion currentQuestion =  gameSession.Questions.FirstOrDefault(q => q.Order == gameSession.CurrentPosition);
            if (currentQuestion == null) throw new Exception("Question not found");
            string answer = currentQuestion.QuestionType == "artist" ? currentQuestion.Track.Album.Artist.Name : currentQuestion.Track.Title;
            string userAnswer = dto.Answer.Trim();
            bool isCorrect = string.Equals(userAnswer, answer, StringComparison.OrdinalIgnoreCase);
            gameSession.CurrentPosition++;
            bool isGameOver = gameSession.CurrentPosition >= gameSession.Questions.Count;
            if ( isGameOver) _blindTestDbContext.GameSessions.Remove(gameSession);
            await _blindTestDbContext.SaveChangesAsync();

            TrackDetailDto trackDetailDto = new TrackDetailDto();
            trackDetailDto.Id = currentQuestion.Track.Id;
            trackDetailDto.Title = currentQuestion.Track.Title;
            trackDetailDto.AlbumImageUrl = currentQuestion.Track.Album.Cover;
            trackDetailDto.AlbumId = currentQuestion.Track.Album.Id;
            trackDetailDto.AlbumName = currentQuestion.Track.Album.Title;
            trackDetailDto.ArtistName = currentQuestion.Track.Album.Artist.Name;
            trackDetailDto.ArtistId = currentQuestion.Track.Album.Artist.ExternalId;
            trackDetailDto.PreviewUrl = currentQuestion.Track.Preview;

            AnswerResultDto answerResultDto = new AnswerResultDto();
            answerResultDto.IsCorrect = isCorrect;
            answerResultDto.CorrectAnswer = answer;
            answerResultDto.Track = trackDetailDto;
            answerResultDto.IsGameOver = isGameOver;

            return answerResultDto;
        }
        private async Task<GameSession> LoadSessionWithCurrentQuestionAsync(Guid sessionId)
        {
            GameSession gameSession = await _blindTestDbContext.GameSessions
                .Include(gs => gs.Questions)
                .ThenInclude(q => q.Track)
                .ThenInclude(t => t.Album)
                .ThenInclude(a => a.Artist)
                .FirstOrDefaultAsync(gs => gs.Id == sessionId);

            if (gameSession == null) throw new Exception("Session not found");

            return gameSession;
        }
        private async Task<GameQuestionDto> BuildQuestionDto(GameSession gameSession, GameSessionQuestion question)
        {
            await _spotifyApiService.RefreshPreviewIfNeededAsync(question.Track);

            GameQuestionDto dto = new GameQuestionDto();
            dto.SessionId = gameSession.Id;
            dto.QuestionType = question.QuestionType;
            dto.PreviewUrl = question.Track.Preview;
            dto.QuestionNumber = question.Order + 1;
            dto.TotalQuestions = gameSession.Questions.Count;
            return dto;
        }
        public async Task<GameQuestionDto> GetCurrentQuestionAsync(Guid sessionId)
        {
            GameSession gameSession = await LoadSessionWithCurrentQuestionAsync(sessionId);
            GameSessionQuestion currentQuestion = gameSession.Questions.FirstOrDefault(q => q.Order == gameSession.CurrentPosition);
            if (currentQuestion == null) throw new Exception("Question not found");
            return await BuildQuestionDto(gameSession, currentQuestion);
        }
    }
}

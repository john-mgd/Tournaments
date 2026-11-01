using Microsoft.EntityFrameworkCore;
using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Matches;
using TournamentHub.Application.Interfaces;
using TournamentHub.Domain.Entities;
using TournamentHub.Domain.Enums;
using TournamentHub.Data;

namespace TournamentHub.Application.Services
{
    public class EventService : IEventService
    {
        private readonly ApplicationDbContext  _context;

        public EventService(ApplicationDbContext  context) => _context = context;

        public async Task<Result<MatchEventDto>> AddEventAsync(Guid matchId, CreateEventDto dto, Guid judgeId)
        {
            var match = await _context.Matches
                .Include(m => m.Protocol)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null) return Result<MatchEventDto>.Failure("Матч не найден", ResultErrorType.NotFound);
            if (match.JudgeId != judgeId)
                return Result<MatchEventDto>.Failure("Вы не назначены судьей на этот матч", ResultErrorType.Forbidden);
            if (match.Protocol?.IsConfirmed == true)
                return Result<MatchEventDto>.Failure("Протокол уже подтвержден, редактирование запрещено");

            var evt = new MatchEvent
            {
                Id = Guid.NewGuid(),
                MatchId = matchId,
                Type = dto.Type,
                Minute = dto.Minute,
                PlayerId = dto.PlayerId,
                RelatedPlayerId = dto.RelatedPlayerId
            };

            _context.MatchEvents.Add(evt);
            await _context.SaveChangesAsync();

            // Загружаем связанных игроков для ответа
            var playerName = dto.PlayerId.HasValue 
                ? (await _context.Players.FindAsync(dto.PlayerId))?.FullName 
                : null;
            var relatedName = dto.RelatedPlayerId.HasValue 
                ? (await _context.Players.FindAsync(dto.RelatedPlayerId))?.FullName 
                : null;

            return Result<MatchEventDto>.Success(new MatchEventDto(
                evt.Id, evt.Type, evt.Minute, playerName, relatedName));
        }

        public async Task<Result<bool>> UpdateEventAsync(Guid eventId, UpdateEventDto dto, Guid judgeId)
        {
            var evt = await _context.MatchEvents
                .Include(e => e.Match).ThenInclude(m => m.Protocol)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (evt == null) return Result<bool>.Failure("Событие не найдено", ResultErrorType.NotFound);
            if (evt.Match.JudgeId != judgeId)
                return Result<bool>.Failure("Нет прав на редактирование", ResultErrorType.Forbidden);
            if (evt.Match.Protocol?.IsConfirmed == true)
                return Result<bool>.Failure("Протокол уже подтвержден");

            evt.Type = dto.Type;
            evt.Minute = dto.Minute;
            evt.PlayerId = dto.PlayerId;
            evt.RelatedPlayerId = dto.RelatedPlayerId;
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteEventAsync(Guid eventId, Guid judgeId)
        {
            var evt = await _context.MatchEvents
                .Include(e => e.Match).ThenInclude(m => m.Protocol)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (evt == null) return Result<bool>.Failure("Событие не найдено", ResultErrorType.NotFound);
            if (evt.Match.JudgeId != judgeId)
                return Result<bool>.Failure("Нет прав на удаление", ResultErrorType.Forbidden);
            if (evt.Match.Protocol?.IsConfirmed == true)
                return Result<bool>.Failure("Протокол уже подтвержден");

            _context.MatchEvents.Remove(evt);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
    }
}
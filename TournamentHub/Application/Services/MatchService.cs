using Microsoft.EntityFrameworkCore;
using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Matches;
using TournamentHub.Application.DTOs.Teams;   // ✅ для TeamDto
using TournamentHub.Application.Interfaces;
using TournamentHub.Data;
using TournamentHub.Domain.Entities;
using TournamentHub.Domain.Enums;

namespace TournamentHub.Application.Services
{
    public class MatchService : IMatchService
    {
        private readonly ApplicationDbContext _context;
        private readonly IStatisticsService _statisticsService;

        public MatchService(ApplicationDbContext context, IStatisticsService statisticsService)
        {
            _context = context;
            _statisticsService = statisticsService;
        }

        // === ЧТЕНИЕ ===

        public async Task<MatchDetailsDto?> GetMatchByIdAsync(Guid id)
        {
            var match = await _context.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Tournament)
                .Include(m => m.Judge)
                .Include(m => m.Events).ThenInclude(e => e.Player)
                .Include(m => m.Events).ThenInclude(e => e.RelatedPlayer)
                .Include(m => m.Lineups).ThenInclude(l => l.Player)
                .Include(m => m.Protocol)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null) return null;

            return new MatchDetailsDto(
                match.Id, match.DateTime, match.Location, match.Status,
                match.HomeScore, match.AwayScore, match.Tournament.Name,
                match.Judge?.FullName,
                MapTeamToDto(match.HomeTeam),
                MapTeamToDto(match.AwayTeam),
                match.Lineups.GroupBy(l => l.TeamId).Select(MapLineupToDto),
                match.Events.Select(MapEventToDto),
                match.Protocol?.IsConfirmed ?? false
            );
        }

        // === ОРГАНИЗАТОР ===

        public async Task<Result<bool>> UpdateMatchAsync(Guid id, UpdateMatchDto dto, Guid currentUserId)
        {
            var match = await _context.Matches
                .Include(m => m.Tournament)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
                return Result<bool>.Failure("Матч не найден", ResultErrorType.NotFound);

            // Проверка прав: только организатор турнира может менять матч
            if (match.Tournament.OrganizerId != currentUserId)
                return Result<bool>.Failure("Нет прав на редактирование матча", ResultErrorType.Forbidden);

            match.DateTime = dto.DateTime;
            match.Location = dto.Location;
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> AssignJudgeAsync(Guid matchId, Guid judgeId)
        {
            var match = await _context.Matches.FindAsync(matchId);
            if (match == null)
                return Result<bool>.Failure("Матч не найден", ResultErrorType.NotFound);

            // Судью нельзя менять после начала матча
            if (match.Status != MatchStatus.Scheduled)
                return Result<bool>.Failure("Нельзя изменить судью после начала матча");

            var judge = await _context.Users.FindAsync(judgeId);
            if (judge == null || judge.Role != UserRole.Judge)
                return Result<bool>.Failure("Пользователь не является судьей");

            match.JudgeId = judgeId;
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> PostponeMatchAsync(Guid matchId, DateTime newDate, Guid currentUserId)
        {
            var match = await _context.Matches
                .Include(m => m.Tournament)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
                return Result<bool>.Failure("Матч не найден", ResultErrorType.NotFound);

            if (match.Tournament.OrganizerId != currentUserId)
                return Result<bool>.Failure("Нет прав", ResultErrorType.Forbidden);

            match.DateTime = newDate;
            match.Status = MatchStatus.Postponed;
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> CancelMatchAsync(Guid matchId, Guid currentUserId)
        {
            var match = await _context.Matches
                .Include(m => m.Tournament)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
                return Result<bool>.Failure("Матч не найден", ResultErrorType.NotFound);

            if (match.Tournament.OrganizerId != currentUserId)
                return Result<bool>.Failure("Нет прав", ResultErrorType.Forbidden);

            match.Status = MatchStatus.Cancelled;
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        // === ПРЕДСТАВИТЕЛЬ КОМАНДЫ ===

        public async Task<Result<bool>> SubmitLineupAsync(Guid matchId, SubmitLineupDto dto, Guid currentUserId)
        {
            var match = await _context.Matches
                .Include(m => m.Tournament)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
                return Result<bool>.Failure("Матч не найден", ResultErrorType.NotFound);

            // Определяем, за какую команду подаёт состав представитель
            // (по условию задачи teamId в DTO нет — берём из текущего пользователя)
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.RepresentativeId == currentUserId
                                       && (t.Id == match.HomeTeamId || t.Id == match.AwayTeamId));

            if (team == null)
                return Result<bool>.Failure("Вы не являетесь представителем команды в этом матче", ResultErrorType.Forbidden);

            // Проверка дедлайна
            if (DateTime.UtcNow > match.Tournament.LineupDeadline)
                return Result<bool>.Failure("Срок подачи состава истек");

            // Удаляем старый состав
            var existing = _context.MatchLineups.Where(l => l.MatchId == matchId && l.TeamId == team.Id);
            _context.MatchLineups.RemoveRange(existing);

            // Добавляем новый
            var allPlayers = dto.StarterPlayerIds.Concat(dto.SubstitutePlayerIds);
            foreach (var playerId in allPlayers)
            {
                _context.MatchLineups.Add(new MatchLineup
                {
                    Id = Guid.NewGuid(),
                    MatchId = matchId,
                    TeamId = team.Id,
                    PlayerId = playerId,
                    IsStarter = dto.StarterPlayerIds.Contains(playerId)
                });
            }

            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        // === СУДЬЯ ===

        public async Task<Result<bool>> ConfirmProtocolAsync(Guid matchId, Guid judgeId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var match = await _context.Matches
                    .Include(m => m.Protocol)
                    .Include(m => m.Events)
                    .Include(m => m.HomeTeam)
                    .Include(m => m.AwayTeam)
                    .Include(m => m.Lineups)
                    .FirstOrDefaultAsync(m => m.Id == matchId);

                if (match == null)
                    return Result<bool>.Failure("Матч не найден", ResultErrorType.NotFound);

                if (match.JudgeId != judgeId)
                    return Result<bool>.Failure("Вы не назначены судьей на этот матч", ResultErrorType.Forbidden);

                if (match.Protocol?.IsConfirmed == true)
                    return Result<bool>.Failure("Протокол уже подтвержден");

                // === Подсчет счета ===
                int homeScore = 0, awayScore = 0;
                var homePlayerIds = match.Lineups
                    .Where(l => l.TeamId == match.HomeTeamId)
                    .Select(l => l.PlayerId)
                    .ToHashSet();
                var awayPlayerIds = match.Lineups
                    .Where(l => l.TeamId == match.AwayTeamId)
                    .Select(l => l.PlayerId)
                    .ToHashSet();

                foreach (var evt in match.Events)
                {
                    bool isHome = evt.PlayerId.HasValue && homePlayerIds.Contains(evt.PlayerId.Value);
                    bool isAway = evt.PlayerId.HasValue && awayPlayerIds.Contains(evt.PlayerId.Value);

                    switch (evt.Type)
                    {
                        case MatchEventType.Goal:
                        case MatchEventType.PenaltyGoal:
                            if (isHome) homeScore++;
                            else if (isAway) awayScore++;
                            break;
                        case MatchEventType.OwnGoal:
                            // Автогол — засчитывается сопернику
                            if (isHome) awayScore++;
                            else if (isAway) homeScore++;
                            break;
                        case MatchEventType.CancelledGoal:
                        case MatchEventType.Assist:
                        case MatchEventType.YellowCard:
                        case MatchEventType.RedCard:
                        case MatchEventType.Substitution:
                            break;
                    }
                }

                match.HomeScore = homeScore;
                match.AwayScore = awayScore;
                match.Status = MatchStatus.Completed;

                if (match.Protocol == null)
                {
                    match.Protocol = new Protocol
                    {
                        Id = Guid.NewGuid(),
                        MatchId = match.Id,
                        IsConfirmed = true,
                        ConfirmedAt = DateTime.UtcNow
                    };
                    _context.Protocols.Add(match.Protocol);
                }
                else
                {
                    match.Protocol.IsConfirmed = true;
                    match.Protocol.ConfirmedAt = DateTime.UtcNow;
                }

                // Пересчет статистики (голы, карточки, турнирная таблица)
                await _statisticsService.RecalculateStatisticsAsync(match);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<bool>.Failure($"Ошибка: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UploadProtocolFileAsync(Guid matchId, Stream fileStream, string fileName, Guid judgeId)
        {
            var match = await _context.Matches
                .Include(m => m.Protocol)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
                return Result<bool>.Failure("Матч не найден", ResultErrorType.NotFound);

            if (match.JudgeId != judgeId)
                return Result<bool>.Failure("Нет прав", ResultErrorType.Forbidden);

            if (match.Protocol == null)
                return Result<bool>.Failure("Протокол не найден", ResultErrorType.NotFound);

            // TODO: сохранить файл (например, в wwwroot/uploads)
            // Здесь для примера только путь
            match.Protocol.OfficialFileUrl = $"/uploads/protocols/{matchId}_{fileName}";
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        // === ПРИВАТНЫЕ МЕТОДЫ МАППИНГА ===

        private static TeamDto MapTeamToDto(Team t)
            => new(t.Id, t.Name, t.LogoUrl, t.City, t.ContactInfo, t.Representative?.FullName ?? "");

        private static MatchLineupDto MapLineupToDto(IGrouping<Guid, MatchLineup> g)
            => new(g.Key, g.FirstOrDefault()?.Match?.HomeTeam?.Name ?? "",
                   g.Select(l => new LineupPlayerDto(l.PlayerId, l.Player.FullName, l.Player.JerseyNumber, l.IsStarter)));

        private static MatchEventDto MapEventToDto(MatchEvent e)
            => new(e.Id, e.Type, e.Minute, e.Player?.FullName, e.RelatedPlayer?.FullName);
    }
}
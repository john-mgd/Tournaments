using Microsoft.EntityFrameworkCore;
using TournamentHub.Application.Interfaces;
using TournamentHub.Domain.Entities;
using TournamentHub.Domain.Enums;
using TournamentHub.Data;

namespace TournamentHub.Application.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext  _context;

        public StatisticsService(ApplicationDbContext  context) => _context = context;

        public async Task RecalculateStatisticsAsync(Match match)
        {
            // 1. Подсчет голов
            int homeGoals = 0, awayGoals = 0;
            var homeTeamPlayerIds = await _context.MatchLineups
                .Where(l => l.MatchId == match.Id && l.TeamId == match.HomeTeamId)
                .Select(l => l.PlayerId).ToListAsync();
            var awayTeamPlayerIds = await _context.MatchLineups
                .Where(l => l.MatchId == match.Id && l.TeamId == match.AwayTeamId)
                .Select(l => l.PlayerId).ToListAsync();

            foreach (var evt in match.Events)
            {
                var isHomePlayer = evt.PlayerId.HasValue && homeTeamPlayerIds.Contains(evt.PlayerId.Value);
                var isAwayPlayer = evt.PlayerId.HasValue && awayTeamPlayerIds.Contains(evt.PlayerId.Value);

                switch (evt.Type)
                {
                    case MatchEventType.Goal:
                    case MatchEventType.PenaltyGoal:
                        if (isHomePlayer) homeGoals++;
                        else if (isAwayPlayer) awayGoals++;
                        break;
                    case MatchEventType.OwnGoal:
                        // Автогол: засчитывается сопернику
                        if (isHomePlayer) awayGoals++;
                        else if (isAwayPlayer) homeGoals++;
                        break;
                    // Отмененный гол и остальные события счет не меняют
                }
            }

            match.HomeScore = homeGoals;
            match.AwayScore = awayGoals;

            // 2. Обновление турнирной таблицы (упрощенно - пересчет всех матчей турнира)
            await RecalculateTournamentStandingsAsync(match.TournamentId);

            await _context.SaveChangesAsync();
        }

        private async Task RecalculateTournamentStandingsAsync(Guid tournamentId)
        {
            // Получаем все завершенные матчи турнира
            var completedMatches = await _context.Matches
                .Where(m => m.TournamentId == tournamentId && m.Status == MatchStatus.Completed)
                .ToListAsync();

            // Собираем список всех команд турнира
            var teamIds = completedMatches
                .SelectMany(m => new[] { m.HomeTeamId, m.AwayTeamId })
                .Distinct()
                .ToList();

            // Формируем статистику для каждой команды
            var standings = new Dictionary<Guid, (int Played, int Wins, int Draws, int Losses, int Scored, int Conceded, int Points)>();

            foreach (var teamId in teamIds)
                standings[teamId] = (0, 0, 0, 0, 0, 0, 0);

            foreach (var m in completedMatches)
            {
                if (m.HomeScore == null || m.AwayScore == null) continue;

                var home = standings[m.HomeTeamId];
                var away = standings[m.AwayTeamId];

                home.Played++; away.Played++;
                home.Scored += m.HomeScore.Value; home.Conceded += m.AwayScore.Value;
                away.Scored += m.AwayScore.Value; away.Conceded += m.HomeScore.Value;

                if (m.HomeScore > m.AwayScore)
                {
                    home.Wins++; home.Points += 3;
                    away.Losses++;
                }
                else if (m.HomeScore < m.AwayScore)
                {
                    away.Wins++; away.Points += 3;
                    home.Losses++;
                }
                else
                {
                    home.Draws++; home.Points += 1;
                    away.Draws++; away.Points += 1;
                }

                standings[m.HomeTeamId] = home;
                standings[m.AwayTeamId] = away;
            }

            // Здесь можно сохранить standings в отдельную таблицу TeamStats,
            // либо возвращать их как вычисляемый DTO по запросу GET /api/tournaments/{id}/table.
            // Для простоты мы оставляем пересчет "на лету" при каждом запросе таблицы.
        }
    }
}
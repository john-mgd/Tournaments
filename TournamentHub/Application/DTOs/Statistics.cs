namespace TournamentHub.Application.DTOs.Statistics;

// Статистика игрока (Запрос: GET /api/players/{id}/stats)
public record PlayerStatsDto(
    Guid PlayerId,
    string PlayerName,
    string TeamName,
    int MatchesPlayed,
    int Goals,
    int Assists,
    int YellowCards,
    int RedCards
);

// Статистика команды (Запрос: GET /api/teams/{id}/stats)
public record TeamStatsDto(
    Guid TeamId,
    string TeamName,
    int MatchesPlayed,
    int Wins,
    int Draws,
    int Losses,
    int GoalsScored,
    int GoalsConceded,
    int Points
);
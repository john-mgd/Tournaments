using TournamentHub.Domain.Enums;

namespace TournamentHub.Application.DTOs.Tournaments;

// --- ТУРНИРЫ ---

public record TournamentDto(
    Guid Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    TournamentStatus Status,
    TournamentFormat Format,
    DateTime LineupDeadline,
    string OrganizerName
);

public record CreateTournamentDto(
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    TournamentFormat Format,
    DateTime LineupDeadline
);

public record UpdateTournamentDto(
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    DateTime LineupDeadline
);

// Специальный DTO для турнирной таблицы (Запрос: GET /api/tournaments/{id}/table)
public record TournamentTableRowDto(
    int Position,
    string TeamName,
    int MatchesPlayed,
    int Wins,
    int Draws,
    int Losses,
    int GoalsScored,
    int GoalsConceded,
    int Points
);

// --- ЗАЯВКИ И ПРИГЛАШЕНИЯ ---

public record ApplicationDto(
    Guid Id,
    string TournamentName,
    string TeamName,
    ApplicationStatus Status,
    DateTime CreatedAt
);

public record ApplyTournamentDto(
    Guid TeamId
);

public record InviteTeamDto(
    Guid TeamId
);
using TournamentHub.Domain.Enums;
using TournamentHub.Application.DTOs.Teams;

namespace TournamentHub.Application.DTOs.Matches;

// --- МАТЧИ ---
public record UpdateMatchDto(DateTime DateTime, string Location);
public record AssignJudgeDto(Guid JudgeId);
public record PostponeMatchDto(DateTime NewDate);
public record SubmitLineupDto(List<Guid> StarterPlayerIds, List<Guid> SubstitutePlayerIds);
// Краткий DTO для списков (расписание)
public record MatchListDto(
    Guid Id,
    DateTime DateTime,
    string HomeTeamName,
    string AwayTeamName,
    MatchStatus Status,
    int? HomeScore,
    int? AwayScore
);

// Детальный DTO для карточки матча (включает составы и события)
public record MatchDetailsDto(
    Guid Id,
    DateTime DateTime,
    string Location,
    MatchStatus Status,
    int? HomeScore,
    int? AwayScore,
    string TournamentName,
    string? JudgeName,
    TeamDto HomeTeam,
    TeamDto AwayTeam,
    IEnumerable<MatchLineupDto> Lineups,
    IEnumerable<MatchEventDto> Events,
    bool IsProtocolConfirmed
);


public record MatchLineupDto(
    Guid TeamId,
    string TeamName,
    IEnumerable<LineupPlayerDto> Players
);

public record LineupPlayerDto(
    Guid PlayerId,
    string FullName,
    int JerseyNumber,
    bool IsStarter
);

public record MatchEventDto(
    Guid Id,
    MatchEventType Type,
    int Minute,
    string? PlayerName,
    string? RelatedPlayerName
);

public record CreateEventDto(
    MatchEventType Type,
    int Minute,
    Guid? PlayerId,
    Guid? RelatedPlayerId // Для замен и ассистов
);

public record UpdateEventDto(
    MatchEventType Type,
    int Minute,
    Guid? PlayerId,
    Guid? RelatedPlayerId
);
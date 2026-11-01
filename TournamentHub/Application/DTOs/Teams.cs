namespace TournamentHub.Application.DTOs.Teams;

// --- КОМАНДЫ ---

public record TeamDto(
    Guid Id,
    string Name,
    string? LogoUrl,
    string City,
    string ContactInfo,
    string RepresentativeName
);

public record CreateTeamDto(
    string Name,
    string City,
    string ContactInfo,
    string? LogoUrl
);

public record UpdateTeamDto(
    string Name,
    string City,
    string ContactInfo,
    string? LogoUrl
);

// --- ИГРОКИ ---

public record PlayerDto(
    Guid Id,
    string FullName,
    string? PhotoUrl,
    int Age,
    int JerseyNumber,
    string Position,
    Guid TeamId,
    string TeamName
);

public record CreatePlayerDto(
    string FullName,
    int Age,
    int JerseyNumber,
    string Position,
    string? PhotoUrl
);

public record UpdatePlayerDto(
    string FullName,
    int Age,
    int JerseyNumber,
    string Position,
    string? PhotoUrl
);
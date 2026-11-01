using TournamentHub.Domain.Enums;

namespace TournamentHub.Domain.Entities;

public class TournamentApplication
{
    public Guid Id { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Внешние ключи
    public Guid TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
}
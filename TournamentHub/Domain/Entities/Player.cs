namespace TournamentHub.Domain.Entities;

public class Player
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public int Age { get; set; }
    public int JerseyNumber { get; set; }
    public string Position { get; set; } = string.Empty;

    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public ICollection<MatchEvent> MatchEvents { get; set; } = new List<MatchEvent>();
}
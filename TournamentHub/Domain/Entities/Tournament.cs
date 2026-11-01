using TournamentHub.Domain.Enums;
namespace TournamentHub.Domain.Entities;

public class Tournament
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TournamentStatus Status { get; set; }
    public TournamentFormat Format { get; set; }
    public DateTime LineupDeadline { get; set; }

    public Guid OrganizerId { get; set; }
    public User Organizer { get; set; } = null!;

    public ICollection<TournamentApplication> Applications { get; set; } = new List<TournamentApplication>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
}
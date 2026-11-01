namespace TournamentHub.Domain.Entities;

public class Team
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string City { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;

    public Guid RepresentativeId { get; set; }
    public User Representative { get; set; } = null!;

    public ICollection<Player> Players { get; set; } = new List<Player>();
    public ICollection<TournamentApplication> Applications { get; set; } = new List<TournamentApplication>();
}
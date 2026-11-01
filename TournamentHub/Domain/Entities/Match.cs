using TournamentHub.Domain.Enums;

namespace TournamentHub.Domain.Entities;


public class Match
{
    public Guid Id { get; set; }
    public DateTime DateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public MatchStatus Status { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }

    public Guid TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    public Guid HomeTeamId { get; set; }
    public Team HomeTeam { get; set; } = null!;

    public Guid AwayTeamId { get; set; }
    public Team AwayTeam { get; set; } = null!;

    public Guid? JudgeId { get; set; }
    public User? Judge { get; set; }

    public ICollection<MatchLineup> Lineups { get; set; } = new List<MatchLineup>();
    public ICollection<MatchEvent> Events { get; set; } = new List<MatchEvent>();
    public Protocol? Protocol { get; set; }
}
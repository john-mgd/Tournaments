namespace TournamentHub.Domain.Entities;

public class TeamStats
{
    public Guid TeamId { get; set; }
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int GoalsScored { get; set; }
    public int GoalsConceded { get; set; }
    public int Points { get; set; } // 3 за победу, 1 за ничью
}

public class PlayerStats
{
    public Guid PlayerId { get; set; }
    public int MatchesPlayed { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
}
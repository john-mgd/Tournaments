namespace TournamentHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using TournamentHub.Domain.Enums;

public class User : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.RegularUser;
    public ICollection<Team> ManagedTeams { get; set; } = new List<Team>();
    public ICollection<Tournament> OrganizedTournaments { get; set; } = new List<Tournament>();
    public ICollection<Match> AssignedMatches { get; set; } = new List<Match>();
}
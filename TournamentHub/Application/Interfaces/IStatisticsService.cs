using TournamentHub.Domain.Entities;

namespace TournamentHub.Application.Interfaces
{
    public interface IStatisticsService
    {
        Task RecalculateStatisticsAsync(Match match);
    }
}
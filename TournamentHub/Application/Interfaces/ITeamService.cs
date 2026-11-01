using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Teams;

namespace TournamentHub.Application.Interfaces
{
    public interface ITeamService
    {
        Task<IEnumerable<TeamDto>> GetAllTeamsAsync();
        Task<TeamDto?> GetTeamByIdAsync(Guid id);
        Task<Result<TeamDto>> CreateTeamAsync(CreateTeamDto dto, Guid representativeId);
        Task<Result<bool>> UpdateTeamAsync(Guid id, UpdateTeamDto dto, Guid currentUserId);
    }
}
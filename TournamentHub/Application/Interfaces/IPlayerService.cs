using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Teams;

namespace TournamentHub.Application.Interfaces
{
    public interface IPlayerService
    {
        Task<IEnumerable<PlayerDto>> GetPlayersByTeamIdAsync(Guid teamId);
        Task<Result<PlayerDto>> AddPlayerAsync(Guid teamId, CreatePlayerDto dto, Guid currentUserId);
        Task<Result<bool>> UpdatePlayerAsync(Guid playerId, UpdatePlayerDto dto, Guid currentUserId);
        Task<Result<bool>> DeletePlayerAsync(Guid playerId, Guid currentUserId);
    }
}
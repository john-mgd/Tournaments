using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Tournaments;

namespace TournamentHub.Application.Interfaces
{
    public interface IApplicationService
    {
        Task<Result<ApplicationDto>> ApplyAsync(Guid tournamentId, Guid teamId, Guid currentUserId);
        Task<Result<ApplicationDto>> InviteAsync(Guid tournamentId, Guid teamId, Guid currentUserId);
        Task<Result<bool>> ApproveAsync(Guid applicationId, Guid currentUserId);
        Task<Result<bool>> RejectAsync(Guid applicationId, Guid currentUserId);
        Task<Result<bool>> AcceptInviteAsync(Guid applicationId, Guid currentUserId);
        Task<Result<bool>> DeclineInviteAsync(Guid applicationId, Guid currentUserId);
        Task<IEnumerable<ApplicationDto>> GetApplicationsForTournamentAsync(Guid tournamentId);
    }
}
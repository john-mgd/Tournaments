using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Matches;

namespace TournamentHub.Application.Interfaces
{
    public interface IMatchService
    {
        Task<MatchDetailsDto?> GetMatchByIdAsync(Guid id);

        Task<Result<bool>> UpdateMatchAsync(Guid id, UpdateMatchDto dto, Guid currentUserId);
        Task<Result<bool>> AssignJudgeAsync(Guid matchId, Guid judgeId);
        Task<Result<bool>> PostponeMatchAsync(Guid matchId, DateTime newDate, Guid currentUserId);
        Task<Result<bool>> CancelMatchAsync(Guid matchId, Guid currentUserId);

        Task<Result<bool>> SubmitLineupAsync(Guid matchId, SubmitLineupDto dto, Guid currentUserId);

        Task<Result<bool>> ConfirmProtocolAsync(Guid matchId, Guid judgeId);
        Task<Result<bool>> UploadProtocolFileAsync(Guid matchId, Stream fileStream, string fileName, Guid judgeId);
    }
}
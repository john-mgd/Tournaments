using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Matches;

namespace TournamentHub.Application.Interfaces
{
    public interface IEventService
    {
        Task<Result<MatchEventDto>> AddEventAsync(Guid matchId, CreateEventDto dto, Guid judgeId);
        Task<Result<bool>> UpdateEventAsync(Guid eventId, UpdateEventDto dto, Guid judgeId);
        Task<Result<bool>> DeleteEventAsync(Guid eventId, Guid judgeId);
    }
}
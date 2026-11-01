using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Tournaments;

namespace TournamentHub.Application.Interfaces
{
    public interface ITournamentService
    {
        Task<IEnumerable<TournamentDto>> GetAllTournamentsAsync();
        Task<TournamentDto?> GetTournamentByIdAsync(Guid id);
        Task<Result<TournamentDto>> CreateTournamentAsync(CreateTournamentDto dto, Guid organizerId);
        Task<Result<bool>> UpdateTournamentAsync(Guid id, UpdateTournamentDto dto, Guid currentUserId);
        Task<Result<bool>> GenerateScheduleAsync(Guid id, Guid currentUserId);
        Task<Result<bool>> CancelTournamentAsync(Guid id, Guid currentUserId);
    }
}
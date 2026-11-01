using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Users;
using TournamentHub.Domain.Enums;

namespace TournamentHub.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task<Result<UserDto>> UpdateUserAsync(Guid id, UpdateUserDto dto);
        Task<Result<bool>> AssignRoleAsync(Guid id, UserRole role);
        
        Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto);
        Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
    }
}
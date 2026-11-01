using TournamentHub.Domain.Enums;

namespace TournamentHub.Application.DTOs.Users;

// Исходящий DTO (без пароля!)
public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role
);

// Входящий DTO для обновления профиля
public record UpdateUserDto(
    string FullName,
    string Email
);

// Входящий DTO для смены роли (Админ)
public record AssignRoleDto(
    UserRole Role
);

// --- DTO для аутентификации ---

public record RegisterDto(
    string FullName,
    string Email,
    string Password
);

public record LoginDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    string Token,
    UserDto User
);
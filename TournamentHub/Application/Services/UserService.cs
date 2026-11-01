using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Users;
using TournamentHub.Application.Interfaces;
using TournamentHub.Data;
using TournamentHub.Domain.Entities;
using TournamentHub.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace TournamentHub.Application.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;

        public UserService(ApplicationDbContext context, UserManager<User> userManager, IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _config = config;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Select(u => new UserDto(u.Id, u.FullName, u.Email!, u.Role))
                .ToListAsync();
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UserDto(u.Id, u.FullName, u.Email!, u.Role))
                .FirstOrDefaultAsync();
        }

        public async Task<Result<UserDto>> UpdateUserAsync(Guid id, UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return Result<UserDto>.Failure("Пользователь не найден", ResultErrorType.NotFound);

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id))
                return Result<UserDto>.Failure("Email уже используется");

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            await _context.SaveChangesAsync();

            return Result<UserDto>.Success(new UserDto(user.Id, user.FullName, user.Email!, user.Role));
        }

        public async Task<Result<bool>> AssignRoleAsync(Guid id, UserRole role)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return Result<bool>.Failure("Пользователь не найден", ResultErrorType.NotFound);

            user.Role = role;
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        // public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto)
        // {
        //     if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        //         return Result<AuthResponseDto>.Failure("Email уже зарегистрирован");

        //     var user = new User
        //     {
        //         Id = Guid.NewGuid(),
        //         UserName = dto.Email,       // Identity требует UserName
        //         FullName = dto.FullName,
        //         Email = dto.Email,
        //         PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        //         Role = UserRole.RegularUser
        //     };

        //     _context.Users.Add(user);
        //     await _context.SaveChangesAsync();

        //     var token = GenerateJwtToken(user);
        //     var userDto = new UserDto(user.Id, user.FullName, user.Email!, user.Role);
        //     return Result<AuthResponseDto>.Success(new AuthResponseDto(token, userDto));
        // }
        public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto)
        {
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                return Result<AuthResponseDto>.Failure("Email уже зарегистрирован");

            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                Role = UserRole.RegularUser
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                return Result<AuthResponseDto>.Failure(errors);
            }

            var token = GenerateJwtToken(user);
            var userDto = new UserDto(user.Id, user.FullName, user.Email!, user.Role);
            return Result<AuthResponseDto>.Success(new AuthResponseDto(token, userDto));
        }

        // public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
        // {
        //     var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        //     if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        //         return Result<AuthResponseDto>.Failure("Неверный email или пароль", ResultErrorType.Unauthorized);

        //     var token = GenerateJwtToken(user);
        //     var userDto = new UserDto(user.Id, user.FullName, user.Email!, user.Role);
        //     return Result<AuthResponseDto>.Success(new AuthResponseDto(token, userDto));
        // }
        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            // 1. Ищем пользователя по email
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Result<AuthResponseDto>.Failure(
                    "Неверный email или пароль",
                    ResultErrorType.Unauthorized);

            // 2. Проверяем пароль через UserManager
            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
                return Result<AuthResponseDto>.Failure(
                    "Неверный email или пароль",
                    ResultErrorType.Unauthorized);

            // 3. (опционально) Проверка: подтверждён ли email
            // Если вы включили RequireConfirmedAccount = true, раскомментируйте:
            // if (!await _userManager.IsEmailConfirmedAsync(user))
            //     return Result<AuthResponseDto>.Failure(
            //         "Email не подтверждён",
            //         ResultErrorType.Unauthorized);

            // 4. Генерируем JWT
            var token = GenerateJwtToken(user);

            // 5. Формируем ответ
            var userDto = new UserDto(user.Id, user.FullName, user.Email!, user.Role);
            return Result<AuthResponseDto>.Success(new AuthResponseDto(token, userDto));
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
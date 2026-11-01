using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentHub.Application.DTOs.Users;
using TournamentHub.Application.Interfaces;

namespace TournamentHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // По умолчанию все методы требуют авторизации
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET /api/users
        [HttpGet]
        [Authorize(Roles = "Admin")] // Только Админ может видеть всех
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET /api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            // В реальности здесь также нужна проверка: либо это Админ, либо сам пользователь
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            
            return Ok(user);
        }

        // PUT /api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
        {
            await _userService.UpdateUserAsync(id, dto);
            return NoContent();
        }

        // PATCH /api/users/{id}/role
        [HttpPatch("{id}/role")]
        [Authorize(Roles = "Admin")] // Только Админ может менять роли
        public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleDto dto)
        {
            await _userService.AssignRoleAsync(id, dto.Role);
            return Ok(new { Message = "Role updated successfully" });
        }
    }
}
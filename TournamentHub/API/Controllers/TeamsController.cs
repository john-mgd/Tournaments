using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Teams;
using TournamentHub.Application.Interfaces;

namespace TournamentHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamService _teamService;
        private readonly IPlayerService _playerService;

        public TeamsController(ITeamService teamService, IPlayerService playerService)
        {
            _teamService = teamService;
            _playerService = playerService;
        }

        private Guid GetCurrentUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idClaim))
                throw new UnauthorizedAccessException("Пользователь не авторизован");
            return Guid.Parse(idClaim);
        }

        // === КОМАНДЫ ===

        // GET /api/teams (публичный)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var teams = await _teamService.GetAllTeamsAsync();
            return Ok(teams);
        }

        // GET /api/teams/{id} (публичный)
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var team = await _teamService.GetTeamByIdAsync(id);
            if (team == null) return NotFound(new { error = "Команда не найдена" });
            return Ok(team);
        }

        // POST /api/teams
        [HttpPost]
        [Authorize(Roles = "TeamRepresentative")]
        public async Task<IActionResult> Create([FromBody] CreateTeamDto dto)
        {
            // ✅ Передаём representativeId из JWT
            var result = await _teamService.CreateTeamAsync(dto, GetCurrentUserId());
            return ToActionResult(result);
        }

        // PUT /api/teams/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "TeamRepresentative")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeamDto dto)
        {
            // ✅ Передаём currentUserId
            var result = await _teamService.UpdateTeamAsync(id, dto, GetCurrentUserId());
            return ToActionResult(result);
        }

        // === ИГРОКИ ===

        // GET /api/teams/{teamId}/players (публичный)
        [HttpGet("{teamId}/players")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlayers(Guid teamId)
        {
            var players = await _playerService.GetPlayersByTeamIdAsync(teamId);
            return Ok(players);
        }

        // POST /api/teams/{teamId}/players
        [HttpPost("{teamId}/players")]
        [Authorize(Roles = "TeamRepresentative")]
        public async Task<IActionResult> AddPlayer(Guid teamId, [FromBody] CreatePlayerDto dto)
        {
            // ✅ Передаём currentUserId
            var result = await _playerService.AddPlayerAsync(teamId, dto, GetCurrentUserId());
            return ToActionResult(result);
        }

        // PUT /api/players/{id} — вынесено в отдельный маршрут
        [HttpPut("/api/players/{id}")]
        [Authorize(Roles = "TeamRepresentative")]
        public async Task<IActionResult> UpdatePlayer(Guid id, [FromBody] UpdatePlayerDto dto)
        {
            var result = await _playerService.UpdatePlayerAsync(id, dto, GetCurrentUserId());
            return ToActionResult(result);
        }

        // DELETE /api/players/{id}
        [HttpDelete("/api/players/{id}")]
        [Authorize(Roles = "TeamRepresentative")]
        public async Task<IActionResult> DeletePlayer(Guid id)
        {
            var result = await _playerService.DeletePlayerAsync(id, GetCurrentUserId());
            return ToActionResult(result);
        }

        // === УНИВЕРСАЛЬНЫЙ МАППЕР ===

        private IActionResult ToActionResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return Ok(result.Value);

            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { error = result.Error }),
                ResultErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, new { error = result.Error }),
                ResultErrorType.Unauthorized => Unauthorized(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }
    }
}
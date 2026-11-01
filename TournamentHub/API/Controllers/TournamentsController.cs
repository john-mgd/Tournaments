using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Tournaments;
using TournamentHub.Application.Interfaces;

namespace TournamentHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TournamentsController : ControllerBase
    {
        private readonly ITournamentService _tournamentService;

        public TournamentsController(ITournamentService tournamentService)
        {
            _tournamentService = tournamentService;
        }

        private Guid GetCurrentUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idClaim))
                throw new UnauthorizedAccessException("Пользователь не авторизован");
            return Guid.Parse(idClaim);
        }

        // === ПУБЛИЧНЫЕ МЕТОДЫ ===

        // GET /api/tournaments
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var tournaments = await _tournamentService.GetAllTournamentsAsync();
            return Ok(tournaments);
        }

        // GET /api/tournaments/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var tournament = await _tournamentService.GetTournamentByIdAsync(id);
            if (tournament == null) return NotFound(new { error = "Турнир не найден" });
            return Ok(tournament);
        }

        // === ОПЕРАЦИИ ОРГАНИЗАТОРА ===

        // POST /api/tournaments
        [HttpPost]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Create([FromBody] CreateTournamentDto dto)
        {
            var result = await _tournamentService.CreateTournamentAsync(dto, GetCurrentUserId());
            return ToActionResult(result);
        }

        // PUT /api/tournaments/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTournamentDto dto)
        {
            var result = await _tournamentService.UpdateTournamentAsync(id, dto, GetCurrentUserId());
            return ToActionResult(result);
        }

        // POST /api/tournaments/{id}/generate-schedule
        [HttpPost("{id}/generate-schedule")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> GenerateSchedule(Guid id)
        {
            var result = await _tournamentService.GenerateScheduleAsync(id, GetCurrentUserId());
            return ToActionResult(result);
        }

        // POST /api/tournaments/{id}/cancel
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var result = await _tournamentService.CancelTournamentAsync(id, GetCurrentUserId());
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
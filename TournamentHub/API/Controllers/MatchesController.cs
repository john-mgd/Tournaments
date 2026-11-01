using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Matches;
using TournamentHub.Application.Interfaces;

namespace TournamentHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchService _matchService;

        public MatchesController(IMatchService matchService)
        {
            _matchService = matchService;
        }

        // Вспомогательный метод для получения ID текущего пользователя из JWT
        private Guid GetCurrentUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idClaim))
                throw new UnauthorizedAccessException("Пользователь не авторизован");

            return Guid.Parse(idClaim);
        }

        // === ПУБЛИЧНЫЕ МЕТОДЫ (без авторизации) ===

        // GET /api/matches/{id} — карточка матча
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMatchById(Guid id)
        {
            var match = await _matchService.GetMatchByIdAsync(id);
            if (match == null) return NotFound(new { error = "Матч не найден" });
            return Ok(match);
        }

        // === ОПЕРАЦИИ ОРГАНИЗАТОРА ===

        // PUT /api/matches/{id} — редактировать дату/место
        [HttpPut("{id}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> UpdateMatch(Guid id, [FromBody] UpdateMatchDto dto)
        {
            var result = await _matchService.UpdateMatchAsync(id, dto, GetCurrentUserId());
            return ToActionResult(result);
        }

        // PUT /api/matches/{id}/judge — назначить/заменить судью
        [HttpPut("{id}/judge")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> AssignJudge(Guid id, [FromBody] AssignJudgeDto dto)
        {
            var result = await _matchService.AssignJudgeAsync(id, dto.JudgeId);
            return ToActionResult(result);
        }

        // PATCH /api/matches/{id}/postpone — перенести матч
        [HttpPatch("{id}/postpone")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> PostponeMatch(Guid id, [FromBody] PostponeMatchDto dto)
        {
            var result = await _matchService.PostponeMatchAsync(id, dto.NewDate, GetCurrentUserId());
            return ToActionResult(result);
        }

        // PATCH /api/matches/{id}/cancel — отменить матч
        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> CancelMatch(Guid id)
        {
            var result = await _matchService.CancelMatchAsync(id, GetCurrentUserId());
            return ToActionResult(result);
        }

        // === ОПЕРАЦИИ ПРЕДСТАВИТЕЛЯ КОМАНДЫ ===

        // PUT /api/matches/{id}/lineup — подать состав
        [HttpPut("{id}/lineup")]
        [Authorize(Roles = "TeamRepresentative")]
        public async Task<IActionResult> SubmitLineup(Guid id, [FromBody] SubmitLineupDto dto)
        {
            // В реальном проекте teamId берётся из токена или проверяется принадлежность
            // Здесь для простоты предполагаем, что teamId передаётся в DTO
            var result = await _matchService.SubmitLineupAsync(id, dto, GetCurrentUserId());
            return ToActionResult(result);
        }

        // === ОПЕРАЦИИ СУДЬИ ===

        // POST /api/matches/{id}/protocol/confirm — подтвердить протокол
        [HttpPost("{id}/protocol/confirm")]
        [Authorize(Roles = "Judge")]
        public async Task<IActionResult> ConfirmProtocol(Guid id)
        {
            var result = await _matchService.ConfirmProtocolAsync(id, GetCurrentUserId());
            return ToActionResult(result);
        }

        // POST /api/matches/{id}/protocol/file — загрузить официальный протокол
        [HttpPost("{id}/protocol/file")]
        [Authorize(Roles = "Judge")]
        public async Task<IActionResult> UploadProtocolFile(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Файл не передан" });

            using var stream = file.OpenReadStream();
            var result = await _matchService.UploadProtocolFileAsync(id, stream, file.FileName, GetCurrentUserId());
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
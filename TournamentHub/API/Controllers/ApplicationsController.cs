using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TournamentHub.Application.Common;
using TournamentHub.Application.Interfaces;

namespace TournamentHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        // Вспомогательный метод для получения ID текущего пользователя
        private Guid GetCurrentUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idClaim)) 
                throw new UnauthorizedAccessException("Пользователь не авторизован");
            
            return Guid.Parse(idClaim);
        }

        // PATCH /api/applications/{id}/approve
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var result = await _applicationService.ApproveAsync(id, GetCurrentUserId());
            return ToActionResult(result);
        }

        // PATCH /api/applications/{id}/reject
        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var result = await _applicationService.RejectAsync(id, GetCurrentUserId());
            return ToActionResult(result);
        }

        // PATCH /api/applications/invites/{id}/accept
        [HttpPatch("invites/{id}/accept")]
        [Authorize(Roles = "TeamRepresentative")]
        public async Task<IActionResult> AcceptInvite(Guid id)
        {
            var result = await _applicationService.AcceptInviteAsync(id, GetCurrentUserId());
            return ToActionResult(result);
        }

        // PATCH /api/applications/invites/{id}/decline
        [HttpPatch("invites/{id}/decline")]
        [Authorize(Roles = "TeamRepresentative")]
        public async Task<IActionResult> DeclineInvite(Guid id)
        {
            var result = await _applicationService.DeclineInviteAsync(id, GetCurrentUserId());
            return ToActionResult(result);
        }

        // GET /api/applications/tournament/{tournamentId}
        [HttpGet("tournament/{tournamentId}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> GetForTournament(Guid tournamentId)
        {
            var applications = await _applicationService.GetApplicationsForTournamentAsync(tournamentId);
            return Ok(applications);
        }

        // Универсальный маппер Result -> IActionResult
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
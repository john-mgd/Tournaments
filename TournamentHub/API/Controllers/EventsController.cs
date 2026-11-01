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
    [Authorize(Roles = "Judge")] // Все методы здесь — только для судей
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        // Вспомогательный метод для получения ID текущего пользователя (судьи)
        private Guid GetCurrentUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idClaim))
                throw new UnauthorizedAccessException("Пользователь не авторизован");

            return Guid.Parse(idClaim);
        }

        // POST /api/events/match/{matchId}
        // Добавить событие к матчу (судья)
        [HttpPost("match/{matchId}")]
        public async Task<IActionResult> AddEvent(Guid matchId, [FromBody] CreateEventDto dto)
        {
            var result = await _eventService.AddEventAsync(matchId, dto, GetCurrentUserId());
            return ToActionResult(result);
        }

        // PUT /api/events/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventDto dto)
        {
            var result = await _eventService.UpdateEventAsync(id, dto, GetCurrentUserId());
            return ToActionResult(result);
        }

        // DELETE /api/events/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var result = await _eventService.DeleteEventAsync(id, GetCurrentUserId());
            return ToActionResult(result);
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
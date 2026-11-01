using Microsoft.EntityFrameworkCore;
using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Tournaments;
using TournamentHub.Application.Interfaces;
using TournamentHub.Data;                          // ← ваш реальный namespace
using TournamentHub.Domain.Entities;
using TournamentHub.Domain.Enums;

namespace TournamentHub.Application.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly ApplicationDbContext _context;   // ← не AppDbContext

        public ApplicationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ApplicationDto>> ApplyAsync(Guid tournamentId, Guid teamId, Guid currentUserId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null) return Result<ApplicationDto>.Failure("Команда не найдена", ResultErrorType.NotFound);
            if (team.RepresentativeId != currentUserId)
                return Result<ApplicationDto>.Failure("Вы не являетесь представителем этой команды", ResultErrorType.Forbidden);

            var tournament = await _context.Tournaments.FindAsync(tournamentId);
            if (tournament == null) return Result<ApplicationDto>.Failure("Турнир не найден", ResultErrorType.NotFound);
            if (tournament.Status != TournamentStatus.Planned)
                return Result<ApplicationDto>.Failure("Турнир уже начался или завершен");

            if (await _context.TournamentApplications.AnyAsync(a => a.TournamentId == tournamentId && a.TeamId == teamId))
                return Result<ApplicationDto>.Failure("Заявка от этой команды уже существует");

            var app = new TournamentApplication
            {
                Id = Guid.NewGuid(),
                TournamentId = tournamentId,
                TeamId = teamId,
                Status = ApplicationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.TournamentApplications.Add(app);
            await _context.SaveChangesAsync();

            return Result<ApplicationDto>.Success(new ApplicationDto(
                app.Id, tournament.Name, team.Name, app.Status, app.CreatedAt));
        }

        public async Task<Result<ApplicationDto>> InviteAsync(Guid tournamentId, Guid teamId, Guid currentUserId)
        {
            var tournament = await _context.Tournaments.FindAsync(tournamentId);
            if (tournament == null) return Result<ApplicationDto>.Failure("Турнир не найден", ResultErrorType.NotFound);
            if (tournament.OrganizerId != currentUserId)
                return Result<ApplicationDto>.Failure("Нет прав", ResultErrorType.Forbidden);

            var team = await _context.Teams.FindAsync(teamId);
            if (team == null) return Result<ApplicationDto>.Failure("Команда не найдена", ResultErrorType.NotFound);

            if (await _context.TournamentApplications.AnyAsync(a => a.TournamentId == tournamentId && a.TeamId == teamId))
                return Result<ApplicationDto>.Failure("Приглашение или заявка уже существует");

            var invite = new TournamentApplication
            {
                Id = Guid.NewGuid(),
                TournamentId = tournamentId,
                TeamId = teamId,
                Status = ApplicationStatus.Invited,
                CreatedAt = DateTime.UtcNow
            };

            _context.TournamentApplications.Add(invite);
            await _context.SaveChangesAsync();

            return Result<ApplicationDto>.Success(new ApplicationDto(
                invite.Id, tournament.Name, team.Name, invite.Status, invite.CreatedAt));
        }

        public async Task<Result<bool>> ApproveAsync(Guid applicationId, Guid currentUserId)
        {
            var app = await _context.TournamentApplications
                .Include(a => a.Tournament)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (app == null) return Result<bool>.Failure("Заявка не найдена", ResultErrorType.NotFound);
            if (app.Tournament.OrganizerId != currentUserId)
                return Result<bool>.Failure("Нет прав", ResultErrorType.Forbidden);
            if (app.Status != ApplicationStatus.Pending)
                return Result<bool>.Failure("Можно принять только заявку в статусе Pending");

            app.Status = ApplicationStatus.Approved;
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RejectAsync(Guid applicationId, Guid currentUserId)
        {
            var app = await _context.TournamentApplications
                .Include(a => a.Tournament)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (app == null) return Result<bool>.Failure("Заявка не найдена", ResultErrorType.NotFound);
            if (app.Tournament.OrganizerId != currentUserId)
                return Result<bool>.Failure("Нет прав", ResultErrorType.Forbidden);

            app.Status = ApplicationStatus.Rejected;
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> AcceptInviteAsync(Guid applicationId, Guid currentUserId)
        {
            var app = await _context.TournamentApplications
                .Include(a => a.Team)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (app == null) return Result<bool>.Failure("Приглашение не найдено", ResultErrorType.NotFound);
            if (app.Team.RepresentativeId != currentUserId)
                return Result<bool>.Failure("Нет прав", ResultErrorType.Forbidden);
            if (app.Status != ApplicationStatus.Invited)
                return Result<bool>.Failure("Это не приглашение");

            app.Status = ApplicationStatus.Approved;
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeclineInviteAsync(Guid applicationId, Guid currentUserId)
        {
            var app = await _context.TournamentApplications
                .Include(a => a.Team)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (app == null) return Result<bool>.Failure("Приглашение не найдено", ResultErrorType.NotFound);
            if (app.Team.RepresentativeId != currentUserId)
                return Result<bool>.Failure("Нет прав", ResultErrorType.Forbidden);

            app.Status = ApplicationStatus.Rejected;
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<IEnumerable<ApplicationDto>> GetApplicationsForTournamentAsync(Guid tournamentId)
        {
            return await _context.TournamentApplications
                .AsNoTracking()
                .Where(a => a.TournamentId == tournamentId)
                .Select(a => new ApplicationDto(
                    a.Id, a.Tournament.Name, a.Team.Name, a.Status, a.CreatedAt))
                .ToListAsync();
        }
    }
}
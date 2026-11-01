using Microsoft.EntityFrameworkCore;
using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Teams;
using TournamentHub.Application.Interfaces;
using TournamentHub.Domain.Entities;
using TournamentHub.Data;

namespace TournamentHub.Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly ApplicationDbContext  _context;

        public TeamService(ApplicationDbContext  context) => _context = context;

        public async Task<IEnumerable<TeamDto>> GetAllTeamsAsync()
        {
            return await _context.Teams
                .AsNoTracking()
                .Select(t => new TeamDto(
                    t.Id, t.Name, t.LogoUrl, t.City, t.ContactInfo, t.Representative.FullName))
                .ToListAsync();
        }

        public async Task<TeamDto?> GetTeamByIdAsync(Guid id)
        {
            return await _context.Teams
                .AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new TeamDto(
                    t.Id, t.Name, t.LogoUrl, t.City, t.ContactInfo, t.Representative.FullName))
                .FirstOrDefaultAsync();
        }

        public async Task<Result<TeamDto>> CreateTeamAsync(CreateTeamDto dto, Guid representativeId)
        {
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                City = dto.City,
                ContactInfo = dto.ContactInfo,
                LogoUrl = dto.LogoUrl,
                RepresentativeId = representativeId
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return Result<TeamDto>.Success(new TeamDto(
                team.Id, team.Name, team.LogoUrl, team.City, team.ContactInfo, ""));
        }

        public async Task<Result<bool>> UpdateTeamAsync(Guid id, UpdateTeamDto dto, Guid currentUserId)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null) return Result<bool>.Failure("Команда не найдена", ResultErrorType.NotFound);

            // Проверка прав: только представитель этой команды может её редактировать
            if (team.RepresentativeId != currentUserId)
                return Result<bool>.Failure("Нет прав на редактирование этой команды", ResultErrorType.Forbidden);

            team.Name = dto.Name;
            team.City = dto.City;
            team.ContactInfo = dto.ContactInfo;
            team.LogoUrl = dto.LogoUrl;
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
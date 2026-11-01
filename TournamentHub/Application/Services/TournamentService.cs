using Microsoft.EntityFrameworkCore;
using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Tournaments;
using TournamentHub.Application.Interfaces;
using TournamentHub.Domain.Entities;
using TournamentHub.Domain.Enums;
using TournamentHub.Data;

namespace TournamentHub.Application.Services
{
    public class TournamentService : ITournamentService
    {
        private readonly ApplicationDbContext  _context;

        public TournamentService(ApplicationDbContext  context) => _context = context;

        public async Task<IEnumerable<TournamentDto>> GetAllTournamentsAsync()
        {
            return await _context.Tournaments
                .AsNoTracking()
                .Select(t => new TournamentDto(
                    t.Id, t.Name, t.Description, t.StartDate, t.EndDate,
                    t.Status, t.Format, t.LineupDeadline, t.Organizer.FullName))
                .ToListAsync();
        }

        public async Task<TournamentDto?> GetTournamentByIdAsync(Guid id)
        {
            return await _context.Tournaments
                .AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new TournamentDto(
                    t.Id, t.Name, t.Description, t.StartDate, t.EndDate,
                    t.Status, t.Format, t.LineupDeadline, t.Organizer.FullName))
                .FirstOrDefaultAsync();
        }

        public async Task<Result<TournamentDto>> CreateTournamentAsync(CreateTournamentDto dto, Guid organizerId)
        {
            if (dto.EndDate < dto.StartDate)
                return Result<TournamentDto>.Failure("Дата окончания не может быть раньше даты начала");

            var tournament = new Tournament
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Format = dto.Format,
                LineupDeadline = dto.LineupDeadline,
                Status = TournamentStatus.Planned,
                OrganizerId = organizerId
            };

            _context.Tournaments.Add(tournament);
            await _context.SaveChangesAsync();

            return Result<TournamentDto>.Success(new TournamentDto(
                tournament.Id, tournament.Name, tournament.Description,
                tournament.StartDate, tournament.EndDate, tournament.Status,
                tournament.Format, tournament.LineupDeadline, ""));
        }

        public async Task<Result<bool>> UpdateTournamentAsync(Guid id, UpdateTournamentDto dto, Guid currentUserId)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null) return Result<bool>.Failure("Турнир не найден", ResultErrorType.NotFound);
            if (tournament.OrganizerId != currentUserId)
                return Result<bool>.Failure("Нет прав на редактирование", ResultErrorType.Forbidden);
            if (tournament.Status == TournamentStatus.Cancelled)
                return Result<bool>.Failure("Нельзя редактировать отмененный турнир");

            tournament.Name = dto.Name;
            tournament.Description = dto.Description;
            tournament.StartDate = dto.StartDate;
            tournament.EndDate = dto.EndDate;
            tournament.LineupDeadline = dto.LineupDeadline;
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> GenerateScheduleAsync(Guid id, Guid currentUserId)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Applications)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null) return Result<bool>.Failure("Турнир не найден", ResultErrorType.NotFound);
            if (tournament.OrganizerId != currentUserId)
                return Result<bool>.Failure("Нет прав", ResultErrorType.Forbidden);
            if (tournament.Matches.Any())
                return Result<bool>.Failure("Расписание уже сгенерировано");

            // Получаем команды, чьи заявки приняты
            var teamIds = tournament.Applications
                .Where(a => a.Status == ApplicationStatus.Approved)
                .Select(a => a.TeamId)
                .ToList();

            if (teamIds.Count < 2)
                return Result<bool>.Failure("Для генерации расписания нужно минимум 2 принятые команды");

            var matches = new List<Match>();
            var startDate = tournament.StartDate;

            if (tournament.Format == TournamentFormat.RoundRobin)
            {
                // Круговой формат: каждая команда играет с каждой
                for (int i = 0; i < teamIds.Count; i++)
                {
                    for (int j = i + 1; j < teamIds.Count; j++)
                    {
                        matches.Add(new Match
                        {
                            Id = Guid.NewGuid(),
                            TournamentId = id,
                            HomeTeamId = teamIds[i],
                            AwayTeamId = teamIds[j],
                            DateTime = startDate.AddDays(matches.Count),
                            Location = "TBD",
                            Status = MatchStatus.Scheduled
                        });
                    }
                }
            }
            else // Knockout
            {
                // Упрощенная сетка на выбывание: только 1-й раунд (пары команд)
                if (teamIds.Count % 2 != 0)
                    return Result<bool>.Failure("Для турнира на выбывание нужно четное количество команд");

                for (int i = 0; i < teamIds.Count; i += 2)
                {
                    matches.Add(new Match
                    {
                        Id = Guid.NewGuid(),
                        TournamentId = id,
                        HomeTeamId = teamIds[i],
                        AwayTeamId = teamIds[i + 1],
                        DateTime = startDate.AddDays(i / 2),
                        Location = "TBD",
                        Status = MatchStatus.Scheduled
                    });
                }
            }

            _context.Matches.AddRange(matches);
            tournament.Status = TournamentStatus.Ongoing;
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> CancelTournamentAsync(Guid id, Guid currentUserId)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null) return Result<bool>.Failure("Турнир не найден", ResultErrorType.NotFound);
            if (tournament.OrganizerId != currentUserId)
                return Result<bool>.Failure("Нет прав", ResultErrorType.Forbidden);

            tournament.Status = TournamentStatus.Cancelled;

            // Все незавершенные матчи тоже отменяем
            foreach (var match in tournament.Matches.Where(m => m.Status != MatchStatus.Completed))
            {
                match.Status = MatchStatus.Cancelled;
            }

            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
    }
}
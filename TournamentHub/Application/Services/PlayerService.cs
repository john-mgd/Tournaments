using Microsoft.EntityFrameworkCore;
using TournamentHub.Application.Common;
using TournamentHub.Application.DTOs.Teams;
using TournamentHub.Application.Interfaces;
using TournamentHub.Domain.Entities;
using TournamentHub.Data;

namespace TournamentHub.Application.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly ApplicationDbContext  _context;

        public PlayerService(ApplicationDbContext  context) => _context = context;

        public async Task<IEnumerable<PlayerDto>> GetPlayersByTeamIdAsync(Guid teamId)
        {
            return await _context.Players
                .AsNoTracking()
                .Where(p => p.TeamId == teamId)
                .Select(p => new PlayerDto(
                    p.Id, p.FullName, p.PhotoUrl, p.Age, p.JerseyNumber,
                    p.Position, p.TeamId, p.Team.Name))
                .ToListAsync();
        }

        public async Task<Result<PlayerDto>> AddPlayerAsync(Guid teamId, CreatePlayerDto dto, Guid currentUserId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null) return Result<PlayerDto>.Failure("Команда не найдена", ResultErrorType.NotFound);
            if (team.RepresentativeId != currentUserId)
                return Result<PlayerDto>.Failure("Нет прав на добавление игроков", ResultErrorType.Forbidden);

            // Проверка уникальности номера в команде
            if (await _context.Players.AnyAsync(p => p.TeamId == teamId && p.JerseyNumber == dto.JerseyNumber))
                return Result<PlayerDto>.Failure($"Игрок с номером {dto.JerseyNumber} уже есть в команде");

            var player = new Player
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Age = dto.Age,
                JerseyNumber = dto.JerseyNumber,
                Position = dto.Position,
                PhotoUrl = dto.PhotoUrl,
                TeamId = teamId
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            return Result<PlayerDto>.Success(new PlayerDto(
                player.Id, player.FullName, player.PhotoUrl, player.Age,
                player.JerseyNumber, player.Position, teamId, team.Name));
        }

        public async Task<Result<bool>> UpdatePlayerAsync(Guid playerId, UpdatePlayerDto dto, Guid currentUserId)
        {
            var player = await _context.Players.Include(p => p.Team).FirstOrDefaultAsync(p => p.Id == playerId);
            if (player == null) return Result<bool>.Failure("Игрок не найден", ResultErrorType.NotFound);
            if (player.Team.RepresentativeId != currentUserId)
                return Result<bool>.Failure("Нет прав на редактирование этого игрока", ResultErrorType.Forbidden);

            // Проверка уникальности номера (исключая самого игрока)
            if (await _context.Players.AnyAsync(p => p.TeamId == player.TeamId 
                && p.JerseyNumber == dto.JerseyNumber && p.Id != playerId))
                return Result<bool>.Failure($"Игрок с номером {dto.JerseyNumber} уже есть в команде");

            player.FullName = dto.FullName;
            player.Age = dto.Age;
            player.JerseyNumber = dto.JerseyNumber;
            player.Position = dto.Position;
            player.PhotoUrl = dto.PhotoUrl;
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeletePlayerAsync(Guid playerId, Guid currentUserId)
        {
            var player = await _context.Players.Include(p => p.Team).FirstOrDefaultAsync(p => p.Id == playerId);
            if (player == null) return Result<bool>.Failure("Игрок не найден", ResultErrorType.NotFound);
            if (player.Team.RepresentativeId != currentUserId)
                return Result<bool>.Failure("Нет прав на удаление игрока", ResultErrorType.Forbidden);

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
    }
}
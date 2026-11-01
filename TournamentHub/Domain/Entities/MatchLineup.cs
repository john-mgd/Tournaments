namespace TournamentHub.Domain.Entities;

public class MatchLineup
{
    public Guid Id { get; set; }
    
    // Флаг: игрок в стартовом составе (true) или на замене (false)
    public bool IsStarter { get; set; }

    // Внешние ключи
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public Guid TeamId { get; set; } // Для проверки: игрок из нужной ли команды

    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;
}
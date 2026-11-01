using TournamentHub.Domain.Enums;

namespace TournamentHub.Domain.Entities;

public class MatchEvent
{
    public Guid Id { get; set; }
    public MatchEventType Type { get; set; }
    public int Minute { get; set; } // Минута матча (1–90+)

    // Внешние ключи
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;

    // Основной игрок (автор гола, получивший карточку, уходящий с поля)
    public Guid? PlayerId { get; set; }
    public Player? Player { get; set; }

    // Связанный игрок (для замены — выходящий на поле; для ассиста — отдавший передачу)
    public Guid? RelatedPlayerId { get; set; }
    public Player? RelatedPlayer { get; set; }
}
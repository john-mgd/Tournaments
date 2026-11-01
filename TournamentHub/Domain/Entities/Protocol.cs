namespace TournamentHub.Domain.Entities;

public class Protocol
{
    public Guid Id { get; set; }
    public bool IsConfirmed { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public string? OfficialFileUrl { get; set; } // Ссылка на загруженный официальный протокол

    // Внешний ключ (связь 1:1 с матчем)
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;
}
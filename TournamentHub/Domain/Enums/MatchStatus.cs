namespace TournamentHub.Domain.Enums;

public enum MatchStatus
{
    Scheduled,   // Запланирован
    Ongoing,     // Идет
    Completed,   // Завершен
    Postponed,   // Перенесен
    Cancelled    // Отменен
}
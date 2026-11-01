namespace TournamentHub.Domain.Enums;

public enum ApplicationStatus
{
    Pending,     // На рассмотрении (заявка от команды)
    Approved,    // Принята / Одобрена
    Rejected,    // Отклонена
    Invited      // Приглашена организатором (ожидает ответа)
}
namespace TournamentHub.Domain.Enums;

public enum MatchEventType
{
    Goal,            // Гол
    CancelledGoal,   // Отмененный гол
    OwnGoal,         // Автогол
    Assist,          // Голевая передача
    YellowCard,      // Желтая карточка
    RedCard,         // Красная карточка
    Substitution,    // Замена
    PenaltyGoal      // Забитый пенальти
}
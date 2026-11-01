// using Microsoft.EntityFrameworkCore;
// using TournamentHub.Domain.Entities;

// public class AppDbContext : DbContext
// {
//     public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

//     public DbSet<Team> Teams { get; set; }
//     // ... другие DbSet ...
//     protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             base.OnModelCreating(modelBuilder);
//         // 1. User (Representative) 1 --- N Team
//         modelBuilder.Entity<Team>()
//             .HasOne(t => t.Representative)
//             .WithMany(u => u.ManagedTeams)
//             .HasForeignKey(t => t.RepresentativeId)
//             .OnDelete(DeleteBehavior.Restrict); // Нельзя удалить пользователя, если у него есть команды

//         // 2. Team 1 --- N Player
//         modelBuilder.Entity<Player>()
//             .HasOne(p => p.Team)
//             .WithMany(t => t.Players)
//             .HasForeignKey(p => p.TeamId)
//             .OnDelete(DeleteBehavior.Cascade); // При удалении команды удаляются её игроки

//         // 3. User (Organizer) 1 --- N Tournament
//         modelBuilder.Entity<Tournament>()
//             .HasOne(t => t.Organizer)
//             .WithMany(u => u.OrganizedTournaments)
//             .HasForeignKey(t => t.OrganizerId)
//             .OnDelete(DeleteBehavior.Restrict); // Нельзя удалить организатора с турнирами

//         // 4. Tournament M --- N Team (через TournamentApplication)
//         // Связь с турниром
//         modelBuilder.Entity<TournamentApplication>()
//             .HasOne(ta => ta.Tournament)
//             .WithMany(t => t.Applications)
//             .HasForeignKey(ta => ta.TournamentId)
//             .OnDelete(DeleteBehavior.Cascade); // При удалении турнира удаляются заявки

//         // Связь с командой
//         modelBuilder.Entity<TournamentApplication>()
//             .HasOne(ta => ta.Team)
//             .WithMany(t => t.Applications)
//             .HasForeignKey(ta => ta.TeamId)
//             .OnDelete(DeleteBehavior.Restrict); // Нельзя удалить команду, если она подавала заявки

//         // 5. Tournament 1 --- N Match
//         modelBuilder.Entity<Match>()
//             .HasOne(m => m.Tournament)
//             .WithMany(t => t.Matches)
//             .HasForeignKey(m => m.TournamentId)
//             .OnDelete(DeleteBehavior.Cascade); // При удалении турнира удаляются матчи

//         // 6. User (Judge) 1 --- N Match
//         modelBuilder.Entity<Match>()
//             .HasOne(m => m.Judge)
//             .WithMany(u => u.AssignedMatches)
//             .HasForeignKey(m => m.JudgeId)
//             .OnDelete(DeleteBehavior.SetNull); 
//             // Если судью удаляют из системы, матч не удаляется, 
//             // а JudgeId становится null (матч без судьи). 
//             // Либо используйте Restrict, если удаление судей вообще запрещено.

//         // 7. Связи Матча с Командами (HomeTeam и AwayTeam)
//         // (Уже были в предыдущем примере, дублирую для полноты картины)
//         modelBuilder.Entity<Match>()
//             .HasOne(m => m.HomeTeam)
//             .WithMany() 
//             .HasForeignKey(m => m.HomeTeamId)
//             .OnDelete(DeleteBehavior.Restrict); 

//         modelBuilder.Entity<Match>()
//             .HasOne(m => m.AwayTeam)
//             .WithMany()
//             .HasForeignKey(m => m.AwayTeamId)
//             .OnDelete(DeleteBehavior.Restrict);

//         // 8. Match M --- N Player (через MatchLineup)
//         // Связь с матчем
//         modelBuilder.Entity<MatchLineup>()
//             .HasOne(ml => ml.Match)
//             .WithMany(m => m.Lineups)
//             .HasForeignKey(ml => ml.MatchId)
//             .OnDelete(DeleteBehavior.Cascade); // При удалении матча удаляются его составы

//         // Связь с игроком
//         modelBuilder.Entity<MatchLineup>()
//             .HasOne(ml => ml.Player)
//             .WithMany() // Если в Player нет коллекции MatchLineups
//             .HasForeignKey(ml => ml.PlayerId)
//             .OnDelete(DeleteBehavior.Cascade); // При удалении игрока удаляется его запись в составе

//         // 9. Match 1 --- N MatchEvent
//         modelBuilder.Entity<MatchEvent>()
//             .HasOne(e => e.Match)
//             .WithMany(m => m.Events)
//             .HasForeignKey(e => e.MatchId)
//             .OnDelete(DeleteBehavior.Cascade); // При удалении матча удаляются его события

//         // 10. Match 1 --- 1 Protocol
//         modelBuilder.Entity<Protocol>()
//             .HasOne(p => p.Match)
//             .WithOne(m => m.Protocol)
//             .HasForeignKey<Protocol>(p => p.MatchId)
//             .OnDelete(DeleteBehavior.Cascade); // При удалении матча удаляется протокол

//         // 11. Team 1 --- 1 TeamStats
//         modelBuilder.Entity<TeamStats>()
//             .HasOne<Team>()
//             .WithOne()
//             .HasForeignKey<TeamStats>(s => s.TeamId)
//             .OnDelete(DeleteBehavior.Cascade); 
//             // Внимание: если команда удаляется, ее статистика тоже.

//         // 12. Player 1 --- 1 PlayerStats
//         modelBuilder.Entity<PlayerStats>()
//             .HasOne<Player>()
//             .WithOne()
//             .HasForeignKey<PlayerStats>(s => s.PlayerId)
//             .OnDelete(DeleteBehavior.Cascade);
//     }
// }
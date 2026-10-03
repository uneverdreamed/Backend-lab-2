using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Seismic.Alerts.Consumer.DataAccess.Entities;

namespace Seismic.Alerts.Consumer.DataAccess;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : DbContext(options)
{
    public DbSet<Subscriber> Subscribers => Set<Subscriber>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AlertLogEntry> AlertLog => Set<AlertLogEntry>();
    public DbSet<CriticalAlert> CriticalAlerts => Set<CriticalAlert>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToBinaryConverter>();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subscriber>().HasData(
            new Subscriber { Id = 1, Name = "Первый П.П.", Region = "kamchatka", MinMagnitude = 4.0 },
            new Subscriber { Id = 2, Name = "Второй В.В.", Region = "kamchatka", MinMagnitude = 5.5 },
            new Subscriber { Id = 3, Name = "Третья Т.Т.", Region = "caucasus", MinMagnitude = 4.0 },
            new Subscriber { Id = 4, Name = "Четвертая Ч.Ч.", Region = "caucasus", MinMagnitude = 3.0 },
            new Subscriber { Id = 5, Name = "Пятый П.П.", Region = "altai", MinMagnitude = 4.5 }
        );
    }
}
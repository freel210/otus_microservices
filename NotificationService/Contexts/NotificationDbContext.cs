using NotificationService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NotificationService.ConfigOptions;

namespace NotificationService.Contexts;

public sealed class NotificationDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<Notification> Notifications { get; set; } = null!;

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options, IOptionsSnapshot<PostgresOptions> postgresOptions) : base(options)
    {
        string? dbUser = Environment.GetEnvironmentVariable("DB_NOTIFICATION_USER");
        string? dbPassword = Environment.GetEnvironmentVariable("DB_NOTIFICATION_PASSWORD");

        string? host = postgresOptions.Value.Host;
        int port = postgresOptions.Value.Port;
        string? database = postgresOptions.Value.Database;
        bool pooling = postgresOptions.Value.Pooling;

        _connectionString = $"Host={host};Port={port};Database={database};Username={dbUser};Password={dbPassword};Pooling={pooling};"!;
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
    }
}

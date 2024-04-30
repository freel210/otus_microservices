using DeliveriesService.ConfigOptions;
using DeliveriesService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DeliveriesService.Contexts;

public sealed class DeliveryDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<Delivery> Deliveries { get; set; } = null!;

    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options, IOptionsSnapshot<PostgresOptions> postgresOptions) : base(options)
    {
        string? dbUser = Environment.GetEnvironmentVariable("DB_DELIVERY_USER");
        string? dbPassword = Environment.GetEnvironmentVariable("DB_DELIVERY_PASSWORD");

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

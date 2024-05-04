using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrdersService.ConfigOptions;
using OrdersService.Entities;

namespace OrdersService.Contexts;

public sealed class OrdersDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<BasketItem> BasketItems { get; set; } = null!;

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options, IOptionsSnapshot<PostgresOptions> postgresOptions) : base(options)
    {
        string? dbUser = Environment.GetEnvironmentVariable("DB_ORDERS_USER");
        string? dbPassword = Environment.GetEnvironmentVariable("DB_ORDERS_PASSWORD");

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

using BillingService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BillingService.ConfigOptions;

namespace BillingService.Contexts;

public sealed class BillingDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<Amount> Amounts { get; set; } = null!;

    public BillingDbContext(DbContextOptions<BillingDbContext> options, IOptionsSnapshot<PostgresOptions> postgresOptions) : base(options)
    {
        string? dbUser = Environment.GetEnvironmentVariable("DB_BILLING_USER");
        string? dbPassword = Environment.GetEnvironmentVariable("DB_BILLING_PASSWORD");

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

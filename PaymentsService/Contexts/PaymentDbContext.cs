using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentsService.ConfigOptions;
using PaymentsService.Entities;

namespace PaymentsService.Contexts;

public sealed class PaymentDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<Payment> Payments { get; set; } = null!;

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options, IOptionsSnapshot<PostgresOptions> postgresOptions) : base(options)
    {
        string? dbUser = Environment.GetEnvironmentVariable("DB_PAYMENTS_USER");
        string? dbPassword = Environment.GetEnvironmentVariable("DB_PAYMENTS_PASSWORD");

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

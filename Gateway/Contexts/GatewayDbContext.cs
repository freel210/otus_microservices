using Gateway.ConfigOptions;
using Gateway.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Gateway.Contexts;

public sealed class GatewayDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<DistributedTransaction> Transactions { get; set; } = null!;

    public GatewayDbContext(DbContextOptions<GatewayDbContext> options, IOptionsSnapshot<PostgresOptions> postgresOptions) : base(options)
    {
        string? dbUser = Environment.GetEnvironmentVariable("DB_GATEWAY_USER");
        string? dbPassword = Environment.GetEnvironmentVariable("DB_GATEWAY_PASSWORD");

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

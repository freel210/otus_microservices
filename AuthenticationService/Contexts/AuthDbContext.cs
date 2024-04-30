using AuthenticationService.ConfigOptions;
using AuthenticationService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthenticationService.Contexts;

public sealed class AuthDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<Auth> Auths { get; set; } = null!;

    public AuthDbContext(DbContextOptions<AuthDbContext> options, IOptionsSnapshot<PostgresOptions> postgresOptions) : base(options)
    {
        string? dbUser = Environment.GetEnvironmentVariable("DB_AUTH_USER");
        string? dbPassword = Environment.GetEnvironmentVariable("DB_AUTH_PASSWORD");

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

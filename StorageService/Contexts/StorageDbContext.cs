using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StorageService.ConfigOptions;
using StorageService.Entities;

namespace StorageService.Contexts;

public sealed class StorageDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<Item> Items { get; set; } = null!;

    public StorageDbContext(DbContextOptions<StorageDbContext> options, IOptionsSnapshot<PostgresOptions> postgresOptions) : base(options)
    {
        string? dbUser = Environment.GetEnvironmentVariable("DB_STORAGE_USER");
        string? dbPassword = Environment.GetEnvironmentVariable("DB_STORAGE_PASSWORD");

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

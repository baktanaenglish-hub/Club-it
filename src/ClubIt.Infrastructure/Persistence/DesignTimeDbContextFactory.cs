using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClubIt.Infrastructure.Persistence;

/// <summary>Used by <c>dotnet ef</c> to create PostgreSQL migrations without starting the API.</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CLUBIT_MIGRATIONS_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=clubit;Username=clubit;Password=clubit";

        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connectionString).Options;
        return new AppDbContext(options);
    }
}

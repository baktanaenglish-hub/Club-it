using ClubIt.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ClubIt.Tests.Infrastructure;

/// <summary>Runs the API against a throw-away SQLite file with a controllable clock.</summary>
public class ClubItApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"clubit-tests-{Guid.NewGuid():N}.db");

    public FakeClock Clock { get; } = new(new DateOnly(2026, 6, 15));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Database:Provider", "Sqlite");
        builder.UseSetting("ConnectionStrings:ClubIt", $"Data Source={_databasePath}");
        builder.UseSetting("Jwt:SigningKey", "integration-tests-signing-key-0123456789abcdef");
        builder.UseSetting("RateLimiting:AuthPermitsPerMinute", "10000");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(Clock);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
    }
}

public class FakeClock(DateOnly today) : IClock
{
    public DateOnly Today { get; set; } = today;

    public DateTime UtcNow => Today.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc);
}

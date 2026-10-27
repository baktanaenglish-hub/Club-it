using ClubIt.Application.Abstractions;
using ClubIt.Infrastructure.Identity;
using ClubIt.Infrastructure.Persistence;
using ClubIt.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClubIt.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers persistence, identity and token services. The database provider comes from
    /// <c>Database:Provider</c>: "Postgres" (default, uses migrations) or "Sqlite" (local tryouts and tests).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? DatabaseProviders.Postgres;
        var connectionString = configuration.GetConnectionString("ClubIt")
            ?? throw new InvalidOperationException("Connection string 'ClubIt' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
        {
            if (string.Equals(provider, DatabaseProviders.Sqlite, StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseNpgsql(connectionString);
            }
        });
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddErrorDescriber<HebrewIdentityErrorDescriber>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IClock, IsraelClock>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IHouseholdMembers, HouseholdMembers>();

        return services;
    }
}

public static class DatabaseProviders
{
    public const string Postgres = "Postgres";
    public const string Sqlite = "Sqlite";
}

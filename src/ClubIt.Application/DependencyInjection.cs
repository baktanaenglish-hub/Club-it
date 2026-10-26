using ClubIt.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ClubIt.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CatalogService>();
        services.AddScoped<HouseholdService>();
        services.AddScoped<MerchantService>();
        services.AddScoped<BenefitService>();
        return services;
    }
}

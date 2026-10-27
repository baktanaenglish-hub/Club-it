using ClubIt.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClubIt.Infrastructure.Identity;

/// <summary>The User entity from the spec (Id, Email, DisplayName, HouseholdId), backed by ASP.NET Core Identity.</summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;

    public Guid HouseholdId { get; set; }
    public Household? Household { get; set; }
}

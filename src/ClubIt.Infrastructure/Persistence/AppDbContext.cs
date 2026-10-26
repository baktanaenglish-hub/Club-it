using ClubIt.Application.Abstractions;
using ClubIt.Domain.Entities;
using ClubIt.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClubIt.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IAppDbContext
{
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<MerchantAlias> MerchantAliases => Set<MerchantAlias>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Benefit> Benefits => Set<Benefit>();
    public DbSet<BenefitBranch> BenefitBranches => Set<BenefitBranch>();
    public DbSet<Household> Households => Set<Household>();
    public DbSet<HouseholdClub> HouseholdClubs => Set<HouseholdClub>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

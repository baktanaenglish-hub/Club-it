using ClubIt.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClubIt.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Club> Clubs { get; }
    DbSet<Category> Categories { get; }
    DbSet<Merchant> Merchants { get; }
    DbSet<MerchantAlias> MerchantAliases { get; }
    DbSet<Branch> Branches { get; }
    DbSet<Benefit> Benefits { get; }
    DbSet<BenefitBranch> BenefitBranches { get; }
    DbSet<Household> Households { get; }
    DbSet<HouseholdClub> HouseholdClubs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

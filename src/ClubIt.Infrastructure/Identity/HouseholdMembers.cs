using ClubIt.Application.Abstractions;
using ClubIt.Contracts.Households;
using ClubIt.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClubIt.Infrastructure.Identity;

public class HouseholdMembers(AppDbContext db) : IHouseholdMembers
{
    public async Task<IReadOnlyList<HouseholdMemberDto>> GetMembersAsync(Guid householdId, CancellationToken cancellationToken = default) =>
        await db.Users.AsNoTracking()
            .Where(u => u.HouseholdId == householdId)
            .OrderBy(u => u.DisplayName)
            .Select(u => new HouseholdMemberDto(u.Id, u.DisplayName, u.Email!))
            .ToListAsync(cancellationToken);
}

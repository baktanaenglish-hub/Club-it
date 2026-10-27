using ClubIt.Contracts.Households;

namespace ClubIt.Application.Abstractions;

/// <summary>Looks up household members, which live in the identity store.</summary>
public interface IHouseholdMembers
{
    Task<IReadOnlyList<HouseholdMemberDto>> GetMembersAsync(Guid householdId, CancellationToken cancellationToken = default);
}

using ClubIt.Application.Abstractions;
using ClubIt.Application.Common;
using ClubIt.Contracts.Clubs;
using ClubIt.Contracts.Households;
using ClubIt.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClubIt.Application.Services;

public class HouseholdService(IAppDbContext db, ICurrentUser currentUser, IHouseholdMembers members)
{
    public async Task<HouseholdDto> GetHouseholdAsync(CancellationToken cancellationToken = default)
    {
        var household = await db.Households.AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == currentUser.HouseholdId, cancellationToken)
            ?? throw new NotFoundException("משק הבית לא נמצא.");

        var householdMembers = await members.GetMembersAsync(household.Id, cancellationToken);
        return new HouseholdDto(household.Id, household.Name, household.InviteCode, householdMembers);
    }

    public async Task<IReadOnlyList<ClubDto>> GetClubsAsync(CancellationToken cancellationToken = default)
    {
        var clubs = await db.HouseholdClubs.AsNoTracking()
            .Where(hc => hc.HouseholdId == currentUser.HouseholdId)
            .Select(hc => hc.Club!)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return clubs.Select(c => c.ToDto()).ToList();
    }

    /// <summary>Replaces the household's club selection with <paramref name="request"/>.</summary>
    public async Task<IReadOnlyList<ClubDto>> UpdateClubsAsync(UpdateHouseholdClubsRequest request, CancellationToken cancellationToken = default)
    {
        var requested = (request.ClubIds ?? []).Distinct().ToHashSet();

        var existingIds = await db.Clubs.Where(c => requested.Contains(c.Id)).Select(c => c.Id).ToListAsync(cancellationToken);
        var unknown = requested.Except(existingIds).ToList();
        if (unknown.Count > 0)
        {
            throw new ValidationException(nameof(request.ClubIds), $"מועדונים לא קיימים: {string.Join(", ", unknown)}");
        }

        var current = await db.HouseholdClubs
            .Where(hc => hc.HouseholdId == currentUser.HouseholdId)
            .ToListAsync(cancellationToken);

        db.HouseholdClubs.RemoveRange(current.Where(hc => !requested.Contains(hc.ClubId)));

        var currentIds = current.Select(hc => hc.ClubId).ToHashSet();
        foreach (var clubId in requested.Where(id => !currentIds.Contains(id)))
        {
            db.HouseholdClubs.Add(new HouseholdClub { HouseholdId = currentUser.HouseholdId, ClubId = clubId });
        }

        await db.SaveChangesAsync(cancellationToken);
        return await GetClubsAsync(cancellationToken);
    }
}

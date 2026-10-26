using System.Linq.Expressions;
using ClubIt.Application.Abstractions;
using ClubIt.Application.Common;
using ClubIt.Contracts.Benefits;
using ClubIt.Contracts.Merchants;
using ClubIt.Domain.Common;
using ClubIt.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClubIt.Application.Services;

public class BenefitService(IAppDbContext db, ICurrentUser currentUser, IClock clock)
{
    public const int TitleMaxLength = 200;

    /// <summary>
    /// Valid benefits of merchants matching <paramref name="query"/> (name or alias), from the
    /// household's clubs, optionally limited to those that apply at <paramref name="branchId"/>.
    /// Sorted by club, then by the nearest expiry date.
    /// </summary>
    public async Task<IReadOnlyList<BenefitDto>> SearchAsync(string? query, int? branchId, CancellationToken cancellationToken = default)
    {
        var normalized = NameNormalizer.Normalize(query);
        if (normalized.Length == 0 && branchId is null)
        {
            throw new ValidationException("q", "יש להזין שם חנות לחיפוש.");
        }

        var merchantIds = MerchantService.MatchingMerchants(db, query).Select(m => m.Id);
        var benefits = AvailableBenefits().Where(b => merchantIds.Contains(b.MerchantId));

        if (branchId is { } id)
        {
            var branch = await db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
                ?? throw new NotFoundException("הסניף לא נמצא.");

            // A branch belongs to a single merchant, so only that merchant's benefits are relevant.
            benefits = benefits.Where(b =>
                b.MerchantId == branch.MerchantId &&
                (b.AllBranches || b.Branches.Any(bb => bb.BranchId == id)));
        }

        return await ToDtosAsync(benefits, cancellationToken);
    }

    /// <summary>Valid benefits from the household's clubs in entertainment categories (movies, shows, attractions).</summary>
    public async Task<IReadOnlyList<BenefitDto>> GetEntertainmentAsync(int? clubId, CancellationToken cancellationToken = default)
    {
        var benefits = AvailableBenefits().Where(b => b.Merchant!.Category!.IsEntertainment);
        if (clubId is { } id)
        {
            benefits = benefits.Where(b => b.ClubId == id);
        }

        return await ToDtosAsync(benefits, cancellationToken);
    }

    public async Task<BenefitDto> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var householdId = currentUser.HouseholdId;
        var row = await VisibleBenefits()
            .Where(b => b.Id == id)
            .Select(ToRow(householdId))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("ההטבה לא נמצאה.");
        return row.ToDto();
    }

    public async Task<BenefitDto> CreateAsync(SaveBenefitRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(request, cancellationToken);

        var now = clock.UtcNow;
        var benefit = new Benefit
        {
            Title = request.Title.Trim(),
            HouseholdId = currentUser.HouseholdId,
            CreatedAtUtc = now
        };
        Apply(benefit, request, now);

        db.Benefits.Add(benefit);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(benefit.Id, cancellationToken);
    }

    public async Task<BenefitDto> UpdateAsync(int id, SaveBenefitRequest request, CancellationToken cancellationToken = default)
    {
        var benefit = await LoadEditableAsync(id, cancellationToken);
        await ValidateAsync(request, cancellationToken);

        Apply(benefit, request, clock.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(benefit.Id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var benefit = await LoadEditableAsync(id, cancellationToken);
        db.Benefits.Remove(benefit);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Benefits the household may see: its own, and shared ones (no owning household).</summary>
    private IQueryable<Benefit> VisibleBenefits()
    {
        var householdId = currentUser.HouseholdId;
        return db.Benefits.AsNoTracking().Where(b => b.HouseholdId == null || b.HouseholdId == householdId);
    }

    /// <summary>Visible benefits that are valid today and belong to one of the household's clubs.</summary>
    private IQueryable<Benefit> AvailableBenefits()
    {
        var householdId = currentUser.HouseholdId;
        var today = clock.Today;
        var clubIds = db.HouseholdClubs.Where(hc => hc.HouseholdId == householdId).Select(hc => hc.ClubId);

        return VisibleBenefits().Where(b =>
            clubIds.Contains(b.ClubId) &&
            (b.ValidFrom == null || b.ValidFrom <= today) &&
            b.ValidUntil >= today);
    }

    private async Task<IReadOnlyList<BenefitDto>> ToDtosAsync(IQueryable<Benefit> benefits, CancellationToken cancellationToken)
    {
        var rows = await benefits
            .OrderBy(b => b.Club!.Name)
            .ThenBy(b => b.ValidUntil)
            .ThenBy(b => b.Id)
            .Select(ToRow(currentUser.HouseholdId))
            .ToListAsync(cancellationToken);
        return rows.Select(r => r.ToDto()).ToList();
    }

    private static Expression<Func<Benefit, BenefitRow>> ToRow(Guid householdId) => b => new BenefitRow(
        b.Id,
        b.ClubId,
        b.Club!.Name,
        b.Club.LogoUrl,
        b.MerchantId,
        b.Merchant!.Name,
        b.Merchant.CategoryId,
        b.Merchant.Category!.Name,
        b.Merchant.Category.IsEntertainment,
        b.Title,
        b.Description,
        b.BenefitType,
        b.Value,
        b.ValidFrom,
        b.ValidUntil,
        b.AllBranches,
        b.Branches
            .OrderBy(bb => bb.Branch!.City).ThenBy(bb => bb.Branch!.Name)
            .Select(bb => new BranchDto(
                bb.Branch!.Id, bb.Branch.MerchantId, bb.Branch.Name, bb.Branch.City,
                bb.Branch.Address, bb.Branch.Latitude, bb.Branch.Longitude))
            .ToList(),
        b.RedeemUrl,
        b.Notes,
        b.HouseholdId == householdId);

    private async Task<Benefit> LoadEditableAsync(int id, CancellationToken cancellationToken)
    {
        var benefit = await db.Benefits.Include(b => b.Branches).FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (benefit is null || (benefit.HouseholdId is not null && benefit.HouseholdId != currentUser.HouseholdId))
        {
            throw new NotFoundException("ההטבה לא נמצאה.");
        }

        if (benefit.HouseholdId is null)
        {
            throw new ForbiddenException("לא ניתן לערוך הטבה משותפת.");
        }

        return benefit;
    }

    private static void Apply(Benefit benefit, SaveBenefitRequest request, DateTime now)
    {
        benefit.ClubId = request.ClubId;
        benefit.MerchantId = request.MerchantId;
        benefit.Title = request.Title.Trim();
        benefit.Description = NullIfEmpty(request.Description);
        benefit.BenefitType = (Domain.Enums.BenefitType)request.BenefitType;
        benefit.Value = request.Value;
        benefit.ValidFrom = request.ValidFrom;
        benefit.ValidUntil = request.ValidUntil!.Value;
        benefit.AllBranches = request.AllBranches;
        benefit.RedeemUrl = NullIfEmpty(request.RedeemUrl);
        benefit.Notes = NullIfEmpty(request.Notes);
        benefit.UpdatedAtUtc = now;

        var branchIds = request.AllBranches ? [] : (request.BranchIds ?? []).ToHashSet();
        foreach (var removed in benefit.Branches.Where(bb => !branchIds.Contains(bb.BranchId)).ToList())
        {
            benefit.Branches.Remove(removed);
        }

        var existing = benefit.Branches.Select(bb => bb.BranchId).ToHashSet();
        foreach (var branchId in branchIds.Where(id => !existing.Contains(id)))
        {
            benefit.Branches.Add(new BenefitBranch { BranchId = branchId });
        }
    }

    private async Task ValidateAsync(SaveBenefitRequest request, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors.Add(nameof(request.Title), "יש להזין כותרת להטבה.");
        }
        else if (request.Title.Trim().Length > TitleMaxLength)
        {
            errors.Add(nameof(request.Title), $"הכותרת ארוכה מ-{TitleMaxLength} תווים.");
        }

        if (!Enum.IsDefined(request.BenefitType))
        {
            errors.Add(nameof(request.BenefitType), "סוג הטבה לא תקין.");
        }

        if (request.ValidUntil is null)
        {
            errors.Add(nameof(request.ValidUntil), "יש להזין תאריך תוקף להטבה.");
        }
        else if (request.ValidFrom is { } from && from > request.ValidUntil)
        {
            errors.Add(nameof(request.ValidFrom), "תאריך ההתחלה מאוחר מתאריך התפוגה.");
        }

        if (request.Value is < 0)
        {
            errors.Add(nameof(request.Value), "ערך ההטבה אינו יכול להיות שלילי.");
        }
        else if (request.BenefitType == Contracts.BenefitType.Percentage && request.Value is > 100)
        {
            errors.Add(nameof(request.Value), "אחוז ההנחה אינו יכול לעלות על 100.");
        }

        if (!string.IsNullOrWhiteSpace(request.RedeemUrl) &&
            !Uri.TryCreate(request.RedeemUrl.Trim(), UriKind.Absolute, out _))
        {
            errors.Add(nameof(request.RedeemUrl), "קישור המימוש אינו כתובת תקינה.");
        }

        if (!await db.Clubs.AnyAsync(c => c.Id == request.ClubId, cancellationToken))
        {
            errors.Add(nameof(request.ClubId), "יש לבחור מועדון קיים.");
        }

        if (!await db.Merchants.AnyAsync(m => m.Id == request.MerchantId, cancellationToken))
        {
            errors.Add(nameof(request.MerchantId), "יש לבחור רשת קיימת.");
        }
        else if (!request.AllBranches)
        {
            var branchIds = (request.BranchIds ?? []).Distinct().ToList();
            if (branchIds.Count == 0)
            {
                errors.Add(nameof(request.BranchIds), "יש לבחור לפחות סניף אחד, או לסמן שההטבה תקפה בכל הסניפים.");
            }
            else
            {
                var matching = await db.Branches.CountAsync(
                    b => branchIds.Contains(b.Id) && b.MerchantId == request.MerchantId, cancellationToken);
                if (matching != branchIds.Count)
                {
                    errors.Add(nameof(request.BranchIds), "חלק מהסניפים אינם שייכים לרשת שנבחרה.");
                }
            }
        }

        errors.ThrowIfAny();
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>
    /// Query projection that keeps the domain enum: the enum is stored as a string, so converting
    /// it to the contract enum must happen in memory rather than in SQL.
    /// </summary>
    private sealed record BenefitRow(
        int Id,
        int ClubId,
        string ClubName,
        string? ClubLogoUrl,
        int MerchantId,
        string MerchantName,
        int CategoryId,
        string CategoryName,
        bool IsEntertainment,
        string Title,
        string? Description,
        Domain.Enums.BenefitType BenefitType,
        decimal? Value,
        DateOnly? ValidFrom,
        DateOnly ValidUntil,
        bool AllBranches,
        List<BranchDto> Branches,
        string? RedeemUrl,
        string? Notes,
        bool CanEdit)
    {
        public BenefitDto ToDto() => new(
            Id, ClubId, ClubName, ClubLogoUrl, MerchantId, MerchantName, CategoryId, CategoryName, IsEntertainment,
            Title, Description, (Contracts.BenefitType)BenefitType, Value, ValidFrom, ValidUntil, AllBranches,
            Branches, RedeemUrl, Notes, CanEdit);
    }
}

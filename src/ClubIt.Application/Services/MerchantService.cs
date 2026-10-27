using ClubIt.Application.Abstractions;
using ClubIt.Application.Common;
using ClubIt.Contracts.Merchants;
using ClubIt.Domain.Common;
using ClubIt.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClubIt.Application.Services;

public class MerchantService(IAppDbContext db)
{
    public const int MaxSearchResults = 20;

    /// <summary>
    /// Finds merchants whose name or one of their aliases contains <paramref name="query"/>,
    /// ignoring case, whitespace and punctuation. Prefix matches come first.
    /// </summary>
    public async Task<IReadOnlyList<MerchantDto>> SearchAsync(string? query, CancellationToken cancellationToken = default)
    {
        var merchants = await MatchingMerchants(db, query)
            .AsNoTracking()
            .Include(m => m.Category)
            .Include(m => m.Aliases)
            .ToListAsync(cancellationToken);

        var normalized = NameNormalizer.Normalize(query);
        return merchants
            .OrderByDescending(m => normalized.Length > 0 &&
                (m.NormalizedName.StartsWith(normalized, StringComparison.Ordinal) ||
                 m.Aliases.Any(a => a.NormalizedAlias.StartsWith(normalized, StringComparison.Ordinal))))
            .ThenBy(m => m.Name)
            .Take(MaxSearchResults)
            .Select(m => m.ToDto())
            .ToList();
    }

    /// <summary>Merchants matching a free-text query. An empty query matches everything.</summary>
    internal static IQueryable<Merchant> MatchingMerchants(IAppDbContext db, string? query)
    {
        var normalized = NameNormalizer.Normalize(query);
        if (normalized.Length == 0)
        {
            return db.Merchants;
        }

        return db.Merchants.Where(m =>
            m.NormalizedName.Contains(normalized) ||
            m.Aliases.Any(a => a.NormalizedAlias.Contains(normalized)));
    }

    public async Task<MerchantDto> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var merchant = await db.Merchants.AsNoTracking()
            .Include(m => m.Category)
            .Include(m => m.Aliases)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new NotFoundException("הרשת לא נמצאה.");
        return merchant.ToDto();
    }

    public async Task<MerchantDto> CreateAsync(CreateMerchantRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(null, request.Name, request.CategoryId, cancellationToken);

        var merchant = new Merchant { Name = request.Name.Trim(), CategoryId = request.CategoryId };
        SetAliases(merchant, request.Aliases);

        db.Merchants.Add(merchant);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(merchant.Id, cancellationToken);
    }

    public async Task<MerchantDto> UpdateAsync(int id, UpdateMerchantRequest request, CancellationToken cancellationToken = default)
    {
        var merchant = await db.Merchants.Include(m => m.Aliases).FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new NotFoundException("הרשת לא נמצאה.");

        await ValidateAsync(id, request.Name, request.CategoryId, cancellationToken);

        merchant.Name = request.Name.Trim();
        merchant.CategoryId = request.CategoryId;
        SetAliases(merchant, request.Aliases);

        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(merchant.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<BranchDto>> GetBranchesAsync(int merchantId, CancellationToken cancellationToken = default)
    {
        await EnsureMerchantExistsAsync(merchantId, cancellationToken);

        var branches = await db.Branches.AsNoTracking()
            .Where(b => b.MerchantId == merchantId)
            .OrderBy(b => b.City).ThenBy(b => b.Name)
            .ToListAsync(cancellationToken);
        return branches.Select(b => b.ToDto()).ToList();
    }

    public async Task<BranchDto> CreateBranchAsync(int merchantId, CreateBranchRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureMerchantExistsAsync(merchantId, cancellationToken);

        var errors = new ValidationErrors();
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add(nameof(request.Name), "יש להזין שם סניף.");
        }
        if (request.Latitude is < -90 or > 90)
        {
            errors.Add(nameof(request.Latitude), "קו רוחב לא תקין.");
        }
        if (request.Longitude is < -180 or > 180)
        {
            errors.Add(nameof(request.Longitude), "קו אורך לא תקין.");
        }
        errors.ThrowIfAny();

        var branch = new Branch
        {
            MerchantId = merchantId,
            Name = request.Name.Trim(),
            City = request.City?.Trim(),
            Address = request.Address?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };

        db.Branches.Add(branch);
        await db.SaveChangesAsync(cancellationToken);
        return branch.ToDto();
    }

    private async Task EnsureMerchantExistsAsync(int merchantId, CancellationToken cancellationToken)
    {
        if (!await db.Merchants.AnyAsync(m => m.Id == merchantId, cancellationToken))
        {
            throw new NotFoundException("הרשת לא נמצאה.");
        }
    }

    private async Task ValidateAsync(int? merchantId, string? name, int categoryId, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();
        var normalized = NameNormalizer.Normalize(name);

        if (normalized.Length == 0)
        {
            errors.Add("Name", "יש להזין שם רשת.");
        }
        else if (await db.Merchants.AnyAsync(m => m.NormalizedName == normalized && m.Id != merchantId, cancellationToken))
        {
            errors.Add("Name", "רשת בשם זה כבר קיימת.");
        }

        if (!await db.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken))
        {
            errors.Add("CategoryId", "יש לבחור קטגוריה קיימת.");
        }

        errors.ThrowIfAny();
    }

    private static void SetAliases(Merchant merchant, IEnumerable<string>? aliases)
    {
        var wanted = (aliases ?? [])
            .Select(a => a.Trim())
            .Where(a => NameNormalizer.Normalize(a).Length > 0 && NameNormalizer.Normalize(a) != merchant.NormalizedName)
            .DistinctBy(NameNormalizer.Normalize)
            .ToList();

        var wantedKeys = wanted.Select(NameNormalizer.Normalize).ToHashSet();
        foreach (var removed in merchant.Aliases.Where(a => !wantedKeys.Contains(a.NormalizedAlias)).ToList())
        {
            merchant.Aliases.Remove(removed);
        }

        var existingKeys = merchant.Aliases.Select(a => a.NormalizedAlias).ToHashSet();
        foreach (var alias in wanted.Where(a => !existingKeys.Contains(NameNormalizer.Normalize(a))))
        {
            merchant.Aliases.Add(new MerchantAlias { Alias = alias });
        }
    }
}

using ClubIt.Application.Abstractions;
using ClubIt.Application.Common;
using ClubIt.Contracts.Categories;
using ClubIt.Contracts.Clubs;
using Microsoft.EntityFrameworkCore;

namespace ClubIt.Application.Services;

/// <summary>Read-only reference data: clubs and categories.</summary>
public class CatalogService(IAppDbContext db)
{
    public async Task<IReadOnlyList<ClubDto>> GetClubsAsync(CancellationToken cancellationToken = default)
    {
        var clubs = await db.Clubs.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);
        return clubs.Select(c => c.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);
        return categories.Select(c => c.ToDto()).ToList();
    }
}

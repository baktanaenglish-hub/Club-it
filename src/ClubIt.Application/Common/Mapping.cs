using ClubIt.Contracts.Categories;
using ClubIt.Contracts.Clubs;
using ClubIt.Contracts.Merchants;
using ClubIt.Domain.Entities;

namespace ClubIt.Application.Common;

internal static class Mapping
{
    public static ClubDto ToDto(this Club club) =>
        new(club.Id, club.Name, (Contracts.ClubType)club.Type, club.LogoUrl, club.WebsiteUrl);

    public static CategoryDto ToDto(this Category category) =>
        new(category.Id, category.Name, category.IsEntertainment);

    public static BranchDto ToDto(this Branch branch) =>
        new(branch.Id, branch.MerchantId, branch.Name, branch.City, branch.Address, branch.Latitude, branch.Longitude);

    public static MerchantDto ToDto(this Merchant merchant) =>
        new(
            merchant.Id,
            merchant.Name,
            merchant.CategoryId,
            merchant.Category?.Name ?? string.Empty,
            merchant.Aliases.Select(a => a.Alias).OrderBy(a => a).ToList());
}

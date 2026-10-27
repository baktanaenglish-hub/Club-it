using ClubIt.Contracts.Merchants;

namespace ClubIt.Contracts.Benefits;

public record BenefitDto(
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
    BenefitType BenefitType,
    decimal? Value,
    DateOnly? ValidFrom,
    DateOnly ValidUntil,
    bool AllBranches,
    IReadOnlyList<BranchDto> Branches,
    string? RedeemUrl,
    string? Notes,
    bool CanEdit);

/// <summary>Used for both create and update.</summary>
public record SaveBenefitRequest(
    int ClubId,
    int MerchantId,
    string Title,
    string? Description,
    BenefitType BenefitType,
    decimal? Value,
    DateOnly? ValidFrom,
    DateOnly? ValidUntil,
    bool AllBranches,
    IReadOnlyList<int>? BranchIds,
    string? RedeemUrl,
    string? Notes);

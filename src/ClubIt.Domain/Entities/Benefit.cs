using ClubIt.Domain.Enums;

namespace ClubIt.Domain.Entities;

public class Benefit
{
    public int Id { get; set; }

    public int ClubId { get; set; }
    public Club? Club { get; set; }

    public int MerchantId { get; set; }
    public Merchant? Merchant { get; set; }

    /// <summary>
    /// The household that entered the benefit and may edit it. Null means a shared benefit
    /// visible to every household (phase 2 community benefits).
    /// </summary>
    public Guid? HouseholdId { get; set; }
    public Household? Household { get; set; }

    public required string Title { get; set; }
    public string? Description { get; set; }
    public BenefitType BenefitType { get; set; }

    /// <summary>Percent for <see cref="BenefitType.Percentage"/>, amount in ₪ otherwise. Optional for coupons.</summary>
    public decimal? Value { get; set; }

    public DateOnly? ValidFrom { get; set; }
    public DateOnly ValidUntil { get; set; }

    /// <summary>When false, the benefit applies only to the branches in <see cref="Branches"/>.</summary>
    public bool AllBranches { get; set; } = true;

    public string? RedeemUrl { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<BenefitBranch> Branches { get; set; } = new List<BenefitBranch>();

    public bool IsValidOn(DateOnly date) =>
        (ValidFrom is null || ValidFrom <= date) && ValidUntil >= date;
}

using ClubIt.Contracts.Benefits;
using ClubIt.Mobile.Core.Formatting;

namespace ClubIt.Mobile.Core.ViewModels;

/// <summary>A benefit row in the search and entertainment lists.</summary>
public class BenefitItemViewModel(BenefitDto benefit, DateOnly today)
{
    public BenefitDto Benefit { get; } = benefit;

    public int Id => Benefit.Id;
    public string Title => Benefit.Title;
    public string ClubName => Benefit.ClubName;
    public string? ClubLogoUrl => Benefit.ClubLogoUrl;
    public string MerchantName => Benefit.MerchantName;
    public string ValueText => BenefitFormatter.ValueText(Benefit);
    public string ValidityText => BenefitFormatter.ValidityText(Benefit, today);
    public string BranchesText => BenefitFormatter.BranchesText(Benefit);
    public bool ExpiresSoon => Benefit.ValidUntil.DayNumber - today.DayNumber <= 7;
    public bool HasRedeemUrl => !string.IsNullOrWhiteSpace(Benefit.RedeemUrl);
}

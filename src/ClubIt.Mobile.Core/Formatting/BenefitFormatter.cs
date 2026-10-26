using System.Globalization;
using ClubIt.Contracts;
using ClubIt.Contracts.Benefits;

namespace ClubIt.Mobile.Core.Formatting;

public static class BenefitFormatter
{
    private static readonly CultureInfo Hebrew = CultureInfo.GetCultureInfo("he-IL");

    public static string TypeName(BenefitType type) => type switch
    {
        BenefitType.Percentage => "הנחה באחוזים",
        BenefitType.FixedAmount => "הנחה בשקלים",
        BenefitType.Coupon => "קופון",
        BenefitType.DiscountedTicket => "כרטיס מוזל",
        _ => type.ToString()
    };

    /// <summary>Short value badge, e.g. "10%", "₪20", "כרטיס ב-₪25", "קופון".</summary>
    public static string ValueText(BenefitDto benefit) => ValueText(benefit.BenefitType, benefit.Value);

    public static string ValueText(BenefitType type, decimal? value) => (type, value) switch
    {
        (BenefitType.Percentage, { } v) => $"{v.ToString("0.##", Hebrew)}%",
        (BenefitType.FixedAmount, { } v) => $"₪{v.ToString("0.##", Hebrew)} הנחה",
        (BenefitType.DiscountedTicket, { } v) => $"כרטיס ב-₪{v.ToString("0.##", Hebrew)}",
        _ => TypeName(type)
    };

    public static string ValidityText(BenefitDto benefit, DateOnly today)
    {
        var until = benefit.ValidUntil.ToString("dd/MM/yyyy", Hebrew);
        var daysLeft = benefit.ValidUntil.DayNumber - today.DayNumber;
        return daysLeft switch
        {
            < 0 => $"פג תוקף ב-{until}",
            0 => "בתוקף עד היום",
            1 => "בתוקף עד מחר",
            <= 7 => $"בתוקף עוד {daysLeft} ימים ({until})",
            _ => $"בתוקף עד {until}"
        };
    }

    public static string BranchesText(BenefitDto benefit) =>
        benefit.AllBranches || benefit.Branches.Count == 0
            ? "בכל הסניפים"
            : string.Join(", ", benefit.Branches.Select(b => string.IsNullOrEmpty(b.City) ? b.Name : $"{b.Name} ({b.City})"));
}

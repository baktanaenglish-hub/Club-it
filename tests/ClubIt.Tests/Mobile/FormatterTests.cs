using ClubIt.Contracts;
using ClubIt.Mobile.Core.Formatting;
using ClubIt.Mobile.Core.ViewModels;

namespace ClubIt.Tests.Mobile;

public class FormatterTests
{
    [Theory]
    [InlineData(BenefitType.Percentage, 10.0, "10%")]
    [InlineData(BenefitType.Percentage, 12.5, "12.5%")]
    [InlineData(BenefitType.FixedAmount, 30.0, "₪30 הנחה")]
    [InlineData(BenefitType.DiscountedTicket, 25.0, "כרטיס ב-₪25")]
    [InlineData(BenefitType.Coupon, null, "קופון")]
    public void Value_text_matches_benefit_type(BenefitType type, double? value, string expected) =>
        Assert.Equal(expected, BenefitFormatter.ValueText(type, value is null ? null : (decimal)value));

    [Fact]
    public void Aliases_are_split_on_commas_and_trimmed() =>
        Assert.Equal(["שופר סל", "shufersal"], MerchantsViewModel.ParseAliases(" שופר סל ,shufersal,, "));
}

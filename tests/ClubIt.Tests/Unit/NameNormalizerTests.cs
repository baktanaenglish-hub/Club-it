using ClubIt.Domain.Common;

namespace ClubIt.Tests.Unit;

public class NameNormalizerTests
{
    [Theory]
    [InlineData("שופרסל", "שופרסל")]
    [InlineData("שופר סל", "שופרסל")]
    [InlineData("  שופר-סל ", "שופרסל")]
    [InlineData("Shufersal", "shufersal")]
    [InlineData("SHUFER SAL", "shufersal")]
    [InlineData("סופר-פארם", "סופרפארם")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Normalize_ignores_case_whitespace_and_punctuation(string? input, string expected) =>
        Assert.Equal(expected, NameNormalizer.Normalize(input));
}

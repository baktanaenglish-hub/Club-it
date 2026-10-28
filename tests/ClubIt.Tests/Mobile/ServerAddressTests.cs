using ClubIt.Mobile.Core.Services;

namespace ClubIt.Tests.Mobile;

public class ServerAddressTests
{
    [Theory]
    [InlineData("clubit-api.onrender.com", "https://clubit-api.onrender.com/")]
    [InlineData(" https://clubit-api.onrender.com ", "https://clubit-api.onrender.com/")]
    [InlineData("https://clubit-api.onrender.com/", "https://clubit-api.onrender.com/")]
    [InlineData("http://10.0.2.2:5136", "http://10.0.2.2:5136/")]
    [InlineData("https://example.com/clubit?x=1", "https://example.com/clubit/")]
    public void Valid_addresses_are_normalized(string text, string expected)
    {
        Assert.True(ServerAddress.TryParse(text, out var address));
        Assert.Equal(expected, address!.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("ftp://example.com")]
    [InlineData("not a url")]
    public void Invalid_addresses_are_rejected(string? text) =>
        Assert.False(ServerAddress.TryParse(text, out _));
}

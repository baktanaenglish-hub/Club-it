using System.Net;
using System.Net.Http.Json;
using ClubIt.Contracts.Auth;
using ClubIt.Contracts.Clubs;
using ClubIt.Contracts.Households;
using ClubIt.Tests.Infrastructure;

namespace ClubIt.Tests.Api;

public class AuthTests(ClubItApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Register_creates_household_with_invite_code()
    {
        var (client, auth) = await RegisterAsync("משפחת כהן");

        Assert.False(string.IsNullOrEmpty(auth.Token));
        Assert.Equal("משפחת כהן", auth.User.HouseholdName);

        var household = await GetAsync<HouseholdDto>(client, "/api/household");
        Assert.Equal(auth.User.HouseholdId, household.Id);
        Assert.Equal(8, household.InviteCode.Length);
        Assert.Single(household.Members);
    }

    [Fact]
    public async Task Partner_joins_with_invite_code_and_shares_club_settings()
    {
        var (owner, ownerAuth) = await RegisterAsync();
        var household = await GetAsync<HouseholdDto>(owner, "/api/household");
        await SetClubsAsync(owner, Max, Cal);

        var (partner, partnerAuth) = await RegisterAsync(householdName: null, inviteCode: household.InviteCode.ToLowerInvariant());

        Assert.Equal(ownerAuth.User.HouseholdId, partnerAuth.User.HouseholdId);
        var partnerClubs = await GetAsync<List<ClubDto>>(partner, "/api/household/clubs");
        Assert.Equal([Max, Cal], partnerClubs.Select(c => c.Id).Order());

        var members = (await GetAsync<HouseholdDto>(partner, "/api/household")).Members;
        Assert.Equal(2, members.Count);
    }

    [Theory]
    [InlineData("בית", "ABCDEFGH")]
    [InlineData(null, null)]
    public async Task Register_requires_exactly_one_of_household_name_or_invite_code(string? householdName, string? inviteCode)
    {
        var response = await AnonymousClient().PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("a@example.com", "Password1", "שם", householdName, inviteCode), Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_with_unknown_invite_code_fails()
    {
        var response = await AnonymousClient().PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("b@example.com", "Password1", "שם", null, "NOPE0000"), Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_with_duplicate_email_or_weak_password_fails()
    {
        var client = AnonymousClient();
        var email = $"dup-{Guid.NewGuid():N}@example.com";
        var first = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1", "א", "בית", null), Json);
        await EnsureStatusAsync(first, HttpStatusCode.OK);

        var duplicate = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1", "ב", "בית", null), Json);
        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        Assert.Contains("כבר רשומה", await duplicate.Content.ReadAsStringAsync());

        var weak = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest($"weak-{Guid.NewGuid():N}@example.com", "short", "ג", "בית", null), Json);
        Assert.Equal(HttpStatusCode.BadRequest, weak.StatusCode);
    }

    [Fact]
    public async Task Login_returns_token_for_valid_credentials_only()
    {
        var client = AnonymousClient();
        var email = $"login-{Guid.NewGuid():N}@example.com";
        await EnsureStatusAsync(
            await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1", "א", "בית", null), Json),
            HttpStatusCode.OK);

        var ok = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password1"), Json);
        await EnsureStatusAsync(ok, HttpStatusCode.OK);
        var auth = await ok.Content.ReadFromJsonAsync<AuthResponse>(Json);
        Assert.Equal(email, auth!.User.Email);

        var wrong = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "WrongPass1"), Json);
        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);

        var unknown = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("nobody@example.com", "Password1"), Json);
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
    }

    [Theory]
    [InlineData("/api/clubs")]
    [InlineData("/api/household/clubs")]
    [InlineData("/api/benefits/search?q=שופרסל")]
    [InlineData("/api/benefits/entertainment")]
    [InlineData("/api/merchants?q=שופ")]
    [InlineData("/api/categories")]
    public async Task Endpoints_require_authentication(string url)
    {
        var response = await AnonymousClient().GetAsync(url);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClubIt.Contracts;
using ClubIt.Contracts.Auth;
using ClubIt.Contracts.Benefits;
using ClubIt.Contracts.Households;

namespace ClubIt.Tests.Infrastructure;

public abstract class ApiTestBase : IClassFixture<ClubItApiFactory>
{
    // Seeded reference data (see CatalogSeed).
    protected const int Max = 1, Cal = 2, Isracard = 3;
    protected const int Shufersal = 1, RamiLevy = 2, CinemaCity = 12, Cameri = 14;
    protected const int ShufersalAzrieli = 1, ShufersalRamatAviv = 2, CinemaCityGlilot = 4;

    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected ApiTestBase(ClubItApiFactory factory)
    {
        Factory = factory;
    }

    protected ClubItApiFactory Factory { get; }

    protected DateOnly Today => Factory.Clock.Today;

    protected HttpClient AnonymousClient() => Factory.CreateClient();

    /// <summary>Registers a new user in a new household and returns an authenticated client.</summary>
    protected async Task<(HttpClient Client, AuthResponse Auth)> RegisterAsync(string? householdName = "בית הבדיקות", string? inviteCode = null)
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            $"user-{Guid.NewGuid():N}@example.com", "Password1", "משתמש בדיקה", householdName, inviteCode), Json);
        await EnsureStatusAsync(response, HttpStatusCode.OK);

        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>(Json))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return (client, auth);
    }

    protected static async Task SetClubsAsync(HttpClient client, params int[] clubIds)
    {
        var response = await client.PutAsJsonAsync("/api/household/clubs", new UpdateHouseholdClubsRequest(clubIds), Json);
        await EnsureStatusAsync(response, HttpStatusCode.OK);
    }

    protected SaveBenefitRequest NewBenefit(
        int clubId,
        int merchantId,
        string title = "10% הנחה",
        DateOnly? validFrom = null,
        DateOnly? validUntil = null,
        bool allBranches = true,
        int[]? branchIds = null) =>
        new(clubId, merchantId, title, "תיאור", BenefitType.Percentage, 10,
            validFrom, validUntil ?? Today.AddMonths(1), allBranches, branchIds,
            "https://www.example.com/redeem", null);

    protected static async Task<BenefitDto> CreateBenefitAsync(HttpClient client, SaveBenefitRequest request)
    {
        var response = await client.PostAsJsonAsync("/api/benefits", request, Json);
        await EnsureStatusAsync(response, HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<BenefitDto>(Json))!;
    }

    protected static async Task<T> GetAsync<T>(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        await EnsureStatusAsync(response, HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }

    protected static async Task EnsureStatusAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        if (response.StatusCode != expected)
        {
            var body = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Expected {(int)expected} but got {(int)response.StatusCode}: {body}");
        }
    }
}

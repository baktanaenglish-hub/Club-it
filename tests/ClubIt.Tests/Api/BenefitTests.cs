using System.Net;
using System.Net.Http.Json;
using ClubIt.Contracts.Benefits;
using ClubIt.Tests.Infrastructure;

namespace ClubIt.Tests.Api;

public class BenefitTests(ClubItApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Benefit_without_valid_until_is_rejected()
    {
        var (client, _) = await RegisterAsync();

        var request = NewBenefit(Max, Shufersal) with { ValidUntil = null };
        var response = await client.PostAsJsonAsync("/api/benefits", request, Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("ValidUntil", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Benefit_validation_rejects_bad_input()
    {
        var (client, _) = await RegisterAsync();

        SaveBenefitRequest[] invalid =
        [
            NewBenefit(Max, Shufersal, title: " "),
            NewBenefit(Max, Shufersal, validFrom: Today.AddDays(10), validUntil: Today.AddDays(5)),
            NewBenefit(9999, Shufersal),
            NewBenefit(Max, 9999),
            NewBenefit(Max, Shufersal, allBranches: false),
            NewBenefit(Max, Shufersal, allBranches: false, branchIds: [CinemaCityGlilot]),
            NewBenefit(Max, Shufersal) with { Value = 150 },
            NewBenefit(Max, Shufersal) with { RedeemUrl = "not a url" }
        ];

        foreach (var request in invalid)
        {
            var response = await client.PostAsJsonAsync("/api/benefits", request, Json);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Fact]
    public async Task Benefit_can_be_created_updated_and_deleted()
    {
        var (client, _) = await RegisterAsync();

        var created = await CreateBenefitAsync(client,
            NewBenefit(Max, Shufersal, allBranches: false, branchIds: [ShufersalAzrieli, ShufersalRamatAviv]));
        Assert.True(created.CanEdit);
        Assert.Equal("שופרסל", created.MerchantName);
        Assert.Equal("מקס", created.ClubName);
        Assert.Equal("סופרמרקט", created.CategoryName);
        Assert.Equal(2, created.Branches.Count);

        var update = NewBenefit(Cal, Shufersal, title: "₪30 הנחה", allBranches: false, branchIds: [ShufersalRamatAviv]);
        var updatedResponse = await client.PutAsJsonAsync($"/api/benefits/{created.Id}", update, Json);
        await EnsureStatusAsync(updatedResponse, HttpStatusCode.OK);
        var updated = (await updatedResponse.Content.ReadFromJsonAsync<BenefitDto>(Json))!;
        Assert.Equal("₪30 הנחה", updated.Title);
        Assert.Equal(Cal, updated.ClubId);
        Assert.Equal([ShufersalRamatAviv], updated.Branches.Select(b => b.Id));

        var deleted = await client.DeleteAsync($"/api/benefits/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/benefits/{created.Id}")).StatusCode);
    }

    [Theory]
    [InlineData("שופרסל")]
    [InlineData("שופר סל")]
    [InlineData("Shufersal")]
    [InlineData("SHUFER")]
    [InlineData("שופר")]
    public async Task Search_finds_benefits_by_name_alias_and_partial_match(string query)
    {
        var (client, _) = await RegisterAsync();
        await SetClubsAsync(client, Max);
        var benefit = await CreateBenefitAsync(client, NewBenefit(Max, Shufersal));

        var results = await GetAsync<List<BenefitDto>>(client, $"/api/benefits/search?q={Uri.EscapeDataString(query)}");

        Assert.Equal([benefit.Id], results.Select(b => b.Id));
    }

    [Fact]
    public async Task Search_returns_only_valid_benefits_from_household_clubs()
    {
        var (client, _) = await RegisterAsync();
        await SetClubsAsync(client, Max, Cal);

        var valid = await CreateBenefitAsync(client, NewBenefit(Max, Shufersal, validUntil: Today));
        var startsToday = await CreateBenefitAsync(client, NewBenefit(Cal, Shufersal, validFrom: Today));
        await CreateBenefitAsync(client, NewBenefit(Max, Shufersal, validFrom: Today.AddDays(-30), validUntil: Today.AddDays(-1)));
        await CreateBenefitAsync(client, NewBenefit(Max, Shufersal, validFrom: Today.AddDays(1)));
        await CreateBenefitAsync(client, NewBenefit(Isracard, Shufersal));
        await CreateBenefitAsync(client, NewBenefit(Max, RamiLevy));

        var results = await GetAsync<List<BenefitDto>>(client, "/api/benefits/search?q=שופרסל");

        Assert.Equal(new[] { valid.Id, startsToday.Id }.Order(), results.Select(b => b.Id).Order());
    }

    [Fact]
    public async Task Search_sorts_by_club_then_nearest_expiry()
    {
        var (client, _) = await RegisterAsync();
        await SetClubsAsync(client, Max, Cal);

        var maxLater = await CreateBenefitAsync(client, NewBenefit(Max, Shufersal, validUntil: Today.AddDays(20)));
        var calLater = await CreateBenefitAsync(client, NewBenefit(Cal, Shufersal, validUntil: Today.AddDays(30)));
        var maxSooner = await CreateBenefitAsync(client, NewBenefit(Max, Shufersal, validUntil: Today.AddDays(2)));
        var calSooner = await CreateBenefitAsync(client, NewBenefit(Cal, Shufersal, validUntil: Today.AddDays(3)));

        var results = await GetAsync<List<BenefitDto>>(client, "/api/benefits/search?q=שופרסל");

        // "כאל" sorts before "מקס" in Hebrew alphabetical order.
        Assert.Equal([calSooner.Id, calLater.Id, maxSooner.Id, maxLater.Id], results.Select(b => b.Id));
    }

    [Fact]
    public async Task Search_by_branch_keeps_all_branch_benefits_and_branch_specific_ones()
    {
        var (client, _) = await RegisterAsync();
        await SetClubsAsync(client, Max);

        var everywhere = await CreateBenefitAsync(client, NewBenefit(Max, Shufersal));
        var azrieliOnly = await CreateBenefitAsync(client, NewBenefit(Max, Shufersal, allBranches: false, branchIds: [ShufersalAzrieli]));
        var ramatAvivOnly = await CreateBenefitAsync(client, NewBenefit(Max, Shufersal, allBranches: false, branchIds: [ShufersalRamatAviv]));

        var atAzrieli = await GetAsync<List<BenefitDto>>(client, $"/api/benefits/search?q=שופרסל&branchId={ShufersalAzrieli}");
        Assert.Equal(new[] { everywhere.Id, azrieliOnly.Id }.Order(), atAzrieli.Select(b => b.Id).Order());

        var anyBranch = await GetAsync<List<BenefitDto>>(client, "/api/benefits/search?q=שופרסל");
        Assert.Equal(new[] { everywhere.Id, azrieliOnly.Id, ramatAvivOnly.Id }.Order(), anyBranch.Select(b => b.Id).Order());
    }

    [Fact]
    public async Task Search_with_unknown_branch_returns_404_and_empty_query_returns_400()
    {
        var (client, _) = await RegisterAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/benefits/search?q=שופרסל&branchId=9999")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/benefits/search?q=%20")).StatusCode);
    }

    [Fact]
    public async Task Entertainment_tab_shows_only_entertainment_categories_with_optional_club_filter()
    {
        var (client, _) = await RegisterAsync();
        await SetClubsAsync(client, Max, Cal);

        var movie = await CreateBenefitAsync(client, NewBenefit(Max, CinemaCity, title: "כרטיס ב-₪25"));
        var show = await CreateBenefitAsync(client, NewBenefit(Cal, Cameri, title: "1+1 להצגות"));
        await CreateBenefitAsync(client, NewBenefit(Max, Shufersal));
        await CreateBenefitAsync(client, NewBenefit(Isracard, CinemaCity));

        var all = await GetAsync<List<BenefitDto>>(client, "/api/benefits/entertainment");
        Assert.Equal(new[] { movie.Id, show.Id }.Order(), all.Select(b => b.Id).Order());
        Assert.All(all, b => Assert.True(b.IsEntertainment));

        var calOnly = await GetAsync<List<BenefitDto>>(client, $"/api/benefits/entertainment?clubId={Cal}");
        Assert.Equal([show.Id], calOnly.Select(b => b.Id));
    }

    [Fact]
    public async Task Households_cannot_see_or_edit_each_others_benefits()
    {
        var (owner, _) = await RegisterAsync();
        var (other, _) = await RegisterAsync();
        await SetClubsAsync(owner, Max);
        await SetClubsAsync(other, Max);

        var benefit = await CreateBenefitAsync(owner, NewBenefit(Max, Shufersal));

        Assert.Empty(await GetAsync<List<BenefitDto>>(other, "/api/benefits/search?q=שופרסל"));
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync($"/api/benefits/{benefit.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await other.PutAsJsonAsync($"/api/benefits/{benefit.Id}", NewBenefit(Max, Shufersal), Json)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.DeleteAsync($"/api/benefits/{benefit.Id}")).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await owner.GetAsync($"/api/benefits/{benefit.Id}")).StatusCode);
    }

    [Fact]
    public async Task Partner_sees_and_edits_benefits_of_the_shared_household()
    {
        var (owner, _) = await RegisterAsync();
        var household = await GetAsync<ClubIt.Contracts.Households.HouseholdDto>(owner, "/api/household");
        await SetClubsAsync(owner, Max);
        var benefit = await CreateBenefitAsync(owner, NewBenefit(Max, Shufersal));

        var (partner, _) = await RegisterAsync(householdName: null, inviteCode: household.InviteCode);

        var results = await GetAsync<List<BenefitDto>>(partner, "/api/benefits/search?q=שופרסל");
        Assert.Equal([benefit.Id], results.Select(b => b.Id));
        Assert.True(results[0].CanEdit);
        Assert.Equal(HttpStatusCode.NoContent, (await partner.DeleteAsync($"/api/benefits/{benefit.Id}")).StatusCode);
    }
}

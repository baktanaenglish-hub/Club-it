using System.Net;
using System.Net.Http.Json;
using ClubIt.Contracts;
using ClubIt.Contracts.Categories;
using ClubIt.Contracts.Clubs;
using ClubIt.Contracts.Households;
using ClubIt.Contracts.Merchants;
using ClubIt.Tests.Infrastructure;

namespace ClubIt.Tests.Api;

public class CatalogTests(ClubItApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Clubs_and_categories_are_seeded()
    {
        var (client, _) = await RegisterAsync();

        var clubs = await GetAsync<List<ClubDto>>(client, "/api/clubs");
        Assert.Contains(clubs, c => c.Name == "מקס" && c.Type == ClubType.CreditCard);
        Assert.Contains(clubs, c => c.Name == "פיס");

        var categories = await GetAsync<List<CategoryDto>>(client, "/api/categories");
        Assert.Contains(categories, c => c.Name == "קולנוע" && c.IsEntertainment);
        Assert.Contains(categories, c => c.Name == "סופרמרקט" && !c.IsEntertainment);
    }

    [Fact]
    public async Task Household_club_settings_are_replaced_on_update()
    {
        var (client, _) = await RegisterAsync();

        await SetClubsAsync(client, Max, Cal);
        await SetClubsAsync(client, Cal, Isracard);

        var clubs = await GetAsync<List<ClubDto>>(client, "/api/household/clubs");
        Assert.Equal([Cal, Isracard], clubs.Select(c => c.Id).Order());
    }

    [Fact]
    public async Task Household_club_update_rejects_unknown_clubs()
    {
        var (client, _) = await RegisterAsync();

        var response = await client.PutAsJsonAsync("/api/household/clubs", new UpdateHouseholdClubsRequest([Max, 9999]), Json);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("שופרסל")]
    [InlineData("שופר סל")]
    [InlineData("SHUFERSAL")]
    [InlineData("שופ")]
    public async Task Merchant_autocomplete_matches_names_and_aliases(string query)
    {
        var (client, _) = await RegisterAsync();

        var merchants = await GetAsync<List<MerchantDto>>(client, $"/api/merchants?q={Uri.EscapeDataString(query)}");
        Assert.Equal(Shufersal, merchants[0].Id);
    }

    [Fact]
    public async Task Merchant_can_be_created_with_aliases_and_branches()
    {
        var (client, _) = await RegisterAsync();
        var name = $"קפה בדיקה {Guid.NewGuid():N}"[..20];

        var created = await client.PostAsJsonAsync("/api/merchants",
            new CreateMerchantRequest(name, 4, ["Test Cafe", "test cafe", "קפה טסט"]), Json);
        await EnsureStatusAsync(created, HttpStatusCode.Created);
        var merchant = (await created.Content.ReadFromJsonAsync<MerchantDto>(Json))!;
        Assert.Equal(["Test Cafe", "קפה טסט"], merchant.Aliases);

        var found = await GetAsync<List<MerchantDto>>(client, "/api/merchants?q=testcafe");
        Assert.Contains(found, m => m.Id == merchant.Id);

        var branch = await client.PostAsJsonAsync($"/api/merchants/{merchant.Id}/branches",
            new CreateBranchRequest("סניף מרכז", "חיפה", "הרצל 1", 32.8, 35.0), Json);
        await EnsureStatusAsync(branch, HttpStatusCode.Created);

        var branches = await GetAsync<List<BranchDto>>(client, $"/api/merchants/{merchant.Id}/branches");
        Assert.Single(branches);
        Assert.Equal("חיפה", branches[0].City);

        var updated = await client.PutAsJsonAsync($"/api/merchants/{merchant.Id}",
            new UpdateMerchantRequest(name, 4, ["קפה טסט"]), Json);
        await EnsureStatusAsync(updated, HttpStatusCode.OK);
        Assert.Equal(["קפה טסט"], (await updated.Content.ReadFromJsonAsync<MerchantDto>(Json))!.Aliases);
    }

    [Fact]
    public async Task Merchant_with_duplicate_name_is_rejected()
    {
        var (client, _) = await RegisterAsync();

        var response = await client.PostAsJsonAsync("/api/merchants", new CreateMerchantRequest("שופר-סל", 1, null), Json);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Branches_of_unknown_merchant_return_404()
    {
        var (client, _) = await RegisterAsync();

        var response = await client.GetAsync("/api/merchants/9999/branches");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

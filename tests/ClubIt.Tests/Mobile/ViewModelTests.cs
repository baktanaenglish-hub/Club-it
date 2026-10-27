using ClubIt.Mobile.Core.Services;
using ClubIt.Mobile.Core.ViewModels;
using ClubIt.Tests.Infrastructure;

namespace ClubIt.Tests.Mobile;

public class ViewModelTests(ClubItApiFactory factory) : MobileTestBase(factory)
{
    [Fact]
    public async Task Login_view_model_registers_and_opens_main_screen()
    {
        var vm = new LoginViewModel(CreateApi(), Session, Navigation, Server)
        {
            IsRegisterMode = true,
            Email = $"vm-{Guid.NewGuid():N}@example.com",
            Password = "Password1",
            DisplayName = "ריקי",
            HouseholdName = "הבית"
        };

        await vm.SubmitCommand.ExecuteAsync(null);

        Assert.Null(vm.ErrorMessage);
        Assert.True(Session.IsSignedIn);
        Assert.True(Navigation.ShowedMain);
    }

    [Fact]
    public async Task Login_view_model_shows_server_error_for_wrong_password()
    {
        var vm = new LoginViewModel(CreateApi(), Session, Navigation, Server) { Email = "nobody@example.com", Password = "Password1" };

        await vm.SubmitCommand.ExecuteAsync(null);

        Assert.Equal("אימייל או סיסמה שגויים.", vm.ErrorMessage);
        Assert.False(Navigation.ShowedMain);
    }

    [Fact]
    public async Task Login_view_model_validates_and_saves_the_server_address()
    {
        var server = new InMemoryServerSettings();
        var vm = new LoginViewModel(CreateApi(), Session, Navigation, server) { Email = "a@example.com", Password = "Password1" };
        Assert.True(vm.ShowServerSettings);

        vm.ServerAddressText = "not a url";
        await vm.SubmitCommand.ExecuteAsync(null);
        Assert.Contains("כתובת השרת", vm.ErrorMessage);
        Assert.Null(server.BaseAddress);

        vm.ServerAddressText = "localhost";
        await vm.SubmitCommand.ExecuteAsync(null);
        Assert.Equal(new Uri("https://localhost/"), server.BaseAddress);
        Assert.Equal("https://localhost/", vm.ServerAddressText);
    }

    [Fact]
    public async Task Search_view_model_selects_store_loads_branches_and_filters_by_branch()
    {
        var api = await SignedInApiAsync();
        await api.UpdateHouseholdClubsAsync([Max]);
        var everywhere = await api.CreateBenefitAsync(NewBenefit(Max, Shufersal));
        var ramatAviv = await api.CreateBenefitAsync(NewBenefit(Max, Shufersal, allBranches: false, branchIds: [ShufersalRamatAviv]));

        var vm = new SearchViewModel(api, Navigation, TimeProvider.System);
        var merchant = (await api.SearchMerchantsAsync("שופרסל"))[0];

        await vm.SelectSuggestionCommand.ExecuteAsync(merchant);

        Assert.Equal("שופרסל", vm.Query);
        Assert.Same(SearchViewModel.AllBranches, vm.SelectedBranch);
        Assert.True(vm.HasBranches);
        Assert.Equal(new[] { everywhere.Id, ramatAviv.Id }.Order(), vm.Results.Select(r => r.Id).Order());

        vm.SelectedBranch = vm.Branches.Single(b => b.Id == ShufersalAzrieli);
        await WaitUntilAsync(() => vm.IsNotBusy && vm.Results.Count == 1);
        Assert.Equal([everywhere.Id], vm.Results.Select(r => r.Id));

        await vm.OpenBenefitCommand.ExecuteAsync(vm.Results[0]);
        Assert.Equal(Routes.BenefitDetails, Navigation.Visited.Single().Route);
    }

    [Fact]
    public async Task Search_view_model_reports_empty_query_and_empty_results()
    {
        var api = await SignedInApiAsync();
        var vm = new SearchViewModel(api, Navigation, TimeProvider.System);

        await vm.SearchCommand.ExecuteAsync(null);
        Assert.NotNull(vm.ErrorMessage);

        vm.Query = "רמי לוי";
        await vm.SearchCommand.ExecuteAsync(null);
        Assert.Null(vm.ErrorMessage);
        Assert.True(vm.ShowEmptyState);
    }

    [Fact]
    public async Task Benefit_edit_view_model_creates_and_then_edits_a_benefit()
    {
        var api = await SignedInApiAsync();
        var vm = new BenefitEditViewModel(api, Navigation, Dialogs, TimeProvider.System);
        await vm.LoadAsync(null);

        Assert.NotNull(vm.BuildRequestWithError().Error);

        vm.SelectedClub = vm.Clubs.Single(c => c.Id == Cal);
        await vm.SelectMerchantCommand.ExecuteAsync((await api.SearchMerchantsAsync("סינמה סיטי"))[0]);
        vm.Title = "כרטיס ב-₪29";
        vm.SelectedBenefitType = vm.BenefitTypes.Single(t => t.Value == ClubIt.Contracts.BenefitType.DiscountedTicket);
        vm.ValueText = "29";
        vm.AllBranches = false;
        Assert.Equal("יש לבחור לפחות סניף אחד, או לסמן \"בכל הסניפים\".", vm.BuildRequestWithError().Error);
        vm.Branches.Single(b => b.Item.Id == CinemaCityGlilot).IsSelected = true;

        await vm.SaveCommand.ExecuteAsync(null);
        Assert.Null(vm.ErrorMessage);
        Assert.Equal(["ההטבה נשמרה"], Dialogs.Alerts);
        Assert.Null(vm.SelectedMerchant);

        await api.UpdateHouseholdClubsAsync([Cal]);
        var saved = Assert.Single(await api.GetEntertainmentBenefitsAsync(null));
        Assert.Equal("כרטיס ב-₪29", saved.Title);
        Assert.Equal([CinemaCityGlilot], saved.Branches.Select(b => b.Id));

        var edit = new BenefitEditViewModel(api, Navigation, Dialogs, TimeProvider.System);
        await edit.LoadAsync(saved.Id);
        Assert.True(edit.IsEditMode);
        Assert.Equal("סינמה סיטי", edit.MerchantQuery);
        Assert.True(edit.Branches.Single(b => b.Item.Id == CinemaCityGlilot).IsSelected);

        edit.Title = "כרטיס ב-₪27";
        await edit.SaveCommand.ExecuteAsync(null);
        Assert.Equal(1, Navigation.BackCount);
        Assert.Equal("כרטיס ב-₪27", (await api.GetBenefitAsync(saved.Id)).Title);
    }

    [Fact]
    public async Task Club_settings_view_model_saves_selection_for_the_household()
    {
        var api = await SignedInApiAsync();
        var vm = new ClubSettingsViewModel(api, Session, Navigation, Dialogs);
        await vm.LoadAsync();

        Assert.NotNull(vm.Household);
        Assert.All(vm.Clubs, c => Assert.False(c.IsSelected));

        vm.Clubs.Single(c => c.Item.Id == Max).IsSelected = true;
        vm.Clubs.Single(c => c.Item.Id == Isracard).IsSelected = true;
        await vm.SaveCommand.ExecuteAsync(null);

        Assert.Equal(new[] { Max, Isracard }.Order(), (await api.GetHouseholdClubsAsync()).Select(c => c.Id).Order());

        await vm.SignOutCommand.ExecuteAsync(null);
        Assert.False(Session.IsSignedIn);
        Assert.True(Navigation.ShowedLogin);
    }

    [Fact]
    public async Task Entertainment_and_details_view_models_show_and_redeem_benefits()
    {
        var api = await SignedInApiAsync();
        await api.UpdateHouseholdClubsAsync([Max, Cal]);
        var movie = await api.CreateBenefitAsync(NewBenefit(Max, CinemaCity));
        await api.CreateBenefitAsync(NewBenefit(Cal, Cameri));

        var entertainment = new EntertainmentViewModel(api, Navigation, TimeProvider.System);
        await entertainment.LoadAsync();
        Assert.Equal(2, entertainment.Results.Count);

        entertainment.SelectedClub = entertainment.Clubs.Single(c => c.Id == Max);
        await WaitUntilAsync(() => entertainment.IsNotBusy && entertainment.Results.Count == 1);
        Assert.Equal([movie.Id], entertainment.Results.Select(r => r.Id));

        var launcher = new FakeLauncher();
        var details = new BenefitDetailsViewModel(api, Navigation, Dialogs, launcher, TimeProvider.System);
        await details.LoadAsync(movie.Id);
        Assert.Equal("10%", details.ValueText);
        Assert.True(details.CanEdit);

        await details.RedeemCommand.ExecuteAsync(null);
        Assert.Equal(["https://www.example.com/redeem"], launcher.Opened);

        await details.DeleteCommand.ExecuteAsync(null);
        Assert.Equal(1, Navigation.BackCount);
    }

    [Fact]
    public async Task Merchants_view_model_adds_merchant_aliases_and_branch()
    {
        var api = await SignedInApiAsync();
        var vm = new MerchantsViewModel(api, Dialogs, TimeProvider.System);
        await vm.LoadAsync();

        vm.NewMerchantName = $"מאפיית {Guid.NewGuid():N}"[..16];
        vm.NewMerchantCategory = vm.Categories.First();
        vm.NewMerchantAliases = "מאפיה, bakery";
        await vm.AddMerchantCommand.ExecuteAsync(null);

        Assert.Null(vm.ErrorMessage);
        Assert.NotNull(vm.SelectedMerchant);
        Assert.Equal(["bakery", "מאפיה"], vm.SelectedMerchant!.Aliases);

        vm.NewBranchName = "סניף ראשי";
        vm.NewBranchCity = "חולון";
        await vm.AddBranchCommand.ExecuteAsync(null);
        Assert.Single(vm.Branches);

        vm.SelectedAliasesText = "bakery";
        await vm.SaveAliasesCommand.ExecuteAsync(null);
        Assert.Equal(["bakery"], vm.SelectedMerchant!.Aliases);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        for (var i = 0; i < 100 && !condition(); i++)
        {
            await Task.Delay(50);
        }
        Assert.True(condition());
    }
}

internal static class BenefitEditViewModelExtensions
{
    public static (object? Request, string? Error) BuildRequestWithError(this BenefitEditViewModel vm)
    {
        var request = vm.BuildRequest(out var error);
        return (request, error);
    }
}

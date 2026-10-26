using System.Collections.ObjectModel;
using ClubIt.Contracts.Categories;
using ClubIt.Contracts.Merchants;
using ClubIt.Mobile.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClubIt.Mobile.Core.ViewModels;

/// <summary>Merchant and branch management: add merchants with aliases, edit aliases, add branches.</summary>
public partial class MerchantsViewModel(IClubItApi api, IDialogService dialogs, TimeProvider timeProvider) : ViewModelBase
{
    private CancellationTokenSource? _searchCts;

    public ObservableCollection<CategoryDto> Categories { get; } = [];
    public ObservableCollection<MerchantDto> Merchants { get; } = [];
    public ObservableCollection<BranchDto> Branches { get; } = [];

    [ObservableProperty]
    public partial string Query { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedMerchant))]
    public partial MerchantDto? SelectedMerchant { get; set; }

    [ObservableProperty]
    public partial string SelectedAliasesText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewMerchantName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial CategoryDto? NewMerchantCategory { get; set; }

    [ObservableProperty]
    public partial string NewMerchantAliases { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewBranchName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewBranchCity { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewBranchAddress { get; set; } = string.Empty;

    public bool HasSelectedMerchant => SelectedMerchant is not null;

    /// <summary>Splits a comma-separated alias list ("שופר סל, shufersal").</summary>
    public static IReadOnlyList<string> ParseAliases(string text) =>
        text.Split([',', '،', ';', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public async Task LoadAsync()
    {
        await RunAsync(async () =>
        {
            if (Categories.Count == 0)
            {
                foreach (var category in await api.GetCategoriesAsync())
                {
                    Categories.Add(category);
                }
            }
            await ReloadMerchantsAsync(CancellationToken.None);
        });
    }

    partial void OnQueryChanged(string value) => _ = SearchAsync();

    partial void OnSelectedMerchantChanged(MerchantDto? value)
    {
        SelectedAliasesText = value is null ? string.Empty : string.Join(", ", value.Aliases);
        Branches.Clear();
        if (value is not null)
        {
            _ = LoadBranchesAsync(value.Id);
        }
    }

    private async Task SearchAsync()
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;
        try
        {
            await Task.Delay(SearchViewModel.AutocompleteDelay, timeProvider, token);
            await ReloadMerchantsAsync(token);
        }
        catch (OperationCanceledException)
        {
        }
        catch (ApiException e)
        {
            ErrorMessage = e.UserMessage;
        }
    }

    private async Task ReloadMerchantsAsync(CancellationToken token)
    {
        var merchants = await api.SearchMerchantsAsync(Query, token);
        Merchants.Clear();
        foreach (var merchant in merchants)
        {
            Merchants.Add(merchant);
        }
    }

    private async Task LoadBranchesAsync(int merchantId)
    {
        try
        {
            var branches = await api.GetBranchesAsync(merchantId);
            if (SelectedMerchant?.Id != merchantId)
            {
                return;
            }
            Branches.Clear();
            foreach (var branch in branches)
            {
                Branches.Add(branch);
            }
        }
        catch (ApiException e)
        {
            ErrorMessage = e.UserMessage;
        }
    }

    [RelayCommand]
    private async Task AddMerchantAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMerchantName) || NewMerchantCategory is null)
        {
            ErrorMessage = "יש להזין שם רשת ולבחור קטגוריה.";
            return;
        }

        MerchantDto? created = null;
        var request = new CreateMerchantRequest(NewMerchantName.Trim(), NewMerchantCategory.Id, ParseAliases(NewMerchantAliases));
        if (await RunAsync(async () => created = await api.CreateMerchantAsync(request)) && created is not null)
        {
            NewMerchantName = string.Empty;
            NewMerchantAliases = string.Empty;
            Merchants.Insert(0, created);
            SelectedMerchant = created;
            await dialogs.AlertAsync("הרשת נוספה", created.Name);
        }
    }

    [RelayCommand]
    private async Task SaveAliasesAsync()
    {
        if (SelectedMerchant is not { } merchant)
        {
            return;
        }

        MerchantDto? updated = null;
        var request = new UpdateMerchantRequest(merchant.Name, merchant.CategoryId, ParseAliases(SelectedAliasesText));
        if (await RunAsync(async () => updated = await api.UpdateMerchantAsync(merchant.Id, request)) && updated is not null)
        {
            var index = Merchants.IndexOf(merchant);
            if (index >= 0)
            {
                Merchants[index] = updated;
            }
            SelectedMerchant = updated;
        }
    }

    [RelayCommand]
    private async Task AddBranchAsync()
    {
        if (SelectedMerchant is not { } merchant)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(NewBranchName))
        {
            ErrorMessage = "יש להזין שם סניף.";
            return;
        }

        BranchDto? created = null;
        var request = new CreateBranchRequest(
            NewBranchName.Trim(),
            string.IsNullOrWhiteSpace(NewBranchCity) ? null : NewBranchCity.Trim(),
            string.IsNullOrWhiteSpace(NewBranchAddress) ? null : NewBranchAddress.Trim(),
            null,
            null);
        if (await RunAsync(async () => created = await api.CreateBranchAsync(merchant.Id, request)) && created is not null)
        {
            Branches.Add(created);
            NewBranchName = string.Empty;
            NewBranchCity = string.Empty;
            NewBranchAddress = string.Empty;
        }
    }
}

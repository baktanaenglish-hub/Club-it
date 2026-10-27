using System.Collections.ObjectModel;
using ClubIt.Contracts.Merchants;
using ClubIt.Mobile.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClubIt.Mobile.Core.ViewModels;

/// <summary>
/// Home screen: store search with autocomplete, optional branch selection and the benefit results.
/// </summary>
public partial class SearchViewModel(IClubItApi api, INavigationService navigation, TimeProvider timeProvider) : ViewModelBase
{
    public static readonly TimeSpan AutocompleteDelay = TimeSpan.FromMilliseconds(300);

    /// <summary>Placeholder entry in the branch picker meaning "no specific branch".</summary>
    public static readonly BranchDto AllBranches = new(0, 0, "כל הסניפים", null, null, null, null);

    private CancellationTokenSource? _autocompleteCts;
    private bool _suppressAutocomplete;

    public ObservableCollection<MerchantDto> Suggestions { get; } = [];
    public ObservableCollection<BranchDto> Branches { get; } = [];
    public ObservableCollection<BenefitItemViewModel> Results { get; } = [];

    [ObservableProperty]
    public partial string Query { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBranches))]
    public partial MerchantDto? SelectedMerchant { get; set; }

    [ObservableProperty]
    public partial BranchDto? SelectedBranch { get; set; }

    [ObservableProperty]
    public partial bool ShowSuggestions { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    public partial bool HasSearched { get; set; }

    public bool HasBranches => SelectedMerchant is not null && Branches.Count > 1;

    public bool ShowEmptyState => HasSearched && Results.Count == 0 && !IsBusy;

    partial void OnQueryChanged(string value)
    {
        if (_suppressAutocomplete)
        {
            return;
        }

        // Typing a different store name drops the previous store and branch selection.
        if (SelectedMerchant is not null && value != SelectedMerchant.Name)
        {
            SelectedMerchant = null;
            Branches.Clear();
            SelectedBranch = null;
        }

        _ = UpdateSuggestionsAsync(value);
    }

    partial void OnSelectedBranchChanged(BranchDto? value)
    {
        if (SelectedMerchant is not null && !_suppressAutocomplete)
        {
            _ = SearchCommand.ExecuteAsync(null);
        }
    }

    private async Task UpdateSuggestionsAsync(string text)
    {
        _autocompleteCts?.Cancel();
        _autocompleteCts = new CancellationTokenSource();
        var token = _autocompleteCts.Token;

        if (string.IsNullOrWhiteSpace(text))
        {
            Suggestions.Clear();
            ShowSuggestions = false;
            return;
        }

        try
        {
            await Task.Delay(AutocompleteDelay, timeProvider, token);
            var merchants = await api.SearchMerchantsAsync(text, token);
            if (token.IsCancellationRequested)
            {
                return;
            }

            Suggestions.Clear();
            foreach (var merchant in merchants)
            {
                Suggestions.Add(merchant);
            }
            ShowSuggestions = Suggestions.Count > 0;
        }
        catch (OperationCanceledException)
        {
            // A newer keystroke replaced this request.
        }
        catch (ApiException)
        {
            // Autocomplete is best-effort; the search itself reports errors.
            ShowSuggestions = false;
        }
    }

    [RelayCommand]
    private async Task SelectSuggestionAsync(MerchantDto merchant)
    {
        _autocompleteCts?.Cancel();
        ShowSuggestions = false;

        _suppressAutocomplete = true;
        try
        {
            Query = merchant.Name;
            SelectedMerchant = merchant;
            Branches.Clear();
            SelectedBranch = null;
        }
        finally
        {
            _suppressAutocomplete = false;
        }

        try
        {
            var branches = await api.GetBranchesAsync(merchant.Id);
            _suppressAutocomplete = true;
            Branches.Add(AllBranches);
            foreach (var branch in branches)
            {
                Branches.Add(branch);
            }
            SelectedBranch = AllBranches;
        }
        catch (ApiException)
        {
            // Branches are optional; search all branches.
        }
        finally
        {
            _suppressAutocomplete = false;
            OnPropertyChanged(nameof(HasBranches));
        }

        await SearchAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        _autocompleteCts?.Cancel();
        ShowSuggestions = false;

        if (string.IsNullOrWhiteSpace(Query))
        {
            ErrorMessage = "הקלידו שם של חנות או רשת.";
            return;
        }

        var branchId = SelectedBranch is { Id: > 0 } branch ? branch.Id : (int?)null;
        await RunAsync(async () =>
        {
            var benefits = await api.SearchBenefitsAsync(Query.Trim(), branchId);
            var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);

            Results.Clear();
            foreach (var benefit in benefits)
            {
                Results.Add(new BenefitItemViewModel(benefit, today));
            }
            HasSearched = true;
        });
        OnPropertyChanged(nameof(ShowEmptyState));
    }

    [RelayCommand]
    private async Task OpenBenefitAsync(BenefitItemViewModel item) =>
        await navigation.GoToAsync(Routes.BenefitDetails, new Dictionary<string, object> { [Routes.IdParameter] = item.Id });

    [RelayCommand]
    private void Clear()
    {
        Query = string.Empty;
        Results.Clear();
        HasSearched = false;
        ErrorMessage = null;
    }
}

using System.Collections.ObjectModel;
using System.Globalization;
using ClubIt.Contracts;
using ClubIt.Contracts.Benefits;
using ClubIt.Contracts.Clubs;
using ClubIt.Contracts.Merchants;
using ClubIt.Mobile.Core.Formatting;
using ClubIt.Mobile.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClubIt.Mobile.Core.ViewModels;

/// <summary>Add / edit benefit form: club, merchant (with its category), type and value, validity, branches and link.</summary>
public partial class BenefitEditViewModel(
    IClubItApi api,
    INavigationService navigation,
    IDialogService dialogs,
    TimeProvider timeProvider) : ViewModelBase
{
    private CancellationTokenSource? _merchantSearchCts;
    private bool _suppressMerchantSearch;

    public IReadOnlyList<Option<BenefitType>> BenefitTypes { get; } =
        Enum.GetValues<BenefitType>().Select(t => new Option<BenefitType>(t, BenefitFormatter.TypeName(t))).ToList();

    public ObservableCollection<ClubDto> Clubs { get; } = [];
    public ObservableCollection<MerchantDto> MerchantSuggestions { get; } = [];
    public ObservableCollection<SelectableItem<BranchDto>> Branches { get; } = [];

    /// <summary>Null when adding a new benefit.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditMode), nameof(PageTitle))]
    public partial int? BenefitId { get; set; }

    [ObservableProperty]
    public partial ClubDto? SelectedClub { get; set; }

    [ObservableProperty]
    public partial string MerchantQuery { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CategoryText), nameof(HasMerchant))]
    public partial MerchantDto? SelectedMerchant { get; set; }

    [ObservableProperty]
    public partial bool ShowMerchantSuggestions { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueLabel))]
    public partial Option<BenefitType>? SelectedBenefitType { get; set; }

    [ObservableProperty]
    public partial string ValueText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasValidFrom { get; set; }

    [ObservableProperty]
    public partial DateTime ValidFrom { get; set; }

    [ObservableProperty]
    public partial DateTime ValidUntil { get; set; }

    [ObservableProperty]
    public partial bool AllBranches { get; set; } = true;

    [ObservableProperty]
    public partial string RedeemUrl { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Notes { get; set; } = string.Empty;

    public bool IsEditMode => BenefitId is not null;
    public string PageTitle => IsEditMode ? "עריכת הטבה" : "הוספת הטבה";
    public bool HasMerchant => SelectedMerchant is not null;
    public string CategoryText => SelectedMerchant is null ? string.Empty : $"קטגוריה: {SelectedMerchant.CategoryName}";

    public string ValueLabel => SelectedBenefitType?.Value switch
    {
        BenefitType.Percentage => "אחוז הנחה",
        BenefitType.FixedAmount => "סכום הנחה (₪)",
        BenefitType.DiscountedTicket => "מחיר כרטיס (₪)",
        _ => "ערך (לא חובה)"
    };

    private DateTime Today => timeProvider.GetLocalNow().Date;

    /// <summary>Loads the clubs and, in edit mode, the benefit. Resets the form when <paramref name="benefitId"/> is null.</summary>
    public async Task LoadAsync(int? benefitId)
    {
        await RunAsync(async () =>
        {
            if (Clubs.Count == 0)
            {
                foreach (var club in await api.GetClubsAsync())
                {
                    Clubs.Add(club);
                }
            }

            if (benefitId is { } id)
            {
                Populate(await api.GetBenefitAsync(id));
                if (SelectedMerchant is not null)
                {
                    await LoadBranchesAsync(SelectedMerchant.Id, Branches.Where(b => b.IsSelected).Select(b => b.Item.Id).ToHashSet());
                }
            }
            else if (BenefitId is not null || SelectedBenefitType is null)
            {
                Reset();
            }
        });
    }

    partial void OnMerchantQueryChanged(string value)
    {
        if (_suppressMerchantSearch)
        {
            return;
        }

        if (SelectedMerchant is not null && value != SelectedMerchant.Name)
        {
            SelectedMerchant = null;
            Branches.Clear();
        }

        _ = SearchMerchantsAsync(value);
    }

    private async Task SearchMerchantsAsync(string text)
    {
        _merchantSearchCts?.Cancel();
        _merchantSearchCts = new CancellationTokenSource();
        var token = _merchantSearchCts.Token;

        if (string.IsNullOrWhiteSpace(text))
        {
            MerchantSuggestions.Clear();
            ShowMerchantSuggestions = false;
            return;
        }

        try
        {
            await Task.Delay(SearchViewModel.AutocompleteDelay, timeProvider, token);
            var merchants = await api.SearchMerchantsAsync(text, token);
            MerchantSuggestions.Clear();
            foreach (var merchant in merchants)
            {
                MerchantSuggestions.Add(merchant);
            }
            ShowMerchantSuggestions = MerchantSuggestions.Count > 0;
        }
        catch (OperationCanceledException)
        {
        }
        catch (ApiException)
        {
            ShowMerchantSuggestions = false;
        }
    }

    [RelayCommand]
    private async Task SelectMerchantAsync(MerchantDto merchant)
    {
        _merchantSearchCts?.Cancel();
        ShowMerchantSuggestions = false;
        SetMerchant(merchant);

        try
        {
            await LoadBranchesAsync(merchant.Id, new HashSet<int>());
        }
        catch (ApiException e)
        {
            ErrorMessage = e.UserMessage;
        }
    }

    [RelayCommand]
    private Task ManageMerchantsAsync() => navigation.GoToAsync(Routes.Merchants);

    [RelayCommand]
    private async Task SaveAsync()
    {
        var request = BuildRequest(out var error);
        if (request is null)
        {
            ErrorMessage = error;
            return;
        }

        BenefitDto? saved = null;
        var id = BenefitId;
        var ok = await RunAsync(async () =>
        {
            saved = id is { } existing
                ? await api.UpdateBenefitAsync(existing, request)
                : await api.CreateBenefitAsync(request);
        });

        if (!ok || saved is null)
        {
            return;
        }

        if (id is not null)
        {
            await navigation.GoBackAsync();
        }
        else
        {
            await dialogs.AlertAsync("ההטבה נשמרה", $"\"{saved.Title}\" נוספה ל{saved.ClubName} ב{saved.MerchantName}.");
            Reset();
        }
    }

    /// <summary>Validates the form locally; the server validates again.</summary>
    public SaveBenefitRequest? BuildRequest(out string? error)
    {
        error = null;
        if (SelectedClub is null)
        {
            error = "יש לבחור מועדון.";
        }
        else if (SelectedMerchant is null)
        {
            error = "יש לבחור רשת או עסק מהרשימה.";
        }
        else if (string.IsNullOrWhiteSpace(Title))
        {
            error = "יש להזין כותרת להטבה.";
        }
        else if (SelectedBenefitType is null)
        {
            error = "יש לבחור סוג הטבה.";
        }
        else if (HasValidFrom && ValidFrom.Date > ValidUntil.Date)
        {
            error = "תאריך ההתחלה מאוחר מתאריך התפוגה.";
        }
        else if (!AllBranches && !Branches.Any(b => b.IsSelected))
        {
            error = "יש לבחור לפחות סניף אחד, או לסמן \"בכל הסניפים\".";
        }

        decimal? value = null;
        if (error is null && !string.IsNullOrWhiteSpace(ValueText))
        {
            if (decimal.TryParse(ValueText.Trim().TrimEnd('%').TrimStart('₪'), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) && parsed >= 0)
            {
                value = parsed;
            }
            else
            {
                error = "ערך ההטבה צריך להיות מספר.";
            }
        }

        if (error is not null)
        {
            return null;
        }

        return new SaveBenefitRequest(
            SelectedClub!.Id,
            SelectedMerchant!.Id,
            Title.Trim(),
            NullIfEmpty(Description),
            SelectedBenefitType!.Value,
            value,
            HasValidFrom ? DateOnly.FromDateTime(ValidFrom) : null,
            DateOnly.FromDateTime(ValidUntil),
            AllBranches,
            AllBranches ? null : Branches.Where(b => b.IsSelected).Select(b => b.Item.Id).ToList(),
            NullIfEmpty(RedeemUrl),
            NullIfEmpty(Notes));
    }

    private void Populate(BenefitDto benefit)
    {
        BenefitId = benefit.Id;
        SelectedClub = Clubs.FirstOrDefault(c => c.Id == benefit.ClubId);
        SetMerchant(new MerchantDto(benefit.MerchantId, benefit.MerchantName, benefit.CategoryId, benefit.CategoryName, []));
        Title = benefit.Title;
        Description = benefit.Description ?? string.Empty;
        SelectedBenefitType = BenefitTypes.First(t => t.Value == benefit.BenefitType);
        ValueText = benefit.Value?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;
        HasValidFrom = benefit.ValidFrom is not null;
        ValidFrom = benefit.ValidFrom?.ToDateTime(TimeOnly.MinValue) ?? Today;
        ValidUntil = benefit.ValidUntil.ToDateTime(TimeOnly.MinValue);
        AllBranches = benefit.AllBranches;
        RedeemUrl = benefit.RedeemUrl ?? string.Empty;
        Notes = benefit.Notes ?? string.Empty;

        Branches.Clear();
        foreach (var branch in benefit.Branches)
        {
            Branches.Add(new SelectableItem<BranchDto>(branch, isSelected: true));
        }
    }

    private void Reset()
    {
        BenefitId = null;
        SelectedClub = null;
        SetMerchant(null);
        Branches.Clear();
        Title = string.Empty;
        Description = string.Empty;
        SelectedBenefitType = BenefitTypes[0];
        ValueText = string.Empty;
        HasValidFrom = false;
        ValidFrom = Today;
        ValidUntil = Today.AddMonths(1);
        AllBranches = true;
        RedeemUrl = string.Empty;
        Notes = string.Empty;
        ErrorMessage = null;
    }

    private void SetMerchant(MerchantDto? merchant)
    {
        _suppressMerchantSearch = true;
        try
        {
            SelectedMerchant = merchant;
            MerchantQuery = merchant?.Name ?? string.Empty;
        }
        finally
        {
            _suppressMerchantSearch = false;
        }
    }

    private async Task LoadBranchesAsync(int merchantId, IReadOnlySet<int> selectedIds)
    {
        var branches = await api.GetBranchesAsync(merchantId);
        Branches.Clear();
        foreach (var branch in branches)
        {
            Branches.Add(new SelectableItem<BranchDto>(branch, selectedIds.Contains(branch.Id)));
        }
    }

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

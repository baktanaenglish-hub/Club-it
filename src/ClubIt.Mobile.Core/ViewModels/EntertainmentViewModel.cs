using System.Collections.ObjectModel;
using ClubIt.Contracts.Clubs;
using ClubIt.Mobile.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClubIt.Mobile.Core.ViewModels;

/// <summary>Outings tab: movies, shows and attractions, filterable by club.</summary>
public partial class EntertainmentViewModel(IClubItApi api, INavigationService navigation, TimeProvider timeProvider) : ViewModelBase
{
    /// <summary>Placeholder entry in the club filter meaning "all my clubs".</summary>
    public static readonly ClubDto AllClubs = new(0, "כל המועדונים", Contracts.ClubType.CreditCard, null, null);

    private bool _loadingClubs;

    public ObservableCollection<ClubDto> Clubs { get; } = [];
    public ObservableCollection<BenefitItemViewModel> Results { get; } = [];

    [ObservableProperty]
    public partial ClubDto? SelectedClub { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; }

    /// <summary>Pull-to-refresh state; kept apart from <see cref="ViewModelBase.IsBusy"/>, which guards re-entry.</summary>
    [ObservableProperty]
    public partial bool IsRefreshing { get; set; }

    partial void OnSelectedClubChanged(ClubDto? value)
    {
        if (!_loadingClubs)
        {
            _ = RefreshCommand.ExecuteAsync(null);
        }
    }

    public async Task LoadAsync()
    {
        try
        {
            _loadingClubs = true;
            var previous = SelectedClub?.Id ?? 0;
            var clubs = await api.GetHouseholdClubsAsync();

            Clubs.Clear();
            Clubs.Add(AllClubs);
            foreach (var club in clubs)
            {
                Clubs.Add(club);
            }
            SelectedClub = Clubs.FirstOrDefault(c => c.Id == previous) ?? AllClubs;
        }
        catch (ApiException e)
        {
            ErrorMessage = e.UserMessage;
            return;
        }
        finally
        {
            _loadingClubs = false;
        }

        await RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        var clubId = SelectedClub is { Id: > 0 } club ? club.Id : (int?)null;
        await RunAsync(async () =>
        {
            var benefits = await api.GetEntertainmentBenefitsAsync(clubId);
            var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);

            Results.Clear();
            foreach (var benefit in benefits)
            {
                Results.Add(new BenefitItemViewModel(benefit, today));
            }
        });
        IsEmpty = Results.Count == 0 && !HasError;
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task OpenBenefitAsync(BenefitItemViewModel item) =>
        await navigation.GoToAsync(Routes.BenefitDetails, new Dictionary<string, object> { [Routes.IdParameter] = item.Id });
}

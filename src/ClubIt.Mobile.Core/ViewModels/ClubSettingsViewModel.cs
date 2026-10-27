using System.Collections.ObjectModel;
using ClubIt.Contracts;
using ClubIt.Contracts.Clubs;
using ClubIt.Contracts.Households;
using ClubIt.Mobile.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClubIt.Mobile.Core.ViewModels;

/// <summary>Settings: which clubs the household has, household invite code and sign-out.</summary>
public partial class ClubSettingsViewModel(
    IClubItApi api,
    ISessionStore session,
    INavigationService navigation,
    IDialogService dialogs) : ViewModelBase
{
    public ObservableCollection<SelectableClub> Clubs { get; } = [];

    [ObservableProperty]
    public partial HouseholdDto? Household { get; set; }

    public string UserName => session.User?.DisplayName ?? string.Empty;

    public static string ClubTypeName(ClubType type) => type switch
    {
        ClubType.CreditCard => "כרטיס אשראי",
        ClubType.ConsumerClub => "מועדון צרכנות",
        ClubType.WorkersCommittee => "ועד עובדים",
        _ => type.ToString()
    };

    public async Task LoadAsync()
    {
        await RunAsync(async () =>
        {
            var allClubs = api.GetClubsAsync();
            var householdClubs = api.GetHouseholdClubsAsync();
            var household = api.GetHouseholdAsync();
            await Task.WhenAll(allClubs, householdClubs, household);

            var selected = householdClubs.Result.Select(c => c.Id).ToHashSet();
            Clubs.Clear();
            foreach (var club in allClubs.Result.OrderBy(c => c.Type).ThenBy(c => c.Name))
            {
                Clubs.Add(new SelectableClub(club, selected.Contains(club.Id)));
            }
            Household = household.Result;
        });
        OnPropertyChanged(nameof(UserName));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var clubIds = Clubs.Where(c => c.IsSelected).Select(c => c.Item.Id).ToList();
        if (await RunAsync(() => api.UpdateHouseholdClubsAsync(clubIds)))
        {
            await dialogs.AlertAsync("נשמר", clubIds.Count == 0
                ? "לא סומנו מועדונים, ולכן החיפוש לא יחזיר הטבות."
                : $"סומנו {clubIds.Count} מועדונים למשק הבית.");
        }
    }

    [RelayCommand]
    private Task ManageMerchantsAsync() => navigation.GoToAsync(Routes.Merchants);

    [RelayCommand]
    private async Task SignOutAsync()
    {
        if (await dialogs.ConfirmAsync("התנתקות", "להתנתק מהחשבון?", "התנתקות"))
        {
            await session.ClearAsync();
            await navigation.ShowLoginAsync();
        }
    }
}

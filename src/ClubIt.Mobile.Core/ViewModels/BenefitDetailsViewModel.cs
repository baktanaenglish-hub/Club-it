using ClubIt.Contracts.Benefits;
using ClubIt.Mobile.Core.Formatting;
using ClubIt.Mobile.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClubIt.Mobile.Core.ViewModels;

/// <summary>Benefit card: club, title, value, validity, branches and the redeem button.</summary>
public partial class BenefitDetailsViewModel(
    IClubItApi api,
    INavigationService navigation,
    IDialogService dialogs,
    IUrlLauncher launcher,
    TimeProvider timeProvider) : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasBenefit), nameof(ValueText), nameof(ValidityText), nameof(BranchesText),
        nameof(TypeText), nameof(CanRedeem), nameof(CanEdit))]
    public partial BenefitDto? Benefit { get; set; }

    public bool HasBenefit => Benefit is not null;
    public string ValueText => Benefit is null ? string.Empty : BenefitFormatter.ValueText(Benefit);
    public string TypeText => Benefit is null ? string.Empty : BenefitFormatter.TypeName(Benefit.BenefitType);
    public string BranchesText => Benefit is null ? string.Empty : BenefitFormatter.BranchesText(Benefit);
    public string ValidityText => Benefit is null
        ? string.Empty
        : BenefitFormatter.ValidityText(Benefit, DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime));
    public bool CanRedeem => !string.IsNullOrWhiteSpace(Benefit?.RedeemUrl);
    public bool CanEdit => Benefit?.CanEdit == true;

    public async Task LoadAsync(int id) =>
        await RunAsync(async () => Benefit = await api.GetBenefitAsync(id));

    [RelayCommand]
    private async Task RedeemAsync()
    {
        if (Benefit?.RedeemUrl is not { } url)
        {
            return;
        }

        if (!await launcher.OpenAsync(url))
        {
            await dialogs.AlertAsync("לא ניתן לפתוח את הקישור", url);
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (Benefit is not null)
        {
            await navigation.GoToAsync(Routes.BenefitEdit, new Dictionary<string, object> { [Routes.IdParameter] = Benefit.Id });
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Benefit is null || !await dialogs.ConfirmAsync("מחיקת הטבה", $"למחוק את \"{Benefit.Title}\"?", "מחיקה"))
        {
            return;
        }

        var id = Benefit.Id;
        if (await RunAsync(() => api.DeleteBenefitAsync(id)))
        {
            await navigation.GoBackAsync();
        }
    }
}

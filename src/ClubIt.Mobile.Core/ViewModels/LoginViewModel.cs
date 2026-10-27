using ClubIt.Contracts.Auth;
using ClubIt.Mobile.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClubIt.Mobile.Core.ViewModels;

/// <summary>Login and registration (create a household or join one with an invite code).</summary>
public partial class LoginViewModel : ViewModelBase
{
    private readonly IClubItApi _api;
    private readonly ISessionStore _session;
    private readonly INavigationService _navigation;
    private readonly IServerSettings _server;

    public LoginViewModel(IClubItApi api, ISessionStore session, INavigationService navigation, IServerSettings server)
    {
        _api = api;
        _session = session;
        _navigation = navigation;
        _server = server;

        ServerAddressText = server.BaseAddress?.ToString() ?? string.Empty;
        // Without a configured server nothing works, so open the field straight away.
        ShowServerSettings = server.BaseAddress is null;
    }

    [ObservableProperty]
    public partial string ServerAddressText { get; set; }

    [ObservableProperty]
    public partial bool ShowServerSettings { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title), nameof(SubmitText), nameof(ToggleModeText))]
    public partial bool IsRegisterMode { get; set; }

    /// <summary>When registering: true = join an existing household, false = create a new one.</summary>
    [ObservableProperty]
    public partial bool JoinExistingHousehold { get; set; }

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DisplayName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HouseholdName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string InviteCode { get; set; } = string.Empty;

    public string Title => IsRegisterMode ? "הרשמה" : "התחברות";

    public string SubmitText => IsRegisterMode ? "הרשמה" : "כניסה";

    public string ToggleModeText => IsRegisterMode ? "כבר רשומים? להתחברות" : "משתמשים חדשים? להרשמה";

    [RelayCommand]
    private void ToggleServerSettings() => ShowServerSettings = !ShowServerSettings;

    [RelayCommand]
    private void ToggleMode()
    {
        IsRegisterMode = !IsRegisterMode;
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (!ServerAddress.TryParse(ServerAddressText, out var serverAddress))
        {
            ShowServerSettings = true;
            ErrorMessage = "כתובת השרת אינה תקינה. למשל: clubit-api.onrender.com";
            return;
        }

        if (serverAddress != _server.BaseAddress)
        {
            _server.SetBaseAddress(serverAddress!);
            ServerAddressText = serverAddress!.ToString();
        }

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "יש להזין אימייל וסיסמה.";
            return;
        }

        if (IsRegisterMode && string.IsNullOrWhiteSpace(DisplayName))
        {
            ErrorMessage = "יש להזין שם.";
            return;
        }

        if (IsRegisterMode && JoinExistingHousehold && string.IsNullOrWhiteSpace(InviteCode))
        {
            ErrorMessage = "יש להזין את קוד ההצטרפות שקיבלתם מבן/בת הזוג.";
            return;
        }

        if (IsRegisterMode && !JoinExistingHousehold && string.IsNullOrWhiteSpace(HouseholdName))
        {
            ErrorMessage = "יש להזין שם למשק הבית.";
            return;
        }

        AuthResponse? auth = null;
        var succeeded = await RunAsync(async () =>
        {
            auth = IsRegisterMode
                ? await _api.RegisterAsync(new RegisterRequest(
                    Email.Trim(),
                    Password,
                    DisplayName.Trim(),
                    JoinExistingHousehold ? null : HouseholdName.Trim(),
                    JoinExistingHousehold ? InviteCode.Trim() : null))
                : await _api.LoginAsync(new LoginRequest(Email.Trim(), Password));
        });

        if (succeeded && auth is not null)
        {
            await _session.SaveAsync(auth);
            Password = string.Empty;
            await _navigation.ShowMainAsync();
        }
    }
}

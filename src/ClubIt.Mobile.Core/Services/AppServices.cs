namespace ClubIt.Mobile.Core.Services;

/// <summary>Shell navigation, implemented by the MAUI app.</summary>
public interface INavigationService
{
    Task GoToAsync(string route, IDictionary<string, object>? parameters = null);
    Task GoBackAsync();
    Task ShowLoginAsync();
    Task ShowMainAsync();
}

/// <summary>User dialogs, implemented by the MAUI app.</summary>
public interface IDialogService
{
    Task AlertAsync(string title, string message);
    Task<bool> ConfirmAsync(string title, string message, string accept = "כן", string cancel = "ביטול");
}

/// <summary>Opens a redeem link in the club's app (deep link) or the browser.</summary>
public interface IUrlLauncher
{
    Task<bool> OpenAsync(string url);
}

public static class Routes
{
    public const string Login = "//login";
    public const string Search = "//main/search";
    public const string BenefitDetails = "benefit";
    public const string BenefitEdit = "benefit-edit";
    public const string Merchants = "merchants";

    public const string IdParameter = "id";
}

using ClubIt.Mobile.Core.Services;

namespace ClubIt.Mobile.Services;

public class ShellNavigationService : INavigationService
{
    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null) =>
        MainThread.InvokeOnMainThreadAsync(() =>
            parameters is null ? Shell.Current.GoToAsync(route) : Shell.Current.GoToAsync(route, parameters));

    public Task GoBackAsync() => MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(".."));

    public Task ShowLoginAsync() => MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(Routes.Login));

    public Task ShowMainAsync() => MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(Routes.Search));
}

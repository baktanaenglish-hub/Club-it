using ClubIt.Mobile.Core.Services;

namespace ClubIt.Tests.Mobile;

public class FakeNavigation : INavigationService
{
    public List<(string Route, IDictionary<string, object>? Parameters)> Visited { get; } = [];
    public int BackCount { get; private set; }
    public bool ShowedMain { get; private set; }
    public bool ShowedLogin { get; private set; }

    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        Visited.Add((route, parameters));
        return Task.CompletedTask;
    }

    public Task GoBackAsync()
    {
        BackCount++;
        return Task.CompletedTask;
    }

    public Task ShowLoginAsync()
    {
        ShowedLogin = true;
        return Task.CompletedTask;
    }

    public Task ShowMainAsync()
    {
        ShowedMain = true;
        return Task.CompletedTask;
    }
}

public class FakeDialogs(bool confirm = true) : IDialogService
{
    public List<string> Alerts { get; } = [];

    public Task AlertAsync(string title, string message)
    {
        Alerts.Add(title);
        return Task.CompletedTask;
    }

    public Task<bool> ConfirmAsync(string title, string message, string accept = "כן", string cancel = "ביטול") =>
        Task.FromResult(confirm);
}

public class FakeLauncher : IUrlLauncher
{
    public List<string> Opened { get; } = [];

    public Task<bool> OpenAsync(string url)
    {
        Opened.Add(url);
        return Task.FromResult(true);
    }
}

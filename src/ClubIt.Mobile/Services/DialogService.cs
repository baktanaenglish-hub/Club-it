using ClubIt.Mobile.Core.Services;

namespace ClubIt.Mobile.Services;

public class DialogService : IDialogService
{
    private static Page? CurrentPage => Shell.Current?.CurrentPage ?? Application.Current?.Windows.FirstOrDefault()?.Page;

    public Task AlertAsync(string title, string message) =>
        MainThread.InvokeOnMainThreadAsync(() =>
            CurrentPage?.DisplayAlertAsync(title, message, "אישור", FlowDirection.RightToLeft) ?? Task.CompletedTask);

    public Task<bool> ConfirmAsync(string title, string message, string accept = "כן", string cancel = "ביטול") =>
        MainThread.InvokeOnMainThreadAsync(() =>
            CurrentPage?.DisplayAlertAsync(title, message, accept, cancel, FlowDirection.RightToLeft) ?? Task.FromResult(false));
}

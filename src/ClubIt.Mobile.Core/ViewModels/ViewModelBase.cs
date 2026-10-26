using ClubIt.Mobile.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClubIt.Mobile.Core.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string? ErrorMessage { get; set; }

    public bool IsNotBusy => !IsBusy;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>Runs <paramref name="action"/> with the busy flag set, turning API errors into <see cref="ErrorMessage"/>.</summary>
    protected async Task<bool> RunAsync(Func<Task> action)
    {
        if (IsBusy)
        {
            return false;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await action();
            return true;
        }
        catch (ApiException e)
        {
            ErrorMessage = e.UserMessage;
            return false;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

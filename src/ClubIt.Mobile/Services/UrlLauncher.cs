using ClubIt.Mobile.Core.Services;

namespace ClubIt.Mobile.Services;

/// <summary>
/// Opens redeem links. Web links go through the system launcher, so universal / app links open the
/// club's app when it is installed. Custom-scheme deep links (e.g. "clubapp://...") are opened directly.
/// </summary>
public class UrlLauncher : IUrlLauncher
{
    public async Task<bool> OpenAsync(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        try
        {
            return await MainThread.InvokeOnMainThreadAsync(() => Launcher.Default.OpenAsync(uri));
        }
        catch (Exception) when (uri.Scheme is "http" or "https")
        {
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

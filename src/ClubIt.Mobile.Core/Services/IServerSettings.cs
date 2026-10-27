namespace ClubIt.Mobile.Core.Services;

/// <summary>Where the API lives. Set at build time and changeable from the login screen.</summary>
public interface IServerSettings
{
    Uri? BaseAddress { get; }

    void SetBaseAddress(Uri address);
}

public class InMemoryServerSettings(Uri? address = null) : IServerSettings
{
    public Uri? BaseAddress { get; private set; } = address;

    public void SetBaseAddress(Uri address) => BaseAddress = address;
}

public static class ServerAddress
{
    /// <summary>
    /// Parses what a user types as the server address. "clubit-api.onrender.com" becomes
    /// "https://clubit-api.onrender.com/". Only http and https are accepted.
    /// </summary>
    public static bool TryParse(string? text, out Uri? address)
    {
        address = null;
        var value = text?.Trim();
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (!value.Contains("://", StringComparison.Ordinal))
        {
            value = "https://" + value;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp) ||
            string.IsNullOrEmpty(uri.Host))
        {
            return false;
        }

        // A trailing slash makes relative API paths resolve under the address instead of replacing its last segment.
        address = new Uri(uri.GetLeftPart(UriPartial.Path).TrimEnd('/') + "/");
        return true;
    }
}

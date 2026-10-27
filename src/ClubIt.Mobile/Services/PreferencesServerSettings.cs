using System.Reflection;
using ClubIt.Mobile.Core.Services;

namespace ClubIt.Mobile.Services;

/// <summary>
/// The API address saved on the device. Until the user sets one, the address baked in at build time
/// (MSBuild property <c>ClubItApiUrl</c>) is used, and in debug builds the development machine.
/// </summary>
public class PreferencesServerSettings : IServerSettings
{
    private const string Key = "server_address";

    public PreferencesServerSettings()
    {
        var saved = Preferences.Default.Get<string?>(Key, null);
        BaseAddress = ServerAddress.TryParse(saved, out var address) ? address : DefaultAddress();
    }

    public Uri? BaseAddress { get; private set; }

    public void SetBaseAddress(Uri address)
    {
        BaseAddress = address;
        Preferences.Default.Set(Key, address.ToString());
    }

    private static Uri? DefaultAddress()
    {
        var configured = typeof(PreferencesServerSettings).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "ClubItApiUrl")?.Value;
        if (ServerAddress.TryParse(configured, out var address))
        {
            return address;
        }

#if DEBUG
        // The Android emulator reaches the development machine as 10.0.2.2.
        return DeviceInfo.Platform == DevicePlatform.Android
            ? new Uri("http://10.0.2.2:5136/")
            : new Uri("http://localhost:5136/");
#else
        return null;
#endif
    }
}

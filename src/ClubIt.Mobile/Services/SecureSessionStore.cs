using System.Text.Json;
using ClubIt.Contracts.Auth;
using ClubIt.Mobile.Core.Services;

namespace ClubIt.Mobile.Services;

/// <summary>Keeps the JWT in the platform keychain / keystore via SecureStorage.</summary>
public class SecureSessionStore : ISessionStore
{
    private const string SessionKey = "clubit_session";

    private AuthResponse? _session;

    public string? Token => IsSignedIn ? _session!.Token : null;
    public UserDto? User => _session?.User;
    public bool IsSignedIn => _session is not null && _session.ExpiresAtUtc > DateTime.UtcNow;

    public async Task LoadAsync()
    {
        try
        {
            var json = await SecureStorage.Default.GetAsync(SessionKey);
            _session = json is null ? null : JsonSerializer.Deserialize<AuthResponse>(json, ClubItApiClient.JsonOptions);
        }
        catch (Exception)
        {
            // Corrupt or inaccessible storage (e.g. after restoring a backup): start signed out.
            _session = null;
            SecureStorage.Default.Remove(SessionKey);
        }
    }

    public async Task SaveAsync(AuthResponse auth)
    {
        _session = auth;
        await SecureStorage.Default.SetAsync(SessionKey, JsonSerializer.Serialize(auth, ClubItApiClient.JsonOptions));
    }

    public Task ClearAsync()
    {
        _session = null;
        SecureStorage.Default.Remove(SessionKey);
        return Task.CompletedTask;
    }
}

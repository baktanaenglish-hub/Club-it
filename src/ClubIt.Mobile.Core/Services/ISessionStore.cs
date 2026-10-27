using ClubIt.Contracts.Auth;

namespace ClubIt.Mobile.Core.Services;

/// <summary>Persists the signed-in session (JWT and user). Implemented with SecureStorage on devices.</summary>
public interface ISessionStore
{
    string? Token { get; }
    UserDto? User { get; }
    bool IsSignedIn { get; }

    Task LoadAsync();
    Task SaveAsync(AuthResponse auth);
    Task ClearAsync();
}

/// <summary>Non-persistent session store, used by tests and as a fallback.</summary>
public class InMemorySessionStore : ISessionStore
{
    private DateTime _expiresAtUtc;

    public string? Token { get; private set; }
    public UserDto? User { get; private set; }
    public bool IsSignedIn => Token is not null && _expiresAtUtc > DateTime.UtcNow;

    public Task LoadAsync() => Task.CompletedTask;

    public Task SaveAsync(AuthResponse auth)
    {
        Token = auth.Token;
        User = auth.User;
        _expiresAtUtc = auth.ExpiresAtUtc;
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        Token = null;
        User = null;
        _expiresAtUtc = default;
        return Task.CompletedTask;
    }
}

using ClubIt.Contracts.Auth;
using ClubIt.Mobile.Core.Services;
using ClubIt.Tests.Infrastructure;

namespace ClubIt.Tests.Mobile;

/// <summary>Drives the mobile API client and view models against the in-process API.</summary>
public abstract class MobileTestBase(ClubItApiFactory factory) : ApiTestBase(factory)
{
    protected InMemorySessionStore Session { get; } = new();
    protected FakeNavigation Navigation { get; } = new();
    protected FakeDialogs Dialogs { get; } = new();
    protected InMemoryServerSettings Server { get; } = new(new Uri("http://localhost/"));

    protected ClubItApiClient CreateApi() => new(Factory.CreateClient(), Session);

    protected async Task<ClubItApiClient> SignedInApiAsync()
    {
        var api = CreateApi();
        var auth = await api.RegisterAsync(new RegisterRequest(
            $"mobile-{Guid.NewGuid():N}@example.com", "Password1", "נייד", "בית נייד", null));
        await Session.SaveAsync(auth);
        return api;
    }
}

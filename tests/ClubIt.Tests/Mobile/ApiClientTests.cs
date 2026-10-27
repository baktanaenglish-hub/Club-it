using System.Net;
using ClubIt.Contracts.Auth;
using ClubIt.Mobile.Core.Services;
using ClubIt.Tests.Infrastructure;

namespace ClubIt.Tests.Mobile;

public class ApiClientTests(ClubItApiFactory factory) : MobileTestBase(factory)
{
    [Fact]
    public async Task Client_round_trips_benefits_through_the_api()
    {
        var api = await SignedInApiAsync();
        await api.UpdateHouseholdClubsAsync([Max]);

        var created = await api.CreateBenefitAsync(NewBenefit(Max, Shufersal, allBranches: false, branchIds: [ShufersalAzrieli]));
        var found = await api.SearchBenefitsAsync("shufer sal", ShufersalAzrieli);

        Assert.Equal([created.Id], found.Select(b => b.Id));
        Assert.Equal(ClubIt.Contracts.BenefitType.Percentage, found[0].BenefitType);

        await api.DeleteBenefitAsync(created.Id);
        Assert.Empty(await api.SearchBenefitsAsync("שופרסל", null));
    }

    [Fact]
    public async Task Validation_errors_surface_as_hebrew_messages()
    {
        var api = await SignedInApiAsync();

        var error = await Assert.ThrowsAsync<ApiException>(() =>
            api.CreateBenefitAsync(NewBenefit(Max, Shufersal) with { ValidUntil = null }));

        Assert.Equal(HttpStatusCode.BadRequest, error.StatusCode);
        Assert.Contains("תאריך תוקף", error.UserMessage);
    }

    [Fact]
    public async Task Rejected_token_raises_session_expired_but_failed_login_does_not()
    {
        var api = CreateApi();
        var expired = 0;
        api.SessionExpired += (_, _) => expired++;

        var login = await Assert.ThrowsAsync<ApiException>(() => api.LoginAsync(new LoginRequest("nobody@example.com", "Password1")));
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
        Assert.Equal(0, expired);

        await Session.SaveAsync(new AuthResponse("not-a-valid-token", DateTime.UtcNow.AddDays(1),
            new UserDto(Guid.NewGuid(), "x@example.com", "x", Guid.NewGuid(), "x")));
        await Assert.ThrowsAsync<ApiException>(() => api.GetClubsAsync());
        Assert.Equal(1, expired);
    }

    [Fact]
    public async Task Server_address_comes_from_settings_and_can_change_at_runtime()
    {
        var settings = new InMemoryServerSettings();
        var api = new ClubItApiClient(new HttpClient(Factory.Server.CreateHandler()), Session, settings);

        var missing = await Assert.ThrowsAsync<ApiException>(() => api.LoginAsync(new LoginRequest("a@example.com", "x")));
        Assert.Contains("כתובת שרת", missing.Message);

        settings.SetBaseAddress(new Uri("http://localhost/"));
        var wrongPassword = await Assert.ThrowsAsync<ApiException>(() => api.LoginAsync(new LoginRequest("a@example.com", "x")));
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
    }

    [Fact]
    public async Task Slow_server_reports_a_timeout_message()
    {
        var http = new HttpClient(new DelayHandler()) { BaseAddress = new Uri("http://localhost/"), Timeout = TimeSpan.FromMilliseconds(50) };
        var api = new ClubItApiClient(http, Session);

        var error = await Assert.ThrowsAsync<ApiException>(() => api.GetClubsAsync());

        Assert.Equal(HttpStatusCode.RequestTimeout, error.StatusCode);
        Assert.Contains("מתעורר", error.Message);
    }

    private sealed class DelayHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}

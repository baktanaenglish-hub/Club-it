using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClubIt.Contracts;
using ClubIt.Contracts.Auth;
using ClubIt.Contracts.Benefits;
using ClubIt.Contracts.Categories;
using ClubIt.Contracts.Clubs;
using ClubIt.Contracts.Households;
using ClubIt.Contracts.Merchants;

namespace ClubIt.Mobile.Core.Services;

public class ClubItApiClient(HttpClient http, ISessionStore session) : IClubItApi
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>Raised when the server rejects the stored token, so the app can return to the login screen.</summary>
    public event EventHandler? SessionExpired;

    public Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<AuthResponse>(HttpMethod.Post, "api/auth/register", request, cancellationToken);

    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<AuthResponse>(HttpMethod.Post, "api/auth/login", request, cancellationToken);

    public Task<IReadOnlyList<ClubDto>> GetClubsAsync(CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<ClubDto>>(HttpMethod.Get, "api/clubs", null, cancellationToken);

    public Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<CategoryDto>>(HttpMethod.Get, "api/categories", null, cancellationToken);

    public Task<HouseholdDto> GetHouseholdAsync(CancellationToken cancellationToken = default) =>
        SendAsync<HouseholdDto>(HttpMethod.Get, "api/household", null, cancellationToken);

    public Task<IReadOnlyList<ClubDto>> GetHouseholdClubsAsync(CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<ClubDto>>(HttpMethod.Get, "api/household/clubs", null, cancellationToken);

    public Task<IReadOnlyList<ClubDto>> UpdateHouseholdClubsAsync(IReadOnlyList<int> clubIds, CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<ClubDto>>(HttpMethod.Put, "api/household/clubs", new UpdateHouseholdClubsRequest(clubIds), cancellationToken);

    public Task<IReadOnlyList<BenefitDto>> SearchBenefitsAsync(string query, int? branchId, CancellationToken cancellationToken = default)
    {
        var url = $"api/benefits/search?q={Uri.EscapeDataString(query)}";
        if (branchId is { } id)
        {
            url += $"&branchId={id}";
        }
        return SendAsync<IReadOnlyList<BenefitDto>>(HttpMethod.Get, url, null, cancellationToken);
    }

    public Task<IReadOnlyList<BenefitDto>> GetEntertainmentBenefitsAsync(int? clubId, CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<BenefitDto>>(
            HttpMethod.Get, clubId is { } id ? $"api/benefits/entertainment?clubId={id}" : "api/benefits/entertainment", null, cancellationToken);

    public Task<BenefitDto> GetBenefitAsync(int id, CancellationToken cancellationToken = default) =>
        SendAsync<BenefitDto>(HttpMethod.Get, $"api/benefits/{id}", null, cancellationToken);

    public Task<BenefitDto> CreateBenefitAsync(SaveBenefitRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<BenefitDto>(HttpMethod.Post, "api/benefits", request, cancellationToken);

    public Task<BenefitDto> UpdateBenefitAsync(int id, SaveBenefitRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<BenefitDto>(HttpMethod.Put, $"api/benefits/{id}", request, cancellationToken);

    public Task DeleteBenefitAsync(int id, CancellationToken cancellationToken = default) =>
        SendAsync<object?>(HttpMethod.Delete, $"api/benefits/{id}", null, cancellationToken);

    public Task<IReadOnlyList<MerchantDto>> SearchMerchantsAsync(string? query, CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<MerchantDto>>(
            HttpMethod.Get, $"api/merchants?q={Uri.EscapeDataString(query ?? string.Empty)}", null, cancellationToken);

    public Task<MerchantDto> CreateMerchantAsync(CreateMerchantRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<MerchantDto>(HttpMethod.Post, "api/merchants", request, cancellationToken);

    public Task<MerchantDto> UpdateMerchantAsync(int id, UpdateMerchantRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<MerchantDto>(HttpMethod.Put, $"api/merchants/{id}", request, cancellationToken);

    public Task<IReadOnlyList<BranchDto>> GetBranchesAsync(int merchantId, CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<BranchDto>>(HttpMethod.Get, $"api/merchants/{merchantId}/branches", null, cancellationToken);

    public Task<BranchDto> CreateBranchAsync(int merchantId, CreateBranchRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<BranchDto>(HttpMethod.Post, $"api/merchants/{merchantId}/branches", request, cancellationToken);

    private async Task<T> SendAsync<T>(HttpMethod method, string url, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url);
        if (session.Token is { } token)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, body.GetType(), options: JsonOptions);
        }

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException e)
        {
            throw new ApiException(HttpStatusCode.ServiceUnavailable, "אין חיבור לשרת. בדקו את החיבור לאינטרנט ונסו שוב.", null, e);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await ToExceptionAsync(response, cancellationToken);
            }

            if (response.StatusCode == HttpStatusCode.NoContent || typeof(T) == typeof(object))
            {
                return default!;
            }

            return (await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken))!;
        }
    }

    private async Task<ApiException> ToExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var isAuthCall = response.RequestMessage?.RequestUri?.AbsolutePath.Contains("/api/auth/", StringComparison.Ordinal) == true;
        if (response.StatusCode == HttpStatusCode.Unauthorized && !isAuthCall)
        {
            SessionExpired?.Invoke(this, EventArgs.Empty);
        }

        ProblemResponse? problem = null;
        try
        {
            problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            // Not a problem-details body; fall back to a generic message.
        }

        var message = problem?.Title ?? response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "יש להתחבר מחדש.",
            HttpStatusCode.NotFound => "הפריט לא נמצא.",
            HttpStatusCode.TooManyRequests => "יותר מדי ניסיונות. נסו שוב בעוד דקה.",
            _ => "אירעה שגיאה. נסו שוב."
        };

        return new ApiException(response.StatusCode, message, problem?.Errors?.AsReadOnly());
    }
}

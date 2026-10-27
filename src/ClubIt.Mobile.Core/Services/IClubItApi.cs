using ClubIt.Contracts.Auth;
using ClubIt.Contracts.Benefits;
using ClubIt.Contracts.Categories;
using ClubIt.Contracts.Clubs;
using ClubIt.Contracts.Households;
using ClubIt.Contracts.Merchants;

namespace ClubIt.Mobile.Core.Services;

/// <summary>Typed client for the Club it API. The app never talks to the database directly.</summary>
public interface IClubItApi
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClubDto>> GetClubsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    Task<HouseholdDto> GetHouseholdAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClubDto>> GetHouseholdClubsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClubDto>> UpdateHouseholdClubsAsync(IReadOnlyList<int> clubIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BenefitDto>> SearchBenefitsAsync(string query, int? branchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BenefitDto>> GetEntertainmentBenefitsAsync(int? clubId, CancellationToken cancellationToken = default);
    Task<BenefitDto> GetBenefitAsync(int id, CancellationToken cancellationToken = default);
    Task<BenefitDto> CreateBenefitAsync(SaveBenefitRequest request, CancellationToken cancellationToken = default);
    Task<BenefitDto> UpdateBenefitAsync(int id, SaveBenefitRequest request, CancellationToken cancellationToken = default);
    Task DeleteBenefitAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MerchantDto>> SearchMerchantsAsync(string? query, CancellationToken cancellationToken = default);
    Task<MerchantDto> CreateMerchantAsync(CreateMerchantRequest request, CancellationToken cancellationToken = default);
    Task<MerchantDto> UpdateMerchantAsync(int id, UpdateMerchantRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchDto>> GetBranchesAsync(int merchantId, CancellationToken cancellationToken = default);
    Task<BranchDto> CreateBranchAsync(int merchantId, CreateBranchRequest request, CancellationToken cancellationToken = default);
}

namespace ClubIt.Contracts.Merchants;

public record MerchantDto(int Id, string Name, int CategoryId, string CategoryName, IReadOnlyList<string> Aliases);

public record CreateMerchantRequest(string Name, int CategoryId, IReadOnlyList<string>? Aliases);

public record UpdateMerchantRequest(string Name, int CategoryId, IReadOnlyList<string>? Aliases);

public record BranchDto(int Id, int MerchantId, string Name, string? City, string? Address, double? Latitude, double? Longitude);

public record CreateBranchRequest(string Name, string? City, string? Address, double? Latitude, double? Longitude);

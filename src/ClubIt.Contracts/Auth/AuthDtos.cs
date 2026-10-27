namespace ClubIt.Contracts.Auth;

/// <summary>
/// Registration either creates a new household (<see cref="HouseholdName"/>) or joins
/// an existing one (<see cref="InviteCode"/>). Exactly one of them must be provided.
/// </summary>
public record RegisterRequest(
    string Email,
    string Password,
    string DisplayName,
    string? HouseholdName,
    string? InviteCode);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Token,
    DateTime ExpiresAtUtc,
    UserDto User);

public record UserDto(Guid Id, string Email, string DisplayName, Guid HouseholdId, string HouseholdName);

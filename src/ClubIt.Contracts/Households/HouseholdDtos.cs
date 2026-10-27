namespace ClubIt.Contracts.Households;

public record HouseholdDto(Guid Id, string Name, string InviteCode, IReadOnlyList<HouseholdMemberDto> Members);

public record HouseholdMemberDto(Guid Id, string DisplayName, string Email);

public record UpdateHouseholdClubsRequest(IReadOnlyList<int> ClubIds);

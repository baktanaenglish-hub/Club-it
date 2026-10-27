namespace ClubIt.Contracts.Clubs;

public record ClubDto(int Id, string Name, ClubType Type, string? LogoUrl, string? WebsiteUrl);

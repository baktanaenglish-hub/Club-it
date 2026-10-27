using ClubIt.Domain.Enums;

namespace ClubIt.Domain.Entities;

public class Club
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ClubType Type { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }

    public ICollection<Benefit> Benefits { get; set; } = new List<Benefit>();
    public ICollection<HouseholdClub> Households { get; set; } = new List<HouseholdClub>();
}

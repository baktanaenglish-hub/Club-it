namespace ClubIt.Domain.Entities;

public class Household
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    /// <summary>Code a partner enters at registration to join this household.</summary>
    public required string InviteCode { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<HouseholdClub> Clubs { get; set; } = new List<HouseholdClub>();
}

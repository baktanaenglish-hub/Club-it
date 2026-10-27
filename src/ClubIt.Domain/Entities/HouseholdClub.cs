namespace ClubIt.Domain.Entities;

/// <summary>A club the household marked in settings. Shared by all household members.</summary>
public class HouseholdClub
{
    public Guid HouseholdId { get; set; }
    public Household? Household { get; set; }

    public int ClubId { get; set; }
    public Club? Club { get; set; }
}

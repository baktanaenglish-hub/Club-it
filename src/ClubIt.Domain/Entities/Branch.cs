namespace ClubIt.Domain.Entities;

public class Branch
{
    public int Id { get; set; }
    public int MerchantId { get; set; }
    public Merchant? Merchant { get; set; }

    public required string Name { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }

    // Kept for the location-based search planned for phase 1.5.
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public ICollection<BenefitBranch> Benefits { get; set; } = new List<BenefitBranch>();
}

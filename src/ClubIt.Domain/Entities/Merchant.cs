using ClubIt.Domain.Common;

namespace ClubIt.Domain.Entities;

/// <summary>A chain or a business (e.g. Shufersal, Cinema City).</summary>
public class Merchant
{
    private string _name = string.Empty;

    public int Id { get; set; }

    public required string Name
    {
        get => _name;
        set
        {
            _name = value;
            NormalizedName = NameNormalizer.Normalize(value);
        }
    }

    /// <summary>Search key maintained from <see cref="Name"/>.</summary>
    public string NormalizedName { get; private set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<MerchantAlias> Aliases { get; set; } = new List<MerchantAlias>();
    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<Benefit> Benefits { get; set; } = new List<Benefit>();
}

using ClubIt.Domain.Common;

namespace ClubIt.Domain.Entities;

/// <summary>An alternative name used for search (e.g. "shufersal", "שופר סל").</summary>
public class MerchantAlias
{
    private string _alias = string.Empty;

    public int Id { get; set; }
    public int MerchantId { get; set; }
    public Merchant? Merchant { get; set; }

    public required string Alias
    {
        get => _alias;
        set
        {
            _alias = value;
            NormalizedAlias = NameNormalizer.Normalize(value);
        }
    }

    public string NormalizedAlias { get; private set; } = string.Empty;
}

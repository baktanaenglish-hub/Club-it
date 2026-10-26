namespace ClubIt.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }

    /// <summary>Marks "outings" categories (movies, shows, attractions) shown in the entertainment tab.</summary>
    public bool IsEntertainment { get; set; }

    public ICollection<Merchant> Merchants { get; set; } = new List<Merchant>();
}

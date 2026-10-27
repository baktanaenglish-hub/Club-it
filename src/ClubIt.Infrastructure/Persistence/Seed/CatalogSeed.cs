using ClubIt.Domain.Common;
using ClubIt.Domain.Entities;
using ClubIt.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubIt.Infrastructure.Persistence.Seed;

/// <summary>
/// Reference data shipped with the database: clubs, categories and common merchants.
/// Benefits are not seeded; in phase 1 the household enters them manually.
/// Changing this data requires a new migration.
/// </summary>
internal class ClubSeed : IEntityTypeConfiguration<Club>
{
    public void Configure(EntityTypeBuilder<Club> builder) => builder.HasData(
        Club(1, "מקס", ClubType.CreditCard, "https://www.max.co.il"),
        Club(2, "כאל", ClubType.CreditCard, "https://www.cal-online.co.il"),
        Club(3, "ישראכרט", ClubType.CreditCard, "https://www.isracard.co.il"),
        Club(4, "אמריקן אקספרס", ClubType.CreditCard, "https://www.americanexpress.co.il"),
        Club(5, "דיינרס", ClubType.CreditCard, "https://www.diners.co.il"),
        Club(6, "פיס", ClubType.ConsumerClub, "https://www.pais.co.il"),
        Club(7, "חבר", ClubType.ConsumerClub, "https://www.hvr.co.il"),
        Club(8, "הייטקזון", ClubType.WorkersCommittee, "https://www.hitechzone.co.il"));

    private static object Club(int id, string name, ClubType type, string website) =>
        new { Id = id, Name = name, Type = type, WebsiteUrl = website };
}

internal class CategorySeed : IEntityTypeConfiguration<Category>
{
    public const int Supermarket = 1;
    public const int Pharmacy = 2;
    public const int Fashion = 3;
    public const int Restaurants = 4;
    public const int Electronics = 5;
    public const int Home = 6;
    public const int Fuel = 7;
    public const int Movies = 8;
    public const int Shows = 9;
    public const int Attractions = 10;
    public const int Other = 11;

    public void Configure(EntityTypeBuilder<Category> builder) => builder.HasData(
        new { Id = Supermarket, Name = "סופרמרקט", IsEntertainment = false },
        new { Id = Pharmacy, Name = "פארם וטיפוח", IsEntertainment = false },
        new { Id = Fashion, Name = "אופנה והנעלה", IsEntertainment = false },
        new { Id = Restaurants, Name = "מסעדות ובתי קפה", IsEntertainment = false },
        new { Id = Electronics, Name = "אלקטרוניקה", IsEntertainment = false },
        new { Id = Home, Name = "בית ועיצוב", IsEntertainment = false },
        new { Id = Fuel, Name = "דלק", IsEntertainment = false },
        new { Id = Movies, Name = "קולנוע", IsEntertainment = true },
        new { Id = Shows, Name = "הצגות ומופעים", IsEntertainment = true },
        new { Id = Attractions, Name = "אטרקציות", IsEntertainment = true },
        new { Id = Other, Name = "אחר", IsEntertainment = false });
}

internal class MerchantSeed : IEntityTypeConfiguration<Merchant>, IEntityTypeConfiguration<MerchantAlias>
{
    private static readonly (int Id, string Name, int CategoryId, string[] Aliases)[] Merchants =
    [
        (1, "שופרסל", CategorySeed.Supermarket, ["שופר סל", "shufersal", "שופרסל דיל", "שופרסל אקספרס"]),
        (2, "רמי לוי", CategorySeed.Supermarket, ["rami levy", "רמי לוי שיווק השקמה"]),
        (3, "ויקטורי", CategorySeed.Supermarket, ["victory"]),
        (4, "יוחננוף", CategorySeed.Supermarket, ["yochananof"]),
        (5, "סופר-פארם", CategorySeed.Pharmacy, ["super pharm"]),
        (6, "BE", CategorySeed.Pharmacy, ["בי", "בי פארם"]),
        (7, "קסטרו", CategorySeed.Fashion, ["castro"]),
        (8, "פוקס", CategorySeed.Fashion, ["fox"]),
        (9, "KSP", CategorySeed.Electronics, ["קיי אס פי"]),
        (10, "איקאה", CategorySeed.Home, ["ikea", "איקיאה"]),
        (11, "פז", CategorySeed.Fuel, ["paz"]),
        (12, "סינמה סיטי", CategorySeed.Movies, ["cinema city"]),
        (13, "יס פלאנט", CategorySeed.Movies, ["yes planet", "פלאנט"]),
        (14, "תיאטרון הקאמרי", CategorySeed.Shows, ["קאמרי", "cameri"]),
        (15, "תיאטרון הבימה", CategorySeed.Shows, ["הבימה", "habima"]),
        (16, "הספארי ברמת גן", CategorySeed.Attractions, ["ספארי", "safari"]),
        (17, "לונה פארק", CategorySeed.Attractions, ["luna park"])
    ];

    public void Configure(EntityTypeBuilder<Merchant> builder) => builder.HasData(
        Merchants.Select(m => (object)new
        {
            m.Id,
            m.Name,
            NormalizedName = NameNormalizer.Normalize(m.Name),
            m.CategoryId
        }));

    public void Configure(EntityTypeBuilder<MerchantAlias> builder) => builder.HasData(
        Merchants
            .SelectMany(m => m.Aliases.Select(alias => (MerchantId: m.Id, Alias: alias)))
            .Select((a, index) => (object)new
            {
                Id = index + 1,
                a.MerchantId,
                a.Alias,
                NormalizedAlias = NameNormalizer.Normalize(a.Alias)
            }));
}

internal class BranchSeed : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder) => builder.HasData(
        new { Id = 1, MerchantId = 1, Name = "שופרסל דיל עזריאלי", City = "תל אביב" },
        new { Id = 2, MerchantId = 1, Name = "שופרסל שלי רמת אביב", City = "תל אביב" },
        new { Id = 3, MerchantId = 1, Name = "שופרסל דיל תלפיות", City = "ירושלים" },
        new { Id = 4, MerchantId = 12, Name = "סינמה סיטי גלילות", City = "רמת השרון" },
        new { Id = 5, MerchantId = 12, Name = "סינמה סיטי ירושלים", City = "ירושלים" },
        new { Id = 6, MerchantId = 13, Name = "יס פלאנט ראשון לציון", City = "ראשון לציון" });
}

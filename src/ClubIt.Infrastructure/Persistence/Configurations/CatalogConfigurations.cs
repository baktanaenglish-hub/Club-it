using ClubIt.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubIt.Infrastructure.Persistence.Configurations;

internal class ClubConfiguration : IEntityTypeConfiguration<Club>
{
    public void Configure(EntityTypeBuilder<Club> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(c => c.LogoUrl).HasMaxLength(500);
        builder.Property(c => c.WebsiteUrl).HasMaxLength(500);
        builder.HasIndex(c => c.Name).IsUnique();
    }
}

internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.Name).IsUnique();
    }
}

internal class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.Property(m => m.Name).HasMaxLength(150).IsRequired();
        builder.Property(m => m.NormalizedName).HasMaxLength(150).IsRequired();
        builder.HasIndex(m => m.NormalizedName).IsUnique();

        builder.HasOne(m => m.Category)
            .WithMany(c => c.Merchants)
            .HasForeignKey(m => m.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal class MerchantAliasConfiguration : IEntityTypeConfiguration<MerchantAlias>
{
    public void Configure(EntityTypeBuilder<MerchantAlias> builder)
    {
        builder.Property(a => a.Alias).HasMaxLength(150).IsRequired();
        builder.Property(a => a.NormalizedAlias).HasMaxLength(150).IsRequired();
        builder.HasIndex(a => new { a.MerchantId, a.NormalizedAlias }).IsUnique();
        builder.HasIndex(a => a.NormalizedAlias);

        builder.HasOne(a => a.Merchant)
            .WithMany(m => m.Aliases)
            .HasForeignKey(a => a.MerchantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.Property(b => b.Name).HasMaxLength(150).IsRequired();
        builder.Property(b => b.City).HasMaxLength(100);
        builder.Property(b => b.Address).HasMaxLength(250);

        builder.HasOne(b => b.Merchant)
            .WithMany(m => m.Branches)
            .HasForeignKey(b => b.MerchantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

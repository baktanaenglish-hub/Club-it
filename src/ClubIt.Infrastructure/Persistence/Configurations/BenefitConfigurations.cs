using ClubIt.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubIt.Infrastructure.Persistence.Configurations;

internal class BenefitConfiguration : IEntityTypeConfiguration<Benefit>
{
    public void Configure(EntityTypeBuilder<Benefit> builder)
    {
        builder.Property(b => b.Title).HasMaxLength(200).IsRequired();
        builder.Property(b => b.Description).HasMaxLength(2000);
        builder.Property(b => b.BenefitType).HasConversion<string>().HasMaxLength(32);
        builder.Property(b => b.Value).HasPrecision(10, 2);
        builder.Property(b => b.RedeemUrl).HasMaxLength(1000);
        builder.Property(b => b.Notes).HasMaxLength(2000);

        builder.HasOne(b => b.Club)
            .WithMany(c => c.Benefits)
            .HasForeignKey(b => b.ClubId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Merchant)
            .WithMany(m => m.Benefits)
            .HasForeignKey(b => b.MerchantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Household)
            .WithMany()
            .HasForeignKey(b => b.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => new { b.MerchantId, b.ValidUntil });
        builder.HasIndex(b => new { b.HouseholdId, b.ClubId });
    }
}

internal class BenefitBranchConfiguration : IEntityTypeConfiguration<BenefitBranch>
{
    public void Configure(EntityTypeBuilder<BenefitBranch> builder)
    {
        builder.HasKey(bb => new { bb.BenefitId, bb.BranchId });

        builder.HasOne(bb => bb.Benefit)
            .WithMany(b => b.Branches)
            .HasForeignKey(bb => bb.BenefitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bb => bb.Branch)
            .WithMany(b => b.Benefits)
            .HasForeignKey(bb => bb.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

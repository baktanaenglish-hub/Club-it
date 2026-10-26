using ClubIt.Domain.Entities;
using ClubIt.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubIt.Infrastructure.Persistence.Configurations;

internal class HouseholdConfiguration : IEntityTypeConfiguration<Household>
{
    public void Configure(EntityTypeBuilder<Household> builder)
    {
        builder.Property(h => h.Name).HasMaxLength(100).IsRequired();
        builder.Property(h => h.InviteCode).HasMaxLength(16).IsRequired();
        builder.HasIndex(h => h.InviteCode).IsUnique();
    }
}

internal class HouseholdClubConfiguration : IEntityTypeConfiguration<HouseholdClub>
{
    public void Configure(EntityTypeBuilder<HouseholdClub> builder)
    {
        builder.HasKey(hc => new { hc.HouseholdId, hc.ClubId });

        builder.HasOne(hc => hc.Household)
            .WithMany(h => h.Clubs)
            .HasForeignKey(hc => hc.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(hc => hc.Club)
            .WithMany(c => c.Households)
            .HasForeignKey(hc => hc.ClubId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();

        builder.HasOne(u => u.Household)
            .WithMany()
            .HasForeignKey(u => u.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.HouseholdId);
    }
}

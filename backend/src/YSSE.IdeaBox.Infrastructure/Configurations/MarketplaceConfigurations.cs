using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Infrastructure.Configurations;

public class MarketplaceListingConfiguration : IEntityTypeConfiguration<MarketplaceListing>
{
    public void Configure(EntityTypeBuilder<MarketplaceListing> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text").IsRequired();
        builder.Property(x => x.Category).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Condition).HasMaxLength(60).IsRequired();
        builder.Property(x => x.PhotoUrl).HasMaxLength(500);
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.CoarseLocation).HasMaxLength(150);
        builder.Property(x => x.ModerationReason).HasMaxLength(500);

        builder.HasIndex(x => x.Status);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.Property(x => x.Message).HasMaxLength(1000);

        builder.HasOne(x => x.Listing)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.MarketplaceListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.RequesterUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

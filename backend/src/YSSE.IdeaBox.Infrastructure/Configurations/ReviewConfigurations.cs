using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Infrastructure.Configurations;

public class IdeaReviewConfiguration : IEntityTypeConfiguration<IdeaReview>
{
    public void Configure(EntityTypeBuilder<IdeaReview> builder)
    {
        builder.Property(x => x.Body).HasColumnType("text").IsRequired();
        builder.Property(x => x.ModerationReason).HasMaxLength(500);

        builder.HasIndex(x => new { x.IdeaId, x.ModerationStatus });

        builder.HasIndex(x => new { x.ReviewerUserId, x.IdeaId })
            .IsUnique()
            .HasFilter("is_deleted = false AND reviewer_user_id IS NOT NULL");

        builder.HasIndex(x => new { x.GuestVisitorId, x.IdeaId })
            .IsUnique()
            .HasFilter("is_deleted = false AND guest_visitor_id IS NOT NULL");

        builder.HasOne(x => x.Idea)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.GuestVisitor)
            .WithMany()
            .HasForeignKey(x => x.GuestVisitorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class GuestVisitorConfiguration : IEntityTypeConfiguration<GuestVisitor>
{
    public void Configure(EntityTypeBuilder<GuestVisitor> builder)
    {
        builder.Property(x => x.GuestIdHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.IpHash).HasMaxLength(128);

        builder.HasIndex(x => x.GuestIdHash).IsUnique();
    }
}

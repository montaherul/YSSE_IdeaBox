using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Infrastructure.Configurations;

public class EntrepreneurInquiryConfiguration : IEntityTypeConfiguration<EntrepreneurInquiry>
{
    public void Configure(EntityTypeBuilder<EntrepreneurInquiry> builder)
    {
        builder.Property(x => x.Message).HasColumnType("text").IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(100);

        builder.HasIndex(x => x.IdeaId);
        builder.HasIndex(x => x.IdempotencyKey);

        builder.HasOne(x => x.Idea)
            .WithMany()
            .HasForeignKey(x => x.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Seeker)
            .WithMany()
            .HasForeignKey(x => x.SeekerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Conversation)
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WatchlistItemConfiguration : IEntityTypeConfiguration<WatchlistItem>
{
    public void Configure(EntityTypeBuilder<WatchlistItem> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.IdeaId }).IsUnique();

        builder.HasOne(x => x.Idea)
            .WithMany()
            .HasForeignKey(x => x.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

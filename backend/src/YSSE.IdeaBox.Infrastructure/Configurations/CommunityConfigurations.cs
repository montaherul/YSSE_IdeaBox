using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Infrastructure.Configurations;

public class ProblemPostConfiguration : IEntityTypeConfiguration<ProblemPost>
{
    public void Configure(EntityTypeBuilder<ProblemPost> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(150).IsRequired();
        builder.Property(x => x.WhoAffected).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.CoarseLocation).HasMaxLength(150);
        builder.Property(x => x.WhyItMatters).HasColumnType("text").IsRequired();
        builder.Property(x => x.Urgency).HasMaxLength(50);
        builder.Property(x => x.PhotoUrl).HasMaxLength(500);
        builder.Property(x => x.Attribution).HasMaxLength(20).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProblemEndorsementConfiguration : IEntityTypeConfiguration<ProblemEndorsement>
{
    public void Configure(EntityTypeBuilder<ProblemEndorsement> builder)
    {
        builder.HasIndex(x => new { x.ProblemPostId, x.UserId }).IsUnique();

        builder.HasOne(x => x.ProblemPost)
            .WithMany(x => x.Endorsements)
            .HasForeignKey(x => x.ProblemPostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class IdeaProblemLinkConfiguration : IEntityTypeConfiguration<IdeaProblemLink>
{
    public void Configure(EntityTypeBuilder<IdeaProblemLink> builder)
    {
        builder.HasOne(x => x.Idea)
            .WithMany()
            .HasForeignKey(x => x.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ProblemPost)
            .WithMany(x => x.IdeaLinks)
            .HasForeignKey(x => x.ProblemPostId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Infrastructure.Configurations;

public class IdeaVoteConfiguration : IEntityTypeConfiguration<IdeaVote>
{
    public void Configure(EntityTypeBuilder<IdeaVote> builder)
    {
        builder.Property(x => x.ExclusionReason).HasMaxLength(500);

        builder.HasIndex(x => new { x.IdeaId, x.IsCounted });
        builder.HasIndex(x => x.VotingPeriodId);

        builder.HasIndex(x => new { x.UserId, x.IdeaId, x.VotingPeriodId })
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasOne(x => x.Idea)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.VotingPeriod)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.VotingPeriodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VotingPeriodConfiguration : IEntityTypeConfiguration<VotingPeriod>
{
    public void Configure(EntityTypeBuilder<VotingPeriod> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
    }
}

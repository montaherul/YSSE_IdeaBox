using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Infrastructure.Configurations;

public class OpportunityOutcomeConfiguration : IEntityTypeConfiguration<OpportunityOutcome>
{
    public void Configure(EntityTypeBuilder<OpportunityOutcome> builder)
    {
        builder.Property(x => x.Description).HasColumnType("text");

        builder.HasIndex(x => x.IdeaId);

        builder.HasOne(x => x.Idea)
            .WithMany()
            .HasForeignKey(x => x.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.ReporterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.VerifiedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Infrastructure.Configurations;

public class IdeaConfiguration : IEntityTypeConfiguration<Idea>
{
    public void Configure(EntityTypeBuilder<Idea> builder)
    {
        builder.Property(x => x.Slug).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ShortSummary).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ProblemStatement).HasColumnType("text");
        builder.Property(x => x.ProposedSolution).HasColumnType("text");
        builder.Property(x => x.TargetAudience).HasMaxLength(500);
        builder.Property(x => x.ExpectedImpact).HasMaxLength(500);
        builder.Property(x => x.SupportSought).HasMaxLength(500);
        builder.Property(x => x.SkillsNeeded).HasMaxLength(500);
        builder.Property(x => x.EstimatedBudget).HasPrecision(18, 2);
        builder.Property(x => x.ExpectedTimeline).HasMaxLength(200);
        builder.Property(x => x.SuccessMetrics).HasMaxLength(1000);
        builder.Property(x => x.CollaborationPreferences).HasMaxLength(500);
        builder.Property(x => x.LanguageTag).HasMaxLength(8).IsRequired();

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => new { x.Status, x.PublishedAt });
        builder.HasIndex(x => new { x.IsFeatured, x.Status });
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.CreatorUserId);

        builder.HasOne(x => x.Creator)
            .WithMany()
            .HasForeignKey(x => x.CreatorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Ideas)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class IdeaRevisionConfiguration : IEntityTypeConfiguration<IdeaRevision>
{
    public void Configure(EntityTypeBuilder<IdeaRevision> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ShortSummary).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ProblemStatement).HasColumnType("text");
        builder.Property(x => x.ProposedSolution).HasColumnType("text");
        builder.Property(x => x.ChangeNote).HasMaxLength(500);

        builder.HasOne(x => x.Idea)
            .WithMany(x => x.Revisions)
            .HasForeignKey(x => x.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class IdeaAttachmentConfiguration : IEntityTypeConfiguration<IdeaAttachment>
{
    public void Configure(EntityTypeBuilder<IdeaAttachment> builder)
    {
        builder.Property(x => x.FileName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();

        builder.HasOne(x => x.Idea)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class IdeaStatusHistoryConfiguration : IEntityTypeConfiguration<IdeaStatusHistory>
{
    public void Configure(EntityTypeBuilder<IdeaStatusHistory> builder)
    {
        builder.Property(x => x.Reason).HasMaxLength(1000);

        builder.HasOne(x => x.Idea)
            .WithMany(x => x.StatusHistory)
            .HasForeignKey(x => x.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class IdeaScreeningDecisionConfiguration : IEntityTypeConfiguration<IdeaScreeningDecision>
{
    public void Configure(EntityTypeBuilder<IdeaScreeningDecision> builder)
    {
        builder.Property(x => x.Feedback).HasMaxLength(2000);

        builder.HasOne(x => x.Idea)
            .WithMany(x => x.ScreeningDecisions)
            .HasForeignKey(x => x.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasIndex(x => x.Slug).IsUnique();
    }
}

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(120).IsRequired();

        builder.HasIndex(x => x.Slug).IsUnique();
    }
}

public class IdeaTagConfiguration : IEntityTypeConfiguration<IdeaTag>
{
    public void Configure(EntityTypeBuilder<IdeaTag> builder)
    {
        builder.HasOne(x => x.Idea)
            .WithMany(x => x.IdeaTags)
            .HasForeignKey(x => x.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tag)
            .WithMany(x => x.IdeaTags)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class IdeaCommentConfiguration : IEntityTypeConfiguration<IdeaComment>
{
    public void Configure(EntityTypeBuilder<IdeaComment> builder)
    {
        builder.Property(x => x.Body).HasColumnType("text").IsRequired();
        builder.Property(x => x.ModerationReason).HasMaxLength(500);

        builder.HasOne(x => x.Idea)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<IdeaComment>()
            .WithMany()
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class IdeaFollowConfiguration : IEntityTypeConfiguration<IdeaFollow>
{
    public void Configure(EntityTypeBuilder<IdeaFollow> builder)
    {
        builder.HasOne(x => x.Idea)
            .WithMany(x => x.Follows)
            .HasForeignKey(x => x.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.IdeaId, x.UserId }).IsUnique();
    }
}

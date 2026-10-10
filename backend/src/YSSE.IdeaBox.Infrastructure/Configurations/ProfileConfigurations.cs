using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Infrastructure.Configurations;

public class MemberProfileConfiguration : IEntityTypeConfiguration<MemberProfile>
{
    public void Configure(EntityTypeBuilder<MemberProfile> builder)
    {
        builder.Property(x => x.DisplayName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Biography).HasColumnType("text");
        builder.Property(x => x.Skills).HasColumnType("text");
        builder.Property(x => x.Interests).HasColumnType("text");
        builder.Property(x => x.ImageUrl).HasMaxLength(500);
        builder.Property(x => x.PreferredLanguage).HasMaxLength(8).IsRequired();

        builder.HasOne(x => x.User)
            .WithOne(x => x.MemberProfile)
            .HasForeignKey<MemberProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId).IsUnique();
    }
}

public class CreatorProfileConfiguration : IEntityTypeConfiguration<CreatorProfile>
{
    public void Configure(EntityTypeBuilder<CreatorProfile> builder)
    {
        builder.Property(x => x.Alias).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Headline).HasMaxLength(160);
        builder.Property(x => x.ShortBio).HasColumnType("text");
        builder.Property(x => x.District).HasMaxLength(120);
        builder.Property(x => x.Languages).HasMaxLength(200);
        builder.Property(x => x.SkillsOffered).HasColumnType("text");
        builder.Property(x => x.HelpNeeded).HasColumnType("text");
        builder.Property(x => x.AreasOfInterest).HasColumnType("text");
        builder.Property(x => x.Links).HasColumnType("text");
        builder.Property(x => x.Availability).HasMaxLength(120);
        builder.Property(x => x.PhotoUrl).HasMaxLength(500);

        builder.HasIndex(x => x.Alias).IsUnique();

        builder.HasOne(x => x.User)
            .WithOne(x => x.CreatorProfile)
            .HasForeignKey<CreatorProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.FieldVisibilities)
            .WithOne(x => x.CreatorProfile)
            .HasForeignKey(x => x.CreatorProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProfileFieldVisibilityConfiguration : IEntityTypeConfiguration<ProfileFieldVisibility>
{
    public void Configure(EntityTypeBuilder<ProfileFieldVisibility> builder)
    {
        builder.Property(x => x.FieldName).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => new { x.CreatorProfileId, x.FieldName }).IsUnique();
    }
}

public class EntrepreneurProfileConfiguration : IEntityTypeConfiguration<EntrepreneurProfile>
{
    public void Configure(EntityTypeBuilder<EntrepreneurProfile> builder)
    {
        builder.Property(x => x.Organization).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Industry).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.Website).HasMaxLength(300);
        builder.Property(x => x.CoarseLocation).HasMaxLength(150);
        builder.Property(x => x.InterestAreas).HasMaxLength(500);
        builder.Property(x => x.Expertise).HasMaxLength(500);
        builder.Property(x => x.PreferredCollaborationTypes).HasMaxLength(300);

        builder.HasOne(x => x.User)
            .WithOne(x => x.EntrepreneurProfile)
            .HasForeignKey<EntrepreneurProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId).IsUnique();
    }
}

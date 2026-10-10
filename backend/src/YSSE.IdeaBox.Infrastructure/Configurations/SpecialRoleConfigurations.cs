using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Infrastructure.Configurations;

public class HelperAssistedSubmissionConfiguration : IEntityTypeConfiguration<HelperAssistedSubmission>
{
    public void Configure(EntityTypeBuilder<HelperAssistedSubmission> builder)
    {
        builder.Property(x => x.ConsentMethod).HasMaxLength(50).IsRequired();

        builder.HasOne<Idea>()
            .WithMany()
            .HasForeignKey(x => x.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.HelperUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.CreatorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class GuardianLinkConfiguration : IEntityTypeConfiguration<GuardianLink>
{
    public void Configure(EntityTypeBuilder<GuardianLink> builder)
    {
        builder.HasIndex(x => new { x.GuardianUserId, x.MinorUserId }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.GuardianUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.MinorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OrganizationPartnerConfiguration : IEntityTypeConfiguration<OrganizationPartner>
{
    public void Configure(EntityTypeBuilder<OrganizationPartner> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasMany(x => x.Members)
            .WithOne(x => x.OrganizationPartner)
            .HasForeignKey(x => x.OrganizationPartnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrganizationPartnerMemberConfiguration : IEntityTypeConfiguration<OrganizationPartnerMember>
{
    public void Configure(EntityTypeBuilder<OrganizationPartnerMember> builder)
    {
        builder.HasIndex(x => new { x.OrganizationPartnerId, x.UserId }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

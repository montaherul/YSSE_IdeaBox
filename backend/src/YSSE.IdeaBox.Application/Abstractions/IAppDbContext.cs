using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Application.Abstractions;

/// <summary>
/// Data-access abstraction implemented by the Infrastructure EF Core context.
/// Application services depend on this interface (Dependency Inversion), never on
/// the concrete DbContext.
/// </summary>
public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<PlatformSetting> PlatformSettings { get; }

    DbSet<MemberProfile> MemberProfiles { get; }
    DbSet<CreatorProfile> CreatorProfiles { get; }
    DbSet<ProfileFieldVisibility> ProfileFieldVisibilities { get; }
    DbSet<EntrepreneurProfile> EntrepreneurProfiles { get; }

    DbSet<Idea> Ideas { get; }
    DbSet<IdeaRevision> IdeaRevisions { get; }
    DbSet<IdeaAttachment> IdeaAttachments { get; }
    DbSet<IdeaStatusHistory> IdeaStatusHistories { get; }
    DbSet<IdeaScreeningDecision> IdeaScreeningDecisions { get; }
    DbSet<Category> Categories { get; }
    DbSet<Tag> Tags { get; }
    DbSet<IdeaTag> IdeaTags { get; }
    DbSet<IdeaComment> IdeaComments { get; }
    DbSet<IdeaFollow> IdeaFollows { get; }

    DbSet<IdeaReview> IdeaReviews { get; }
    DbSet<GuestVisitor> GuestVisitors { get; }
    DbSet<IdeaVote> IdeaVotes { get; }
    DbSet<VotingPeriod> VotingPeriods { get; }

    DbSet<ProblemPost> ProblemPosts { get; }
    DbSet<ProblemEndorsement> ProblemEndorsements { get; }
    DbSet<IdeaProblemLink> IdeaProblemLinks { get; }

    DbSet<EntrepreneurInquiry> EntrepreneurInquiries { get; }
    DbSet<WatchlistItem> WatchlistItems { get; }

    DbSet<Conversation> Conversations { get; }
    DbSet<ConversationParticipant> ConversationParticipants { get; }
    DbSet<Message> Messages { get; }
    DbSet<MessageAttachment> MessageAttachments { get; }
    DbSet<UserBlock> UserBlocks { get; }

    DbSet<OpportunityOutcome> OpportunityOutcomes { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectMember> ProjectMembers { get; }
    DbSet<ProjectMilestone> ProjectMilestones { get; }
    DbSet<ProjectTask> ProjectTasks { get; }
    DbSet<ProjectUpdate> ProjectUpdates { get; }

    DbSet<MarketplaceListing> MarketplaceListings { get; }
    DbSet<Reservation> Reservations { get; }

    DbSet<Report> Reports { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<NotificationPreference> NotificationPreferences { get; }

    DbSet<HelperAssistedSubmission> HelperAssistedSubmissions { get; }
    DbSet<GuardianLink> GuardianLinks { get; }
    DbSet<OrganizationPartner> OrganizationPartners { get; }
    DbSet<OrganizationPartnerMember> OrganizationPartnerMembers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Begins a transaction for operations that must succeed or fail together.</summary>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

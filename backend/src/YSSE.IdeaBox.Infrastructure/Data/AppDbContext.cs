using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Infrastructure.Data;

/// <summary>
/// EF Core implementation of <see cref="IAppDbContext"/>. This is the only place in
/// the solution that touches the database write model directly; application services
/// depend on the <see cref="IAppDbContext"/> abstraction (Dependency Inversion).
/// </summary>
public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<PlatformSetting> PlatformSettings { get; set; } = null!;

    public DbSet<MemberProfile> MemberProfiles { get; set; } = null!;
    public DbSet<CreatorProfile> CreatorProfiles { get; set; } = null!;
    public DbSet<ProfileFieldVisibility> ProfileFieldVisibilities { get; set; } = null!;
    public DbSet<EntrepreneurProfile> EntrepreneurProfiles { get; set; } = null!;

    public DbSet<Idea> Ideas { get; set; } = null!;
    public DbSet<IdeaRevision> IdeaRevisions { get; set; } = null!;
    public DbSet<IdeaAttachment> IdeaAttachments { get; set; } = null!;
    public DbSet<IdeaStatusHistory> IdeaStatusHistories { get; set; } = null!;
    public DbSet<IdeaScreeningDecision> IdeaScreeningDecisions { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<IdeaTag> IdeaTags { get; set; } = null!;
    public DbSet<IdeaComment> IdeaComments { get; set; } = null!;
    public DbSet<IdeaFollow> IdeaFollows { get; set; } = null!;

    public DbSet<IdeaReview> IdeaReviews { get; set; } = null!;
    public DbSet<GuestVisitor> GuestVisitors { get; set; } = null!;
    public DbSet<IdeaVote> IdeaVotes { get; set; } = null!;
    public DbSet<VotingPeriod> VotingPeriods { get; set; } = null!;

    public DbSet<ProblemPost> ProblemPosts { get; set; } = null!;
    public DbSet<ProblemEndorsement> ProblemEndorsements { get; set; } = null!;
    public DbSet<IdeaProblemLink> IdeaProblemLinks { get; set; } = null!;

    public DbSet<EntrepreneurInquiry> EntrepreneurInquiries { get; set; } = null!;
    public DbSet<WatchlistItem> WatchlistItems { get; set; } = null!;

    public DbSet<Conversation> Conversations { get; set; } = null!;
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<MessageAttachment> MessageAttachments { get; set; } = null!;
    public DbSet<UserBlock> UserBlocks { get; set; } = null!;

    public DbSet<OpportunityOutcome> OpportunityOutcomes { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;
    public DbSet<ProjectMilestone> ProjectMilestones { get; set; } = null!;
    public DbSet<ProjectTask> ProjectTasks { get; set; } = null!;
    public DbSet<ProjectUpdate> ProjectUpdates { get; set; } = null!;

    public DbSet<MarketplaceListing> MarketplaceListings { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;

    public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<NotificationPreference> NotificationPreferences { get; set; } = null!;

    public DbSet<HelperAssistedSubmission> HelperAssistedSubmissions { get; set; } = null!;
    public DbSet<GuardianLink> GuardianLinks { get; set; } = null!;
    public DbSet<OrganizationPartner> OrganizationPartners { get; set; } = null!;
    public DbSet<OrganizationPartnerMember> OrganizationPartnerMembers { get; set; } = null!;

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Database.BeginTransactionAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

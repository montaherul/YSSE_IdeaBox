namespace YSSE.IdeaBox.Application.Common;

/// <summary>
/// Centralized permission policy names used for authorization.
/// Maps to Permission.Code values in the database.
/// </summary>
public static class Permissions
{
    // User management
    public const string UsersRead = "users.read";
    public const string UsersCreate = "users.create";
    public const string UsersUpdate = "users.update";
    public const string UsersDelete = "users.delete";
    public const string UsersSuspend = "users.suspend";
    public const string UsersManageRoles = "users.manage_roles";

    // Idea management
    public const string IdeasRead = "ideas.read";
    public const string IdeasCreate = "ideas.create";
    public const string IdeasUpdate = "ideas.update";
    public const string IdeasDelete = "ideas.delete";
    public const string IdeasPublish = "ideas.publish";
    public const string IdeasScreen = "ideas.screen";
    public const string IdeasFeature = "ideas.feature";

    // Category/Tag management
    public const string CategoriesRead = "categories.read";
    public const string CategoriesCreate = "categories.create";
    public const string CategoriesUpdate = "categories.update";
    public const string CategoriesDelete = "categories.delete";
    public const string TagsRead = "tags.read";
    public const string TagsCreate = "tags.create";
    public const string TagsUpdate = "tags.update";
    public const string TagsDelete = "tags.delete";

    // Problem management
    public const string ProblemsRead = "problems.read";
    public const string ProblemsCreate = "problems.create";
    public const string ProblemsUpdate = "problems.update";
    public const string ProblemsModerate = "problems.moderate";

    // Review management
    public const string ReviewsRead = "reviews.read";
    public const string ReviewsCreate = "reviews.create";
    public const string ReviewsModerate = "reviews.moderate";
    public const string ReviewsUpdate = "reviews.update";
    public const string ReviewsDelete = "reviews.delete";

    // Voting
    public const string VotingRead = "voting.read";
    public const string VotingCast = "voting.cast";
    public const string VotingManage = "voting.manage";

    // Marketplace
    public const string MarketplaceRead = "marketplace.read";
    public const string MarketplaceCreate = "marketplace.create";
    public const string MarketplaceUpdate = "marketplace.update";
    public const string MarketplaceDelete = "marketplace.delete";
    public const string MarketplaceManage = "marketplace.manage";

    // Messaging
    public const string MessagesRead = "messages.read";
    public const string MessagesSend = "messages.send";
    public const string MessagesUpdate = "messages.update";
    public const string MessagesDelete = "messages.delete";
    public const string MessagesManage = "messages.manage";

    // Inquiry/Entrepreneur
    public const string InquiriesRead = "inquiries.read";
    public const string InquiriesCreate = "inquiries.create";
    public const string InquiriesUpdate = "inquiries.update";
    public const string InquiriesDelete = "inquiries.delete";
    public const string InquiriesManage = "inquiries.manage";

    // Outcomes
    public const string OutcomesRead = "outcomes.read";
    public const string OutcomesCreate = "outcomes.create";
    public const string OutcomesUpdate = "outcomes.update";
    public const string OutcomesDelete = "outcomes.delete";
    public const string OutcomesVerify = "outcomes.verify";

    // Projects
    public const string ProjectsRead = "projects.read";
    public const string ProjectsCreate = "projects.create";
    public const string ProjectsUpdate = "projects.update";
    public const string ProjectsDelete = "projects.delete";
    public const string ProjectsManage = "projects.manage";

    // Reports/Moderation
    public const string ReportsRead = "reports.read";
    public const string ReportsManage = "reports.manage";

    // Settings/Platform
    public const string SettingsRead = "settings.read";
    public const string SettingsManage = "settings.manage";

    // Audit logs
    public const string AuditRead = "audit.read";

    // Dashboard
    public const string DashboardRead = "dashboard.read";

    // Roles & Permissions
    public const string RolesRead = "roles.read";
    public const string RolesManage = "roles.manage";
    public const string PermissionsRead = "permissions.read";
    public const string PermissionsManage = "permissions.manage";

    // Reports (content)
    public const string ContentReportsRead = "content.reports.read";
    public const string ContentReportsManage = "content.reports.manage";

    // Static content
    public const string StaticContentRead = "staticcontent.read";
    public const string StaticContentManage = "staticcontent.manage";

    // Voting Periods
    public const string VotingPeriodsRead = "votingperiods.read";
    public const string VotingPeriodsManage = "votingperiods.manage";

    // Notifications
    public const string NotificationsRead = "notifications.read";
    public const string NotificationsDelete = "notifications.delete";
    public const string NotificationsManage = "notifications.manage";

    // Profiles
    public const string ProfilesRead = "profiles.read";
    public const string ProfilesUpdate = "profiles.update";

    // Screening
    public const string ScreeningRead = "screening.read";
    public const string ScreeningDecide = "screening.decide";

    // Idea Attachments (private)
    public const string IdeaAttachmentsRead = "ideas.attachments.read";

    // All permissions as array for registration/seeding
    public static readonly string[] All =
    [
        UsersRead, UsersCreate, UsersUpdate, UsersDelete, UsersSuspend, UsersManageRoles,
        IdeasRead, IdeasCreate, IdeasUpdate, IdeasDelete, IdeasPublish, IdeasScreen, IdeasFeature, IdeaAttachmentsRead,
        CategoriesRead, CategoriesCreate, CategoriesUpdate, CategoriesDelete,
        TagsRead, TagsCreate, TagsUpdate, TagsDelete,
        ProblemsRead, ProblemsCreate, ProblemsUpdate, ProblemsModerate,
        ReviewsRead, ReviewsCreate, ReviewsUpdate, ReviewsDelete, ReviewsModerate,
        VotingRead, VotingCast, VotingManage,
        VotingPeriodsRead, VotingPeriodsManage,
        MarketplaceRead, MarketplaceCreate, MarketplaceUpdate, MarketplaceDelete, MarketplaceManage,
        MessagesRead, MessagesSend, MessagesUpdate, MessagesDelete, MessagesManage,
        InquiriesRead, InquiriesCreate, InquiriesUpdate, InquiriesDelete, InquiriesManage,
        OutcomesRead, OutcomesCreate, OutcomesUpdate, OutcomesDelete, OutcomesVerify,
        ProjectsRead, ProjectsCreate, ProjectsUpdate, ProjectsDelete, ProjectsManage,
        ReportsRead, ReportsManage,
        ContentReportsRead, ContentReportsManage,
        SettingsRead, SettingsManage,
        AuditRead,
        DashboardRead,
        RolesRead, RolesManage,
        PermissionsRead, PermissionsManage,
        StaticContentRead, StaticContentManage,
        NotificationsRead, NotificationsDelete, NotificationsManage,
        ProfilesRead, ProfilesUpdate,
        ScreeningRead, ScreeningDecide
    ];

    private static readonly HashSet<string> CodeSet =
        new(All, StringComparer.OrdinalIgnoreCase);

    /// <summary>True when the value is a platform-defined permission code (a policy name).</summary>
    public static bool IsDefined(string? code) =>
        !string.IsNullOrWhiteSpace(code) && CodeSet.Contains(code);
}
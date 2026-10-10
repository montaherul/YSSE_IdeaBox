namespace YSSE.IdeaBox.Domain.Enums;

public enum RoleName
{
    Member = 1,
    Creator = 2,
    Entrepreneur = 3,
    Investor = 4,
    Mentor = 5,
    Moderator = 6,
    ScreeningReviewer = 7,
    Administrator = 8,
    IdeaHelper = 9,
    OrganizationPartner = 10,
    Guardian = 11,
    LanguageEditor = 12
}

public enum IdeaStatus
{
    Draft = 0,
    Submitted = 1,
    UnderReview = 2,
    RevisionRequired = 3,
    Published = 4,
    Rejected = 5,
    Shortlisted = 6,
    ApprovedForDevelopment = 7,
    InProgress = 8,
    Completed = 9,
    Archived = 10,
    Withdrawn = 11
}

public enum IdeaAttribution
{
    Public = 0,
    Anonymous = 1
}

public enum DevelopmentStage
{
    Concept = 0,
    Prototype = 1,
    Pilot = 2,
    EarlyRevenue = 3,
    Scaling = 4
}

public enum ReviewModerationStatus
{
    Pending = 0,
    Published = 1,
    Hidden = 2,
    Removed = 3,
    Spam = 4
}

public enum VoteVisibility
{
    Live = 0,
    HiddenUntilClose = 1,
    AfterClose = 2
}

public enum ProblemStatus
{
    Open = 0,
    SolutionsProposed = 1,
    InProgress = 2,
    Solved = 3,
    Closed = 4
}

public enum InquiryType
{
    Investment = 0,
    Sponsorship = 1,
    Mentorship = 2,
    BusinessPartnership = 3,
    TechnicalCollaboration = 4,
    Other = 5
}

public enum OutcomeStatus
{
    InquiryReceived = 0,
    DiscussionOngoing = 1,
    ProposalExchanged = 2,
    AgreementReported = 3,
    NotProceeding = 4,
    OutcomeUnknown = 5
}

public enum OutcomeKind
{
    Collaboration = 0,
    Venture = 1,
    Mentorship = 2,
    Investment = 3,
    Other = 4
}

public enum ConversationStatus
{
    Open = 0,
    Muted = 1,
    Blocked = 2,
    Closed = 3
}

public enum ProjectStatus
{
    Planned = 0,
    InProgress = 1,
    Paused = 2,
    Completed = 3,
    Cancelled = 4
}

public enum ListingType
{
    Sale = 0,
    Donation = 1,
    Exchange = 2
}

public enum ListingStatus
{
    Draft = 0,
    PendingReview = 1,
    Available = 2,
    Reserved = 3,
    Completed = 4,
    Rejected = 5,
    Expired = 6,
    Removed = 7
}

public enum ReservationStatus
{
    Requested = 0,
    Accepted = 1,
    Declined = 2,
    Cancelled = 3,
    Completed = 4
}

public enum ReportTargetType
{
    Idea = 0,
    Review = 1,
    Comment = 2,
    Message = 3,
    Listing = 4,
    User = 5
}

public enum ReportStatus
{
    Open = 0,
    UnderReview = 1,
    Actioned = 2,
    Dismissed = 3
}

public enum NotificationType
{
    IdeaSubmitted = 0,
    ScreeningDecision = 1,
    RevisionRequested = 2,
    NewReview = 3,
    NewComment = 4,
    VotingOpened = 5,
    VotingClosed = 6,
    LeaderboardResult = 7,
    NewInquiry = 8,
    NewMessage = 9,
    CollaborationResponse = 10,
    MarketplaceReservation = 11,
    ModerationDecision = 12,
    OutcomeUpdate = 13,
    AccountNotice = 14
}

public enum NotificationChannel
{
    InApp = 0,
    Email = 1
}

public enum ProfileVisibility
{
    Public = 0,
    MembersOnly = 1,
    InConversation = 2,
    Private = 3
}

public enum OpportunitySeekerKind
{
    Entrepreneur = 0,
    Investor = 1,
    Mentor = 2
}

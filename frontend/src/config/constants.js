export const API_BASE_URL =
  import.meta.env.VITE_API_URL ?? 'http://localhost:5080/api/v1'

export const APP_NAME = 'YSSE Idea Box'
export const APP_TAGLINE = 'Think boldly. Build together.'
export const APP_KICKER = "A community for what's next"

/** YSSE's non-negotiable business boundary (SRS §1.1). */
export const BOUNDARY_NOTICE =
  'YSSE facilitates connections; it does not manage funding. Negotiations, agreements, and money transfers happen directly between the parties. Popularity never makes an idea automatically eligible for investment.'

export const DEFAULT_PAGE_SIZE = 20
export const MAX_PAGE_SIZE = 100

export const GUEST_REVIEW_NOTICE =
  'As a guest you can submit one anonymous review on one idea (best-effort limit). Signing in lets you review more ideas.'

export const STATUS_TRANSLATIONS = {
  Draft: 'Draft',
  Submitted: 'Submitted',
  UnderReview: 'Under review',
  RevisionRequired: 'Revision required',
  Published: 'Published',
  Rejected: 'Rejected',
  Shortlisted: 'Shortlisted',
  ApprovedForDevelopment: 'Approved for development',
  InProgress: 'In progress',
  Completed: 'Completed',
  Archived: 'Archived',
  Withdrawn: 'Withdrawn',
}

export const IDEA_STATUSES = Object.keys(STATUS_TRANSLATIONS)

export const STAGES = [
  { value: 'Concept', label: 'Concept' },
  { value: 'Prototype', label: 'Prototype' },
  { value: 'Pilot', label: 'Pilot' },
  { value: 'EarlyRevenue', label: 'Early revenue' },
  { value: 'Scaling', label: 'Scaling' },
]

export const INQUIRY_TYPES = [
  { value: 'Investment', label: 'Investment' },
  { value: 'Sponsorship', label: 'Sponsorship' },
  { value: 'Mentorship', label: 'Mentorship' },
  { value: 'BusinessPartnership', label: 'Business partnership' },
  { value: 'TechnicalCollaboration', label: 'Technical collaboration' },
  { value: 'Other', label: 'Other support' },
]

export const RESERVATION_STATUSES = {
  Requested: 'Requested',
  Accepted: 'Accepted',
  Declined: 'Declined',
  Cancelled: 'Cancelled',
  Completed: 'Completed',
}

export const NOTIFICATION_TYPE_LABELS = {
  IdeaSubmitted: 'Idea submitted',
  ScreeningDecision: 'Screening decision',
  RevisionRequested: 'Revision requested',
  NewReview: 'New review',
  NewComment: 'New comment',
  VotingOpened: 'Voting opened',
  VotingClosed: 'Voting closed',
  LeaderboardResult: 'Leaderboard result',
  NewInquiry: 'New inquiry',
  NewMessage: 'New message',
  CollaborationResponse: 'Collaboration response',
  MarketplaceReservation: 'Marketplace reservation',
  ModerationDecision: 'Moderation decision',
  OutcomeUpdate: 'Outcome update',
  AccountNotice: 'Account notice',
}
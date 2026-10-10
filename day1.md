# YSSE Idea Box — Day 1 Progress

**Project root:** `G:\.NET SAAS\Idea_BOX`
**Date:** Day 1
**Status:** Backend layers Domain + Application + Infrastructure complete and compiling. Frontend SPA complete and building. API layer **not yet implemented**. No EF migration generated yet.

---

## 1. What was built

```
G:\.NET SAAS\Idea_BOX\
├── AGENTS.md                         (generic engineering rules — followed)
├── rule.md                           (architecture/scale/security/a11y rules — followed)
├── YSSE Idea Box — Software Requirements Specification v3.1.md
├── day1.md                           (this file)
├── day2.md                           (remaining work)
├── backend\
│   ├── YSSE.IdeaBox.slnx             (solution, .slnx format)
│   └── src\
│       ├── YSSE.IdeaBox.Domain\          ✅ entities + enums (no dependencies)
│       ├── YSSE.IdeaBox.Application\     ✅ DTOs, service interfaces, 18 service impls, validators, DI
│       ├── YSSE.IdeaBox.Infrastructure\  ✅ EF DbContext, configs, Dapper, services, seeder, DI
│       └── YSSE.IdeaBox.Api\             ⏳ NOT DONE (still default template)
└── frontend\                         ✅ React 19 + Vite + Tailwind v4 (builds, dist/ produced)
```

Solution build (verified):
`dotnet build YSSE.IdeaBox.slnx` → **Build succeeded, 0 errors**.
Frontend build (verified by subagent): `npm run build` → 2319 modules, success. `npm run lint` (oxlint) → exit 0.

---

## 2. Environment / local database

- .NET SDK: **8.0.425** (10.0.300 preview also installed; solution targets `net8.0`).
- Node: **v24.11** / npm **11.6**.
- PostgreSQL **18** running locally on `localhost:5432` (data dir `G:\postgress`, service `postgresql-x64-18`).
- Database created: **`ysse_ideabox`**.
- Connection string (Development): `Host=localhost;Port=5432;Database=ysse_ideabox;Username=postgres;Password=admin`
- No Docker used (native Postgres).

---

## 3. Backend — Domain (complete)

Project: `backend\src\YSSE.IdeaBox.Domain` — zero external dependencies.

- `Common\BaseEntity.cs` — `BaseEntity` (Id, CreatedAt, UpdatedAt) + `ISoftDeletable`.
- `Enums\Enums.cs` — IdeaStatus, IdeaAttribution, DevelopmentStage, ReviewModerationStatus, VoteVisibility, ProblemStatus, InquiryType, OutcomeStatus, OutcomeKind, ConversationStatus, ProjectStatus, ListingType, ListingStatus, ReservationStatus, ReportTargetType, ReportStatus, NotificationType, NotificationChannel, ProfileVisibility, OpportunitySeekerKind, RoleName.
- `Entities\` (grouped by module):
  - `Identity.cs` — User, Role, UserRole, Permission, RolePermission, RefreshToken, PlatformSetting
  - `Profiles.cs` — MemberProfile, CreatorProfile, ProfileFieldVisibility, EntrepreneurProfile
  - `Ideas.cs` — Idea, IdeaRevision, IdeaAttachment, IdeaStatusHistory, IdeaScreeningDecision, Category, Tag, IdeaTag, IdeaComment, IdeaFollow
  - `Reviews.cs` — IdeaReview, GuestVisitor
  - `Voting.cs` — IdeaVote, VotingPeriod
  - `Community.cs` — ProblemPost, ProblemEndorsement, IdeaProblemLink
  - `Entrepreneur.cs` — EntrepreneurInquiry, WatchlistItem
  - `Messaging.cs` — Conversation, ConversationParticipant, Message, MessageAttachment, UserBlock
  - `Outcomes.cs` — OpportunityOutcome
  - `Projects.cs` — Project, ProjectMember, ProjectMilestone, ProjectTask, ProjectUpdate
  - `Marketplace.cs` — MarketplaceListing, Reservation
  - `Moderation.cs` — Report, AuditLog, Notification, NotificationPreference
  - `SpecialRoles.cs` — HelperAssistedSubmission, GuardianLink, OrganizationPartner, OrganizationPartnerMember

---

## 4. Backend — Application (complete)

Project: `backend\src\YSSE.IdeaBox.Application` (refs EF Core 8.0.11, FluentValidation 11.9.2). No DbContext; depends on `IAppDbContext` (DIP).

- `Common\Results.cs` — `Result`, `Result<T>`, `PagedResult<T>`, `Pagination.Normalize` (default 20, max 100).
- `Common\Exceptions.cs` — `AppException(StatusCode)`, `NotFoundException`, `ForbiddenException`, `UnauthorizedException`, `ConflictException`, `ValidationAppException`.
- `Abstractions\IAppDbContext.cs` — all DbSets + `SaveChangesAsync` + `BeginTransactionAsync` (sole EF boundary).
- `Abstractions\IIdentityServices.cs` — `ICurrentUserService`, `IPasswordHasher`, `ITokenService`, `IDateTimeProvider`, `IGuestIdentityService`, `IVerificationCodeStore`, `IHashingService`.
- `Abstractions\IIntegrationServices.cs` — `ISqlQueryExecutor` (Dapper), `IAuditWriter`, `INotificationDispatcher`, `IFileStorage`.
- `Abstractions\Services\*.cs` — 18 service interfaces (Auth, Profile, Category, Idea, Screening, Comment, Problem, Review, Voting, Leaderboard, Inquiry, Messaging, Report, Outcome, Project, Marketplace, Notification, Admin).
- `Dtos\*.cs` — all request/response DTOs (Auth, Profile, Idea, Review, Voting, Community, Entrepreneur, Messaging, Admin, Misc, Query).
- `Services\` — implementations for all interfaces:
  - Identity: AuthService, ProfileService, CategoryService
  - Ideas: IdeaService, ScreeningService, CommentService, ProblemService
  - Engagement: ReviewService, VotingService, LeaderboardService, InquiryService, MessagingService, ReportService, OutcomeService, ProjectService, MarketplaceService, NotificationService
  - Admin: AdminService
- `Validators\` — FluentValidation validators (Register, Login, IdeaCreate, ReviewCreate, InquiryCreate, MessageSend, ListingCreate, CategoryUpsert, ProblemPostCreate).
- `DependencyInjection.cs` — `AddApplication(this IServiceCollection)` registers all services + validators.

Business rules implemented per SRS: idea status transition table, guest one-review-one-idea best-effort limit, review counts (published only, guest vs registered), vote eligibility + uniqueness, weighted idea-maker leaderboard (§9.2), rule-based matching with explainable reasons, messaging participant-only access + blocks, audit + notifications, admin moderation/roles/settings/dashboard.

---

## 5. Backend — Infrastructure (complete)

Project: `backend\src\YSSE.IdeaBox.Infrastructure` (EF Core 8.0.11 + Npgsql 8.0.11 + EFCore.NamingConventions + Dapper 2.1.35 + BCrypt + JWT).

- `Data\AppDbContext.cs` — implements `IAppDbContext` (all DbSets, SaveChanges, BeginTransaction).
- `Data\AppDbContextOptions.cs` / `AppDbContextFactory.cs` — Npgsql + `UseSnakeCaseNamingConvention()`; design-time factory reads `YSSE_DB_CONNECTION` (fallback: local conn).
- `Configurations\` (13 files) — max lengths, decimal(18,2), FK delete behaviors, and required constraints/indexes (unique active review, unique active vote, unique guest hash, unique alias/slug/email/phone, watchlist/follow/endorsement/participant/userrole uniques, plus query indexes).
- `Services\` — `JwtTokenService`, `BcryptPasswordHasher`, `GuestIdentityService`, `HashingService`, `InMemoryVerificationCodeStore`, `SystemDateTimeProvider`, `DapperSqlQueryExecutor`, `LocalFileStorage`, `NotificationDispatcher`, `AuditWriter`.
- `Data\DbInitializer.cs` — `InitializeAsync(IServiceProvider, bool migrate, CancellationToken)`: optional `MigrateAsync`, then idempotent seed (roles, permissions, [Config] settings, admin `admin@ysse.local` / `Admin@12345`, members, categories, tags, ~8 ideas in mixed statuses, voting period, reviews, votes, problems, listings, notifications, conversations, inquiries).
- `DependencyInjection.cs` — `AddInfrastructure(this IServiceCollection, IConfiguration)`.

**Not done here (by design):** EF migration + `database update` (belongs to Day 2). No DB connection was made.

---

## 6. Frontend (complete)

Project: `frontend\` — React 19.3.0 + Vite 8.3.0 (JSX only) + Tailwind CSS 4.3.3 + Radix/shadcn-style UI + TanStack Query 5 + Zustand 5 + axios + react-hook-form + zod + react-router-dom 7 + sonner; linted with oxlint.

- Feature-based structure (`src/features/*`: auth, ideas, reviews, voting, problems, entrepreneur, messaging, marketplace, projects, notifications, admin, profile, leaderboard, comments, categories, dashboard, static).
- `src/lib/api-client.js` — axios with env base URL, `withCredentials`, bearer token, 401→refresh→retry, envelope unwrap + centralized errors.
- Auth store (zustand, persisted) + `useAuth`/role gating; protected/admin routes; lazy-loaded admin/marketplace.
- All SRS modules have pages/routes (public discovery, idea detail with reviews + guest review + voting, submit idea incl. Quick mode, dashboards, entrepreneur portal, messaging, marketplace, projects, notifications, admin).
- Accessibility per WCAG 2.2 AA (labels, focus, skip-link, skeletons, empty states, responsive).
- Default API base URL: `http://localhost:5080/api/v1` (`.env.example` → `VITE_API_URL`).

**Note / risk:** the frontend was built before any controllers existed, so pages defensively accept multiple response shapes and sent enums as **strings**. The API must serialize enums as strings (JsonStringEnumConverter) and honor the envelope below, or Day 2 must reconcile field names.

---

## 7. Frozen contracts for Day 2

**DI entry points (already exist):**
- `YSSE.IdeaBox.Application.DependencyInjection.AddApplication(IServiceCollection)`
- `YSSE.IdeaBox.Infrastructure.DependencyInjection.AddInfrastructure(IServiceCollection, IConfiguration)`
- `YSSE.IdeaBox.Infrastructure.Data.DbInitializer.InitializeAsync(IServiceProvider, bool migrate, CancellationToken)`

**Response envelope (must match frontend):**
```json
// success
{ "success": true, "data": <payload>, "page": 1, "pageSize": 20, "total": 2450, "hasNextPage": true, "traceId": "..." }
// error
{ "success": false, "error": { "message": "...", "code": "VALIDATION", "details": {} }, "traceId": "..." }
```

**Routes:** `/api/v1/...` (auth, profiles, categories, ideas, screening, reviews, votes, voting-periods, leaderboards, comments, problems, inquiries, watchlist, matches, conversations, reports, outcomes, projects, marketplace, reservations, notifications, admin). Health: `/health/live`, `/health/ready`.

Full endpoint list and API requirements are in `day2.md`.

---

## 8. Known assumptions / follow-ups

1. API layer (controllers, Program wiring, middleware, contracts) is pending — see `day2.md`.
2. Enum (de)serialization must be strings; reconcile with frontend.
3. Migration not generated/applied; DB has no tables yet.
4. Frontend `dist/` was produced to prove the build; rerun after any change.
5. `ICurrentUserService` is intentionally implemented in the Api project (HTTP-dependent).

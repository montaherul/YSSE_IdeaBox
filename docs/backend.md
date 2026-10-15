# Backend

ASP.NET Core 8 Web API organized as a Clean Architecture solution. This document covers the domain model, the application service layer, the API boundary, and the authorization model. Endpoint-by-endpoint details live in [docs/api-reference.md](api-reference.md).

Root: `backend/` · Solution: `backend/YSSE.IdeaBox.slnx`

---

## 1. Solution & projects

| Project | Path |
| --- | --- |
| Domain | `backend/src/YSSE.IdeaBox.Domain` |
| Application | `backend/src/YSSE.IdeaBox.Application` |
| Infrastructure | `backend/src/YSSE.IdeaBox.Infrastructure` |
| Api | `backend/src/YSSE.IdeaBox.Api` |
| Tests | `backend/tests/YSSE.IdeaBox.Tests` |

All target `net8.0`. Dependency direction and request lifecycle are described in [architecture.md](architecture.md).

---

## 2. Domain model

Entities live in `Domain/Entities/`, enums in `Domain/Enums/Enums.cs`, base types in `Domain/Common/`.

### Base types
- `BaseEntity` — `Id`, `CreatedAt`, `UpdatedAt`.
- `ISoftDeletable` — `IsDeleted`, `DeletedAt`.

### Entities (by file)

| File | Entities |
| --- | --- |
| `Identity.cs` | `User`, `Role`, `UserRole`, `Permission`, `RolePermission`, `RefreshToken`, `PlatformSetting` |
| `Ideas.cs` | `Idea`, `IdeaRevision`, `IdeaAttachment`, `IdeaStatusHistory`, `IdeaScreeningDecision`, `Category`, `Tag`, `IdeaTag`, `IdeaComment`, `IdeaFollow` |
| `Community.cs` | `ProblemPost`, `ProblemEndorsement`, `IdeaProblemLink` |
| `Reviews.cs` | `IdeaReview`, `GuestVisitor` |
| `Voting.cs` | `IdeaVote`, `VotingPeriod` |
| `Entrepreneur.cs` | `EntrepreneurInquiry`, `WatchlistItem` |
| `Messaging.cs` | `Conversation`, `ConversationParticipant`, `Message`, `MessageAttachment`, `UserBlock` |
| `Outcomes.cs` | `OpportunityOutcome` |
| `Projects.cs` | `Project`, `ProjectMember`, `ProjectMilestone`, `ProjectTask`, `ProjectUpdate` |
| `Marketplace.cs` | `MarketplaceListing`, `Reservation` |
| `Moderation.cs` | `Report`, `AuditLog`, `Notification`, `NotificationPreference` |
| `Profiles.cs` | `MemberProfile`, `CreatorProfile`, `ProfileFieldVisibility`, `EntrepreneurProfile` |
| `StaticContent.cs` | `StaticContent` |

### Key enums

**`SystemRoles`** (static constants, not an enum): `Member`, `Creator`, `Entrepreneur`, `Investor`, `Mentor`, `Moderator`, `ScreeningReviewer`, `Administrator`.

**`IdeaStatus`**: `Draft 0`, `Submitted 1`, `UnderReview 2`, `RevisionRequired 3`, `Published 4`, `Rejected 5`, `Shortlisted 6`, `ApprovedForDevelopment 7`, `InProgress 8`, `Completed 9`, `Archived 10`, `Withdrawn 11`.

**`IdeaAttribution`**: `Public 0`, `Anonymous 1` · **`DevelopmentStage`**: `Concept 0`, `Prototype 1`, `Pilot 2`, `EarlyRevenue 3`, `Scaling 4`.

Other enums: `ReviewModerationStatus`, `VoteVisibility`, `ProblemStatus`, `InquiryType`, `OutcomeStatus`, `OutcomeKind`, `ConversationStatus`, `ProjectStatus`, `ListingType`, `ListingStatus`, `ReservationStatus`, `ReportTargetType`, `ReportStatus`, `NotificationType`, `NotificationChannel`, `ProfileVisibility`, `OpportunitySeekerKind`.

### Idea status machine
`IdeaStatusMachine` (`Application/Services/Common/ServiceHelpers.cs`, `internal`) encodes the permitted transitions and is used to validate every status change. See [features.md](features.md#idea-lifecycle) for the transition table.

---

## 3. Application layer

`Application/` contains the business logic: service interfaces (`Abstractions/`), implementations (`Services/`), DTOs (`Dtos/`), FluentValidation validators (`Validators/`), and shared helpers (`Common/`).

### Services (grouped)

| Group | Services |
| --- | --- |
| `Services/Identity` | `AuthService`, `ProfileService`, `CategoryService`, `UserManagementService`, `UserRoleService`, `RoleService`, `PermissionService`, `RolePermissionService` |
| `Services/Ideas` | `IdeaService`, `ScreeningService`, `CommentService`, `ProblemService` |
| `Services/Engagement` | `ReviewService`, `VotingService`, `LeaderboardService`, `InquiryService`, `MessagingService`, `NotificationService`, `ReportService`, `OutcomeService`, `ProjectService`, `MarketplaceService` (+ `LeaderboardScoring`, `MatchingScorer` helpers) |
| `Services/Platform` | `AuditLogService`, `PlatformSettingsService`, `StaticContentService`, `AdminDashboardService` |

All are registered scoped in `Application/DependencyInjection.cs` (`AddApplication`).

### Common helpers (`Application/Common/`)
- `Result` / `Result<T>` — success flag + optional message/data.
- `PagedResult<T>` + `Pagination.Normalize(page, pageSize)` — default page size **20**, max **100**.
- `Exceptions.cs` — `AppException` and subclasses used to signal HTTP-status outcomes from services.
- `Permissions.cs` — the permission catalog (see below).
- `UploadLimits.cs` — avatar 5 MB, idea attachment 15 MB.
- `MediaPaths.cs` — media route + URL/storage-path helpers.

The application layer is testable in isolation using the in-memory `AppDbContext` harness (`tests/YSSE.IdeaBox.Tests/TestDb.cs`).

---

## 4. API boundary

Controllers live in `backend/src/YSSE.IdeaBox.Api/Controllers/`. They inherit `ApiControllerBase` (`[ApiController]`, `[Authorize]`, route `api/v1/[controller]`) and are deliberately thin: bind → validate → authorize → delegate to a service → map to a response envelope.

Helper methods on `ApiControllerBase`: `OkData<T>`, `OkResult(Result)`, `OkResult<T>(Result<T>)`, `Paged<T>(PagedResult<T>)`, `Failed`, `NotFoundError`, `ForbiddenError`.

### Controller inventory

| Controller | Area |
| --- | --- |
| `AuthController` | Register, login, refresh, logout, verify, forgot/reset password, `me` |
| `IdeasController` | Idea CRUD, submit/withdraw, attachments, feature/unfeature, similar, view |
| `CategoriesController` | Categories + tags (public read, admin write) |
| `CommentsController` | Idea comments, follow/unfollow, followed list |
| `ScreeningController` | Reviewer queue, decisions, status change |
| `ProblemsController` | Problem posts, endorsements, status, linking |
| `ReviewsController` | Idea reviews (registered + guest), moderation, my reviews |
| `VotesController` | Idea votes, voting periods |
| `LeaderboardsController` | Idea and idea-maker leaderboards, monthly top, announce |
| `InquiriesController` | Entrepreneur inquiries, watchlist, matches |
| `ConversationsController` | Messaging, read state, block |
| `MarketplaceController` | Listings, status, reservations |
| `OutcomesController` | Opportunity outcomes, verification |
| `ProjectsController` | Projects, milestones, tasks, updates |
| `ReportsController` | Content reports |
| `NotificationsController` | Notifications + preferences |
| `ProfilesController` | Member/creator/entrepreneur profiles, avatar |
| `UsersController` | User administration |
| `RolesController` | Role administration + role permissions |
| `PermissionsController` | Permission administration |
| `SettingsController` | Platform settings |
| `AuditLogsController` | Audit log listing |
| `DashboardController` | Admin dashboard metrics |
| `StaticContentController` | CMS static content |
| `MediaController` | Anonymous media file serving |

For every route, HTTP method, and policy, see [api-reference.md](api-reference.md).

### Middleware & cross-cutting (Api)
- `CorrelationIdMiddleware`, `GlobalExceptionMiddleware`, `FluentValidationFilter`.
- `PermissionPolicyProvider`, `PermissionAuthorizationHandler`, `PermissionRequirement`, `CurrentUserService`.
- `NotificationHub` (SignalR) and `SignalRRealtimeNotifier`.
- `DatabaseHealthCheck`.

---

## 5. Authorization model

Two complementary layers, both server-side:

1. **Role policies** — `Admin`, `Moderator`, `ScreeningReviewer`, `OpportunitySeeker` (used for coarse access and a few controllers).
2. **Permission policies** — the primary mechanism. Every protected endpoint declares a permission code (for example `ideas.create`, `screening.decide`). `PermissionPolicyProvider` recognizes any code in `Permissions.All`; `PermissionAuthorizationHandler` loads the caller's effective permissions from the database (user → roles → role-permissions) via `IUserPermissionService`, cached per request. This makes grants/revocations take effect immediately.

The permission catalog (`Application/Common/Permissions.cs`) groups ~80 codes across users, ideas, categories/tags, problems, reviews, voting, marketplace, messaging, inquiries, outcomes, projects, reports, settings, audit, dashboard, roles/permissions, static content, notifications, profiles, and screening. Roles are seeded with sensible defaults in `DbInitializer` (`Member` gets self-service capabilities; `Moderator`, `ScreeningReviewer`, and `Administrator` get progressively more; `Administrator` = all permissions).

See [security.md](security.md) for the full treatment.

---

## 6. Data access

- **EF Core** — `AppDbContext` (`Infrastructure/Data`) exposes all DbSets and applies configurations from `Infrastructure/Configurations`. Snake_case naming via `UseSnakeCaseNamingConvention()`.
- **Repository / Unit of Work** — `GenericRepository<T>` and `UnitOfWork` (`Infrastructure/Repositories`, `Infrastructure/UnitOfWork`) share the scoped `DbContext`; the unit of work exposes `SaveChangesAsync`, `BeginTransactionAsync`, `CommitAsync`, `RollbackAsync`.
- **Dapper** — `ISqlQueryExecutor` + `Infrastructure/Queries/` (`LeaderboardQueries`, `AdminDashboardQueries`) for set-based analytical reads.
- **Integrations** (`Infrastructure/Services`) — `JwtTokenService`, `BcryptPasswordHasher`, `SmtpEmailSender`, `HttpSmsSender`, `LocalFileStorage`, `NotificationDispatcher`, `AuditWriter`, `GuestIdentityService`, `HashingService`, `UserPermissionService`, `InMemoryVerificationCodeStore`, `SystemDateTimeProvider`, `DapperSqlQueryExecutor`.

Details in [database.md](database.md).

---

## 7. Startup & seeding

`Program.cs` runs `app.Services.InitializeAsync(isDevelopment)` (`Infrastructure/Data/DbInitializer.cs`) after the app is built:

- **All environments:** seed roles, the permission catalog, role→permission defaults, and platform settings.
- **Development only:** run `MigrateAsync()`, seed demo users, categories, tags, profiles, and sample ideas/content.

---

## See also

- [API Reference](api-reference.md) · [Database](database.md) · [Security](security.md)
- [Architecture](architecture.md) · [Features & Lifecycle](features.md)

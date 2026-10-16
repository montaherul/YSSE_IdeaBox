# Database

YSSE Idea Box uses **PostgreSQL** as the single source of truth. Access is through **EF Core 8** (primary) with **Dapper** for a few analytical reads. All schema changes go through migrations.

---

## 1. Technology & conventions

| Concern | Choice |
| --- | --- |
| Provider | `Npgsql.EntityFrameworkCore.PostgreSQL` |
| Naming | `UseSnakeCaseNamingConvention()` — tables/columns are `snake_case`. |
| Primary keys | `int` identity, from `BaseEntity.Id`. |
| Timestamps | `CreatedAt` / `UpdatedAt` (`DateTime.UtcNow` via `ISystemDateTimeProvider`). |
| Soft delete | `ISoftDeletable` (`IsDeleted`, `DeletedAt`) with query filters. |
| Migrations | `backend/src/YSSE.IdeaBox.Infrastructure/Migrations`. |

`AppDbContext` (`Infrastructure/Data/AppDbContext.cs`) exposes every aggregate as a `DbSet` and applies entity configurations from `Infrastructure/Configurations/*Configuration.cs` (one per entity), where relations, indexes, defaults, and constraints are declared.

---

## 2. Entity groups

| Group | Tables |
| --- | --- |
| Identity | `users`, `roles`, `user_roles`, `permissions`, `role_permissions`, `refresh_tokens`, `platform_settings` |
| Ideas | `ideas`, `idea_revisions`, `idea_attachments`, `idea_status_history`, `idea_screening_decisions`, `categories`, `tags`, `idea_tags`, `idea_comments`, `idea_follows` |
| Community | `problem_posts`, `problem_endorsements`, `idea_problem_links` |
| Reviews | `idea_reviews`, `guest_visitors` |
| Voting | `idea_votes`, `voting_periods` |
| Entrepreneurship | `entrepreneur_inquiries`, `watchlist_items` |
| Messaging | `conversations`, `conversation_participants`, `messages`, `message_attachments`, `user_blocks` |
| Outcomes | `opportunity_outcomes` |
| Projects | `projects`, `project_members`, `project_milestones`, `project_tasks`, `project_updates` |
| Marketplace | `marketplace_listings`, `reservations` |
| Moderation | `reports`, `audit_logs`, `notifications`, `notification_preferences` |
| Profiles | `member_profiles`, `creator_profiles`, `profile_field_visibilities`, `entrepreneur_profiles` |
| Content | `static_contents` |

Full entity definitions are in `Domain/Entities/` (see [backend.md](backend.md#2-domain-model)).

---

## 3. Enums

Enums are stored as `int` (their declaration order is intentional and matches the requirements). API responses serialize them as strings via `JsonStringEnumConverter`.

| Enum | Values |
| --- | --- |
| `IdeaStatus` | Draft 0, Submitted 1, UnderReview 2, RevisionRequired 3, Published 4, Rejected 5, Shortlisted 6, ApprovedForDevelopment 7, InProgress 8, Completed 9, Archived 10, Withdrawn 11 |
| `IdeaAttribution` | Public 0, Anonymous 1 |
| `DevelopmentStage` | Concept 0, Prototype 1, Pilot 2, EarlyRevenue 3, Scaling 4 |
| `ReviewModerationStatus` | Pending, Approved, Rejected, Flagged |
| `VoteVisibility` | Public, Private/AggregateOnly |
| `ProblemStatus` | Open, InReview, Endorsed, Linked, Resolved, Archived |
| `InquiryType` | Mentorship, Investment, Partnership, Feedback, Other |
| `OutcomeStatus` | Pending, Verified, Rejected |
| `ProjectStatus` | Planned, InProgress, Paused, Completed, Cancelled, Archived (final; set via `/projects/{id}/archive`) |
| `ListingType` | Sale, Donation, Exchange |
| `ListingStatus` | Draft, Active, Reserved, Sold, Withdrawn, Archived |
| `ReservationStatus` | Pending, Accepted, Declined, Cancelled, Completed |
| `ReportStatus` | Open, UnderReview, Resolved, Dismissed |

> Only `IdeaStatus` and `IdeaAttribution` are shown in detail here; the remainder follow the same int-backed pattern. The authoritative list is `Domain/Enums/Enums.cs`.

---

## 4. Repositories & unit of work

`GenericRepository<T>` (`Infrastructure/Repositories/GenericRepository.cs`) provides:

```csharp
IQueryable<T> Query();                               // composable, filtered by soft-delete
Task<T?> GetByIdAsync(int id, CancellationToken ct);
Task<IReadOnlyList<T>> ListAsync(Expression<Func<T,bool>>? predicate, CancellationToken ct);
Task AddAsync(T entity, CancellationToken ct);
void Update(T entity);
void Remove(T entity);
```

`UnitOfWork` (`Infrastructure/UnitOfWork`) owns the scoped `DbContext`, exposes repositories, and coordinates persistence:

```csharp
Task<int> SaveChangesAsync(CancellationToken ct);
Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct);
Task CommitAsync(CancellationToken ct);
Task RollbackAsync(CancellationToken ct);
```

Repositories share the same `DbContext`, so a unit-of-work commit persists all pending changes. For multi-step atomic operations, services begin an explicit transaction.

---

## 5. Dapper reporting queries

Set-based reads where a single SQL statement is clearer and faster than composed LINQ:

| Interface / implementation | Used for |
| --- | --- |
| `ISqlQueryExecutor` / `DapperSqlQueryExecutor` | Thin Dapper wrapper over the connection. |
| `LeaderboardQueries` | Idea and idea-maker rankings (votes, reviews, outcomes) — all-time and monthly. |
| `AdminDashboardQueries` | Dashboard counts and time-series metrics. |

Queries are parameterized; no user input is concatenated into SQL.

---

## 6. Migrations

- Location: `backend/src/YSSE.IdeaBox.Infrastructure/Migrations`.
- Context project: `YSSE.IdeaBox.Infrastructure`; startup project: `YSSE.IdeaBox.Api`.

```powershell
# add a migration (from the repository root)
dotnet ef migrations add <Name> `
  --project backend/src/YSSE.IdeaBox.Infrastructure `
  --startup-project backend/src/YSSE.IdeaBox.Api

# apply
dotnet ef database update `
  --project backend/src/YSSE.IdeaBox.Infrastructure `
  --startup-project backend/src/YSSE.IdeaBox.Api
```

The API also applies migrations automatically **in Development** at startup (`DbInitializer`). In production, apply migrations through a controlled deployment step — do not rely on startup migration.

**Migration rules**
1. Review generated migrations before applying; never assume they are safe because they compile.
2. Avoid destructive changes (dropping columns/tables) without explicit approval and a backup.
3. Add foreign keys, unique constraints, and indexes deliberately.
4. Add migration files to source control (unlike `appsettings.Development.json`).

---

## 7. Seeding

`DbInitializer.InitializeAsync` runs after startup:

- **All environments:** roles, the permission catalog, role→permission defaults, and `platform_settings`.
- **Development only:** `MigrateAsync()`, demo users (see [README](../README.md#demo-accounts-development-only)), categories, tags, profiles, and sample ideas/content.

Seeding is idempotent — it checks for existing rows before inserting.

---

## 8. Indexes & integrity

Indexes and constraints are declared per entity configuration (for example a unique index on `ideas.slug`, a unique constraint enforcing one active vote per eligible user per idea, and indexes on foreign keys and common filter/sort columns). Because these live in the configuration classes, use `dotnet ef migrations` to materialize changes — do not hand-edit the database.

**Guidelines**
- Enforce invariants in the database (unique, check, foreign key) rather than relying solely on application code.
- Use transactions for operations that must succeed or fail together.
- Use optimistic concurrency (row versioning) where concurrent edits can conflict.
- Index based on real query patterns; verify with `EXPLAIN ANALYZE`.
- Avoid N+1 queries; project only needed columns.

---

## 9. Backups

Back up with `pg_dump` and **restore drills** — verify restores, not just backup creation:

```powershell
pg_dump -Fc -d ysse_ideabox -f ysse_ideabox.dump
pg_restore -d ysse_ideabox_restore ysse_ideabox.dump
```

---

## See also

- [Backend](backend.md) · [Security](security.md) · [Features & Lifecycle](features.md)

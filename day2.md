# YSSE Idea Box — Day 2 Plan (Remaining Work)

**Project root:** `G:\.NET SAAS\Idea_BOX`
**Predecessor:** `day1.md`
**Goal for Day 2:** Finish the API layer, generate + apply the EF migration, seed, run backend + frontend end-to-end, reconcile frontend/backend contracts, add tests, and document how to run.

> Day 1 delivered Domain + Application + Infrastructure (all compiling) and a complete React/Vite/Tailwind SPA (building). Only the **Api** project and database activation remain.

---

## 0. Ground rules (unchanged)

- Follow `AGENTS.md` and `rule.md`.
- Work in the established architecture; do NOT add projects or change layering.
- **Do NOT import `IAppDbContext`/`AppDbContext` into controllers**; call Application services only.
- Do NOT run servers / apply migrations until the user approves (see Day 1 instruction).
- Preferred approach: delegate each block below to a subagent, then verify with `dotnet build`.

---

## 1. Implement the API layer (`backend\src\YSSE.IdeaBox.Api`)

Read first (contracts): `Application\Abstractions\Services\*.cs`, `Application\Dtos\*.cs`, `Application\Common\Results.cs` + `Exceptions.cs`, `Application\Abstractions\IIdentityServices.cs` / `IIntegrationServices.cs`, `Application\DependencyInjection.cs`, `Infrastructure\DependencyInjection.cs`, `Infrastructure\Services\JwtTokenService.cs` (exact claim types), `Infrastructure\Data\DbInitializer.cs`.

### 1.1 Common (envelope)
- `ApiResponse<T>` `{ bool success; T? data; string? traceId; }`
- `PagedApiResponse<T>` `{ bool success; IReadOnlyList<T> data; int page; int pageSize; int total; bool hasNextPage; string? traceId; }`
- `ApiErrorResponse` `{ bool success=false; ApiError error; string? traceId; }`, `ApiError { string message; string code; IDictionary<string,string[]>? details; }`
- `ApiResults` helpers mapping `Result` / `Result<T>` / `PagedResult<T>` → `IActionResult`.

### 1.2 Security
- `CurrentUserService : ICurrentUserService` reading `HttpContext.User` (UserId, roles, IP). Register scoped + `AddHttpContextAccessor()`.

### 1.3 Middleware
- `CorrelationIdMiddleware` — honor/produce `X-Correlation-ID`, set `TraceIdentifier`, echo header, push to Serilog context.
- `GlobalExceptionMiddleware` — AppException→its code; ValidationAppException→400 + details; else 500 safe message; always the error envelope + correlation id; log via ILogger (no secrets).
- `FluentValidationFilter` (IAsyncActionFilter) — resolve `IValidator<T>`, return VALIDATION envelope when invalid.
- `ApiBehaviorOptions.InvalidModelStateResponseFactory` — error envelope for 400.

### 1.4 Program.cs wiring (order matters)
- Serilog console; Kestrel body limit (~15 MB) + `FormOptions.MultipartBodyLengthLimit`; request timeout.
- `AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))` (**enums as strings**).
- Swagger + JWT bearer security definition (enabled in Development).
- `AddApplication()` + `AddInfrastructure(builder.Configuration)`.
- JWT Bearer validation matching `JwtTokenService` claim types (`RoleClaimType`, `NameClaimType`).
- Authorization policies: `Admin` (`Administrator`), `Moderator` (`Moderator`,`Administrator`), `ScreeningReviewer` (`ScreeningReviewer`,`Administrator`), `OpportunitySeeker` (`Entrepreneur`,`Investor`,`Mentor`).
- CORS `spa` from `Cors:Origins` (default `http://localhost:5173`), `.AllowCredentials()`.
- Built-in `AddRateLimiter` — global fixed-window + named: `auth` (~10/min), `guest-review` (~3/hr), `write` (~30/min), `messaging` (~60/min); reject → 429 envelope; `UseRateLimiter()`.
- Health checks — `DatabaseHealthCheck : IHealthCheck` running `SELECT 1` via `ISqlQueryExecutor`; `/health/live` (no DB), `/health/ready` (DB).
- Pipeline order: CorrelationId → GlobalException → Serilog request logging → (HTTPS redir dev-off) → CORS → RateLimiter → Authentication → Authorization → MapControllers.
- Graceful shutdown (`HostOptions.ShutdownTimeout`); root `GET /` info JSON.
- Startup: `using var scope ...; await app.Services.InitializeAsync(migrate: app.Environment.IsDevelopment(), ct);` (seeds; migrate only in Development).

### 1.5 Controllers (`Controllers\`, prefix `api/v1`, shared `ApiControllerBase`)
- **AuthController** `auth`: POST register/login/refresh/logout, GET me, POST verify/request, verify. Rate `auth`. ([Authorize] where noted)
- **ProfilesController** `profiles`: GET/PUT me; GET/PUT/DELETE creator/me; GET creator/{userId} (anon); GET/PUT entrepreneur/me; GET entrepreneur/pending (Admin); POST entrepreneur/{userId}/approve (Admin).
- **CategoriesController** `categories`: GET categories, GET tags (anon); POST/PUT/DELETE (Admin).
- **IdeasController** `ideas`: GET (search, anon); GET {slug} (anon); GET id/{id} (anon); POST; POST quick; PUT {id}; POST {id}/submit; POST {id}/withdraw; DELETE {id}; GET mine; GET {id}/similar (anon); POST {id}/view (anon).
- **ScreeningController** `screening`: GET queue, GET {ideaId}/decisions, POST {ideaId}/decision, POST {ideaId}/status (ScreeningReviewer).
- **ReviewsController**: GET `ideas/{ideaId}/reviews` (anon); GET `ideas/{ideaId}/reviews/counts` (anon); POST `ideas/{ideaId}/reviews` (auth, rate write); POST `ideas/{ideaId}/reviews/guest` (anon, rate guest-review) — read/set cookie `ysse_guest` via `IGuestIdentityService`, build `GuestContext`, `HttpOnly; Secure; SameSite=Lax`, lifetime `Guest:ReviewCookieDays` (365); PATCH/DELETE `reviews/{id}`; POST `reviews/{id}/moderate` (Moderator).
- **VotesController**: GET `ideas/{ideaId}/votes` (anon); POST/DELETE `ideas/{ideaId}/votes`; GET `voting-periods` (anon); POST `voting-periods`, POST `voting-periods/{id}/activate|close` (Admin).
- **LeaderboardsController** `leaderboards`: GET ideas, GET idea-makers (anon; optional `votingPeriodId`, `take`).
- **CommentsController**: GET `ideas/{ideaId}/comments` (anon); POST `ideas/{ideaId}/comments`; DELETE `comments/{id}`; POST `comments/{id}/moderate` (Moderator); POST `ideas/{ideaId}/follow`; GET `ideas/followed`.
- **ProblemsController** `problems`: GET list, GET {id} (anon); POST, POST {id}/endorse, POST {id}/status (auth + Moderator), POST {id}/link.
- **InquiriesController**: POST `ideas/{ideaId}/inquiries` (OpportunitySeeker, rate write); GET `inquiries/received|sent`; POST `inquiries/{id}/respond`; GET `watchlist`; POST/DELETE `watchlist/{ideaId}`; GET `matches` (OpportunitySeeker).
- **ConversationsController** `conversations`: GET, GET {id}, POST, POST {id}/messages, POST {id}/read, POST {id}/status (rate messaging); plus POST `users/{id}/block`; POST `reports`.
- **AdminController** `admin`: GET users; POST users/{id}/suspend|reactivate; POST users/{id}/roles; DELETE users/{id}/roles/{role}; GET audit-logs; GET/PUT settings; GET dashboard (Admin); GET reports, POST reports/{id}/resolve (Moderator).
- **OutcomesController**: GET `ideas/{ideaId}/outcomes` (anon); POST `outcomes`; POST `outcomes/{id}/verify` (Admin).
- **ProjectsController**: GET `ideas/{ideaId}/projects` (anon); POST `projects`; POST `projects/{id}/status|milestones|tasks|updates`.
- **MarketplaceController** `marketplace`: GET list (anon); GET mine; GET {id} (anon); POST; POST {id}/status; POST {id}/reservations; GET {id}/reservations; POST `reservations/{id}/respond`.
- **NotificationsController** `notifications`: GET; POST {id}/read; POST read-all; GET/PUT preferences.

### 1.6 Config files
- `appsettings.json`: `Jwt` (Issuer `YSSE.IdeaBox`, Audience `YSSE.IdeaBox.Client`, Key ≥32 chars dev, AccessTokenMinutes 60, RefreshTokenDays 14), `Cors:Origins` [`http://localhost:5173`], `Guest:ReviewCookieDays` 365, `Storage:Root` `storage/uploads`, AllowedHosts `*`.
- `appsettings.Development.json`: `ConnectionStrings:Default` = `Host=localhost;Port=5432;Database=ysse_ideabox;Username=postgres;Password=admin`.
- `Properties\launchSettings.json`: http profile `http://localhost:5080`.

---

## 2. Generate the EF migration (do NOT apply yet)

From `G:\.NET SAAS\Idea_BOX\backend`:
```powershell
dotnet new tool-manifest            # if .config/dotnet-tools.json missing
dotnet tool install dotnet-ef --version 8.0.11
dotnet ef migrations add InitialCreate --project src\YSSE.IdeaBox.Infrastructure --startup-project src\YSSE.IdeaBox.Api --output-dir Migrations
```
- Verify the generated migration (tables, FKs, indexes, snake_case).
- Do NOT run `dotnet ef database update` yet.

---

## 3. Build + verify (no server)
```powershell
cd "G:\.NET SAAS\Idea_BOX\backend"
dotnet build "YSSE.IdeaBox.slnx"      # must be 0 errors
```

---

## 4. Ask the user, then run end-to-end

**STOP and get approval before these steps.**
1. Apply migration: `dotnet ef database update --project src\YSSE.IdeaBox.Infrastructure --startup-project src\YSSE.IdeaBox.Api` (or let startup `InitializeAsync(migrate:true)` do it in Development).
2. Verify seed: `psql` (via `C:\Program Files\PostgreSQL\18\bin\psql.exe`, `$env:PGPASSWORD='admin'`) — check `users`, `categories`, `ideas` rows; login `admin@ysse.local` / `Admin@12345`.
3. Start API (`dotnet run --project src\YSSE.IdeaBox.Api`, port 5080) and frontend (`npm run dev`, port 5173).
4. Smoke test: health endpoints; register/login/refresh; browse ideas; post + moderate review; guest review cookie; vote; leaderboard; inquiry/match; conversation; marketplace reservation; admin dashboard.

---

## 5. Contract reconciliation (frontend ↔ backend)

- Confirm enum string values match the frontend option values (IdeaStatus, Stage, Attribution, etc.). Adjust one side if mismatched.
- Confirm DTO field casing matches (API camelCase vs frontend field access).
- Confirm paged shape (`data`, `page`, `pageSize`, `total`, `hasNextPage`) matches what grids/pagination expect.
- Fix any 401/refresh loop or CORS credential issues.

---

## 6. Tests
- Add unit tests for core business rules (status transitions, review counts, vote eligibility, leaderboard weighting, matching) — a `YSSE.IdeaBox.Tests` project may be added with approval.
- Add at least a few integration tests via `WebApplicationFactory` for auth + ideas if time allows.
- Run tests and report actual results.

---

## 7. Documentation
- Root `README.md`: prerequisites, DB setup, connection string, how to run backend + frontend, seeded admin credentials, endpoint base, test commands.

---

## 8. Definition of Done (Day 2)
- [ ] API project fully implemented; solution builds 0 errors.
- [ ] `InitialCreate` migration generated and reviewed.
- [ ] DB migrated + seeded; admin login works.
- [ ] API + SPA run together; core user journeys pass.
- [ ] Frontend/backend contracts reconciled.
- [ ] Tests added and run (results reported honestly).
- [ ] README added.

---

## 9. Blockers / prerequisites to confirm with user
1. Approval to apply the migration and run servers.
2. Whether to add a test project (new project — needs approval per `AGENTS.md`).
3. Any preference on JWT dev key / secrets handling beyond dev defaults.

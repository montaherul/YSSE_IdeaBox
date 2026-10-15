# YSSE Idea Box — Architecture Audit & Refactoring Plan

Status: Phase 1 (Audit) — produced before any refactoring.
Scope: Full repository (`backend/` .NET 8 Clean Architecture solution; `frontend/` React 18 + Vite SPA).
Principle: refactor, not rewrite. Preserve functionality, database compatibility, and API contracts where practical.

---

## 1. Repository orientation

### Backend (`backend/src`, solution `YSSE.IdeaBox.slnx`)

| Project | Responsibility | Notes |
| --- | --- | --- |
| `YSSE.IdeaBox.Domain` | Entities, enums, base types | No EF/ASP.NET dependencies. Good. |
| `YSSE.IdeaBox.Application` | Services, DTOs, validators, abstractions | Depends on Domain. |
| `YSSE.IdeaBox.Infrastructure` | EF Core, repositories, UoW, integrations | Implements Application abstractions. |
| `YSSE.IdeaBox.Api` | Controllers, middleware, auth, DI composition | Presentation boundary. |
| `YSSE.IdeaBox.Tests` | xUnit tests | 16 test classes. |

Dependency direction is correct: Domain ← Application ← (Infrastructure | Api). No circular project references observed.

### Frontend (`frontend/src`)

Feature-based structure already present: `app/`, `layouts/`, `features/*`, `components/{ui,shared}`, `services/api`, `hooks/`, `navigation/`, `config/`, `store/`. Data fetching via TanStack Query; client state via Zustand; forms via react-hook-form + Zod.

---

## 2. Findings

Severity: **H**igh (architectural/security), **M**edium (maintainability), **L**ow (polish).

| # | Sev | Finding | Location | Impact |
| --- | --- | --- | --- | --- |
| F1 | H | **God service**: `AdminService` (768 lines) owns 9 unrelated concerns — users, user-roles, roles, permissions, role-permissions, audit logs, platform settings, static content, dashboard. | `Application/Services/Admin/AdminService.cs`, `Abstractions/Services/AdminServiceInterfaces.cs` | Untestable in isolation, high change-coupling, violates SRP. |
| F2 | H | **Duplicated role-based authorization inside the service**: `RequireAdmin()` calls `ICurrentUserService.IsAdmin` (role enum). Controllers *already* enforce permission policies (`users.read`, `roles.manage`, …). Authorization logic exists in two layers, and the inner one is role-based. | `AdminService.RequireAdmin`; controller `[Authorize(Policy=…)]` | Conflicting authorization models; blocks dynamic roles; runs on every method. |
| F3 | H | **Roles are enum-backed, not dynamic**: `Role.Name` is `RoleName` enum; create/update parse `Enum.TryParse<RoleName>`. New roles require a code change + migration. | `Domain/Entities/Identity.cs`, `Enums/Enums.cs`, `AdminService.CreateRoleAsync/UpdateRoleAsync` | "Create role" only works for predefined enum values. Documented limitation. |
| F4 | M | Role-based authorization policies (`"Admin"`, `"Moderator"`, `"ScreeningReviewer"`, `"OpportunitySeeker"`) coexist with permission policies. `ICurrentUserService` exposes `IsAdmin`/`IsModeratorOrAdmin`/`IsScreeningReviewer`. | `Api/Program.cs`, `Api/Security/CurrentUserService.cs`, `AdminService`, various services | Coupling business logic to role names; risk of drift. |
| F5 | M | `SettingsController.UpdateSettings` loops and calls `UpdateSettingAsync` once per key → N `SaveChanges` round-trips, no atomicity. | `Api/Controllers/SettingsController.cs` | Partial updates on failure; chatty. |
| F6 | M | `StaticContentController` mixes anonymous read (`GET {key}`) with authenticated Admin CRUD in one controller. | `Api/Controllers/StaticContentController.cs` | Acceptable but blurs resource ownership; consider a public read surface. |
| F7 | L | Frontend consumed `AdminService` via `/admin/*`-prefixed feature module (`features/admin`) even for pure resources (users, roles, permissions). Partially addressed: `features/users/` now exists. | `frontend/src/features/admin`, `features/users` | Resource pages living under the admin module. |
| F8 | L | Navigation was role-scoped by `audience`; now hybrid role-then-permission (Phase 5/6 work in progress from prior session). | `frontend/src/navigation`, `layouts/Sidebar.jsx` | Keep going toward permission-driven. |

No evidence of: direct `DbContext` access in controllers, raw SQL string concatenation, secrets in source, or swallowed exceptions. Rate limiting, health checks, correlation IDs, global exception handling, and request timeouts are present and correct.

---

## 3. Dynamic roles and permissions — assessment and decision

The RBAC model is already database-backed and permission-based at the enforcement point:

- Tables: `Users`, `Roles`, `UserRoles`, `Permissions`, `RolePermissions`.
- Permission codes are strings (`users.read`, `ideas.approve`, …) defined in `Application/Common/Permissions.cs` and seeded.
- Enforcement: `PermissionPolicyProvider` + `PermissionAuthorizationHandler` resolve the caller's permissions **from the database per request** via `IUserPermissionService`. This means permission/role changes take effect immediately (supports revocation without waiting for JWT expiry). Good.
- JWT carries only role claims; permissions are resolved server-side each request — the correct design.

**Limitation:** the *set of roles* is constrained by the `RoleName` enum and `Role.Name` is persisted as an integer. Making roles fully dynamic (arbitrary string roles) requires a schema/type migration and a data migration of existing `Role.Name`/`UserRole` references, plus replacing `ICurrentUserService.IsAdmin`-style checks.

**Decision (this refactor):** Do **not** convert roles to free-form strings now — that is a high-risk persistence change with no current product requirement, and the prompt requires preserving production data. Instead:
1. Remove role-based authorization from the service layer (F2), relying on the existing permission policies at the controller boundary.
2. Keep the enum as the *bootstrap/known-roles* catalog, and keep role→permission mapping as the mechanism that grants capabilities. New capabilities are granted by assigning permissions, not by branching on role name.
3. Document the enum constraint as an explicit, verified limitation rather than silently implying fully dynamic roles.

This narrows role coupling without weakening authorization (permission policies remain authoritative).

---

## 4. Target architecture

### Backend — decompose `AdminService` into focused application services

Replace the single `IAdminService` with cohesive interfaces and implementations (namespace `YSSE.IdeaBox.Application.Abstractions.Services` / `...Services.Admin`):

| New interface | Owns | Source methods |
| --- | --- | --- |
| `IUserManagementService` | User list/detail/suspend/reactivate | `GetUsersAsync`, `GetUserByIdAsync`, `SuspendUserAsync`, `ReactivateUserAsync` |
| `IUserRoleService` | Assign/revoke a user's roles (incl. protected-role invariants) | `AssignRoleAsync`, `RevokeRoleAsync` |
| `IRoleService` | Role CRUD | `GetRolesAsync`, `GetRoleByIdAsync`, `CreateRoleAsync`, `UpdateRoleAsync`, `DeleteRoleAsync` |
| `IPermissionService` | Permission CRUD | `GetPermissionsAsync`, `GetPermissionByIdAsync`, `Create*`, `Update*`, `Delete*` |
| `IRolePermissionService` | Role↔permission assignment | `AssignPermissionToRoleAsync`, `RemovePermissionFromRoleAsync` |
| `IAuditLogService` | Audit log listing | `GetAuditLogsAsync` |
| `IPlatformSettingsService` | Settings read/write | `GetSettingsAsync`, `UpdateSettingAsync` |
| `IStaticContentService` | Static content CRUD + public read | `GetStaticContentsAsync`, `Create*`, `Update*`, `Delete*`, `GetStaticContentByKeyAsync` |
| `IAdminDashboardService` | Admin dashboard aggregation | `GetDashboardAsync` |

Rules applied:
- Controllers depend on the focused interface only.
- Authorization is **not** re-implemented in services; the controller's permission policy is the boundary. Business invariants (self-suspend guard, last-admin guard, system-role protection, delete-while-assigned guards) remain in the services.
- `IAdminService` is deleted only after all controllers and DI migrate.

### Backend — other improvements
- `SettingsController.UpdateSettings`: add a batch application method so all keys commit atomically (single `SaveChanges`), preserving the existing response contract.
- Keep resource-oriented routes exactly as-is (already correct: `/api/v1/users`, `/api/v1/roles`, `/api/v1/permissions`, `/api/v1/audit-logs`, `/api/v1/settings`, `/api/v1/static-content`, `/api/v1/dashboard`).

### Frontend
- Continue moving resource pages into resource features (`features/users` done; `roles`, `permissions` are candidates) and keep `features/admin` for genuine governance/dashboard/moderator workflows.
- Centralize API access (already in `services/apiClient` + per-feature `api.js`).
- Permission-driven navigation (already introduced); finish replacing `audience`-only gating with `roles`/`permissions` per item.

---

## 5. Migration sequence

1. **Phase 2 — Service decomposition.** Add focused interfaces + implementations, register in DI, migrate the 7 controllers, delete `IAdminService`/`AdminService`. Batch settings update. Build + tests.
2. **Phase 3 — Resource-based APIs.** Verify routes, status codes, DTOs, and the settings batch behavior. No contract breaking.
3. **Phase 4 — Authorization.** Confirm controller-level permission policies cover every operation after removing `RequireAdmin()`; add tests asserting 403 on missing permission. Do not weaken checks.
4. **Phase 5 — Frontend features.** Relocate remaining resource pages out of `features/admin` where they duplicate a resource; centralize cross-cutting behavior.
5. **Phase 6 — Layouts/navigation.** Finish permission-driven sidebar; keep two shared layouts.
6. **Phase 7 — Testing & cleanup.** Full build, unit/integration tests, lint, remove obsolete code.

Each phase: build, run tests, report changed files, decisions, and actual command results.

---

## 6. Validation strategy

Backend:
- `dotnet build` (application + API where the running process allows; otherwise build individual projects).
- `dotnet test` for `YSSE.IdeaBox.Tests`.
- Add tests for the new services and for authorization (missing permission → 403).
- Manual review of EF Core queries for the migrated methods (no behavior change expected — logic is moved verbatim).

Frontend:
- `npm run build`, `npm test -- --run`, `npx oxlint <changed files>`.

Integration:
- Confirm controllers resolve the new services via DI (build + test host).
- Confirm response envelopes unchanged (`ApiResponse`/`ApiResults`).

---

## 7. Risks and mitigations

| Risk | Mitigation |
| --- | --- |
| Behavior drift when moving methods | Move code verbatim; keep guards and audit calls; rely on existing service tests. |
| Removing `RequireAdmin()` weakens auth | Controller permission policies already enforce every operation; verify each endpoint has a policy; add tests. |
| Enum-role constraint misread as bug | Documented as an intentional limitation with a migration path, not silently changed. |
| Running API locks build artifacts | Build individual projects; report the full-solution build status honestly. |
| Frontend breaks on API change | No route/DTO changes in Phase 2–3; only internal DI/class changes. |

---

## 8. Verification log (Phase 1)

- Enumerated application services (top file: `AdminService.cs`, 768 lines) and all controllers.
- Confirmed `IAdminService` consumers: 7 controllers + DI registration only; no other service or test depends on it.
- Confirmed authorization is permission-policy based at the controller boundary and DB-resolved per request.
- Confirmed RBAC schema is database-backed; roles constrained by `RoleName` enum.
- No code changed during Phase 1.

---

## 9. Phase 2 — Service decomposition (completed)

`AdminService` / `IAdminService` removed and replaced by nine focused services. Code moved verbatim; the duplicated service-layer `RequireAdmin()` role check was dropped (authorization remains enforced by the controllers' permission policies, which fail closed).

Interfaces — `Application/Abstractions/Services/`:
- `UserManagementServiceInterfaces.cs` → `IUserManagementService`, `IUserRoleService`
- `RbacServiceInterfaces.cs` → `IRoleService`, `IPermissionService`, `IRolePermissionService`
- `PlatformServiceInterfaces.cs` → `IAuditLogService`, `IPlatformSettingsService`, `IStaticContentService`, `IAdminDashboardService`

Implementations:
- `Application/Services/Identity/` → `UserManagementService`, `UserRoleService`, `RoleService`, `PermissionService`, `RolePermissionService`
- `Application/Services/Platform/` → `AuditLogService`, `PlatformSettingsService`, `StaticContentService`, `AdminDashboardService`

Wiring:
- `Application/DependencyInjection.cs` registers all nine services.
- Controllers updated: `UsersController` (`IUserManagementService` + `IUserRoleService`), `RolesController` (`IRoleService` + `IRolePermissionService`), `PermissionsController`, `AuditLogsController`, `SettingsController`, `StaticContentController`, `DashboardController`.

Behavior preserved: business invariants retained (self-suspend guard, protected/system roles, last-admin guard, delete-while-assigned guards, audit records). API routes, DTOs, response envelopes, and public static-content read unchanged.

Verification (actually run):
- `dotnet build src/YSSE.IdeaBox.Application` → succeeded, 0 warnings, 0 errors.
- `dotnet build src/YSSE.IdeaBox.Api` → 0 errors compiling; the default output copy is blocked by the running API (pid 16956) locking `bin`; a build to a separate output directory succeeded (0 warnings, 0 errors).
- `dotnet test tests/YSSE.IdeaBox.Tests` → 164/164 passed.
- No remaining references to `IAdminService`/`AdminService`/`Services.Admin`.

Known limitations carried forward (documented, not changed):
- `RoleName` enum constrains role creation (F3).
- `SettingsController` now applies all keys in one atomic save (F5 resolved): `IPlatformSettingsService.UpdateSettingsAsync` performs a single `SaveChanges` and records one `PlatformSettingsUpdated` audit entry (previously one save + one `PlatformSettingUpdated` entry per key). The unused `PlatformSettingUpdateRequest` DTO was removed. Response contract unchanged.
- Frontend `features/admin` still holds some resource pages (F7).


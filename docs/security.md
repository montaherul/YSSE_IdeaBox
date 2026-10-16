# Security

YSSE Idea Box follows a **deny-by-default**, server-enforced security model. This document covers authentication, authorization (RBAC), rate limiting, uploads, secret handling, privacy, and the review checklist used before shipping changes.

---

## 1. Principles

- **Server is authoritative.** All permissions and business rules are enforced by the API, even when the UI also validates them. Hiding a button is not authorization.
- **Least privilege.** Roles grant the minimum they need; the administrator role is the only catch-all.
- **No secrets in source.** Real connection strings and keys live in git-ignored config or environment/secret stores.
- **Fail safely.** Errors return generic messages and a trace ID; internals never leak.

---

## 2. Authentication

- **JWT bearer (HS256).** Access and refresh tokens are signed with `Jwt:Key` (required, ≥ 32 chars outside Development).
- Access tokens carry `sub`, `jti`, `name`, `email`, and one `role` claim per role. Lifetime: `Jwt:AccessTokenMinutes` (default 60).
- Refresh tokens are separate JWTs (`typ=refresh`) with `Jwt:RefreshTokenDays` lifetime (default 14). Their **hashes are stored** (`refresh_tokens`) and validated on rotation, so tokens can be **revoked** (logout, password reset).
- Passwords are hashed with **BCrypt** (`BcryptPasswordHasher`); plaintext is never stored or logged.
- Email/phone verification uses short-lived codes (`InMemoryVerificationCodeStore`); verification/forgot/reset flows are rate-limited.
- Endpoints accept `[AllowAnonymous]` only where required (browse, auth, media, health).

### Token handling on the client
- The SPA stores tokens for session persistence and attaches the access token via the API client.
- On `401`, the client attempts a single refresh and retries; on failure it clears the session and returns to login.
- Never place tokens, secrets, or sensitive private messages in `localStorage` beyond what is required for the session, and never in logs or URLs.

---

## 3. Authorization (RBAC)

Two layers, both evaluated server-side:

### Roles
`Member`, `Creator`, `Entrepreneur`, `Investor`, `Mentor`, `Moderator`, `ScreeningReviewer`, `Administrator`. Named policies exist for `Admin`, `Moderator`, `ScreeningReviewer`, `OpportunitySeeker`.

### Permissions (primary mechanism)
- The permission catalog (`Application/Common/Permissions.cs`) defines ~80 dotted codes (for example `ideas.create`, `screening.decide`, `users.manage_roles`, `content.reports.manage`).
- A user's effective permissions = union of permissions granted to their roles (`user_roles` → `role_permissions` → `permissions`).
- `PermissionPolicyProvider` recognizes any catalog code; `PermissionAuthorizationHandler` resolves the caller's permissions **from the database per request** via `IUserPermissionService`. Granting or revoking a permission takes effect immediately — tokens do not need to expire.
- Role→permission defaults are seeded in `DbInitializer`: `Member` gets self-service capabilities; `Moderator`, `ScreeningReviewer`, and `Administrator` progressively more; `Administrator` holds all permissions.

### Resource-level checks
Beyond endpoint policy, services verify **ownership and state** — for example only an idea's creator may edit it, only in allowed statuses; only conversation participants may read messages; project creation is staff-driven (moderator/admin) with a live idea and an entrepreneur/investor lead, and project data is readable only by staff and participants. These checks run in the application layer and cannot be bypassed by the client.

### IDOR protection
Identifiers are never trusted for access decisions. The server loads the resource, verifies the caller's right to it, and returns `403`/`404` accordingly.

---

## 4. Rate limiting & abuse control

Fixed-window policies (configured in `Program.cs`):

| Policy | Limit | Applied to |
| --- | --- | --- |
| global | 120/min | All requests |
| `auth` | 10/min | Register/login/refresh/verify/forgot/reset |
| `guest-review` | 3/hour | Anonymous idea reviews |
| `write` | 30/min | Create/update/upload endpoints |
| `messaging` | 60/min | Send-message endpoints |

Exceeded limits return `429` with the `RATE_LIMITED` error envelope. Request bodies are capped (15 MB), timeouts are 30 seconds, and pagination is bounded (max 100/page) to protect resources.

Anti-abuse features: guest reviews are tracked via an HTTP-only guest cookie and rate-limited (the system does not claim to fully verify guest identity); voting eligibility and uniqueness are enforced in the database; messaging supports blocking and reporting.

---

## 5. Input validation & injection

- **FluentValidation** validators run in a pipeline filter for every request DTO; model-binding failures produce `400 VALIDATION`.
- **Zod** schemas validate on the client for UX only.
- **Parameterized queries** everywhere (EF Core and Dapper). Untrusted input is never concatenated into SQL.
- Rich text/HTML is sanitized before storage/rendering to mitigate stored XSS.
- Enum, id, length, and file-metadata inputs are validated at the boundary.

---

## 6. File uploads

- Size limits: avatar **5 MB**, idea attachment **15 MB** (`UploadLimits`).
- Stored via `IFileStorage` (`LocalFileStorage`) with **generated safe names** under `Storage:Root`; original names are not used for paths.
- Served through `MediaController`, which permits only image extensions (`.webp .png .jpg .jpeg`) and sets an immutable cache policy for media.
- Access permissions are checked before serving non-public files.

---

## 7. Transport & configuration

- HTTPS redirection is enabled outside Development; HSTS is applied in production.
- CORS is restricted to explicit SPA origins (`Cors:Origins`, default `http://localhost:5173`) — wildcard origins are not used with credentials.
- Development and production configuration are separated; `appsettings.Development.json`, `.env*`, and `appsettings.*.local.json` are git-ignored.
- Secrets come from configuration/environment (double-underscore keys, e.g. `Jwt__Key`), user-secrets, or a secret store — never committed.

---

## 8. Logging & audit

- **Serilog** structured logging with correlation IDs from `CorrelationIdMiddleware`.
- **Never logged:** passwords, tokens, connection strings, signing keys, or full private message contents.
- `IAuditWriter` records significant administrative and lifecycle events (for example `IdeaStatusChanged`, `ScreeningDecided`, `ProjectCreated`) to `audit_logs`, with actor, action, target, and reason.
- Unexpected exceptions are logged with diagnostic context and surfaced to clients only as `500 INTERNAL_ERROR` with a trace ID.

---

## 9. Privacy

- Profile visibility is controlled by `ProfileFieldVisibility`.
- Anonymous idea attribution hides the creator publicly while still recording `CreatorUserId` internally.
- Anonymous ideas are excluded from creator profiles, leaderboards, and search inference.
- Data retention/deletion expectations apply to accounts, private messages, reports, and audit records.

---

## 10. Review checklist

Before completing a security-sensitive change:

- [ ] Authentication is correct (token validated: signature, issuer, audience, lifetime).
- [ ] Authorization is enforced server-side and resource ownership is verified.
- [ ] Input is validated; SQL is parameterized.
- [ ] Secrets are not exposed; sensitive data is not logged.
- [ ] Rate limits and request-size limits apply where relevant.
- [ ] Error messages do not leak internals.
- [ ] File uploads are validated (size, type, generated names, access).
- [ ] CORS and transport settings are appropriate for the environment.
- [ ] Guest/anonymous flows are tracked and rate-limited.

---

## See also

- [Backend](backend.md) · [API Reference](api-reference.md) · [Architecture](architecture.md)

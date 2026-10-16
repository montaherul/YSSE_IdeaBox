# API Reference

Base URL: `http://localhost:5080/api/v1` (development). All endpoints are JSON over HTTPS.

Interactive documentation (Development): `http://localhost:5080/swagger`.

---

## 1. Conventions

### Authentication
Send a bearer access token on protected endpoints:

```http
Authorization: Bearer <accessToken>
```

Controllers are authenticated by default. Endpoints marked **Anonymous** below do not require a token. `Policy` columns list the permission code required (role policies are noted where used).

### Response envelope

**Single resource**
```json
{ "success": true, "data": { }, "traceId": "0HN…" }
```

**Paged collection** — used by list endpoints:
```json
{
  "success": true,
  "data": { "items": [], "page": 1, "pageSize": 20, "total": 245, "hasNextPage": true },
  "traceId": "0HN…"
}
```

**Error**
```json
{
  "success": false,
  "error": { "message": "One or more validation errors occurred.", "code": "VALIDATION", "details": { "title": ["…"] } },
  "traceId": "0HN…"
}
```

### Paging, search, sorting
List endpoints accept `page` (1-based), `pageSize` (default **20**, max **100**), and often `search`, plus module-specific filters. Pagination is always applied server-side.

### Status codes
`200` OK · `201` Created · `204` No Content · `400` Bad Request (`VALIDATION`) · `401` Unauthorized · `403` Forbidden · `404` Not Found · `409` Conflict · `429` Too Many Requests (`RATE_LIMITED`) · `500` Internal Server Error (`INTERNAL_ERROR`).

### Rate limits
Fixed-window policies: global **120/min**; `auth` **10/min**; `guest-review` **3/hour**; `write` **30/min**; `messaging` **60/min**.

### Trace IDs
Every response carries `traceId`; `X-Correlation-ID` is echoed (or generated) by the API and useful for support.

---

## 2. Endpoints by module

Legend: **A** = anonymous, **Auth** = any authenticated user, otherwise the permission code required.

### Auth — `/api/v1/auth`

| Method | Path | Access |
| --- | --- | --- |
| POST | `/register` | A · `auth` limit |
| POST | `/login` | A · `auth` limit |
| POST | `/refresh` | A · `auth` limit |
| POST | `/logout` | A |
| GET | `/me` | Auth |
| POST | `/verify/request` | Auth · `auth` limit |
| POST | `/verify` | Auth · `auth` limit |
| POST | `/forgot-password` | A · `auth` limit |
| POST | `/reset-password` | A · `auth` limit |

### Ideas — `/api/v1/ideas`

| Method | Path | Policy | Notes |
| --- | --- | --- | --- |
| GET | `` | A | Search/filter published ideas. |
| GET | `/mine` | `ideas.read` | Current user's ideas. |
| GET | `/id/{id}` | A | By id. |
| GET | `/{slug}` | A | By slug. |
| POST | `` | `ideas.create` (write) | Create full idea. |
| POST | `/quick` | `ideas.create` (write) | Create quick idea. |
| PUT | `/{id}` | `ideas.update` (write) | Update (owner / allowed status). |
| POST | `/{id}/submit` | `ideas.update` (write) | Submit for screening. |
| POST | `/{id}/withdraw` | `ideas.update` | Withdraw. |
| DELETE | `/{id}` | `ideas.delete` | Delete draft. |
| POST | `/{id}/attachment` | `ideas.update` (write) | Upload attachment. |
| DELETE | `/{id}/attachment` | `ideas.update` | Remove attachment. |
| GET | `/{id}/attachment` | Auth | Download attachment. |
| POST | `/{id}/feature` | `ideas.feature` | Feature idea. |
| POST | `/{id}/unfeature` | `ideas.feature` | Unfeature idea. |
| GET | `/{id}/similar` | A | Similar ideas. |
| POST | `/{id}/view` | A (write) | Record a view. |

### Categories & Tags — `/api/v1` (controller `Categories`)

| Method | Path | Policy |
| --- | --- | --- |
| GET | `/categories` | A |
| GET | `/tags` | A (alternate) |
| GET | `/categories/tags` | `tags.read` |
| POST | `/categories` | `categories.create` |
| PUT | `/categories/{id}` | `categories.update` |
| DELETE | `/categories/{id}` | `categories.delete` |
| POST | `/categories/tags` | `tags.create` |
| PUT | `/categories/tags/{id}` | `tags.update` |
| DELETE | `/categories/tags/{id}` | `tags.delete` |

### Comments & Follows — `/api/v1`

| Method | Path | Policy |
| --- | --- | --- |
| GET | `/ideas/{ideaId}/comments` | A |
| POST | `/ideas/{ideaId}/comments` | `reviews.create` |
| PUT | `/comments/{id}` | `reviews.update` |
| DELETE | `/comments/{id}` | `reviews.delete` |
| POST | `/comments/{id}/moderate` | `reviews.moderate` |
| POST | `/ideas/{ideaId}/follow` | `reviews.read` |
| GET | `/ideas/followed` | `reviews.read` |

### Screening — `/api/v1/screening`

| Method | Path | Policy |
| --- | --- | --- |
| GET | `/queue` | screening access (queue) |
| GET | `/{ideaId}/decisions` | screening access |
| POST | `/{ideaId}/decision` | `screening.decide` |
| POST | `/{ideaId}/status` | `screening.decide` |

Decision body accepts target-status values `Published`, `Rejected`, `RevisionRequired` (legacy `Approve`/`Reject`/`RequestRevision` also accepted).

### Problems — `/api/v1/problems`

| Method | Path | Policy |
| --- | --- | --- |
| GET | `` | A |
| GET | `/{id}` | A |
| POST | `` | `problems.create` |
| PUT | `/{id}` | `problems.update` |
| DELETE | `/{id}` | `problems.update` |
| POST | `/{id}/endorse` | `problems.read` |
| POST | `/{id}/status` | `problems.moderate` |
| POST | `/{id}/link` | `problems.update` |

### Reviews — `/api/v1`

| Method | Path | Policy |
| --- | --- | --- |
| GET | `/ideas/{ideaId}/reviews` | A |
| GET | `/ideas/{ideaId}/reviews/counts` | A |
| POST | `/ideas/{ideaId}/reviews` | `reviews.create` (write) |
| POST | `/ideas/{ideaId}/reviews/guest` | A (`guest-review` limit) |
| GET | `/reviews/mine` | `reviews.read` |
| PATCH | `/reviews/{id}` | `reviews.update` |
| DELETE | `/reviews/{id}` | `reviews.delete` |
| POST | `/reviews/{id}/moderate` | `reviews.moderate` |

Guest reviews are tracked by an HTTP-only guest cookie and rate-limited. The cookie lifetime is `Guest:ReviewCookieDays` (default 365).

### Votes & Voting Periods — `/api/v1`

| Method | Path | Policy |
| --- | --- | --- |
| GET | `/ideas/{ideaId}/votes` | A |
| POST | `/ideas/{ideaId}/votes` | `voting.cast` |
| DELETE | `/ideas/{ideaId}/votes` | `voting.cast` |
| GET | `/voting-periods` | A |
| POST | `/voting-periods` | `votingperiods.manage` |
| PUT | `/voting-periods/{id}` | `votingperiods.manage` |
| DELETE | `/voting-periods/{id}` | `votingperiods.manage` |
| POST | `/voting-periods/{id}/activate` | `votingperiods.manage` |
| POST | `/voting-periods/{id}/close` | `votingperiods.manage` |

### Leaderboards — `/api/v1/leaderboards`

| Method | Path | Policy |
| --- | --- | --- |
| GET | `/ideas` | A |
| GET | `/ideas/current-month` | A |
| POST | `/ideas/current-month/announce` | `votingperiods.manage` |
| GET | `/idea-makers` | A |

### Inquiries & Watchlist — `/api/v1`

| Method | Path | Policy |
| --- | --- | --- |
| POST | `/ideas/{ideaId}/inquiries` | `inquiries.create` (write) |
| GET | `/inquiries/received` | `inquiries.read` |
| GET | `/inquiries/sent` | `inquiries.read` |
| POST | `/inquiries/{id}/respond` | `inquiries.manage` |
| PUT | `/inquiries/{id}` | `inquiries.update` |
| DELETE | `/inquiries/{id}` | `inquiries.delete` |
| GET | `/watchlist` | `inquiries.read` |
| POST | `/watchlist/{ideaId}` | `inquiries.create` |
| DELETE | `/watchlist/{ideaId}` | `inquiries.manage` |
| GET | `/matches` | `inquiries.read` |

### Messaging — `/api/v1`

| Method | Path | Policy |
| --- | --- | --- |
| GET | `/conversations` | Auth |
| GET | `/conversations/{id}` | Auth |
| POST | `/conversations` | Auth (`messaging` limit) |
| POST | `/conversations/{id}/messages` | Auth (`messaging` limit) |
| PUT | `/conversations/{id}/messages/{messageId}` | `messages.update` |
| DELETE | `/conversations/{id}/messages/{messageId}` | `messages.delete` |
| POST | `/conversations/{id}/read` | Auth |
| POST | `/conversations/{id}/status` | Auth |
| POST | `/users/{userId}/block` | Auth |

### Marketplace — `/api/v1`

| Method | Path | Policy |
| --- | --- | --- |
| GET | `/marketplace` | A |
| GET | `/marketplace/mine` | `marketplace.read` |
| GET | `/marketplace/{id}` | A |
| POST | `/marketplace` | `marketplace.create` |
| PUT | `/marketplace/{id}` | `marketplace.update` |
| DELETE | `/marketplace/{id}` | `marketplace.delete` |
| POST | `/marketplace/{id}/status` | `marketplace.update` |
| POST | `/marketplace/{listingId}/reservations` | `marketplace.read` |
| GET | `/marketplace/{listingId}/reservations` | `marketplace.manage` |
| POST | `/reservations/{id}/respond` | `marketplace.manage` |

### Outcomes — `/api/v1`

| Method | Path | Policy |
| --- | --- | --- |
| GET | `/ideas/{ideaId}/outcomes` | A |
| POST | `/outcomes` | `outcomes.create` |
| POST | `/outcomes/{id}/verify` | `outcomes.verify` |
| PUT | `/outcomes/{id}` | `outcomes.update` |
| DELETE | `/outcomes/{id}` | `outcomes.delete` |

### Projects — `/api/v1`

| Method | Path | Policy |
| --- | --- | --- |
| GET | `/ideas/{ideaId}/projects` | A — staff or participants only (hidden otherwise) |
| GET | `/projects/{id}` | `projects.read` — staff or participants only |
| GET | `/projects/archive` | `projects.manage` |
| GET | `/projects/mine` | A — assigned projects (lead, member, or idea creator) |
| GET | `/projects/lead-candidates` | `projects.create` |
| POST | `/projects` | `projects.create` |
| POST | `/projects/{id}/archive` | `projects.manage` |
| PUT | `/projects/{id}` | `projects.update` |
| DELETE | `/projects/{id}` | `projects.delete` |
| POST | `/projects/{id}/status` | `projects.update` |
| POST | `/projects/{projectId}/milestones` | `projects.update` |
| POST | `/projects/{projectId}/tasks` | `projects.update` |
| POST | `/projects/{projectId}/updates` | `projects.update` |

Project visibility is restricted to staff (moderator/admin) and participants (the idea's creator and project members); everyone else receives `403`. Project creation is staff-driven: the caller must be a moderator/admin, the idea must be live, and `leadUserId` must reference an active entrepreneur or investor. `Completed → Archived` is final and only possible through `POST /projects/{id}/archive` (`projects.manage`); archived projects appear in `GET /projects/archive`.

### Reports — `/api/v1/reports`

| Method | Path | Policy |
| --- | --- | --- |
| GET | `` | `content.reports.read` |
| POST | `` | Auth |
| POST | `/{id}/resolve` | `content.reports.manage` |
| PUT | `/{id}` | `content.reports.manage` |
| DELETE | `/{id}` | `content.reports.manage` |

### Notifications — `/api/v1/notifications`

| Method | Path | Policy |
| --- | --- | --- |
| GET | `` | Auth |
| POST | `/{id}/read` | `notifications.manage` |
| POST | `/read-all` | `notifications.manage` |
| DELETE | `/{id}` | `notifications.delete` |
| GET | `/preferences` | Auth |
| PUT | `/preferences` | `notifications.manage` |

### Profiles — `/api/v1/profiles`

| Method | Path | Policy |
| --- | --- | --- |
| GET | `/me` | `profiles.read` |
| PUT | `/me` | `profiles.update` |
| POST | `/me/avatar` | `profiles.update` (write) |
| DELETE | `/me/avatar` | `profiles.update` |
| GET | `/creator/me` | `profiles.read` |
| PUT | `/creator/me` | `profiles.update` |
| DELETE | `/creator/me` | `profiles.update` |
| GET | `/creator/{userId}` | A |
| GET | `/entrepreneur/me` | `profiles.read` |
| PUT | `/entrepreneur/me` | `profiles.update` |
| GET | `/entrepreneur/pending` | `profiles.read` |
| POST | `/entrepreneur/{userId}/approve` | `profiles.update` |

### Users — `/api/v1/users`

| Method | Path | Policy |
| --- | --- | --- |
| GET | `` | `users.read` |
| GET | `/{id}` | `users.read` |
| POST | `` | `users.create` |
| PUT | `/{id}` | `users.update` |
| DELETE | `/{id}` | `users.delete` |
| POST | `/{id}/suspend` | `users.suspend` |
| POST | `/{id}/reactivate` | `users.suspend` |
| POST | `/{id}/roles` | `users.manage_roles` |
| DELETE | `/{id}/roles/{role}` | `users.manage_roles` |

### Roles & Permissions — `/api/v1`

| Method | Path | Policy |
| --- | --- | --- |
| GET | `/roles` | `roles.read` |
| GET | `/roles/{id}` | `roles.read` |
| GET | `/roles/{id}/users` | `roles.read` |
| POST | `/roles` | `roles.manage` |
| PUT | `/roles/{id}` | `roles.manage` |
| DELETE | `/roles/{id}` | `roles.manage` |
| POST | `/roles/{roleId}/permissions/{permissionId}` | `roles.manage` |
| DELETE | `/roles/{roleId}/permissions/{permissionId}` | `roles.manage` |
| GET | `/permissions` | `permissions.read` |
| GET | `/permissions/paged` | `permissions.read` |
| GET | `/permissions/{id}` | `permissions.read` |
| POST | `/permissions` | `permissions.manage` |
| PUT | `/permissions/{id}` | `permissions.manage` |
| DELETE | `/permissions/{id}` | `permissions.manage` |

### Platform — `/api/v1`

| Method | Path | Policy |
| --- | --- | --- |
| GET | `/settings` | `settings.read` |
| PUT | `/settings` | `settings.manage` |
| GET | `/audit-logs` | `audit.read` |
| GET | `/dashboard` | `dashboard.read` |
| GET | `/static-content` | `staticcontent.read` |
| POST | `/static-content` | `staticcontent.manage` |
| PUT | `/static-content/{id}` | `staticcontent.manage` |
| DELETE | `/static-content/{id}` | `staticcontent.manage` |
| GET | `/static-content/{key}` | A |

### Media & system

| Method | Path | Access |
| --- | --- | --- |
| GET | `/api/v1/media/{fileName}` | A — images only (`.webp/.png/.jpg/.jpeg`), immutable cache |
| GET | `/` | A — `{ name, status, version }` |
| GET | `/health/live` | A — liveness |
| GET | `/health/ready` | A — readiness (includes DB check) |
| WS | `/hubs/notifications` | Auth — SignalR hub |

---

## 3. Notes

- `204 No Content` is returned by delete-style operations that have no body.
- Enum values are serialized as **strings** (for example `"status": "Published"`) via `JsonStringEnumConverter`.
- Permission codes are lower-case dotted strings; role names are Pascal-case (`Administrator`, `Moderator`, `ScreeningReviewer`, …).
- Do not rely on client-side checks — the API is the authoritative authorization boundary.

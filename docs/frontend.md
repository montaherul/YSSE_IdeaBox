# Frontend

React 19 single-page application built with Vite, styled with Tailwind CSS 4, and organized **by feature**. Server state is handled by TanStack Query; authentication, UI, and locale state by Zustand.

Root: `frontend/`

---

## 1. Tech stack

| Concern | Library |
| --- | --- |
| Build | Vite 8 |
| UI | React 19.3, Tailwind CSS 4, Radix UI primitives, lucide-react, sonner |
| Routing | React Router 7 |
| Server state | TanStack Query 5 (`@tanstack/react-query`) |
| Client state | Zustand 5 |
| HTTP | Axios |
| Forms | React Hook Form 7 + Zod 4 (`@hookform/resolvers`) |
| Rich text | TipTap |
| Real-time | `@microsoft/signalr` |
| Lint | oxlint |
| Test | Vitest + Testing Library; Playwright for E2E |

---

## 2. Directory structure

```text
frontend/src/
├── app/          router.jsx, providers.jsx, query-client.js
├── layouts/      PublicLayout, WorkspaceLayout, ProtectedRoute, AdminRoute, Sidebar, Topbar
├── navigation/   navigation.config.js
├── features/     auth, ideas, reviews, screening, voting, problems, messaging,
│                 marketplace, outcomes, projects, admin, profiles, notifications, home, …
├── components/
│   ├── ui/       primitives (button, input, dialog, dropdown, async-select, data-table, …)
│   ├── shared/   composites (badges, empty-state, error-boundary, pagination, …)
│   └── errors/   error pages
├── services/     apiClient.js
├── hooks/        useAuth, usePermission, useFormat, useNotificationHub, useDebounce, …
├── store/        auth.store.js, ui.store.js, locale.store.js
├── config/       constants.js
└── lib/          format.js, validation.js, sanitize.js, media.js, table.js
```

Each feature folder typically contains `pages/`, `components/`, `api.js`, and `hooks.js`.

---

## 3. Routing

Routes are defined in `frontend/src/app/router.jsx` and wrapped in guards.

| Guard | Behavior |
| --- | --- |
| `ProtectedRoute` | Requires authentication; redirects to `/login` preserving the return path. |
| `AdminRoute` | Requires an admin-capable context (used for admin-only branches). |
| `canAccess({ roles, permissions })` | Union check — access if the user has **any** listed role **or** permission. |

Route gating uses `guard(<Element />, { roles, permissions })`. Examples: `/projects/new` requires `projects.create`; `/admin/*` requires admin roles/permissions.

### Main routes

| Path | Page | Access |
| --- | --- | --- |
| `/` | Home / landing | Public |
| `/login`, `/register`, `/forgot-password`, `/reset-password` | Auth pages | Public (redirect if signed in) |
| `/ideas` | Idea browse/search | Public |
| `/ideas/new` | Create idea (full or quick) | `ideas.create` |
| `/ideas/:slug` | Idea detail | Public |
| `/ideas/:id/edit` | Edit idea | `ideas.update` |
| `/my-ideas` | Current user's ideas | Authenticated |
| `/problems`, `/problems/:id` | Problems | Public / `problems.create` to post |
| `/leaderboards`, `/voting` | Leaderboards & voting | Public |
| `/marketplace`, `/marketplace/:id`, `/marketplace/new` | Marketplace | Public / `marketplace.create` |
| `/messages` | Messaging | Authenticated |
| `/notifications` | Notifications | Authenticated |
| `/profile`, `/profile/settings` | Own profile | Authenticated |
| `/creators/:id` | Public creator profile | Public |
| `/entrepreneur` | Entrepreneur dashboard | Entrepreneur |
| `/projects`, `/projects/new`, `/projects/:id` | Projects | Public list / `projects.create` |
| `/screening` | Screening queue | Screening access |
| `/admin/*` | Administration dashboards | Admin roles/permissions |

`navigation/navigation.config.js` drives the sidebar and applies the same role/permission gating, so navigation and routes stay consistent.

---

## 4. Data & state

### Server state — TanStack Query
- Query keys are per feature (for example `['ideas', filters]`, `['idea', slug]`).
- Mutations invalidate the relevant keys so listings refresh automatically.
- Paginated hooks use the paged unwrapping from `apiClient` (`{ items, page, pageSize, total, hasNextPage }`).
- Request cancellation, retry, and stale-time behavior come from the shared `query-client.js`.
- To prevent stale responses overwriting newer results (for example fast tabular typing), hooks use request tokens / cancel previous queries.

### Client state — Zustand
| Store | Holds |
| --- | --- |
| `auth.store.js` | Access/refresh tokens, current user, roles, permissions, sign-in/out, `setSession`. |
| `ui.store.js` | Theme, sidebar collapsed state, transient UI preferences. |
| `locale.store.js` | Current language (`en`/`bn`), persisted preference. |

Tokens are persisted so sessions survive reload; the API client refreshes them on `401`.

---

## 5. HTTP client

`services/apiClient.js` is the single Axios instance:

- Base URL from `VITE_API_URL` (default `http://localhost:5080/api/v1`).
- Attaches `Authorization: Bearer <accessToken>`.
- **Unwraps the response envelope** so callers receive `data` directly.
- Normalizes paged responses and unwraps BusinessException-style `{ error }` payloads.
- On `401`, performs a single refresh-token attempt and retries once; on failure, clears the session and routes to login.
- Exposes helpers such as `api.get`, `api.post`, `api.put`, `api.patch`, `api.delete`, `api.getPaged`.

---

## 6. Permissions on the client

`hooks/usePermission.js` exposes:

```js
const { can, canPermission, hasRole, canAccess } = usePermission();
```

- `can(role)` / `hasRole(role)` — role check.
- `canPermission(code)` — permission check against the server-provided permission set.
- `canAccess({ roles, permissions })` — union check used by routes/navigation.

**Client checks are UX only.** Every action is re-verified server-side; hiding a button is never authorization.

---

## 7. Forms & validation

- Forms use **React Hook Form** with **Zod** resolvers; field-level errors render beside inputs.
- Server validation is authoritative — client validation mirrors it for responsiveness.
- Buttons disable while submitting to prevent duplicate submissions; loading and empty states are explicit.
- Rich text (idea descriptions) uses TipTap and is sanitized before rendering.

---

## 8. Accessibility & responsive design

- Target: **WCAG 2.2 AA**.
- Semantic HTML, labeled controls, visible focus indicators, keyboard operability (Tab/Enter/Space/Escape), and `aria-live` announcements for validation/status.
- Status is never conveyed by color alone.
- Layouts are responsive across mobile, tablet, laptop, and desktop with no horizontal overflow; below-the-fold content is lazy-loaded and images compressed for low-bandwidth users.
- UI text is localization-ready for **English and Bangla**.

---

## 9. Real-time

`hooks/useNotificationHub` connects to the SignalR `/hubs/notifications` hub with the bearer token, subscribes to notification events, updates the notifications query cache, and toasts new items. It reconnects on transient failures.

---

## 10. Scripts

Run from `frontend/`:

| Command | Purpose |
| --- | --- |
| `npm run dev` | Vite dev server (`http://localhost:5173`). |
| `npm run build` | Production build to `dist/`. |
| `npm run preview` | Serve the production build locally. |
| `npm run lint` | oxlint over `src`. |
| `npm run test` | Vitest (unit + integration). |
| `npm run test:coverage` | Vitest with coverage. |
| `npm run test:e2e` | Playwright E2E (`npx playwright install` first). |

---

## See also

- [Architecture](architecture.md) · [Features & Lifecycle](features.md) · [API Reference](api-reference.md)

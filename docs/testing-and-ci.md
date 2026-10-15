# Testing & CI

---

## 1. Strategy

| Level | Backend | Frontend |
| --- | --- | --- |
| Unit | xUnit — domain rules, services, validators, status machine | Vitest — `lib/` helpers (format, validation, utils) |
| Integration | xUnit with in-memory EF Core (`TestDb`) | Vitest + Testing Library — components, hooks, form wizard |
| End-to-end | Controller flows (where applicable) | Playwright — public routes and critical flows |
| Load / security / a11y | Manual / planned | Manual / planned (WCAG AA target) |

Tests assert behavior and security (authorization, validation, lifecycle transitions), not just happy paths.

---

## 2. Backend tests

Location: `backend/tests/YSSE.IdeaBox.Tests` (xUnit). Run from the repository root:

```powershell
dotnet test backend/YSSE.IdeaBox.slnx
```

Common options:

```powershell
# release, no rebuild
dotnet test backend/YSSE.IdeaBox.slnx -c Release --no-build
```

### Test infrastructure
- `TestDb` — creates an `AppDbContext` backed by the **EF Core in-memory provider** with a unique store, plus a `UnitOfWork` and test doubles.
- `TestDoubles` — fakes for time, current user, tokens, email, storage, notifiers, etc.

### Suites

| File | Covers |
| --- | --- |
| `IdeaStatusMachineTests` | Full transition matrix (legal/illegal transitions). |
| `IdeaServiceTests` | Idea create/update/submit/withdraw, validation, tags, revisions. |
| `VotingServiceTests` | Vote eligibility, uniqueness, toggling, week/period rules. |
| `ReviewServiceTests` | Registered + guest reviews, moderation. |
| `ProblemServiceTests` | Problem posts, endorsements, status, linking. |
| `MarketplaceServiceTests` | Listings, status transitions, reservations. |
| `PermissionAuthorizationHandlerTests` | Permission policy evaluation. |
| `PermissionsTests` | Permission catalog integrity. |
| `UserPermissionServiceTests` | Effective-permission resolution and caching. |
| `PaginationTests` | `Pagination.Normalize` bounds (default 20, max 100). |
| `MatchingScorerTests` | Idea↔opportunity-seeker scoring. |
| `LeaderboardScoringTests` | Leaderboard score computation. |
| `DbInitializerSeedingTests` | Role/permission seeding and defaults. |

The suite currently totals **152 tests**, all passing.

---

## 3. Frontend tests

Location: `frontend/tests`. Configuration: Vitest (jsdom) + Testing Library; E2E via Playwright.

```powershell
cd frontend
npm run test            # unit + integration (Vitest, run once)
npm run test:watch      # watch mode
npm run test:coverage   # coverage report in tests/coverage
npm run test:e2e        # Playwright (install browsers first: npx playwright install)
```

### Suites

| File | Covers |
| --- | --- |
| `tests/unit/format.test.js` | Date/number/text formatting helpers. |
| `tests/unit/validation.test.js` | Zod schema validation. |
| `tests/unit/utils.test.js` | General `lib/` utilities. |
| `tests/integration/stepper.test.jsx` | Multi-step stepper component. |
| `tests/integration/shared-components.test.jsx` | Shared UI components. |
| `tests/integration/idea-form-wizard.test.jsx` | Idea creation form flow. |
| `tests/integration/auth-hooks.test.jsx` | Auth hooks. |
| `tests/e2e/public-routes.spec.js` | Playwright — public route smoke tests. |

`tests/setup.js` installs `@testing-library/jest-dom` and jsdom globals.

### Lint
`npm run lint` runs **oxlint** over `src`. Widespread `react-in-jsx-scope` warnings are pre-existing noise (the React 17+ JSX transform makes the import unnecessary); they are warnings, not errors.

---

## 4. Continuous integration

Two GitHub Actions workflows, path-filtered so each runs only when its side changes.

### Backend CI — `.github/workflows/backend-ci.yml`

- Trigger: `backend/**` or the workflow file (push + pull request).
- Steps: checkout → `setup-dotnet 10.0.x` → `dotnet restore` → `dotnet build -c Release` → `dotnet test -c Release` (TRX logger) → upload `backend-test-results` artifact.
- Working directory: `backend`.

### Frontend CI — `.github/workflows/frontend-ci.yml`

Two jobs:
1. **Lint, test and build** — `npm ci` → `npm run lint` → `npm run test` → `npm run build`.
2. **End-to-end (Playwright)** — `npm ci` → install Chromium → `npm run test:e2e` → upload `playwright-report`.

- Trigger: `frontend/**` or the workflow file. Working directory: `frontend`. Node 22 with npm cache.

### Note on .NET version
The backend workflow installs the **.NET 10.0.x** SDK while the projects target `net8.0`. This builds and tests `net8.0` successfully; align the SDK version if you prefer strict version parity.

---

## 5. What to run before opening a pull request

```powershell
# backend
dotnet build backend/YSSE.IdeaBox.slnx --no-restore
dotnet test backend/YSSE.IdeaBox.slnx

# frontend
cd frontend
npm run lint
npm run test
npm run build
```

Fix any failure before submitting. If a check cannot be run locally, say so explicitly — never report an unverified result as passing.

---

## See also

- [Getting Started](getting-started.md) · [Contributing](contributing.md) · [Architecture](architecture.md)

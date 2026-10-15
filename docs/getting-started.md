# Getting Started

This guide takes you from a clean checkout to a running backend and frontend.

---

## 1. Prerequisites

| Tool | Version | Notes |
| --- | --- | --- |
| .NET SDK | 8.0 | All projects target `net8.0`. The CI workflow installs the 10.0.x SDK; either works for building `net8.0`. |
| PostgreSQL | 14+ | The API uses Npgsql and snake_case naming conventions. |
| Node.js | 20+ (CI uses 22) | For the React SPA. |
| npm | 10+ | Bundled with Node. |
| Git | any | — |

Optional: Redis (SignalR scale-out + distributed cache), an SMTP server (email), an HTTP SMS gateway.

---

## 2. Configuration

The API reads configuration from `appsettings.json`, then environment-specific files, then environment variables / user-secrets. **Do not commit real secrets.**

### Backend (`backend/src/YSSE.IdeaBox.Api`)

Create `appsettings.Development.json` by copying `appsettings.Development.example.json`:

```jsonc
{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=ysse_ideabox;Username=postgres;Password=REPLACE_ME"
  },
  "Jwt": {
    "Key": "REPLACE_WITH_A_RANDOM_STRING_OF_AT_LEAST_32_CHARACTERS"
  },
  "Email": { "Enabled": false },
  "Sms": { "Enabled": false },
  "Redis": { "ConnectionString": "" }
}
```

`appsettings.Development.json` is git-ignored on purpose.

| Setting | Section | Purpose |
| --- | --- | --- |
| `ConnectionStrings:Default` | ConnectionStrings | PostgreSQL connection string. |
| `Jwt:Key` | Jwt | HS256 signing key. **Required**; must be ≥ 32 chars outside Development. |
| `Jwt:Issuer` / `Jwt:Audience` | Jwt | Default `YSSE.IdeaBox` / `YSSE.IdeaBox.Client`. |
| `Jwt:AccessTokenMinutes` / `Jwt:RefreshTokenDays` | Jwt | Default 60 / 14. |
| `Cors:Origins` | Cors | Allowed SPA origins (default `http://localhost:5173`). |
| `Storage:Root` | Storage | Upload root (default `storage/uploads`). |
| `Email:*` | Email | SMTP. Disabled/no-op when `Enabled=false`. |
| `Sms:*` | Sms | HTTP SMS gateway. Disabled/no-op when `Enabled=false`. |
| `Redis:ConnectionString` | Redis | Optional. Enables SignalR backplane + distributed cache. |
| `Guest:ReviewCookieDays` | Guest | Lifetime of the guest-review cookie (default 365). |

You can also use user-secrets or environment variables (double-underscore: `Jwt__Key`, `ConnectionStrings__Default`).

### Frontend (`frontend`)

```powershell
copy .env.example .env
```

| Variable | Default | Purpose |
| --- | --- | --- |
| `VITE_API_URL` | `http://localhost:5080/api/v1` | Base URL of the API, including the `/api/v1` prefix. |

The frontend defaults to this value even if `.env` is absent, so it only needs changing for other hosts/ports.

---

## 3. Database

Create an empty database (the API applies migrations and seeds reference data on startup in Development):

```sql
CREATE DATABASE ysse_ideabox;
```

The API connects with the `ConnectionStrings:Default` value. In Development, `DbInitializer` runs `MigrateAsync()` and then seeds roles, permissions, platform settings, demo users, categories, tags, and sample content.

---

## 4. Run the backend

```powershell
dotnet restore backend/YSSE.IdeaBox.slnx
dotnet run --project backend/src/YSSE.IdeaBox.Api
```

- API root: `http://localhost:5080/`
- Swagger UI (Development): `http://localhost:5080/swagger`
- Health: `http://localhost:5080/health/live` and `http://localhost:5080/health/ready`
- SignalR hub: `http://localhost:5080/hubs/notifications`

Launch profiles (`backend/src/YSSE.IdeaBox.Api/Properties/launchSettings.json`): `http` → `http://localhost:5080`; `https` → `https://localhost:7080;http://localhost:5080`.

---

## 5. Run the frontend

```powershell
cd frontend
npm install
npm run dev
```

The SPA runs at `http://localhost:5173`. The API's default CORS policy already trusts this origin.

Useful scripts (run from `frontend/`):

| Command | Purpose |
| --- | --- |
| `npm run dev` | Start the Vite dev server. |
| `npm run build` | Production build to `dist/`. |
| `npm run preview` | Preview the production build. |
| `npm run lint` | Run oxlint over `src`. |
| `npm run test` | Run unit + integration tests (Vitest). |
| `npm run test:coverage` | Tests with coverage (`tests/coverage`). |
| `npm run test:e2e` | Playwright E2E tests (run `npx playwright install` first). |

---

## 6. Log in (development)

The Development seed creates the accounts listed in the [README](../README.md#demo-accounts-development-only). Sign in at `http://localhost:5173/login`, for example `admin@ysse.local` / `Admin@123`.

---

## 7. Troubleshooting

| Symptom | Likely cause / fix |
| --- | --- |
| API fails at startup: `Jwt:Key is required` | Missing `Jwt:Key` in config. Add a ≥ 32-char key. |
| `Npgsql` connection refused | PostgreSQL not running, wrong host/port, or database missing. |
| CORS error in the browser | The SPA origin is not in `Cors:Origins`. |
| `dotnet build` fails with file-in-use on Windows | A running API process is locking `bin/`. Stop it (`Get-Process dotnet \| Stop-Process`) and rebuild. |
| 429 responses in development | Rate-limit policies (`auth` 10/min, `write` 30/min, `guest-review` 3/hour, `messaging` 60/min, global 120/min). Wait or adjust. |
| Emails never arrive | `Email:Enabled` is false or SMTP settings are empty; the sender degrades to a logged no-op. |
| Real-time notifications not received | SignalR requires an authenticated JWT; check the hub URL and network/CORS. |

---

## See also

- [Architecture](architecture.md)
- [Configuration reference is embedded above and in Backend](backend.md)
- [Testing & CI](testing-and-ci.md)

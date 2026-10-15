# YSSE Idea Box

**A community platform that turns social-impact ideas into reviewed, published, and fundable ventures.**

YSSE Idea Box lets anyone submit an idea for social good, gives reviewers a structured screening and publication workflow, and connects creators with entrepreneurs, investors, and mentors through reviews, voting, messaging, inquiries, outcomes, and projects.

- **Backend:** ASP.NET Core 8 Web API, Clean Architecture, EF Core + PostgreSQL (Dapper for reporting reads)
- **Frontend:** React 19 SPA (Vite, Tailwind CSS 4, TanStack Query, React Hook Form + Zod)
- **Auth:** JWT access/refresh tokens with database-backed, permission-based authorization (RBAC)

[![Backend CI](https://github.com/montaherul/YSSE_IdeaBox/actions/workflows/backend-ci.yml/badge.svg)](https://github.com/montaherul/YSSE_IdeaBox/actions/workflows/backend-ci.yml)
[![Frontend CI](https://github.com/montaherul/YSSE_IdeaBox/actions/workflows/frontend-ci.yml/badge.svg)](https://github.com/montaherul/YSSE_IdeaBox/actions/workflows/frontend-ci.yml)

---

## Table of contents

- [Core capabilities](#core-capabilities)
- [Tech stack](#tech-stack)
- [Repository layout](#repository-layout)
- [Architecture at a glance](#architecture-at-a-glance)
- [Quick start](#quick-start)
- [Demo accounts (development only)](#demo-accounts-development-only)
- [Documentation](#documentation)
- [Project status](#project-status)

---

## Core capabilities

| Area | What it does |
| --- | --- |
| **Ideas** | Create/edit (full or quick), submit, withdraw, soft-delete drafts, revision history, tags, attachments, view counts, similar-idea suggestions. |
| **Screening & publication** | Reviewer queue, per-idea decisions (publish / reject / request revision), immutable status history, status machine with an explicit transition matrix. |
| **Reviews & comments** | Registered and guest reviews (guest cookie + rate limit), ratings, review moderation, threaded comments, idea follows. |
| **Voting & leaderboards** | Votes with configurable visibility, voting periods (activate/close), monthly and all-time leaderboards for ideas and idea-makers (Dapper aggregation). |
| **Problems** | Problem statements, endorsements, idea linking, moderation, status lifecycle. |
| **Entrepreneurship** | Entrepreneur/investor/mentor profiles, inquiries (received/sent/respond), watchlists, matching scores. |
| **Messaging** | 1:1 conversations, messages with attachments, read state, user blocking, reporting. |
| **Outcomes** | Verified collaboration/investment outcomes feeding credibility and leaderboards. |
| **Projects** | From an approved idea: projects, members, milestones, tasks, and updates. |
| **Marketplace** | Listings (sale / donation / exchange), status workflow, reservations and responses. |
| **Notifications** | In-app + email notifications, preferences, SignalR real-time delivery. |
| **Administration** | Users, roles, permissions, categories, tags, static content, voting sessions, featured ideas, moderation reports, audit log, platform settings, dashboard metrics. |

---

## Tech stack

| Layer | Technology |
| --- | --- |
| API | ASP.NET Core 8 (`net8.0`), controllers, custom exception middleware, Swagger/OpenAPI |
| Application | Service layer, FluentValidation validators, DTOs, `Result`/`PagedResult` |
| Domain | Entities + enums, no framework dependencies |
| Persistence | EF Core 8 + Npgsql, snake_case naming, Dapper for analytical reads |
| Database | PostgreSQL |
| Auth | JWT bearer (HS256), refresh tokens, permission-based authorization |
| Real-time | SignalR (`/hubs/notifications`), optional Redis backplane |
| Email / SMS | SMTP sender, generic HTTP SMS gateway (both degrade to no-op when unconfigured) |
| Frontend | React 19, Vite 8, React Router 7, Tailwind CSS 4 |
| Client data/state | TanStack Query 5, Zustand 5, Axios, React Hook Form + Zod |
| UI | Radix UI primitives, lucide-react icons, sonner toasts, TipTap rich text |
| Testing | xUnit (.NET), Vitest + Testing Library, Playwright (E2E) |
| CI | GitHub Actions (backend + frontend workflows) |

---

## Repository layout

```text
Idea_BOX/
├── backend/
│   ├── YSSE.IdeaBox.slnx                 # solution (5 projects)
│   ├── src/
│   │   ├── YSSE.IdeaBox.Domain/          # entities, enums, base types
│   │   ├── YSSE.IdeaBox.Application/     # services, DTOs, validators, abstractions
│   │   ├── YSSE.IdeaBox.Infrastructure/  # EF Core, repositories, integrations
│   │   └── YSSE.IdeaBox.Api/             # controllers, middleware, auth, composition
│   └── tests/
│       └── YSSE.IdeaBox.Tests/           # xUnit test suite
├── frontend/
│   ├── src/
│   │   ├── app/                          # router, providers, query client
│   │   ├── layouts/                      # PublicLayout, WorkspaceLayout, guards
│   │   ├── features/                     # feature modules (ideas, reviews, admin, …)
│   │   ├── components/                   # ui primitives + shared components
│   │   ├── services/                     # axios client + envelope handling
│   │   ├── hooks/                        # auth, permissions, formatting
│   │   ├── store/                        # Zustand stores (auth, ui, locale)
│   │   ├── navigation/                   # sidebar/navigation config
│   │   └── lib/                          # format, validation, sanitize, media helpers
│   └── tests/                            # unit, integration, e2e
├── docs/                                 # project documentation (this set)
├── .github/workflows/                    # backend-ci.yml, frontend-ci.yml
└── README.md
```

---

## Architecture at a glance

```text
 React SPA (Vite)
      │  HTTPS / JSON  (+ SignalR WebSocket)
      ▼
 ASP.NET Core API
      │  middleware: correlation → exception → logging → CORS → rate limit
      │              → timeouts → authentication → authorization
      ▼
 Controllers  ──►  Application services  ──►  Infrastructure (EF Core / Dapper)
 (HTTP only)       (business rules)            (PostgreSQL, SMTP, SMS, storage)
      ▲
      └── permission-based authorization resolved from the database per request
```

Dependency direction is strict: **Domain ← Application ← { Infrastructure, Api }**. See [docs/architecture.md](docs/architecture.md) for the full request lifecycle.

---

## Quick start

> Full instructions (prerequisites, configuration, environment variables, troubleshooting) are in **[docs/getting-started.md](docs/getting-started.md)**.

**Backend**

```powershell
# from the repository root
# 1. Create appsettings.Development.json from the example and set the DB + JWT key
#    backend/src/YSSE.IdeaBox.Api/appsettings.Development.example.json
# 2. Ensure PostgreSQL is running with database "ysse_ideabox"
dotnet restore backend/YSSE.IdeaBox.slnx
dotnet run --project backend/src/YSSE.IdeaBox.Api
# API + Swagger: http://localhost:5080/swagger
```

In Development the API migrations and seeds the database automatically on startup.

**Frontend**

```powershell
cd frontend
npm install
# copy .env.example to .env if the API is not on http://localhost:5080
npm run dev
# SPA: http://localhost:5173
```

---

## Demo accounts (development only)

Seeded automatically in the Development environment by `DbInitializer`. Passwords follow the `Role@123` pattern (for example `Admin@123`).

| Email | Role(s) |
| --- | --- |
| `admin@ysse.local` | Administrator + Moderator |
| `creator@ysse.local` | Creator |
| `member@ysse.local` | Member |
| `investor@ysse.local` | Investor |
| `entrepreneur@ysse.local` | Entrepreneur |
| `moderator@ysse.local` | Moderator |

> These accounts exist only when `ASPNETCORE_ENVIRONMENT=Development`. Never seed or enable them in production.

---

## Documentation

The full project documentation lives in [`docs/`](docs/).

| Document | Description |
| --- | --- |
| [Getting Started](docs/getting-started.md) | Prerequisites, configuration, environment variables, running the API and SPA, troubleshooting. |
| [Architecture](docs/architecture.md) | Clean Architecture, backend request lifecycle, project structure, frontend architecture. |
| [Backend](docs/backend.md) | Domain model, enums, application services, controllers, authorization model, data access. |
| [Frontend](docs/frontend.md) | Feature modules, routing, state management, API client, permissions, shared UI. |
| [API Reference](docs/api-reference.md) | Response envelope, error format, and every endpoint grouped by module. |
| [Database](docs/database.md) | Entities, EF Core configuration, migrations, seeding, indexes and constraints. |
| [Security](docs/security.md) | Authentication, authorization (RBAC), rate limiting, uploads, secrets handling. |
| [Features & Lifecycle](docs/features.md) | Functional modules, the idea status lifecycle, notifications, and business rules. |
| [Testing & CI](docs/testing-and-ci.md) | Test strategy, how to run all suites, GitHub Actions pipelines. |
| [Contributing](docs/contributing.md) | Coding conventions, layering rules, and contribution workflow. |
| [Architecture Audit](docs/architecture-audit.md) | Historical audit and refactoring record (AdminService decomposition). |

**Local-only references** (intentionally kept out of source control, see `.gitignore`): `AGENTS.md`, `rule.md`, `module.md`, `IMPLEMENTATION_SUMMARY.md`, and `YSSE Idea Box - Software Requirements Specification v3.1.md`.

---

## Project status

The platform is under active development. The backend and frontend both build and their test suites pass in CI. Core modules (ideas, screening, reviews, voting, messaging, inquiries, outcomes, projects, marketplace, administration) are implemented; some reporting, search, and scale-out capabilities described in the requirements are planned. See [docs/features.md](docs/features.md) for the functional breakdown.

---

## Attribution

YSSE Idea Box facilitates introductions between creators and supporters. It does **not** guarantee investment, verify every business claim, or manage third-party funding.

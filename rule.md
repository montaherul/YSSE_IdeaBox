For your YSSE Idea Box project, you need more than Clean Architecture. You should define a complete set of architecture, scalability, security, performance, accessibility, and production-readiness rules so another AI developer can implement the system consistently.

Your existing stack is React + Vite, ASP.NET Core Web API, Clean Architecture, EF Core/Dapper, and PostgreSQL. I recommend keeping that stack and adding the following engineering standards.

# 1. Core architecture rules

Clean Architecture

Separate business rules, use cases, database operations, and HTTP endpoints.

- Domain must not depend on EF Core, PostgreSQL, or the API.
- Application contains use cases, validation, and abstractions.
- Infrastructure implements persistence and external services.
- API handles HTTP, authentication, middleware, and dependency injection.

Modular monolith

Start with one deployable backend, divided into modules such as Ideas, Reviews, Votes, Messaging, Marketplace, Identity, and Administration. Avoid microservices until independent scaling or deployment justifies them.

Single source of truth

PostgreSQL is the authoritative database. The frontend never accesses it directly. All permissions and business rules are enforced by the backend, even when the UI also validates them.

Secure by default

Deny access unless permission is explicitly granted. Check ownership and tenant or organizational scope on every protected operation; hiding a button is not authorization.

# 2. Load control and load balancing

These are related, but they solve different problems.

- Load control: prevents the application from accepting more work than it can safely handle.
- Load balancing: distributes requests across multiple backend instances.
- Auto-scaling: adds or removes instances according to demand.
- Caching: avoids repeating expensive work.
- Background processing: moves slow, noninteractive work out of the request-response cycle.

## Recommended scaling architecture

Users and clients

React web app · Mobile browser · Admin portal

CDN / WAF / Reverse proxy

Static assets · TLS · Request filtering · Edge rate limits

Load balancer

Health checks · Distribute API traffic

API 1

API 2

API N

PostgreSQL

Shared authoritative data

Redis / queue

Cache, coordination, jobs

Target production architecture; Redis and multiple API instances are optional additions, not local-development requirements.

### Mandatory load-management rules

| Area                 | Rule for Idea Box                                                                                                                      |
| -------------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| Request limits       | Set a maximum request body size, request timeout, and configurable rate limits.                                                        |
| Rate limiting        | Apply stricter limits to anonymous reviews, login, OTP/email operations, messaging, and idea submission.                               |
| Concurrency          | Limit concurrent expensive reports and external API calls.                                                                             |
| Database connections | Use pooled connections, set appropriate command timeouts, and avoid unbounded parallel queries.                                        |
| Pagination           | Default to 20 items per page; enforce a configurable maximum such as 100.                                                              |
| Heavy tasks          | Process email, image processing, notifications, exports, and scheduled jobs in background workers.                                     |
| Backpressure         | Return a controlled `429 Too Many Requests` or `503 Service Unavailable` when appropriate; do not allow queues to grow without limits. |
| Caching              | Cache public, frequently read data with explicit expiry and invalidation rules.                                                        |
| Health checks        | Separate liveness checks from readiness checks, including database readiness where appropriate.                                        |
| Graceful shutdown    | Stop accepting new work, allow in-flight requests to finish within a deadline, and avoid losing queued jobs.                           |

Important: a load balancer does not make a slow database query fast. Optimize queries, indexes, connection usage, and database capacity before adding more API instances.

# 3. Accessibility and easy access

For Idea Box, make the platform usable on low-cost Android phones, slower internet connections, keyboards, and assistive technologies. Use WCAG 2.2 Level AA as the accessibility target.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://www.w3.org\&sz=32)

W3C

+1



| Rule                | Implementation requirement                                                                            |
| ------------------- | ----------------------------------------------------------------------------------------------------- |
| Responsive UI       | Support mobile, tablet, laptop, and desktop without horizontal overflow.                              |
| Keyboard navigation | All interactive features work with Tab, Enter, Space, and Escape where appropriate.                   |
| Screen readers      | Use semantic HTML, accessible labels, and meaningful error announcements.                             |
| Contrast            | Meet WCAG AA text-contrast requirements. Never communicate status using color alone.                  |
| Forms               | Show labels, required fields, validation messages, and errors beside the relevant fields.             |
| Loading states      | Show skeletons or progress indicators for slow requests. Prevent accidental duplicate submissions.    |
| Empty states        | Explain what the user can do when there are no ideas, messages, or marketplace listings.              |
| Low bandwidth       | Compress images, lazy-load below-the-fold content, and avoid unnecessary JavaScript.                  |
| Localization        | Keep UI text ready for Bangla and English; support appropriate fonts and text expansion.              |
| Navigation          | Maintain consistent menus, breadcrumbs where useful, clear page titles, and visible focus indicators. |

Usability rule: a user should be able to discover an idea, understand its status, submit a proposal, and contact an entrepreneur without needing technical knowledge.

# 4. Frontend architecture rules

For your React + Vite + JavaScript/JSX application:

- Organize code by feature, not by one enormous collection of generic components.
- Keep pages, reusable UI components, API clients, hooks, validation schemas, and state management separate.
- Use TanStack Query for server state, caching, retries, and invalidation.
- Use Zustand only for appropriate client-side global state, such as UI preferences or authentication state.
- Use React Hook Form and Zod for form handling and client-side validation; repeat authoritative validation on the backend.
- Use shared design tokens and reusable components to maintain consistency.
- Lazy-load large routes such as the admin dashboard, analytics, and marketplace.
- Avoid putting every API response in global state.
- Never store passwords, API secrets, or sensitive private messages in browser storage unnecessarily.
- Centralize API error handling, authentication expiry handling, and loading behavior.

Suggested feature structure:

```
frontend/src/
├── app/
│   ├── router.jsx
│   ├── providers.jsx
│   └── query-client.js
├── components/
│   ├── ui/
│   ├── layout/
│   └── shared/
├── features/
│   ├── auth/
│   ├── ideas/
│   ├── reviews/
│   ├── voting/
│   ├── messaging/
│   ├── marketplace/
│   └── admin/
├── lib/
│   ├── api-client.js
│   └── validation.js
├── hooks/
└── styles/
```

# 5. Backend and API rules

For ASP.NET Core, establish these rules across all modules.

1. Version the API: use `/api/v1/...`; introduce breaking changes through a planned version migration.
2. Use DTOs: never expose EF Core entities directly as API responses.
3. Validate at boundaries: validate input lengths, enums, IDs, file metadata, and request payloads.
4. Use asynchronous I/O: prefer `async` database and network operations with cancellation tokens.
5. Use consistent responses: standardize pagination, validation errors, not-found responses, and unexpected errors.
6. Use global exception handling: return safe error messages and a trace/correlation ID; never expose stack traces in production.
7. Apply authorization policies: check roles, permissions, ownership, and resource access server-side.
8. Avoid over-fetching: project only the columns needed by each endpoint.
9. Control query behavior: whitelist sorting columns, limit page sizes, and use appropriate indexes.
10. Make retries safe: use idempotency keys where duplicate submissions could cause harmful side effects.

For example, a paginated ideas endpoint should return a predictable structure:

```
{
  "data": [],
  "page": 1,
  "pageSize": 20,
  "total": 2450,
  "hasNextPage": true
}
```

For deep pages or continuously updated feeds, consider cursor/keyset pagination instead of increasingly expensive offset pagination.

# 6. Database and data-integrity rules

PostgreSQL must enforce important invariants, not merely rely on application code.

- Add foreign keys, unique constraints, check constraints, and appropriate indexes.
- Use database transactions for operations that must succeed or fail together.
- Enforce one active vote per eligible user per idea with a database uniqueness constraint.
- Prevent duplicate reviews according to your defined guest and registered-user policies.
- Use optimistic concurrency or equivalent protection for conflicting edits.
- Index common filters and sorting columns, based on real query plans.
- Avoid N+1 queries and unnecessary eager loading.
- Use EF Core for ordinary operations; use Dapper for justified complex read/reporting queries.
- Use stored procedures or functions only where they provide a measurable benefit.
- Apply migrations through a controlled deployment process; do not automatically run destructive schema changes on every production startup.
- Back up the database and test restoration, not just backup creation.
- Define data retention and deletion rules for accounts, private messages, reports, and audit records.

# 7. Security and privacy rules

Use the OWASP Application Security Verification Standard as a verification checklist for access control, input validation, authentication, logging, and other security controls.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://cheatsheetseries.owasp.org\&sz=32)

OWASP Cheat Sheet Series

+1



| Area              | Required rule                                                                                                           |
| ----------------- | ----------------------------------------------------------------------------------------------------------------------- |
| Authentication    | Use a secure session or token strategy, with expiration and revocation controls.                                        |
| Authorization     | Enforce least privilege; do not trust frontend role claims or submitted user IDs.                                       |
| Passwords         | Hash passwords with a suitable password-hashing algorithm; never store plaintext passwords.                             |
| Secrets           | Store connection strings and signing keys in secrets/configuration services, not source control.                        |
| Uploads           | Validate file size, type, content, and access permissions; generate safe storage names.                                 |
| Messaging         | Only conversation participants can read messages. Add blocking, reporting, and abuse controls.                          |
| Anonymous reviews | Apply server-side guest tracking and rate limits; do not claim perfect identity verification.                           |
| Votes             | Enforce eligibility and uniqueness on the server and database.                                                          |
| Audit logs        | Record relevant security and administrative events, but never log passwords, tokens, or message contents unnecessarily. |
| Transport         | Use HTTPS in production and protect cookies and authentication credentials.                                             |
| Admin accounts    | Apply stronger authentication, least privilege, and audit trails for sensitive actions.                                 |

YSSE must also clearly explain that it facilitates introductions but does not guarantee investment, verify every business claim, or manage third-party funding.

# 8. Caching, real-time features, and background jobs

Use these tools only where the actual requirement justifies them.

Caching

Use HTTP/browser caching or ASP.NET Core output caching for appropriate public content. Add Redis when shared caching or distributed coordination is needed across multiple API instances. Never publicly cache private user-specific responses.

Real-time messaging

Use ASP.NET Core SignalR for live messages and notifications if real-time behavior is required. Persist messages in PostgreSQL first, then notify clients. With multiple API instances, configure a supported scale-out mechanism and shared connection coordination.

Background jobs

Move emails, bulk notifications, large exports, image processing, and scheduled cleanup into background workers. Use a durable queue when losing a job would be unacceptable, with retries, dead-letter handling, and idempotent processing.

Search and discovery

Begin with PostgreSQL full-text search and suitable indexes. Consider a dedicated search engine only when measured relevance, filtering, or query volume exceeds what PostgreSQL can handle well.

# 9. Observability and reliability

A production system should let you identify what failed, where it failed, and how much of the service was affected.

- Use structured logs with Serilog and correlation IDs.

- Collect request latency, throughput, error rate, database latency, queue depth, and resource usage.

- Track p50, p95, and p99 API latency instead of looking only at averages.

- Add liveness and readiness health endpoints. ASP.NET Core health checks can support load-balancer routing and deployment decisions.&#x20;

  [image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

  Microsoft Learn

  +1

- Use distributed tracing when requests cross services or background workers.

- Alert on sustained failures, exhausted connection pools, growing queues, and backup failures.

- Define service-level objectives (SLOs) before deciding whether to add infrastructure.

- Use graceful degradation: if recommendations or analytics fail, basic idea browsing should continue when safely possible.

# 10. Testing and deployment rules

| Level               | What must be tested                                                                                     |
| ------------------- | ------------------------------------------------------------------------------------------------------- |
| Unit tests          | Domain rules, validation, voting eligibility, permissions, and business calculations.                   |
| Integration tests   | PostgreSQL constraints, EF Core queries, transactions, and API authentication.                          |
| End-to-end tests    | Idea submission, review, voting, entrepreneur inquiry, and admin moderation.                            |
| Load tests          | Concurrent browsing, searching, submissions, and messaging under realistic workloads.                   |
| Security tests      | Unauthorized access, IDOR, injection, brute-force protection, and upload validation.                    |
| Accessibility tests | Keyboard navigation, focus order, contrast, labels, and screen-reader behavior.                         |
| Deployment tests    | Migration safety, health checks, rollback procedure, backup restoration, and environment configuration. |

For deployment, use separate development, test, and production configuration; pin and update dependencies deliberately; run CI checks before merging; and use a staged rollout when the deployment platform supports it.

# 11. What to implement first

For your project, do not deploy a load balancer, Redis, multiple API instances, and a message broker immediately. First build the architecture so these can be added without rewriting business logic.

## Architecture implementation checklist

21/21

Stage 1 — Foundation

Clean Architecture dependency rules

Feature-based frontend structure

DTOs and consistent API errors

Database constraints and migrations

Authentication and resource authorization

Responsive, accessible UI

Stage 2 — Performance

Server-side pagination and filtering

Query/index optimization

Rate limits and request-size limits

Structured logs and health checks

Unit and integration tests

Stage 3 — Production reliability

CI/CD and safe migrations

Backups and restore drills

Load testing and SLOs

Monitoring and alerting

Security and accessibility audits

Stage 4 — Scale when needed

Redis for measured cache/coordination needs

Background queue and worker

SignalR scale-out for real-time messaging

Load balancer and multiple stateless API instances

Database capacity planning and read scaling if justified

Reset checklistCopy checklist

My main recommendation: make these rules part of a project-level `AGENTS.md` or engineering standards document, and require every feature implementation to follow them. That will help your AI coding assistant produce consistent code instead of introducing a different architecture for each module.
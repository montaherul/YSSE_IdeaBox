# AGENTS.md

# Generic Software Development Rules

This document defines the default engineering rules for AI agents working on this project.

The agent MUST follow these rules when creating, modifying, refactoring, reviewing, or debugging code.

---

# 1. Core Objective

The agent must prioritize:

1. Correctness
2. Maintainability
3. Security
4. Simplicity
5. Reusability
6. Testability
7. Performance where it materially matters
8. Consistency with the existing project

Do not optimize for the number of classes, patterns, abstractions, or technologies used.

The goal is a clean solution, not maximum architecture.

---

# 2. Understand Before Changing

Before modifying code, the agent MUST:

1. Inspect the existing project structure.
2. Identify the application's architecture.
3. Identify existing conventions and patterns.
4. Locate reusable services, utilities, components, repositories, or abstractions.
5. Understand the data flow for the affected feature.
6. Check related implementations before creating new ones.
7. Preserve existing behavior unless a change is explicitly requested.

Do not redesign the project simply because another architecture is preferred.

---

# 3. Architecture Preservation

The agent MUST preserve the project's existing architecture unless the developer explicitly requests an architectural change.

Do NOT automatically introduce:

```text
CQRS
MediatR
Microservices
Event Sourcing
Repository Pattern
Unit of Work
Specification Pattern
Domain Events
Factories
Managers
Wrappers
Additional layers
Additional projects
Additional abstractions
```

unless there is a real requirement.

If the project already uses a pattern, follow the existing implementation consistently.

---

# 4. Separation of Responsibilities

Every major component must have a clear responsibility.

Use this general rule:

```text
UI / API
    ↓
Application / Service
    ↓
Domain / Business Logic
    ↓
Data Access
    ↓
Database / External System
```

The exact names may differ between projects.

The agent must determine responsibility based on the project's architecture rather than blindly applying these names.

---

# 5. Presentation / API Layer

Controllers, API endpoints, route handlers, pages, or UI components should remain focused.

They may:

- Receive requests.
- Validate request structure.
- Perform model binding.
- Call application services.
- Return responses.
- Handle presentation-specific concerns.

They MUST NOT normally:

- Contain complex business logic.
- Execute raw SQL.
- Directly manage database connections.
- Implement large workflows.
- Contain duplicated business rules.
- Perform unrelated data-access operations.

Bad:

```text
Controller
    → SQL
    → business validation
    → transaction
    → mapping
    → response
```

Prefer:

```text
Controller
    → Application Service
    → Data Access
```

---

# 6. Application / Service Layer

The application/service layer should coordinate application workflows and business use cases.

It may contain:

- Business rules.
- Application workflows.
- Validation that depends on business rules.
- Authorization decisions where appropriate.
- Entity/ViewModel/DTO mapping.
- Coordination of multiple data operations.
- Transaction orchestration when required.

It MUST NOT contain low-level database implementation when a data-access abstraction already exists.

Avoid turning one service into a "God Service".

Prefer focused responsibilities.

---

# 7. Domain / Business Logic

Business rules should have a clear home.

When the project uses domain entities:

- Keep entities independent of UI and infrastructure.
- Put domain rules close to the domain when appropriate.
- Do not put HTTP concerns into domain models.
- Do not put database connection logic into domain models.

When the project uses a service-oriented design, business rules may live in application services.

Follow the existing project architecture.

---

# 8. Data Access

Database access must remain in the project's established data-access layer.

Examples include:

```text
Repository
Data Access Service
Query Handler
DbContext
DAO
Gateway
ORM Layer
```

The exact implementation depends on the project.

Data-access code should be responsible for:

- Querying data.
- Persisting data.
- Calling stored procedures where applicable.
- Mapping database results.
- Managing database-specific concerns.

It should NOT contain unrelated business rules.

---

# 9. Database Rules

Use the project's established database technology and conventions.

Prefer:

- Parameterized queries.
- ORM features where appropriate.
- Transactions for multi-step atomic operations.
- Server-side filtering for large datasets.
- Server-side pagination for large datasets.
- Proper indexes where justified.
- Explicit database constraints for important invariants.

Never concatenate untrusted user input into SQL.

Bad:

```csharp
$"SELECT * FROM Users WHERE Name = '{name}'"
```

Prefer parameterized execution.

Do not move business logic into SQL unless the project intentionally uses database-side business logic.

---

# 10. Generic vs Specific Components

Before creating a new class, ask:

```text
Can an existing reusable component handle this?
```

Prefer generic infrastructure for genuinely generic behavior.

However, do NOT force unrelated business workflows into generic abstractions.

Good:

```text
Generic CRUD
    → Generic Repository / Service
```

Good:

```text
Complex business workflow
    → Dedicated Service
```

Avoid:

```text
One Entity
    → Repository
    → Service
    → Manager
    → Factory
    → Helper
    → Wrapper
```

when the extra layers provide no real value.

---

# 11. No Duplicate Abstractions

Before creating a:

```text
Interface
Service
Repository
Helper
Manager
Factory
Wrapper
Base Class
Utility
```

the agent MUST search for an existing implementation that can be reused or extended.

Do not create two abstractions that solve essentially the same problem.

Prefer:

```text
Reuse > Extend > Refactor > Create New
```

when appropriate.

---

# 12. SOLID Principles

The project should follow SOLID pragmatically.

## SRP — Single Responsibility

Each class should have one clear responsibility.

Ask:

```text
What is this class responsible for?
```

If the answer contains several unrelated responsibilities, consider splitting it.

## OCP — Open/Closed

Prefer reusable abstractions and extension points where they provide real value.

Do not modify generic infrastructure for every new entity when it can already support the requirement.

## LSP — Liskov Substitution

Implementations must honor their interfaces and base-class contracts.

Do not create implementations that unexpectedly change behavior.

## ISP — Interface Segregation

Keep interfaces focused.

Avoid large interfaces such as:

```text
IApplicationManager
```

containing unrelated operations.

## DIP — Dependency Inversion

High-level application code should depend on abstractions where the project architecture benefits from them.

Prefer dependency injection over manually constructing infrastructure dependencies.

---

# 13. Dependency Injection

Use the project's dependency injection mechanism.

Prefer:

```text
Class
    ↓
Interface
    ↓
DI Container
    ↓
Implementation
```

Avoid manually creating infrastructure dependencies inside application code:

```csharp
new DbContext()
new Repository()
new Service()
```

unless the project specifically requires it.

Do not introduce dependency injection abstractions solely for classes that have no meaningful dependency or testability benefit.

---

# 14. Async Programming

When the platform supports asynchronous I/O, use asynchronous APIs for:

- Database operations.
- HTTP calls.
- File I/O.
- External service calls.

Prefer:

```text
Async
Await
CancellationToken
```

when appropriate.

Avoid:

```csharp
.Result
.Wait()
```

for asynchronous operations.

Do not make CPU-bound code asynchronous merely to appear modern.

---

# 15. Validation

Separate input validation from business validation.

General rule:

```text
Input / Format Validation
    → Request Model / DTO / ViewModel

Business Validation
    → Application / Domain Layer
```

Examples:

```text
Required field
    → Request validation

Email format
    → Request validation

User cannot approve their own request
    → Business validation

User cannot access another tenant
    → Business authorization/business rule
```

Never rely only on client-side validation for security.

---

# 16. Authorization and Security

Security rules must be enforced server-side.

The UI may hide unauthorized actions for better UX, but UI hiding is NOT security.

Always verify authorization at the appropriate backend boundary.

Never trust:

```text
Client-side flags
Hidden fields
JavaScript variables
Request parameters
Browser state
```

for security decisions.

Never expose:

- Passwords.
- Access tokens.
- API secrets.
- Connection strings.
- Private keys.
- Sensitive personal information.

in logs, responses, source control, or client-side code.

---

# 17. Project Scope and Tenant Isolation

The agent MUST determine whether the project is single-tenant, multi-tenant, organization-scoped, branch-scoped, or uses another data-isolation model before implementing scope-related logic.

## General Rules

- Do NOT assume every project is multi-tenant.
- Do NOT introduce tenant IDs, tenant middleware, tenant resolution services, tenant filters, or tenant-specific database schemas unless required.
- Do NOT add organization or branch isolation unless the project requirements demand it.
- Preserve the existing authorization and data-access model.
- Enforce the project's actual data-access and authorization boundaries server-side.

## If the Project Is NOT Multi-Tenant

- Do not introduce tenant-specific entities, columns, interfaces, middleware, or abstractions.
- Apply normal authentication and authorization requirements where needed.
- Protect resources according to user permissions and ownership requirements.
- Do not impose unnecessary organization or branch restrictions.

## If the Project IS Multi-Tenant

- Resolve the tenant context securely.
- Validate tenant access server-side.
- Enforce tenant isolation consistently across reads, writes, background jobs, caches, and other relevant operations.
- Prevent cross-tenant data access.
- Never trust a client-supplied tenant ID without authorization checks.
- Test tenant isolation explicitly.

**Golden rule:** Implement tenant isolation only when the project's requirements and existing architecture require it.

# 18. Transactions

Use transactions when multiple operations must succeed or fail together.

Example:

```text
Create Order
    +
Create Order Items
    +
Update Inventory
    +
Create Payment Record
```

should use a transaction when atomicity is required.

Do NOT use transactions for every database operation automatically.

Transactions should be managed at the appropriate application/data boundary, not inside UI controllers.

---

# 19. Error Handling

Never silently swallow exceptions.

Bad:

```csharp
try
{
    ...
}
catch
{
}
```

Handle errors meaningfully.

Rules:

- Catch exceptions only when you can handle or enrich them.
- Log unexpected failures.
- Return appropriate application responses.
- Do not expose internal exception details to users.
- Avoid duplicated try/catch blocks when centralized error handling exists.

Prefer centralized exception handling for global failures.

---

# 20. Logging

Use the project's configured logging system.

Do not use permanent:

```csharp
Console.WriteLine(...)
```

as application logging when a logging framework exists.

Do not log:

```text
Passwords
Tokens
API Keys
Connection Strings
Private Keys
Sensitive Personal Data
```

Logs should provide enough context to diagnose failures without exposing secrets.

---

# 21. Configuration

Never hardcode environment-specific values.

Avoid hardcoding:

```text
Connection Strings
API Keys
Passwords
Tokens
URLs
Environment-specific settings
```

Use the project's configuration system and environment variables/secrets management.

Keep development, staging, and production configuration separated appropriately.

---

# 22. API / DTO / ViewModel Rules

Do not expose internal database entities directly when the application benefits from a dedicated contract.

Prefer:

```text
Entity
    ↓
DTO / ViewModel
    ↓
API / UI
```

Use separate models when:

- The UI needs different fields.
- Sensitive fields must be hidden.
- Input and output structures differ.
- Database structure should remain internal.
- Validation differs from persistence.

Do not create DTOs mechanically when direct models are already appropriate and safe.

---

# 23. Mapping

Mapping should have a consistent location.

Do not duplicate the same mapping logic across multiple controllers or services.

Prefer:

```text
Central Mapper
```

or:

```text
Application Service
```

or the project's established mapping mechanism.

Follow the existing convention.

---

# 24. Frontend Rules

Frontend code must remain separated from backend concerns.

Frontend responsibilities include:

- UI behavior.
- Client-side validation.
- AJAX/fetch calls.
- Rendering.
- User interaction.

Frontend MUST NOT contain secrets or security decisions.

For large datasets, prefer server-side:

```text
Pagination
Filtering
Sorting
Searching
```

instead of loading unnecessary records into the browser.

---

# 25. API Response Consistency

Follow the project's existing response format.

If the project uses a standard response structure, reuse it.

Do not introduce a new response format for one endpoint without a strong reason.

Maintain consistency for:

```text
Success
Validation Errors
Authorization Errors
Not Found
Server Errors
Pagination
```

---

# 26. Naming Conventions

Follow the language and framework's standard naming conventions.

Names must be:

- Clear.
- Consistent.
- Meaningful.
- Domain-appropriate.

Avoid meaningless names:

```text
Manager
Helper
Processor
Handler
Util
Data
Thing
Temp
```

unless the name accurately describes the responsibility.

Async methods should use the established `Async` naming convention where applicable.

---

# 27. Code Quality

When modifying code:

1. Keep methods focused.
2. Keep classes focused.
3. Reuse existing abstractions.
4. Avoid duplication.
5. Prefer readable code.
6. Avoid clever code when simple code is clearer.
7. Preserve existing conventions.
8. Remove dead code only when safe.
9. Keep changes localized.
10. Do not introduce unrelated refactoring.

---

# 28. Refactoring Rules

Do not refactor unrelated code while implementing a feature.

Prefer:

```text
Small
Focused
Reviewable
Low-risk
```

changes.

If a refactor is necessary to safely implement the requested feature:

1. Explain why.
2. Keep the refactor minimal.
3. Preserve behavior.
4. Verify affected functionality.

---

# 29. Testing

Before completing a change, determine the appropriate tests.

Depending on the project, consider:

```text
Unit Tests
Integration Tests
API Tests
Database Tests
UI Tests
End-to-End Tests
```

At minimum, verify:

- The changed feature works.
- Existing related behavior still works.
- Validation works.
- Error paths are handled.
- Authorization is enforced where relevant.

Do not claim tests passed unless they were actually run.

---

# 30. Build and Verification

After meaningful code changes, verify the project using the appropriate tools.

Examples:

```text
Build
Compile
Lint
Format
Unit Tests
Integration Tests
Migration Validation
Type Checking
```

If a check cannot be run, clearly state that it was not run.

Never fabricate successful test/build results.

---

# 31. Database Migration Rules

When the project uses migrations:

1. Inspect the current model and migration state.
2. Avoid destructive changes unless explicitly requested.
3. Review generated migrations.
4. Verify foreign keys and indexes.
5. Consider existing production data.
6. Never assume a migration is safe merely because it compiles.

Do not delete or recreate databases as a shortcut unless explicitly authorized.

---

# 32. External Services

When integrating external APIs/services:

- Keep credentials outside source code.
- Use timeouts.
- Handle transient failures appropriately.
- Validate external responses.
- Avoid leaking external errors directly to users.
- Follow the project's existing integration abstraction.

Do not create a new HTTP client abstraction if the project already has one.

---

# 33. Performance

Do not optimize prematurely.

First ensure correctness and clarity.

When performance is actually relevant:

- Avoid N+1 database queries.
- Avoid unnecessary database round trips.
- Use pagination for large datasets.
- Select only required fields.
- Use appropriate indexes.
- Avoid loading large datasets into memory.
- Use caching only when justified.
- Measure before and after optimization when possible.

Prefer evidence-based optimization.

---

# 34. Security Review Before Completion

For security-sensitive changes, verify:

```text
[ ] Authentication is correct
[ ] Authorization is enforced server-side
[ ] Input is validated
[ ] SQL is parameterized
[ ] Secrets are not exposed
[ ] Sensitive data is not logged
[ ] Tenant/scope isolation is enforced
[ ] Error messages do not leak internals
[ ] File uploads are validated if applicable
[ ] External requests are protected against abuse
```

---

# 35. New Feature Workflow

When implementing a new feature:

```text
1. Understand the requirement
2. Inspect existing architecture
3. Find reusable components
4. Identify affected layers
5. Design the smallest appropriate change
6. Implement using existing conventions
7. Add or update tests
8. Build/compile
9. Run relevant tests
10. Review security and authorization
11. Review duplication
12. Report what changed and what was verified
```

---

# 36. New Module Workflow

When adding a new module:

```text
1. Define the domain/data model
2. Define input/output contracts
3. Determine whether generic infrastructure is sufficient
4. Reuse existing services/repositories/components
5. Add dedicated business logic only when required
6. Add data-access logic only where required
7. Add API/controller/UI layer
8. Add validation
9. Add authorization
10. Add tests
11. Verify integration
```

Do NOT automatically create:

```text
NewRepository
NewService
NewManager
NewHelper
NewFactory
NewHandler
```

for every module.

Create them only when they provide meaningful responsibility.

---

# 37. AI Agent Behavior

The AI agent MUST:

- Read before editing.
- Search before creating.
- Reuse before duplicating.
- Follow existing conventions.
- Make minimal changes.
- Explain architectural decisions when relevant.
- Preserve unrelated functionality.
- Verify changes when possible.
- Ask for clarification when requirements are genuinely ambiguous.

The AI agent MUST NOT:

- Invent project structure.
- Invent APIs.
- Invent database tables.
- Invent existing methods.
- Claim tests passed when they were not run.
- Rewrite large portions of the project unnecessarily.
- Introduce architecture without justification.
- Remove working functionality without approval.

---

# 38. When Requirements Are Ambiguous

If ambiguity materially affects implementation:

```text
Ask a clarification question.
```

If the ambiguity is minor and a safe convention exists:

```text
Use the existing project convention.
```

Do not make major architectural decisions silently.

---

# 39. Decision Rules

When deciding where code belongs, ask:

```text
Is it UI/HTTP?
    → Presentation/API layer

Is it an application workflow?
    → Application/Service layer

Is it a business rule?
    → Domain/Application layer according to project architecture

Is it database access?
    → Data-access/Infrastructure layer

Is it an external integration?
    → Integration/Infrastructure layer

Is it a reusable contract?
    → Interface/Contract layer

Is it configuration?
    → Configuration system

Is it generic behavior?
    → Existing generic abstraction, if one exists

Is it unique business behavior?
    → Focused specific component
```

---

# 40. Architecture Decision Priority

When multiple solutions are possible, prefer them in this order:

```text
1. Existing project convention
2. Existing reusable component
3. Simple implementation
4. Focused abstraction
5. New architectural pattern only when justified
```
# 43. ASP.NET Core Web API Engineering Rules

## 43.1 Core Objective

The agent MUST understand and follow the actual ASP.NET Core Web API request lifecycle when creating, modifying, reviewing, debugging, or refactoring an API.

The agent MUST:

- Inspect the existing project before changing its architecture.
- Understand the request flow from HTTP entry to response generation.
- Preserve existing framework conventions and API contracts.
- Apply the simplest maintainable architecture that satisfies the requirements.
- Never assume that every project needs Clean Architecture, repositories, Unit of Work, JWT, CQRS, or multi-tenancy.

---

## 43.2 HTTP Request Lifecycle

Understand this conceptual request flow:

```text
HTTP Client
    ↓
Kestrel / Configured Web Server
    ↓
ASP.NET Core Middleware Pipeline
    ↓
Endpoint Routing
    ↓
Authentication and Authorization
    ↓
Endpoint Execution
    ↓
Model Binding and Validation (MVC Controllers)
    ↓
Controller / Minimal API Endpoint
    ↓
Application Service / Business Logic
    ↓
Data Access Abstraction or DbContext
    ↓
Database / External System
    ↓
HTTP Response
```

This is a conceptual flow, not a universal execution sequence. Middleware, routing, filters, binding, authorization and endpoint execution have framework-specific ordering and behavior.

The agent MUST:

1. Inspect the existing `Program.cs` and application startup configuration.
2. Understand which middleware executes and in what order.
3. Recognize that middleware may short-circuit a request.
4. Understand that not every request reaches the controller, service or database.
5. Understand that responses unwind through the middleware pipeline.
6. Preserve correct routing, authentication, authorization, exception handling and CORS configuration.
7. Avoid introducing duplicate middleware or redundant processing.
8. Diagnose failures at the correct lifecycle stage.

Do not copy a fixed middleware sequence without considering the project's requirements.

---

## 43.3 Middleware and Application Pipeline

Review the following components when relevant:

- Exception handling.
- HTTPS redirection.
- Routing.
- CORS.
- Authentication.
- Authorization.
- Rate limiting.
- Request logging and tracing.
- Response compression.
- Health checks.
- Endpoint mapping.

Rules:

- Register middleware in an order appropriate to its behavior and framework requirements.
- Authentication MUST execute before authorization when both are used.
- Configure CORS according to the frontend origin, authentication mechanism and deployment model.
- Do not enable permissive CORS policies indiscriminately.
- Use centralized exception handling where appropriate.
- Avoid exposing developer exception details in production.
- Do not introduce optional middleware without a real requirement.
- Verify middleware changes against the actual ASP.NET Core version.

---

## 43.4 Routing and Endpoint Design

The agent MUST:

- Use clear, consistent route templates.
- Use appropriate HTTP methods.
- Avoid conflicting or ambiguous routes.
- Keep endpoint definitions focused.
- Respect existing route conventions and API versioning.
- Document important route, query, header and body parameters.
- Avoid breaking existing endpoints without authorization to do so.

Prefer resource-oriented routes when appropriate.

Example:

```text
GET    /api/students
GET    /api/students/{id}
POST   /api/students
PUT    /api/students/{id}
PATCH  /api/students/{id}
DELETE /api/students/{id}
```

Choose `PUT` and `PATCH` according to their intended update semantics. Do not use HTTP methods interchangeably.

---

## 43.5 Controller and Minimal API Responsibilities

Controllers and Minimal API handlers MUST primarily handle HTTP and presentation concerns.

They may:

- Receive requests.
- Bind parameters.
- Perform request-level validation.
- Invoke application services.
- Translate application outcomes into HTTP responses.

They MUST NOT normally:

- Contain complex business workflows.
- Implement extensive database queries.
- Manage database connections.
- Duplicate business rules.
- Coordinate large transactions directly.
- Contain unrelated responsibilities.

Prefer:

```text
Controller / Endpoint
    ↓
Application Service
    ↓
Existing Data Access Layer
```

Follow the project's existing controller-based or Minimal API approach. Do not migrate between endpoint styles without a requirement.

---

## 43.6 Model Binding, Validation and Filters

The agent MUST distinguish:

- Model binding.
- Request DTO validation.
- Business validation.
- Authorization.
- MVC action filters.
- Minimal API endpoint filters.
- Middleware.

Rules:

1. Validate incoming data on the server.
2. Use request DTOs where appropriate.
3. Reject malformed or invalid requests with consistent error responses.
4. Apply business rules in the appropriate application or domain layer.
5. Do not rely on frontend validation for security.
6. Avoid duplicating validation rules unnecessarily.
7. Understand the project's automatic validation behavior before adding manual validation.
8. Respect the behavior of `[ApiController]`, endpoint filters and any existing validation framework.

---

## 43.7 Service, Repository and Unit of Work Architecture

The agent MUST inspect the existing architecture before selecting a data-access pattern.

### Repository-Based Architecture

When the project uses Repository Pattern and Unit of Work:

```text
Controller
    ↓
Application Service
    ↓
Unit of Work / Repository Interfaces
    ↓
Repository Implementations
    ↓
EF Core DbContext
    ↓
Database
```

The exact arrangement may vary. Some Unit of Work implementations expose repositories; others coordinate repositories injected separately.

Rules:

- Services own application workflows and business rules.
- Repositories own persistence operations and database-specific queries.
- Unit of Work coordinates related persistence operations where required.
- Repositories participating in one Unit of Work MUST share the intended DbContext and transaction scope.
- Follow the existing SaveChanges and transaction conventions.
- Do not create duplicate repositories, services or Unit of Work implementations.

### Direct EF Core Architecture

When the existing architecture intentionally uses:

```text
Controller → Service → DbContext → Database
```

preserve it unless a meaningful architectural reason justifies changing it.

### Unit of Work Rules

- EF Core `DbContext` already provides Unit of Work behavior.
- Do not create a separate Unit of Work wrapper automatically.
- Introduce an explicit Unit of Work only when it provides meaningful coordination, abstraction or architectural value.
- Do not assume Unit of Work automatically makes multiple independent saves atomic.
- Use explicit database transactions when a multi-step operation requires atomicity across multiple saves or other database operations.
- Do not assume one `SaveChangesAsync()` call commits multiple independent contexts together.

---

## 43.8 Dependency Injection and Lifetimes

The agent MUST use the application's configured dependency injection container.

Rules:

- Register services and repositories using appropriate lifetimes.
- Register EF Core DbContext with its intended scope, normally scoped for HTTP-request-based applications.
- Do not inject scoped services directly into long-lived singletons.
- Use appropriate scopes for background jobs and hosted services.
- Avoid creating infrastructure dependencies manually.
- Do not register duplicate or conflicting service implementations.
- Inspect existing registrations before adding new ones.

Follow the dependency injection behavior of the framework version being used.

---

## 43.9 HTTP Status Codes and Response Contracts

Use status codes according to the endpoint's actual outcome.

Examples:

- `200 OK`: Successful operation returning a response.
- `201 Created`: Resource created successfully.
- `204 No Content`: Successful operation without a response body.
- `400 Bad Request`: Invalid request or malformed input.
- `401 Unauthorized`: Authentication is missing or invalid.
- `403 Forbidden`: The caller is authenticated but lacks permission.
- `404 Not Found`: Resource does not exist or is not available to the caller under the application's disclosure policy.
- `409 Conflict`: Conflicting resource state or concurrency conflict.
- `422 Unprocessable Content`: Semantically invalid input when the API adopts this convention.
- `429 Too Many Requests`: Rate limit exceeded.
- `500 Internal Server Error`: Unexpected server failure.

Rules:

- Do not return `200 OK` for every outcome.
- Follow the project's established response contract.
- Use consistent validation and error response formats.
- Prefer Problem Details where appropriate.
- Include pagination metadata when returning paginated collections.
- Avoid exposing stack traces, database errors or internal implementation details.
- Do not change response schemas casually.

---

## 43.10 DTOs, Entities and Serialization

The agent MUST distinguish API contracts from database entities when the project requires that separation.

Rules:

- Use request DTOs to control accepted input.
- Use response DTOs to control exposed output.
- Prevent over-posting and unauthorized property updates.
- Never expose password hashes, tokens, internal secrets or other sensitive fields.
- Avoid accidental serialization of navigation-property cycles.
- Configure JSON serialization consistently.
- Handle nullable properties and enum representations deliberately.
- Map entities to response contracts using the project's established approach.
- Do not create unnecessary DTOs for every type when a simpler safe contract is sufficient.

---

## 43.11 Authentication and Authorization

Authentication and authorization MUST be enforced server-side whenever required.

The agent MUST:

- Inspect the existing authentication mechanism before changing it.
- Validate credentials and tokens using the configured framework mechanisms.
- Validate JWT signatures, issuer, audience and lifetime when JWT bearer authentication is used.
- Apply authorization policies and roles where required.
- Verify resource ownership and permissions where applicable.
- Protect against insecure direct object references.
- Avoid trusting client-supplied user IDs, role flags or permissions.
- Keep authorization checks consistent across endpoints and background operations.
- Test unauthorized and forbidden requests.

Do not introduce JWT, cookies, OAuth or another authentication mechanism merely because it is common.

---

## 43.12 Database Access and EF Core

Follow the established database technology, ORM and data-access conventions.

Rules:

- Use asynchronous I/O and propagate cancellation tokens where appropriate.
- Use parameterized SQL.
- Use `AsNoTracking()` for appropriate read-only EF Core queries.
- Project only the fields required by the response.
- Avoid N+1 queries and unnecessary database round trips.
- Avoid loading large datasets into application memory.
- Use indexes based on actual query patterns.
- Review query translation and generated SQL for expensive operations.
- Use transactions when atomicity is required.
- Respect database constraints and concurrency behavior.
- Use stored procedures when justified by requirements or query characteristics.
- Do not automatically replace LINQ queries with stored procedures.
- Do not create migrations or alter database schemas without reviewing their effects.

---

## 43.13 Pagination, Filtering, Sorting and Search

For large collection endpoints, implement database-side pagination and filtering.

Rules:

1. Validate page number, page size and filter parameters.
2. Enforce a configurable maximum page size.
3. Apply authorization restrictions before returning records.
4. Apply filtering and sorting in the database where appropriate.
5. Whitelist allowed sort fields and directions.
6. Use deterministic ordering.
7. Return consistent pagination metadata.
8. Avoid unbounded collection endpoints.
9. Inspect indexes for frequently used filters and sorting.
10. Consider keyset pagination for large offsets or sequential navigation requirements.

Use stored procedures only when justified. EF Core LINQ, SQL queries and stored procedures are all valid options when used appropriately.

---

## 43.14 Transactions, Concurrency and Idempotency

The agent MUST determine whether an operation requires atomicity, concurrency protection or duplicate-request protection.

Rules:

- Use transactions for related operations that must succeed or fail together.
- Avoid unnecessary transactions around independent operations.
- Use appropriate concurrency controls when simultaneous updates can conflict.
- Consider optimistic concurrency where supported by the domain and database.
- Use idempotency mechanisms for operations that must safely tolerate retries or duplicate submissions.
- Do not assume every POST request needs an idempotency key.
- Keep transaction boundaries explicit and consistent with the existing architecture.
- Test partial failure and concurrent update scenarios when relevant.

---

## 43.15 Error Handling and Logging

Rules:

- Use centralized exception handling where appropriate.
- Use structured logging.
- Include useful diagnostic context and correlation information.
- Avoid logging passwords, tokens, connection strings or sensitive personal information.
- Do not swallow exceptions silently.
- Do not expose internal exception messages to clients.
- Distinguish expected application failures from unexpected infrastructure failures.
- Avoid duplicated exception handling when centralized handling already exists.
- Log unexpected failures with sufficient diagnostic context.

---

## 43.16 API Documentation and Versioning

Where applicable:

- Maintain OpenAPI documentation.
- Document endpoint parameters, authentication requirements and response codes.
- Keep request and response schemas accurate.
- Document pagination, filtering and sorting conventions.
- Use API versioning when compatibility requirements justify it.
- Avoid breaking existing clients without an approved change.
- Verify generated API documentation after meaningful endpoint changes.

Do not introduce versioning infrastructure solely for a small API without a relevant requirement.

---

## 43.17 Performance, Caching and Resource Protection

The agent MUST:

- Avoid unnecessary allocations and database calls.
- Use bounded request and query sizes.
- Configure appropriate timeouts and cancellation.
- Apply rate limiting when justified by the API's exposure and risk.
- Consider response compression when appropriate.
- Use caching only when beneficial and correctly invalidated.
- Avoid caching user-specific or authorization-sensitive responses incorrectly.
- Prevent expensive unbounded queries.
- Measure significant performance improvements when possible.

Do not introduce Redis, distributed caching or other infrastructure automatically.

---

## 43.18 External HTTP Calls and Background Work

When external integrations or background processing are required:

- Reuse configured `HttpClient` and existing integration abstractions.
- Configure appropriate timeouts and cancellation.
- Handle transient failures with bounded, suitable retries.
- Avoid retrying non-idempotent operations blindly.
- Validate external responses.
- Keep credentials outside source code.
- Use background services or durable job processing according to the workload.
- Create appropriate dependency injection scopes in background workers.
- Handle duplicate jobs and partial failures where necessary.

Do not introduce a message broker or background job framework without justification.

---

## 43.19 CORS, Rate Limiting and Request Security

Where applicable:

- Configure explicit trusted frontend origins.
- Avoid wildcard origins with credentialed cross-origin requests.
- Apply suitable rate limits.
- Limit request-body sizes and upload sizes.
- Validate file types and content where file uploads exist.
- Protect outbound requests against server-side request forgery when user-controlled destinations are possible.
- Use secure cookie settings when cookie authentication is used.
- Apply HTTPS and secure transport configuration.
- Review security-sensitive endpoint behavior before completion.

---

## 43.20 Health Checks and Observability

For applications that require production operations support:

- Provide appropriate health checks.
- Distinguish application liveness from dependency readiness.
- Avoid exposing sensitive dependency details publicly.
- Use structured logs, metrics and distributed tracing where appropriate.
- Propagate correlation and trace identifiers.
- Monitor database and external-service failures.
- Keep diagnostic endpoints appropriately protected.

Do not expose secrets, connection strings or detailed internal infrastructure information through health endpoints.

---

## 43.21 Production Configuration and Deployment

The agent MUST:

- Separate development and production configuration.
- Keep secrets outside source control.
- Validate required configuration at startup where appropriate.
- Review database migrations before production deployment.
- Avoid destructive migrations without explicit authorization and a recovery plan.
- Configure logging and error handling for production.
- Consider graceful shutdown and readiness behavior.
- Verify deployment-specific connection strings and provider settings.
- Document build, migration and deployment assumptions.
- Never claim deployment succeeded without verification.

---

## 43.22 API Testing Requirements

Select tests based on the endpoint's risk and behavior.

Consider:

- Unit tests for application and business logic.
- Integration tests for HTTP endpoints and middleware.
- Database integration tests for persistence behavior.
- Authorization tests for protected resources.
- Contract tests for important API schemas.
- Performance tests for expensive endpoints.
- End-to-end tests for critical workflows.

Verify relevant cases:

- Successful requests.
- Invalid input.
- Missing or invalid authentication.
- Insufficient permissions.
- Missing resources.
- Database constraint violations.
- Concurrency conflicts.
- Pagination and sorting limits.
- Unexpected failures.
- Cancellation and external-service failures where relevant.

Never claim a test passed unless it was actually executed and its result was checked.

---

## 43.23 Universal Architecture Decision Rules

Before implementing any API feature, the agent MUST determine:

1. Which ASP.NET Core version and endpoint style the project uses.
2. Which middleware and authentication mechanisms already exist.
3. Whether the project uses direct DbContext access, repositories, Unit of Work or another data-access approach.
4. Which response and validation conventions are established.
5. Whether the project is single-tenant or multi-tenant.
6. Which security, performance and reliability requirements apply.
7. Which tests and verification tools are available.

The agent MUST NOT assume that every project needs:

- Multi-tenancy.
- Clean Architecture.
- Repository Pattern.
- A separate Unit of Work class.
- CQRS or MediatR.
- Stored procedures.
- JWT authentication.
- API versioning.
- Redis or distributed caching.
- Background job infrastructure.
- Microservices.

Choose architecture according to actual requirements.

---

## 43.24 Final Web API Review Checklist

Before completing a meaningful API change, verify the applicable items.

### Request Pipeline
- [ ] Middleware ordering is correct.
- [ ] Routing and endpoint execution behave as intended.
- [ ] Authentication and authorization run at the appropriate stages.
- [ ] Exceptions are handled consistently.

### Architecture
- [ ] Controllers/endpoints remain focused.
- [ ] Business logic belongs in the appropriate layer.
- [ ] Existing repository and Unit of Work conventions are preserved.
- [ ] No unnecessary abstractions were introduced.

### API Contract
- [ ] HTTP methods and status codes are appropriate.
- [ ] Inputs are validated.
- [ ] Response schemas remain consistent.
- [ ] Sensitive fields are not exposed.

### Database
- [ ] Queries are appropriately bounded and efficient.
- [ ] Database access follows the existing architecture.
- [ ] Transactions and concurrency controls are appropriate.
- [ ] Pagination is enforced where needed.

### Security and Reliability
- [ ] Authentication and authorization are enforced where required.
- [ ] Secrets and sensitive data are protected.
- [ ] Relevant resource limits are enforced.
- [ ] Errors do not leak internal details.

### Verification
- [ ] Relevant tests were added or updated.
- [ ] The build was checked where possible.
- [ ] Relevant tests were actually run.
- [ ] Unverified checks are clearly reported.
- [ ] No unrelated functionality was changed unnecessarily.

---

## 43.25 Golden Rule

The agent MUST understand the real ASP.NET Core request lifecycle and the existing application's architecture before making changes.

It MUST preserve established conventions, enforce security and correctness, use appropriate data-access and transaction boundaries, and avoid unnecessary architectural complexity.

The objective is not to force every API into one design. The objective is to make each API correct, secure, maintainable, testable, and production-appropriate.

Do not choose a more complicated solution simply because it is theoretically more scalable.

---

# 41. Final Review Checklist

Before completing a change:

### Architecture

- [ ] Existing architecture was preserved.
- [ ] Correct layer owns the new code.
- [ ] No unnecessary project/layer was introduced.
- [ ] No circular dependency was introduced.

### Code Quality

- [ ] Existing abstractions were reused.
- [ ] No unnecessary duplicate class/interface was created.
- [ ] Classes have focused responsibilities.
- [ ] Methods are reasonably focused.
- [ ] Naming follows project conventions.

### Data

- [ ] Database access is in the correct layer.
- [ ] Queries are parameterized.
- [ ] Large data operations use appropriate pagination/filtering.
- [ ] Transactions are used only when necessary.

### Security

- [ ] Authorization is enforced server-side.
- [ ] Sensitive data is protected.
- [ ] Secrets are not hardcoded.
- [ ] Sensitive information is not logged.
- [ ] Tenant/scope boundaries are respected where applicable.

### Reliability

- [ ] Exceptions are not silently swallowed.
- [ ] Relevant error paths are handled.
- [ ] Async operations are used appropriately.
- [ ] External failures are handled where relevant.

### Testing

- [ ] Relevant tests were added or updated.
- [ ] Build/compile was verified.
- [ ] Relevant tests were run.
- [ ] No unverified claims were made.

### Scope

- [ ] No unrelated files were changed unnecessarily.
- [ ] No unrelated refactoring was introduced.
- [ ] Existing behavior was preserved where possible.

---

# 42. Golden Rules

These rules have the highest priority:

1. **Understand the existing project before changing it.**
2. **Preserve the existing architecture unless explicitly asked to change it.**
3. **Keep each layer responsible for its own concern.**
4. **Keep controllers/endpoints thin.**
5. **Keep business logic out of data-access classes.**
6. **Keep database logic out of presentation classes.**
7. **Reuse existing abstractions before creating new ones.**
8. **Do not create duplicate repositories/services/helpers.**
9. **Use SOLID pragmatically, not dogmatically.**
10. **Do not over-engineer.**
11. **Enforce security on the server.**
12. **Never hardcode secrets.**
13. **Never fabricate test/build results.**
14. **Make small, focused, reviewable changes.**
15. **Follow the project's existing naming and implementation conventions.**
16. **Ask before making a major architectural decision when requirements are unclear.**
17. **Prefer simple, maintainable, testable solutions.**

---

# END OF AGENTS.md

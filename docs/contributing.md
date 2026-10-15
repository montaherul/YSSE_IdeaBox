# Contributing

Thank you for contributing to YSSE Idea Box. This guide describes the conventions and workflow so changes stay consistent with the existing architecture.

---

## 1. Before you start

1. Read [architecture.md](architecture.md), [backend.md](backend.md), and [frontend.md](frontend.md).
2. Get the app running locally ([getting-started.md](getting-started.md)).
3. Search for existing components, services, and helpers **before** creating new ones — reuse > extend > refactor > create new.

---

## 2. Ground rules

- **Preserve the architecture.** Keep the Clean Architecture dependency direction (`Domain ← Application ← { Infrastructure, Api }`). Do not introduce CQRS/MediatR, microservices, a new repository abstraction, or other patterns without a clear requirement.
- **Keep layers honest.** Controllers handle HTTP only; business rules live in application services; database access stays in Infrastructure.
- **Keep changes small and focused.** Do not refactor unrelated code in the same change.
- **Server is authoritative.** Enforce authorization and validation on the API even if the UI also checks them.
- **No secrets.** Never commit connection strings, keys, or tokens. Use git-ignored config or environment variables.
- **No fabricated results.** Only report tests/builds you actually ran.

---

## 3. Backend conventions

- Target `net8.0`; async methods use the `Async` suffix and accept/propagate `CancellationToken`.
- Use the existing `Result`/`Result<T>`/`PagedResult<T>` types and throw `AppException` subclasses (`NotFoundException`, `ForbiddenException`, `ConflictException`, `ValidationAppException`) to signal HTTP outcomes — do not hand-roll response shapes in controllers.
- Map DTOs in the application layer; never expose EF entities directly.
- Pagination goes through `Pagination.Normalize` (default 20, max 100). Use `AsNoTracking()` and project only needed columns for reads.
- Parameterize all SQL. Use EF Core for ordinary operations; use Dapper only for justified set-based reads.
- Register new services in `Application/DependencyInjection.cs` (or the appropriate infrastructure registration) with the correct lifetime.
- Log through Serilog with context; never log secrets or sensitive data.

### Adding an endpoint
1. DTO + FluentValidation validator.
2. Interface method on the relevant service in `Application/Abstractions`.
3. Implementation in `Application/Services/…`.
4. Thin controller action on the matching controller, decorated with the **permission** it requires.
5. Tests for success, validation failure, and authorization.

### Adding a permission
Add the code to `Permissions.All` (`Application/Common/Permissions.cs`), grant it to appropriate default roles in `DbInitializer`, and reference it from controllers/routes. The policy provider recognizes it automatically.

### Adding a migration
```powershell
dotnet ef migrations add <Name> `
  --project backend/src/YSSE.IdeaBox.Infrastructure `
  --startup-project backend/src/YSSE.IdeaBox.Api
```
Review the generated migration before applying; avoid destructive changes without approval.

---

## 4. Frontend conventions

- Organize by **feature** (`features/<module>/`), each with `pages/`, `components/`, `api.js`, `hooks.js`.
- All HTTP goes through `services/apiClient.js`; use TanStack Query for server state and invalidate the right keys after mutations.
- Use Zustand only for client-side global state.
- Forms: React Hook Form + Zod; show field-level errors; disable submit while pending.
- Gate routes/navigation with `roles` and `permissions` via the existing guards — remember this is UX only.
- Reuse `components/ui` primitives and shared components; add new primitives only when genuinely needed.
- Target WCAG 2.2 AA: semantic HTML, labels, keyboard support, visible focus, `aria-live` for status, and no color-only meaning.
- Keep UI text localization-ready (English/Bangla).

---

## 5. Commit & pull request workflow

1. Branch from `main` (for example `feature/…` or `fix/…`).
2. Make focused commits with clear messages.
3. Before opening a PR, run:
   ```powershell
   dotnet build backend/YSSE.IdeaBox.slnx --no-restore
   dotnet test backend/YSSE.IdeaBox.slnx
   cd frontend; npm run lint; npm run test; npm run build
   ```
4. Open a PR describing **what changed**, **why**, and **what was verified**. Link related issues.
5. Expect the Backend CI and Frontend CI checks to run. Address failures before merge.

### Commit message style
The repository uses short, descriptive subject lines, often Conventional-Commits-like, for example:

```
feat: RBAC permissions and admin platform modules; screening/project workflow updates; remove special roles; UI restructure
```

---

## 6. Definition of done

- [ ] Change is scoped to the requested work; unrelated files untouched.
- [ ] Correct layer owns the new code; no unnecessary abstraction introduced.
- [ ] Authorization enforced server-side; input validated; SQL parameterized.
- [ ] No secrets or sensitive data exposed or logged.
- [ ] Relevant tests added or updated and **actually run**.
- [ ] Backend builds and tests pass; frontend lints, tests, and builds.
- [ ] Docs updated if behavior or configuration changed.

---

## See also

- [Architecture](architecture.md) · [Backend](backend.md) · [Frontend](frontend.md)
- [Testing & CI](testing-and-ci.md) · [Security](security.md)

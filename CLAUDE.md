# CLAUDE.md

Guidance for Claude Code when working in this repository.

## Project

**Red Star Vibe**: a social platform with events, weddings, a vendor marketplace and local-business pages,
built with React 19 + .NET 10 + SQL Server. It is a **learning project**: the owner builds it phase by phase
to learn production skills. The full roadmap is in [`plan.md`](plan.md) and is the source of truth for scope.

### How to work with the owner
- **Teach while building.** Explain the *why* of non-obvious choices briefly, and add short comments in code
  for concepts a learner would not yet know (DI lifetimes, EF tracking, middleware order, etc.).
- **Follow the phases in `plan.md`.** Don't build features from a later phase unless asked. A phase is done only
  when its **Definition of Done (DoD)** is met. Tick `[x]` items in `plan.md` when they are actually completed and verified.
- **Small, working slices.** Follow the plan's learning loop: smallest working slice → test (including one failure
  case) → harden → document.
- When a phase starts and the owner asks for micro-steps, give them step by step with code, not a wall of everything at once.

## Phase status

- **Phase 0** (planning and foundation): done except owner review of `docs/product/` drafts.
- **Phase 1** (backend skeleton): **in progress**, built in 5 slices, each a PR into `dev`:
  1. ✅ Restructure: `RedStarVibe.slnx`, `src/Hosts/RedStarVibe.Api`, `/api/v1/ping`, `web/`, CI (`ci.yml`)
  2. ✅ ProblemDetails errors, correlation ID, Serilog → Seq, OpenAPI + Scalar, `/health/live` + `/health/ready`
  3. First module (Identity) with its own DbContext + schema, migration `0001_Init`, seed data, retry/timeout
  4. BuildingBlocks: `Result<T>`, FluentValidation pipeline, cursor pagination, idempotency keys, rate limiting
  5. Tests: architecture tests (NetArchTest) + Testcontainers SQL Server integration test running in CI
- Decisions already made: SQL Server only (no SQLite); the old Products scaffold was removed (in git history before Phase 1).

## Current state vs target

| Area | Now | Target (`plan.md` §1) |
|---|---|---|
| Layout | `src/Hosts/RedStarVibe.Api`, `web/` | + `src/BuildingBlocks`, `src/Modules/*`, `tests/`, `infra/` |
| API | Minimal APIs under `/api/v1`, only `/ping` | all endpoints in module `Endpoints` projects |
| Modules | none yet | Identity first (slice 3), then one per phase |
| Frontend | Vite starter + `ApiStatus` ping check | Phase 3 app shell (TanStack Query, Router, Zustand, RHF + Zod, Tailwind, generated client) |

## Repository layout

```
CLAUDE.md, plan.md, README.md, CONTRIBUTING.md, SECURITY.md
RedStarVibe.slnx               solution (root); add every new project to it
Directory.Build.props          net10.0, nullable, warnings-as-errors, analyzers, NuGet vulnerability audit
Directory.Packages.props       Central Package Management: versions live ONLY here, never in .csproj
.editorconfig, .gitattributes, global.json (SDK 10, rollForward latestFeature)
docker-compose.yml, .env.example   Redis / Seq / Mailpit; real secrets in git-ignored .env
docs/adr/                      decisions 0001-0008 (read before changing architecture)
docs/product/                  personas, MVP user stories (US-01..38), core flows
.github/workflows/ci.yml       PRs into dev/staging/production + pushes to dev: restore (audit), build, test, npm audit, lint
.github/workflows/deploy-cloudflare.yml   web/ -> Cloudflare Pages on pushes to dev/staging/production
web/public/_headers           Cloudflare Pages security headers (CSP, HSTS, ...); update CSP connect-src when the API gets its own origin
src/Hosts/RedStarVibe.Api/
  Program.cs                   composition root: services, middleware order, MapGroup("/api/v1")
  Endpoints/                   host-level endpoints (ping); feature endpoints belong in modules
  Errors/                      GlobalExceptionHandler + ProblemDetails setup (no exception text outside Development)
  Observability/               CorrelationIdMiddleware (X-Correlation-ID, validated), Serilog setup + request logging
  Health/                      /health/live (no checks), /health/ready (SQL Server, Redis); JSON without error details
  OpenApi/                     /openapi/v1.json + /scalar, Development only
  RedStarVibe.Api.http         sample requests
web/                           React 19 + TypeScript + Vite; vite.config.ts proxies /api -> http://localhost:5080
```

New module layout (from slice 3): `src/Modules/<Name>/RedStarVibe.Modules.<Name>.{Domain,Application,Infrastructure,Endpoints,Contracts}/`.

## Commands

Dev services (repo root):
```bash
docker compose up -d      # Redis :6379, Seq UI :5380 (ingest :5341), Mailpit SMTP :1025 / UI :8025
docker compose ps         # all healthy?
```

Backend (repo root):
```bash
dotnet build                                                        # whole solution, must be 0 warnings
dotnet run --project src/Hosts/RedStarVibe.Api --launch-profile http   # http://localhost:5080
dotnet test
```

Web (run in `web/`):
```bash
npm install
npm run dev        # http://localhost:5173
npm run build      # tsc -b && vite build
npm run lint
```

## Environment

- Windows 10, .NET SDK 10 (also 9), Node 24, `dotnet-ef` 10 installed globally.
- SQL Server 2025 local instance `DESKTOP-2ERCDGB\MSSQLSERVER01`, Windows auth (`Trusted_Connection=True;TrustServerCertificate=True`).
  SQL Browser is disabled; local connections still work. `sqlcmd` is available for inspecting data.
- Docker compose project name is **`rsv-learning`**. Never rename it to `redstarvibe`: another local project
  (`E:\Practice\RealAIProject\RedStarVibeProject\RedStarVibe`) uses that name, and sharing a name shares containers and
  volumes (it already happened once with its `redis-data` volume). Never run `docker compose down -v` or `--remove-orphans`
  without checking `docker compose ps` first.
- Ports: API 5080 (http) / 7080 (https), Vite 5173. `AllowedHosts` is `localhost` (host-header filtering); production overrides it.
- API config: `ConnectionStrings:Sql` in `appsettings.Development.json` (Windows auth, database `RedStarVibe`);
  `ConnectionStrings:Redis` (contains the password) only in **User Secrets** (`dotnet user-secrets list --project src/Hosts/RedStarVibe.Api`).
  The API refuses to start if either is missing. Serilog sinks/levels are in the `Serilog` config section; Development also logs to Seq (:5341).
- Seq's API needs a login; the first login forces a password change, so automated log queries only work after the owner has done that.
- An old `LearningAppDb` database from the removed scaffold may still exist on the local instance; it is unused.

## Architecture rules (from `plan.md`; apply to all new code)

- Modules never read another module's tables; use public interfaces or integration events.
- Business rules such as visibility, privacy, block lists and tenant filters go **in the SQL query**, never as in-memory filters.
- Every write that triggers side effects goes through the **outbox** (from Phase 6).
- Read queries use `AsNoTracking()`; keep DTOs separate from entities; never return entities from endpoints.
- Errors are returned as ProblemDetails; endpoints validate input; POSTs that create things support idempotency keys (Phase 1+).
- Time: inject `TimeProvider` (never `DateTime.UtcNow` directly); store and return UTC.
- Direct messages and private data are never indexed for search or AI.
- Concurrency-sensitive writes (bookings, slots) are enforced by unique indexes or locking in SQL, not by app checks.

## Conventions

- **Naming:** "Red Star Vibe" in anything people read (titles, docs, UI). `RedStarVibe` for .NET identifiers
  (solution, projects, namespaces such as `RedStarVibe.Modules.Identity`, database name). `red-star-vibe` for npm, URLs and
  Docker image names. The compose project stays `rsv-learning` (see Environment).
- C#: nullable enabled, file-scoped namespaces, primary constructors for DI, async all the way for I/O.
- Keep the build at **0 warnings**. If a NuGet vulnerability warning appears, pin a patched version rather than suppress it.
- TypeScript: strict (on by default in TS 6, no flag in `tsconfig.app.json`); keep the API client typed; frontend calls relative `/api/...` paths in dev.
- Secrets never go in `appsettings.json`; use User Secrets locally (Key Vault later). Windows-auth connection strings are fine to commit.
- `bin/`, `obj/`, `node_modules/`, `dist/`, `.env` are git-ignored.

## Verifying changes

Before saying something works: build both projects, and for API changes, run the API and hit the endpoint
(`Api.http` or `curl`), checking the data in the database when it matters. When starting servers for a
check, wait for a real success response (`curl -sf`), then stop the servers afterwards.

## Git

Commit only when asked.

Branches map to environments: `feat/*` → **`dev`** (default, integration) → **`staging`** → **`production`**.
- New work branches from `dev` and goes back to `dev` through a PR. Never commit directly to `staging` or `production`;
  they only change through promotion PRs (`dev` → `staging`, `staging` → `production`).
- `main` is **frozen**: kept for history, no longer used. Don't commit to it or target PRs at it.
- Hosting: **Cloudflare Pages**, deployed by `.github/workflows/deploy-cloudflare.yml` (ADR-0008) on pushes to `dev`/`staging`/`production`
  touching `web/**`. `production` → live site, `dev`/`staging` → `https://<branch>.my-static-site-1bb.pages.dev`.
  The workflow reruns the web checks, deploys with pinned Wrangler, then smoke-tests the title and CSP header. Read its logs
  with the GitHub API (no `gh` CLI here). Cloudflare's own Git builds are disabled.
- Hotfix: branch from `production`, PR into `production`, then merge `production` back into `staging` and `dev`.
- To promote or fast-forward another branch, **don't `git switch` to it**: switching rewrites files under running dev
  servers (it once removed `web/vite.config.ts` for a moment and Vite restarted without its `/api` proxy). Instead, after
  checking the target has no commits of its own, push directly: `git push origin dev:staging`, `git push origin staging:production`.

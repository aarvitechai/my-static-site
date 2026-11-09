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

- **Phase 0** (planning and foundation): ADRs, compose, tooling done. Product drafts in `docs/product/` await owner review.
- **Phase 1** (backend skeleton): next. Owner decisions already made for it:
  - **Replace the scaffold**: move `frontend/` → `web/`, replace `backend/` with `src/` (`BuildingBlocks`, `Modules`, `Hosts`). Git history keeps the old Products sample.
  - **Drop SQLite**: SQL Server only (ADR-0002).

## Current state vs target

The repo is currently an **early learning scaffold**, not yet the target architecture from `plan.md` §1:

| Area | Now | Target (`plan.md`) |
|---|---|---|
| Layout | `frontend/` + `backend/` | `web/` + `src/{BuildingBlocks,Modules,Hosts}`, `tests/`, `docs/`, `infra/` |
| API style | MVC controllers (`backend/Api/Controllers`) | Minimal APIs, endpoint groups under `/api/v1` |
| Structure | single `Api` project | modular monolith; each module has Domain / Application / Infrastructure / Endpoints |
| Database | one `AppDbContext`, switchable SQL Server / SQLite | SQL Server only, one DbContext + schema per module |
| Frontend | Vite + plain fetch + inline styles | TanStack Query, React Router, Zustand, RHF + Zod, Tailwind, generated OpenAPI client |

**Don't restructure toward the target silently.** Moving to the target layout is a Phase 0/1 task; confirm with
the owner before large moves. Note that SQLite has no schemas, so the SQLite option is incompatible with the
schema-per-module design and will likely be dropped in Phase 1.

## Repository layout (current)

```
CLAUDE.md, plan.md, README.md, CONTRIBUTING.md, SECURITY.md
Directory.Build.props          net10.0, nullable, warnings-as-errors, analyzers, NuGet vulnerability audit (all .NET projects)
.editorconfig, .gitattributes, global.json (SDK 10, rollForward latestFeature)
docker-compose.yml, .env.example   Redis / Seq / Mailpit; real secrets in git-ignored .env
docs/adr/                      decisions 0001-0006 (read before changing architecture)
docs/product/                  personas, MVP user stories (US-01..38), core flows
.github/                       deploy.yml, PR template, CODEOWNERS, dependabot.yml
.github/workflows/deploy.yml   builds frontend/ and deploys to GitHub Pages (pushes to `production` touching frontend/**)
frontend/                      React 19 + TypeScript + Vite
  src/api/products.ts          typed fetch wrapper (base URL from VITE_API_URL, empty in dev)
  src/components/Products.tsx
  vite.config.ts               dev proxy: /api -> http://localhost:5121
backend/
  RedStarVibe.slnx
  Api/
    Controllers/ Dtos/ Models/ Services/    layered: controller -> IProductService -> EF Core
    Data/          AppDbContext + SqlServerDbContext / SqliteDbContext, provider switch, design-time factories
    Migrations/    SqlServer/ and Sqlite/ (separate set per provider)
    Api.http       sample requests
```

## Commands

Dev services (repo root):
```bash
docker compose up -d      # Redis :6379, Seq UI :5380 (ingest :5341), Mailpit SMTP :1025 / UI :8025
docker compose ps         # all healthy?
```

Frontend (run in `frontend/`):
```bash
npm install
npm run dev        # http://localhost:5173
npm run build      # tsc -b && vite build
npm run lint
```

Backend (run in `backend/`):
```bash
dotnet build
dotnet run --project Api --launch-profile http      # http://localhost:5121, OpenAPI at /openapi/v1.json
```

Migrations (run in `backend/Api/`). **Always add one for each provider** while both exist:
```bash
dotnet ef migrations add <Name> --context SqlServerDbContext --output-dir Migrations/SqlServer
dotnet ef migrations add <Name> --context SqliteDbContext   --output-dir Migrations/Sqlite
dotnet ef migrations has-pending-model-changes --context SqlServerDbContext
```
In Development the API runs `Database.Migrate()` on startup.

## Environment

- Windows 10, .NET SDK 10 (also 9), Node 24, `dotnet-ef` 10 installed globally.
- SQL Server 2025 local instance `DESKTOP-2ERCDGB\MSSQLSERVER01`, Windows auth (`Trusted_Connection=True;TrustServerCertificate=True`).
  SQL Browser is disabled; local connections still work. `sqlcmd` is available for inspecting data.
- Provider is set by `Database:Provider` in `backend/Api/appsettings.json` (`SqlServer` default, or `Sqlite`);
  override per run with env var `Database__Provider`.
- Docker compose project name is **`rsv-learning`**. Never rename it to `redstarvibe`: another local project
  (`E:\Practice\RealAIProject\RedStarVibeProject\RedStarVibe`) uses that name, and sharing a name shares containers and
  volumes (it already happened once with its `redis-data` volume). Never run `docker compose down -v` or `--remove-orphans`
  without checking `docker compose ps` first.
- Ports: API 5121 (http) / 7065 (https), Vite 5173. CORS allowed origins are in `Cors:AllowedOrigins`.

## Architecture rules (from `plan.md`; apply to all new code)

- Modules never read another module's tables; use public interfaces or integration events.
- Business rules such as visibility, privacy, block lists and tenant filters go **in the SQL query**, never as in-memory filters.
- Every write that triggers side effects goes through the **outbox** (from Phase 6).
- Read queries use `AsNoTracking()`; keep DTOs separate from entities; never return entities from endpoints.
- Errors are returned as ProblemDetails; endpoints validate input; POSTs that create things support idempotency keys (Phase 1+).
- Store and return `DateTime` as UTC (see the converter in `AppDbContext`).
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
- `app.db*` (SQLite), `bin/`, `obj/`, `node_modules/`, `dist/` are git-ignored.

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
- `deploy.yml` deploys the frontend to GitHub Pages on pushes to `production` that touch `frontend/**`.
- Hotfix: branch from `production`, PR into `production`, then merge `production` back into `staging` and `dev`.

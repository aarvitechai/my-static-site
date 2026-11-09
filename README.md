# Red Star Vibe: React + ASP.NET Core

Roadmap: [`plan.md`](plan.md) · Decisions: [`docs/adr/`](docs/adr/) · Product: [`docs/product/`](docs/product/) · [Contributing](CONTRIBUTING.md) · [Security](SECURITY.md)

## Quick start
```bash
cp .env.example .env          # then set real passwords
docker compose up -d          # Redis, Seq (http://localhost:5380), Mailpit (http://localhost:8025)

dotnet run --project src/Hosts/RedStarVibe.Api --launch-profile http   # API  -> http://localhost:5080
cd web && npm install && npm run dev                                   # web  -> http://localhost:5173
```
SQL Server: local instance `DESKTOP-2ERCDGB\MSSQLSERVER01` (Windows auth).
Smoke test: `GET http://localhost:5080/api/v1/ping` (or open `src/Hosts/RedStarVibe.Api/RedStarVibe.Api.http`).

## Layout
```
RedStarVibe.slnx              .NET solution (open in Visual Studio / Rider)
src/
  Hosts/RedStarVibe.Api/      HTTP API host (Minimal APIs under /api/v1)
  BuildingBlocks/             shared, domain-free code (from Phase 1 slice 4)
  Modules/                    one folder per module (from Phase 1 slice 3)
tests/                        unit, integration, architecture tests (from Phase 1 slice 5)
web/                          React 19 + TypeScript + Vite
docs/                         ADRs, product docs
```
In development Vite proxies `/api/*` to the API, so the browser and API share an origin and no CORS is needed.

## CI/CD
- `.github/workflows/ci.yml`: build (warnings are errors), NuGet and npm vulnerability audit, lint, tests, on PRs into `dev` / `staging` / `production`.
- `.github/workflows/deploy.yml`: deploys `web/` to GitHub Pages on pushes to `production`.

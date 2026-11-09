# Red Star Vibe: React + ASP.NET Core

Roadmap: [`plan.md`](plan.md) · Decisions: [`docs/adr/`](docs/adr/) · Product: [`docs/product/`](docs/product/) · [Contributing](CONTRIBUTING.md) · [Security](SECURITY.md)

## Quick start
```bash
cp .env.example .env          # then set real passwords
docker compose up -d          # Redis, Seq (http://localhost:5380), Mailpit (http://localhost:8025)

# One-time: create the database and store the Redis connection string (it holds the password) outside the repo
sqlcmd -S "DESKTOP-2ERCDGB\MSSQLSERVER01" -E -C -Q "IF DB_ID('RedStarVibe') IS NULL CREATE DATABASE RedStarVibe"
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379,password=<REDIS_PASSWORD from .env>,abortConnect=false" --project src/Hosts/RedStarVibe.Api

dotnet run --project src/Hosts/RedStarVibe.Api --launch-profile http   # API  -> http://localhost:5080
cd web && npm install && npm run dev                                   # web  -> http://localhost:5173
```
SQL Server: local instance `DESKTOP-2ERCDGB\MSSQLSERVER01` (Windows auth). Requests to try: `src/Hosts/RedStarVibe.Api/RedStarVibe.Api.http`.

| URL | What |
|---|---|
| http://localhost:5080/api/v1/ping | smoke test |
| http://localhost:5080/health/live, `/health/ready` | liveness; readiness (SQL Server + Redis, 503 if either is down) |
| http://localhost:5080/scalar | interactive API explorer (Development only) |
| http://localhost:5380 | Seq: structured logs. Search e.g. `CorrelationId = 'abc'` (first login forces a password change) |

## Layout
```
RedStarVibe.slnx              .NET solution (open in Visual Studio / Rider)
src/
  Hosts/RedStarVibe.Api/      HTTP API host (Minimal APIs under /api/v1)
  BuildingBlocks/             shared code: Result/Error, cursors, current user (no deps); .Persistence (EF);
                              .Web (validation, idempotency keys, rate limiting, Result -> HTTP)
  Modules/Identity/           first module: users and roles (schema `identity`, migration 0001_Init)
tests/                        BuildingBlocks tests (run: dotnet test --solution RedStarVibe.slnx); more in slice 5
web/                          React 19 + TypeScript + Vite
docs/                         ADRs, product docs
```
In development Vite proxies `/api/*` to the API, so the browser and API share an origin and no CORS is needed.

## CI/CD
- `.github/workflows/ci.yml`: build (warnings are errors), NuGet and npm vulnerability audit, lint, tests, on PRs into `dev` / `staging` / `production`.
- `.github/workflows/deploy-cloudflare.yml`: builds `web/` and deploys it to **Cloudflare Pages**
  ([ADR-0007](docs/adr/0007-frontend-hosting-cloudflare-pages.md), [ADR-0008](docs/adr/0008-deploy-web-with-github-actions.md)):
  `production` → live site, `dev` and `staging` → preview URLs. Security headers live in `web/public/_headers`.

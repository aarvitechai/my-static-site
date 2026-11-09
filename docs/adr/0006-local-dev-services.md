# ADR-0006: Local dev services: Docker for Redis, Seq and Mailpit; local SQL Server instance

- **Status:** Accepted
- **Date:** 2026-11-09

## Context
plan.md Phase 0.3 lists SQL Server, Redis, Seq and MailHog in `docker-compose.yml`. The owner already runs
SQL Server 2025 locally (`DESKTOP-2ERCDGB\MSSQLSERVER01`), and a SQL Server container needs about 2 GB of RAM.
MailHog has been unmaintained for years. Another local project already uses the compose project name `redstarvibe`.

## Decision
- `docker-compose.yml` at the repo root runs **Redis 7.4, Seq and Mailpit** only. Mailpit replaces MailHog
  (a maintained drop-in replacement with the same SMTP-catcher role).
- Development uses the **local SQL Server instance** with Windows authentication. Integration tests use a
  **Testcontainers** SQL Server, so CI and other machines don't depend on it.
- Compose project name: **`rsv-learning`** (unique). Sharing a project name with another compose project makes them
  share containers and volumes. This happened once during Phase 0 setup: this project's Redis mounted the other
  project's `redis-data` volume before the rename.
- Hardening: all ports bound to `127.0.0.1`; Redis `requirepass` and a Seq admin password come from a git-ignored
  `.env` (template: `.env.example`); pinned image tags; `no-new-privileges`; health checks.

## Consequences
- Positive: lower RAM use; uses the owner's existing SQL Server; services aren't exposed to the LAN.
- Negative: the dev database isn't reproducible from compose alone; a new machine needs SQL Server installed or a connection-string override.
- Follow-ups: if a second developer joins, add an optional `sql` compose profile.

## Alternatives considered
- **SQL Server in compose (as plan.md says)**: fully reproducible, but duplicates the local instance and costs RAM.
- **Keep MailHog**: unmaintained; Mailpit is compatible.

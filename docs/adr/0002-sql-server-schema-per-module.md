# ADR-0002: One SQL Server database, one schema per module

- **Status:** Accepted
- **Date:** 2026-11-09

## Context
ADR-0001 requires that modules own their data. Separate databases per module make local setup, backups and
reporting heavier, and rule out the cheap single-database transaction used by the outbox. The owner already
runs SQL Server 2025 locally, and SQL Server 2025 adds a native `vector` type, which ADR-0004 needs.

## Decision
- **One SQL Server database** (`RedStarVibe`), with **one schema per module** (`identity`, `social`, `chat`,
  `events`, `wedding`, `vendors`, `business`, `bookings`, `search`, `ai`, `notifications`, `media`), plus `outbox`
  for shared infrastructure tables when needed.
- Each module has its **own DbContext** with `HasDefaultSchema("<module>")` and its own migrations history table
  in its schema. Migrations are named `0001_Init`, `0002_...`.
- A module's DbContext maps only its own tables. Reading across modules goes through contracts or events (ADR-0001).
- SQL Server only. The SQLite option from the learning scaffold is dropped: SQLite has no schemas, no `geography`,
  no full-text or vector search, and no `UPDLOCK/READPAST` locking, all of which later phases need.
- Development uses the local instance `DESKTOP-2ERCDGB\MSSQLSERVER01` (ADR-0006). Integration tests use a
  disposable SQL Server container via Testcontainers.
- Production follows least privilege: the app's login has DML rights only. Migrations run as a separate job with a DDL-capable login.

## Consequences
- Positive: simple operations; atomic "change + outbox message" in one transaction; one backup; easy to split a schema into its own database later.
- Negative: everything shares one server's resources; a noisy module can affect others. Mitigated by indexes, query reviews and caching.
- Follow-ups: optional per-module database users that can only access their own schema (Phase 17).

## Alternatives considered
- **Database per module**: stronger isolation, but no local transactions with the outbox and much heavier setup.
- **PostgreSQL**: also a good fit, but the plan and the owner's skills target SQL Server, and SQL Server 2025 vectors avoid a separate vector store.

# ADR-0001: Modular monolith with separate host processes

- **Status:** Accepted
- **Date:** 2026-11-09

## Context
RedStarVibe spans many domains (social, chat, events, weddings, vendors, business, bookings, search, AI).
It is built by one developer at about 10-12 hours a week. Microservices would add network failure modes,
distributed transactions, per-service deployment and observability overhead before there are any users.
A plain monolith tends to rot into a "big ball of mud" where everything reads everything.

## Decision
Build a **modular monolith**:
- One solution, with a module per bounded context under `src/Modules/<Name>/`. Each module is split into
  `Domain`, `Application`, `Infrastructure` and `Endpoints` projects.
- Modules talk only through **public contracts** (a `<Name>.Contracts` project with interfaces and DTOs) or
  **integration events** via the outbox. A module never references another module's internals or reads its tables.
- Shared, domain-free code lives in `src/BuildingBlocks/` (Result, outbox, idempotency, auth, caching).
- Separate **host processes** reuse the same modules: `Api` (HTTP), `Realtime` (SignalR), `Worker` (background
  jobs and outbox dispatch), `Mcp` (MCP server). They scale and restart independently.
- Boundaries are enforced by **architecture tests** (NetArchTest) that run in CI.

## Consequences
- Positive: one deployable unit to start; in-process calls; a single database transaction inside a module;
  refactoring across modules is cheap; a module can be extracted into a service later because its boundary already exists.
- Negative: discipline is needed, so architecture tests are mandatory; one shared runtime means a memory leak in one module affects all.
- Follow-ups: architecture tests in Phase 1.1; outbox in Phase 6; extraction only when metrics demand it (plan.md, Phase 18).

## Alternatives considered
- **Microservices from day one**: too much operational cost for one developer, and the service boundaries are still unknown.
- **Traditional layered monolith (one Domain/Application/Infrastructure for everything)**: no module boundaries, so it couples over time.

# Architecture Decision Records

An ADR records one significant decision: the context, what we chose, and what it costs us.
ADRs are immutable once **Accepted**. To change a decision, write a new ADR that supersedes the old one.

| # | Title | Status |
|---|---|---|
| [001](0001-modular-monolith.md) | Modular monolith with separate host processes | Accepted |
| [002](0002-sql-server-schema-per-module.md) | One SQL Server database, one schema per module | Accepted |
| [003](0003-auth-cookie-and-jwt-refresh.md) | Auth: HttpOnly cookie session + short-lived JWT with rotating refresh | Accepted |
| [004](0004-search-full-text-plus-vectors.md) | Search: SQL Server full-text + vector, hybrid ranking | Accepted |
| [005](0005-react-spa-with-ssr-public-pages.md) | React SPA for the app, SSR only for public SEO pages | Accepted |
| [006](0006-local-dev-services.md) | Local dev services: Docker for Redis/Seq/Mailpit, local SQL Server | Accepted |

New ADR: copy [`template.md`](template.md) to `NNNN-short-title.md` and add a row above.

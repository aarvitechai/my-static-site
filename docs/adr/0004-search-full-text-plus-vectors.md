# ADR-0004: Search: SQL Server full-text + vector search with hybrid ranking

- **Status:** Accepted
- **Date:** 2026-11-09

## Context
Users search posts, businesses, vendors and events with keyword queries ("caterer Delhi") and fuzzy intent
("outdoor wedding photographer on a budget"). Results must respect visibility and block rules, and search must
keep working when the AI or embedding provider is down. Running Elasticsearch or a separate vector database
adds infrastructure that one developer must operate.

## Decision
- **Keyword**: SQL Server Full-Text Search (`CONTAINSTABLE` / `FREETEXTTABLE`) on searchable columns, per module.
- **Semantic**: embeddings stored in a SQL Server 2025 **`vector`** column, generated asynchronously by the Worker
  after an outbox event (never in the request path).
- **Hybrid ranking**: combine keyword and vector results with **Reciprocal Rank Fusion** (RRF).
- **Security in SQL**: visibility, block and tenant filters are part of the SQL query, applied before ranking.
  Private data and direct messages are never embedded or indexed.
- **Degradation**: a provider switch falls back to keyword-only when embeddings are unavailable.
- **Geo**: `geography` columns with spatial indexes for "near me".
- Behind an `ISearchProvider` interface, so Azure AI Search or Qdrant can replace it if scale demands.

## Consequences
- Positive: no extra infrastructure; transactional consistency with the source data; the access rules live in one place.
- Negative: SQL Server full-text is less sophisticated than dedicated engines (typo tolerance, analyzers); vector search shares database resources.
- Follow-ups: implement in Phase 12; measure relevance against a fixed query set.

## Alternatives considered
- **Elasticsearch / OpenSearch**: better relevance tuning, but another cluster to run and keep in sync.
- **Separate vector DB (Qdrant, Pinecone)**: fallback for older SQL Server versions; unnecessary on 2025.

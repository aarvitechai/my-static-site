# ADR-0008: Deploy the web app to Cloudflare Pages from GitHub Actions

- **Status:** Accepted
- **Date:** 2026-11-09
- **Supersedes:** the "Cloudflare builds directly from GitHub" part of ADR-0007 (hosting on Cloudflare Pages still stands)

## Context
With Cloudflare's Git integration (ADR-0007), every build after the app moved into a subfolder failed. The build
log and settings live only in the Cloudflare dashboard, so failures can't be seen or fixed from the repo or by
automation. Meanwhile the same code builds cleanly in GitHub Actions CI. Cloudflare's builds also ran regardless of CI results.

## Decision
- `.github/workflows/deploy-cloudflare.yml` deploys `web/` on pushes to `dev`, `staging` and `production`
  (only when `web/**` or the workflow changes; it can also be run manually).
- The workflow repeats CI's web gates (npm ci, production-dependency audit, lint, build), then uploads `web/dist`
  with a **pinned Wrangler version** (`wrangler pages deploy`).
- It reads the project's production branch from the Cloudflare API, so a `production` push always deploys to the live
  site, and refuses to deploy `dev` or `staging` if either is misconfigured as Cloudflare's production branch.
- After deploying, a smoke test fetches the deployment URL and checks the page title and the CSP header.
- Credentials: repository secrets `CLOUDFLARE_API_TOKEN` (scoped to **Cloudflare Pages: Edit** only) and
  `CLOUDFLARE_ACCOUNT_ID`. The workflow has read-only repo permissions.
- Cloudflare's own automatic Git builds are **disabled**, so the two never race.
- Each branch maps to a GitHub environment (`dev`, `staging`, `production`), which records deployment history and
  can later require approval for `production`.

## Consequences
- Positive: one place for build and deploy logs (GitHub); deploys are gated by the same checks as CI; settings are
  versioned in the repo; failures are diagnosable without dashboard access.
- Negative: a Cloudflare API token must be created, stored and rotated. The Cloudflare project name is hard-coded in the workflow.
- Follow-ups: add required reviewers to the `production` GitHub environment once a second person joins; rotate the API token yearly.

## Alternatives considered
- **Keep Cloudflare's Git integration**: no secret to manage, but logs and settings are dashboard-only and it already failed silently.
- **`cloudflare/wrangler-action`**: a convenient wrapper, but another third-party action in the supply chain. Calling the pinned Wrangler CLI directly is just as simple.

# ADR-0007: Host the web app on Cloudflare Pages

- **Status:** Accepted. The build/deploy method is superseded by [ADR-0008](0008-deploy-web-with-github-actions.md)
- **Date:** 2026-11-09

## Context
The web app was deployed two ways at once: a GitHub Actions workflow to GitHub Pages (`deploy.yml`), and
Cloudflare Pages connected to the repo, which already serves the custom domain `redstarvibe.com`. The GitHub
Pages deploy kept failing on its environment branch rule. GitHub Pages also can't set response headers, so it
can't send a Content Security Policy or HSTS. plan.md already chooses Cloudflare for CDN and WAF (§1, Phase 17.5).

## Decision
- **Cloudflare Pages** is the only host for `web/`. The GitHub Pages workflow is removed.
- Cloudflare builds directly from GitHub (no Actions workflow). Settings, kept in the Cloudflare dashboard:

  | Setting | Value |
  |---|---|
  | Root directory | `web` |
  | Build command | `npm run build` |
  | Build output directory | `dist` |
  | Environment variable | `NODE_VERSION=24` |
  | Production branch | `production` |
  | Preview branches | `dev`, `staging` only |

- Branch → environment: `production` → production site (custom domain); `staging` and `dev` → preview URLs
  (`<branch>.<project>.pages.dev`).
- **Security headers** ship with the site in `web/public/_headers`: a strict same-origin CSP, HSTS,
  `X-Content-Type-Options`, `X-Frame-Options: DENY`, `Referrer-Policy`, `Permissions-Policy`, COOP.
  Hashed `/assets/*` are cached as immutable. Preview URLs send `X-Robots-Tag: noindex`.
- GitHub Actions `ci.yml` remains the quality gate. Cloudflare only builds and serves.

## Consequences
- Positive: one deploy path; free preview environments for `dev` and `staging`; CDN, TLS and WAF at the edge; headers under version control.
- Negative: build settings live in the Cloudflare dashboard, not in the repo (they're recorded in the table above).
  Cloudflare builds even if CI fails, so promote to `production` only after CI is green (branch protection enforces this).
- Follow-ups: when the API is deployed, add its origin to the CSP `connect-src` and set `VITE_API_URL` per environment in
  Cloudflare. Consider Cloudflare Access to make the `dev` and `staging` previews private.

## Alternatives considered
- **GitHub Pages**: no custom response headers, no per-branch previews, and it duplicated the Cloudflare deploy.
- **Deploying to Cloudflare from GitHub Actions (wrangler)**: keeps settings in the repo, but needs an API token
  stored as a secret. The direct Git integration is simpler for now.

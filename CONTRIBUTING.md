# Contributing

## Workflow: environment branches

```
feat/xyz ──PR──> dev ──PR──> staging ──PR──> production
                 │            │               │
            dev deploy   staging deploy   production deploy (manual approval)
```

| Branch | Purpose | How it changes |
|---|---|---|
| `dev` | **Default branch.** Integration of finished work; deploys to the dev environment | PRs from `feat/*`, `fix/*`, `chore/*`, `docs/*` |
| `staging` | Release candidate; deploys to staging for final testing | Promotion PR `dev` → `staging` only |
| `production` | What users run; deploys to production | Promotion PR `staging` → `production` only |
| `main` | **Frozen.** Kept for history; not used | Never |

- Work on short-lived branches cut from `dev` (ideally under 2 days). Names: `feat/<phase>-<short-name>`,
  `fix/<short-name>`, `chore/<short-name>`, `docs/<short-name>` (for example `feat/p1-health-checks`).
- Keep PRs small and focused on one task from `plan.md`. Unfinished features hide behind a feature flag rather than living on a long branch.
- **Into `dev`: squash-merge.** The PR title becomes the commit message, using Conventional Commits:
  `feat(identity): add email verification`, `fix(feed): cursor skips posts with equal timestamps`.
- **Promotions (`dev` → `staging` → `production`): merge commit, never squash.** Squashing a promotion rewrites
  history, so the branches drift apart and every later promotion shows false conflicts.
- **Hotfix:** branch `hotfix/<name>` from `production`, PR into `production`, then merge `production` back into
  `staging` and `dev` so the fix isn't lost on the next promotion.

## Before opening a PR
```bash
docker compose up -d                 # Redis, Seq, Mailpit
dotnet build                         # must be 0 warnings (warnings are errors)
cd frontend && npm run build && npm run lint
```
Fill in the PR template, including the security checklist.

## Recommended GitHub settings
- Settings → General → **Default branch: `dev`**.
- Settings → Rules → Rulesets (or Branches), one rule per branch:

| Rule | `dev` | `staging` | `production` | `main` |
|---|---|---|---|---|
| Require a pull request | ✅ | ✅ | ✅ | ✅ (frozen) |
| Required approvals | 0 (solo) | 0 | 1 when a second person joins | n/a |
| Require status checks (CI, from Phase 1) | ✅ | ✅ | ✅ | n/a |
| Block force pushes and deletion | ✅ | ✅ | ✅ | ✅ |
| Restrict who can push / lock branch | | | ✅ | ✅ lock (read-only) |

- Settings → Environments → `github-pages` → Deployment branches: allow **`production`** (it only allows `main` by default).
  Add required reviewers on that environment for a manual approval before each production deploy.

Also enable: Settings → Code security → **Dependabot alerts**, **Secret scanning + push protection**, **Private vulnerability reporting**.

## Decisions
Significant technical decisions get an ADR in `docs/adr/` (see its README).

# Security Policy

## Reporting a vulnerability
Please **do not open a public issue**. Report privately through GitHub:
**Security → Report a vulnerability** on this repository (private vulnerability reporting).
Include steps to reproduce, the affected endpoint or component, and the impact. You'll get a reply within 7 days.

## Baseline practices in this repo
- Secrets never live in source: local `.env` and .NET User Secrets in development, Azure Key Vault in production.
- Builds fail on any known-vulnerable NuGet package (`NuGetAudit` in `Directory.Build.props`); Dependabot raises update PRs.
- Local dev services listen on `127.0.0.1` only and require passwords where supported.
- Detailed checklist (OWASP Top 10 and API Security Top 10): `docs/security-checklist.md` (Phase 2).

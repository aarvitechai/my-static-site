# ADR-0003: Auth: short-lived access token + rotating refresh token in HttpOnly cookies

- **Status:** Accepted
- **Date:** 2026-11-09

## Context
Clients: the React SPA now, a React Native app later (Phase 18), and the MCP server (Phase 13).
Tokens kept in `localStorage` can be stolen by any XSS. Long-lived JWTs can't be revoked. We also need
"log out other devices" and a list of active sessions (plan.md, Phase 2.2).

## Decision
- **ASP.NET Core Identity** with `Guid` keys stores users. Passwords are hashed with **Argon2id** (custom
  `IPasswordHasher`) and checked against breached passwords (HIBP k-anonymity) at registration and password change.
- **Access token**: a JWT signed with an asymmetric key (ES256), valid for **10 minutes**, containing only the user ID,
  roles and session ID. Validated on every request (issuer, audience, lifetime, signature).
- **Refresh token**: a random 256-bit value stored server-side only as a **SHA-256 hash**, tied to a session/device row.
  It **rotates on every use**. If an already-used refresh token is presented again, the whole token family is revoked (theft detection).
- **Browser**: both tokens travel in **HttpOnly, Secure, SameSite=Strict** cookies. The refresh cookie is
  `Path=/api/v1/auth/refresh`. JavaScript never sees a token.
- **CSRF**: cookie-authenticated state-changing requests require the ASP.NET Core antiforgery token (header `X-XSRF-TOKEN`).
- **Native or MCP clients**: the same tokens in the `Authorization: Bearer` header, with the refresh token kept in the OS secure store. No cookies, so no CSRF.
- Login and registration are rate limited with account lockout. Responses never reveal whether an email exists.

## Consequences
- Positive: XSS can't exfiltrate tokens; sessions are revocable within 10 minutes; one design serves web, mobile and MCP.
- Negative: more moving parts than plain cookie auth (refresh endpoint, rotation, family tracking). Signing keys need rotation (Key Vault in production).
- Follow-ups: implement in Phase 2; negative tests for expired token, replayed refresh, missing CSRF and wrong role.

## Alternatives considered
- **Cookie-only session (no JWT)**: simplest for web, but awkward for native and MCP clients.
- **JWT in localStorage**: vulnerable to XSS and can't be revoked.
- **External identity provider (Entra ID, Auth0)**: good for production, but hides exactly what this project aims to learn. Social logins (Google, Facebook) are still added as external providers.

# ADR-0005: React SPA for the app; server rendering only for public SEO pages

- **Status:** Accepted
- **Date:** 2026-11-09

## Context
Most screens (feed, chat, dashboards, wedding planner) sit behind login. SEO is irrelevant there and a SPA gives
the best interactivity. Some pages must be discoverable by search engines and render well when shared:
business pages, vendor profiles, public events, digital wedding invitations, and city/category landing pages (Phase 18 growth).

## Decision
- **Phases 3-11: one React 19 + Vite SPA** in `web/` for everything, with React Router, TanStack Query, Zustand,
  React Hook Form + Zod, Tailwind, and a typed client generated from the API's OpenAPI document.
- **Public SEO pages: decided when Phase 8 starts.** Default: a separate small **Next.js** app (`web-public/`) that
  renders only public pages from the same public API endpoints. Shared UI components move to a shared package at that point.
- The SPA is served as static files behind the gateway, with a strict CSP (no inline scripts) and long-cache hashed assets.

## Consequences
- Positive: no SSR complexity while learning the core app; the public site can be optimised separately.
- Negative: possibly two frontend apps later, with shared components to maintain.
- Follow-ups: revisit at the start of Phase 8 and write a superseding ADR if the choice changes (for example React Router framework mode with SSR instead of Next.js).

## Alternatives considered
- **Next.js for everything**: SSR and server components everywhere add complexity for screens behind login.
- **Pre-rendering only**: can't cover thousands of dynamic business and vendor pages.

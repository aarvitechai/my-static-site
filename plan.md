# Red Star Vibe: React 19 + .NET 10 + SQL Server

> Social platform + Events + Weddings + Vendor marketplace + Local business growth.
> Goal: build it phase by phase, learn each production skill, ship something real.

**How to use this file**
- Work top to bottom. Tick `[x]` as you finish.
- Each phase has: Goal, Tasks (with substeps), Deliverables, Definition of Done (DoD).
- Do not start the next phase until DoD is met.
- Estimates are for one developer, about 10-12 hrs/week. Adjust as needed.

---

## 0. Product Summary

| Pillar | What users can do |
|---|---|
| Social | Profile, follow/friends, post, like, comment, share, stories, chat, notifications |
| Events | Create event, invite, RSVP, reminders, tickets, event wall |
| Wedding | Checklist, budget, guest list, seating, vendor shortlist, run-sheet |
| Vendors | Catering, photographer, tent/decor, vehicles, boutique, makeup, DJ, venue; packages, availability, quotes, booking, reviews |
| Local business | Shops/factories (e.g. ice cream): page, products, offers, posts, followers, enquiries, analytics |
| AI | Hybrid search, RAG assistant, MCP server |

**Roles:** User, Vendor, BusinessOwner, Moderator, Admin.

---

## 1. Stack and Architecture

| Layer | Choice |
|---|---|
| Frontend | React 19, TypeScript, Vite (Next.js for public SEO pages), TanStack Query, React Router, Zustand, React Hook Form + Zod, Tailwind |
| Backend | .NET 10, ASP.NET Core Minimal APIs, EF Core 10, FluentValidation, Polly, HybridCache, Serilog |
| Database | SQL Server 2022/2025, Redis |
| Realtime | SignalR (+ Redis backplane) |
| Jobs | Hangfire or Quartz + BackgroundService; outbox dispatcher |
| Storage | Cloudflare R2 or Azure Blob, Cloudflare CDN/WAF |
| AI | Claude API, embeddings (bge-m3 / Azure OpenAI), MCP C# SDK |
| Tests | xUnit, Testcontainers, Vitest, Playwright, k6 |
| DevOps | Docker, GitHub Actions / Azure DevOps, nginx or YARP, Azure Key Vault |
| Observability | OpenTelemetry, Serilog + Seq, Sentry, Prometheus/Grafana |

**Style:** modular monolith, separate processes (Api, Realtime, Worker, Mcp), one SQL schema per module.

```
RedStarVibe/
  src/
    BuildingBlocks/        Result, Outbox, Idempotency, Auth, Caching
    Modules/
      Identity/ Social/ Chat/ Events/ Wedding/
      Vendors/ Business/ Bookings/ Search/ AI/ Notifications/ Media/
        (each: Domain / Application / Infrastructure / Endpoints)
    Hosts/ Api  Realtime  Worker  Mcp
  web/                     React app
  tests/                   Unit, Integration, Architecture, E2E, Load
  docs/                    ADR, runbooks, blueprint
  infra/                   docker, nginx, pipelines
```

**Rules**
- Modules never read another module's tables. Use public interfaces or integration events.
- Business rules (visibility, tenant filter) live in SQL queries, never in memory filters.
- Every write that triggers side effects goes through the outbox.

---

## 2. Timeline Overview

| Phase | Topic | Weeks | Cumulative |
|---|---|---|---|
| 0 | Planning, repo, tooling | 1 | 1 |
| 1 | Backend skeleton | 2 | 3 |
| 2 | Identity and security | 2 | 5 |
| 3 | React foundation | 2 | 7 |
| 4 | Profiles, follow, media | 2 | 9 |
| 5 | Posts, feed, like, comment, share | 3 | 12 |
| 6 | Outbox and background workers | 1 | 13 |
| 7 | Notifications and chat | 2 | 15 |
| 8 | Business pages (local business) | 3 | 18 |
| 9 | Vendor marketplace and booking | 4 | 22 |
| 10 | Event management | 2 | 24 |
| 11 | Wedding management | 3 | 27 |
| 12 | Search (keyword, hybrid, geo) | 2 | 29 |
| 13 | AI assistant (RAG) and MCP | 3 | 32 |
| 14 | Admin and moderation | 1 | 33 |
| 15 | Testing and quality gates | 1 (plus continuous) | 34 |
| 16 | Observability | 1 | 35 |
| 17 | Deployment and production | 2 | 37 |
| 18 | Launch and growth | ongoing | 37+ |

**Fast MVP path (about 16-18 weeks):** Phases 0-5, basic 8, basic 9 (enquiry + booking, no payment), basic 10, then 17. Add chat, wedding, AI, payments after launch.

---

## Phase 0: Planning and Foundation (Week 1)

**Goal:** clear scope, clean repo, everything runs locally with one command.

- [ ] **0.1 Product definition** (drafts in `docs/product/`, awaiting owner review)
  - [ ] Write 6 personas (user, vendor, business owner, host, couple, admin)
  - [ ] Write MVP user stories (max 40), tag Must / Should / Could
  - [ ] Draw flows: sign-up, create post, book vendor, create event, wedding checklist
- [x] **0.2 Decisions (ADRs in `docs/adr/`)**
  - [x] ADR-001 modular monolith
  - [x] ADR-002 schema per module on one SQL Server DB
  - [x] ADR-003 auth: cookie + JWT refresh
  - [x] ADR-004 search: SQL full-text + vectors
  - [x] ADR-005 React SPA vs Next.js (public pages SSR)
  - [x] ADR-006 local dev services (added: local SQL Server, Mailpit instead of MailHog)
- [ ] **0.3 Repo and tooling**
  - [x] Git repo, trunk-based flow, PR template, CODEOWNERS (+ Dependabot, SECURITY.md)
  - [ ] Install .NET 10 SDK, Node 22 LTS, Docker Desktop, Bruno/Postman (.NET 10, Node 24 LTS, Docker done; Bruno/Postman not checked)
  - [x] `docker-compose.yml`: Redis, Seq, Mailpit (SQL Server = local instance, see ADR-006)
  - [x] `.editorconfig`, `Directory.Build.props` (nullable on, warnings as errors, analyzers, NuGet audit) + `global.json`

```bash
dotnet new sln -n RedStarVibe
docker compose up -d
```

**Deliverables:** repo, ADRs, compose file, user story list.
**DoD:** `docker compose up` starts all services; ADRs committed.

---

## Phase 1: Backend Skeleton (Weeks 2-3)

**Goal:** a production-shaped API host with logging, errors, health, DB and tests.

- [ ] **1.1 Solution**
  - [ ] Create projects per module (Domain, Application, Infrastructure, Endpoints) (Identity: Domain, Infrastructure, Endpoints done; Application/Contracts added with the first use case in Phase 2)
  - [x] Add architecture tests: no cross-module internals (`tests/RedStarVibe.ArchitectureTests`; assembly-reference rules via reflection, since NetArchTest has had no release since 2021)
- [x] **1.2 API host**
  - [x] Minimal API + endpoint groups under `/api/v1`
  - [x] OpenAPI + Scalar UI
  - [x] Global exception handler returning ProblemDetails
  - [x] Correlation-ID middleware, Serilog to Seq
  - [x] Health: `/health/live`, `/health/ready` (SQL, Redis)
  - [x] Options pattern, per-environment config, User Secrets locally
- [x] **1.3 Database**
  - [x] EF Core 10 SQL Server; one DbContext per module with its own schema
  - [x] Migrations per module, naming `0001_Init`
  - [x] Seed: platform roles (Identity). Vendor categories and business types move to their own modules' first migrations (Phases 9 and 8), so no module seeds another's data
  - [x] `EnableRetryOnFailure`, command timeout, NoTracking default for reads
- [x] **1.4 Building blocks**
  - [x] `Result<T>` and error types, `IClock` (.NET `TimeProvider`), `ICurrentUser`
  - [x] FluentValidation pipeline
  - [x] Cursor pagination helper
  - [x] Idempotency-key filter for POST
  - [x] Rate limiting (fixed window + per-user)

```bash
dotnet new web -n RedStarVibe.Api
dotnet add package Serilog.AspNetCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet ef migrations add 0001_Init -p src/Modules/Identity -s src/Hosts/Api
```

**Deliverables:** running API, first migration, CI build.
**DoD:** `/api/v1/ping` works; migrations apply to empty DB; logs carry correlation IDs; one Testcontainers integration test passes in CI.

---

## Phase 2: Identity and Security (Weeks 3-4)

**Goal:** safe sign-up/login with modern protections.

- [ ] **2.1 Accounts**
  - [ ] ASP.NET Core Identity, Guid keys
  - [ ] Register, verify email, login, logout, forgot/reset password
  - [ ] Argon2id hasher (custom `IPasswordHasher`)
  - [ ] Breached-password check (HIBP k-anonymity)
- [ ] **2.2 Sessions**
  - [ ] Short-lived access token + rotating refresh token (HttpOnly, Secure, SameSite)
  - [ ] CSRF antiforgery for cookie flows
  - [ ] Device/session list and revoke
- [ ] **2.3 Authorization**
  - [ ] Roles and policies; resource-based (owner-only edit)
  - [ ] Google and Facebook login (OAuth 2.0)
- [ ] **2.4 Hardening**
  - [ ] Security headers: CSP with per-request nonce, HSTS, nosniff, Referrer-Policy
  - [ ] Login/register rate limits, lockout
  - [ ] Audit log for sensitive actions
  - [ ] `docs/security-checklist.md` (OWASP Top 10 + API Top 10)

**DoD:** negative tests pass (expired token, wrong role, missing CSRF, brute force blocked).

---

## Phase 3: React Frontend Foundation (Weeks 4-5)

**Goal:** typed, accessible, responsive app shell connected to the API.

- [ ] **3.1 Setup**
  - [ ] `npm create vite@latest web -- --template react-ts`
  - [ ] Strict TS, ESLint, Prettier, Husky + lint-staged
  - [ ] Tailwind + design tokens, dark mode
- [ ] **3.2 Architecture**
  - [ ] Feature folders: `features/auth`, `feed`, `events` ...
  - [ ] Typed API client generated from OpenAPI (`openapi-typescript`)
  - [ ] TanStack Query (server state), Zustand (UI state)
  - [ ] Route guards, error boundaries, 404/500, toasts, skeletons
- [ ] **3.3 UI kit**
  - [ ] Button, Input, Modal, Dropdown, Avatar, Card, Tabs, InfiniteList, ImageUploader
  - [ ] WCAG 2.2 AA: focus, labels, keyboard, contrast
  - [ ] Mobile bottom-nav, desktop sidebar (check 360 / 768 / 1280 px)
- [ ] **3.4 Auth screens**
  - [ ] Login, Register, Verify, Forgot/Reset
  - [ ] Refresh interceptor, multi-tab logout

**DoD:** register, login and open a protected page; Vitest and a Playwright smoke test pass in CI.

---

## Phase 4: Profiles, Follow Graph, Media (Weeks 6-7)

- [ ] **4.1 Backend**
  - [ ] Profile CRUD (avatar, cover, bio, city, interests)
  - [ ] Follow/unfollow, friend request, block, mute
  - [ ] Privacy: public / followers / private
  - [ ] People suggestions (mutual follows, same city)
- [ ] **4.2 Media pipeline (reused everywhere)**
  - [ ] Presigned direct upload to R2/Blob
  - [ ] Server validation: magic bytes, size, allow-list
  - [ ] Worker: re-encode, strip EXIF, thumbnails (ImageSharp)
  - [ ] Optional antivirus scan hook (ClamAV)
- [ ] **4.3 Frontend**
  - [ ] Profile page, edit profile, followers/following lists
  - [ ] Image cropper, upload progress

**DoD:** privacy rules enforced inside SQL queries; unsafe files rejected.

---

## Phase 5: Posts, Feed, Like, Comment, Share (Weeks 8-10)

- [ ] **5.1 Posts**
  - [ ] Text, multi-image, video, hashtags, mentions, location, business/vendor tag
  - [ ] Edit, soft delete, report
  - [ ] Visibility enforced in SQL
- [ ] **5.2 Reactions and comments**
  - [ ] Like with unique index (`PostId`, `UserId`), reaction types
  - [ ] Buffered counters: Redis first, batch flush to SQL
  - [ ] Threaded comments (depth 2), edit/delete, like comment
  - [ ] Share/repost with caption, share to chat, copy link
  - [ ] Plan partitioning for the likes table
- [ ] **5.3 Feed**
  - [ ] v1 pull: followed accounts, cursor pagination, index `(AuthorId, CreatedAt DESC)`
  - [ ] v2 push: fan-out on write via outbox into `FeedItems`; large accounts stay pull
  - [ ] HybridCache for hot pages; invalidate on events
- [ ] **5.4 Stories** (24h expiry via scheduler)
- [ ] **5.5 Frontend**
  - [ ] Infinite feed with virtualization
  - [ ] Optimistic like/comment with rollback
  - [ ] Composer, media carousel, comment drawer

**DoD:** 10k seeded posts, feed p95 under 300 ms locally, k6 baseline saved in `tests/load/`.

---

## Phase 6: Outbox and Workers (Week 13)

- [ ] **6.1 Transactional outbox**
  - [ ] `OutboxMessages` written in same transaction as the change
  - [ ] Dispatcher polls with `UPDLOCK, READPAST`, publishes, marks done
  - [ ] Inbox table (unique MessageId) for idempotent consumers
  - [ ] Retry with backoff, then `DeadLetters`; admin replay
- [ ] **6.2 Consumers**
  - [ ] Notification creator, feed fan-out, search indexer, AI indexer, email sender

**DoD:** kill the worker mid-run and restart: no lost and no duplicate notifications.

---

## Phase 7: Notifications and Chat (Weeks 14-15)

- [ ] **7.1 Notifications**
  - [ ] In-app, email, web push for like/comment/follow/booking/RSVP
  - [ ] Per-channel preferences, digest email
  - [ ] SignalR unread-count push
- [ ] **7.2 Chat**
  - [ ] 1:1 and group chat, typing, read receipts, image messages
  - [ ] SignalR + Redis backplane
  - [ ] Cursor-paginated history
  - [ ] Direct messages are never indexed for search/AI
  - [ ] Business enquiry chat with quick replies

**DoD:** two browsers chat live through two API instances.

---

## Phase 8: Business Pages / Local Business (Weeks 16-18)

- [ ] **8.1 Setup wizard** (shop, factory, ice-cream parlour, boutique...)
  - [ ] Steps: category, info, location (map), hours, photos, products, publish
  - [ ] Verification: phone OTP / document upload, admin approval, Verified badge
  - [ ] Multi-owner roles: Owner, Manager, Editor
- [ ] **8.2 Catalogue and offers**
  - [ ] Products/services with images, price, variants, stock
  - [ ] Offers with start/end dates
  - [ ] Factory/wholesale mode: MOQ, bulk price, B2B enquiry form
- [ ] **8.3 Growth tools**
  - [ ] Follow, share, verified-customer reviews, owner replies
  - [ ] Business posts in feed
  - [ ] Nearby search with SQL `geography` + spatial index
  - [ ] Analytics: reach, visits, clicks, enquiries, follower growth
  - [ ] QR code and short link
- [ ] **8.4 Frontend**
  - [ ] SSR public business page, owner dashboard, wizard, charts

**DoD:** a new ice-cream shop owner sets up a page, adds products, is found by nearby search and gets an enquiry.

---

## Phase 9: Vendor Marketplace and Booking (Weeks 19-22)

- [ ] **9.1 Vendor onboarding**
  - [ ] Types: catering, photographer/videographer, tent and decor, vehicles, boutique/tailor, makeup/beauty/mehndi, DJ, venue, florist
  - [ ] Type-specific fields via JSON attributes validated per category (catering: veg/non-veg, per-plate price; vehicle: seats, AC, driver)
  - [ ] Packages (Silver/Gold/Platinum), add-ons, portfolio, service area
- [ ] **9.2 Availability and booking**
  - [ ] Calendar, blocked dates, per-vendor capacity
  - [ ] **No double booking:** `Slots` table with unique index (VendorId, Date, SlotNo) or serializable transaction with `UPDLOCK, HOLDLOCK`
  - [ ] Flow: Enquiry, Quote, Negotiate, Advance, Confirmed, Completed, Review
  - [ ] Cancellation and refund policy engine
- [ ] **9.3 Payments**
  - [ ] Razorpay/Stripe, multi-currency
  - [ ] Webhooks: signature check + idempotency
  - [ ] Reconciliation job for failed payments
  - [ ] Payout ledger and commission
- [ ] **9.4 Trust**
  - [ ] Reviews with photos (verified bookings only), dispute/report, response-time badge
- [ ] **9.5 Frontend**
  - [ ] Search filters (city, date, budget, rating), compare vendors, booking calendar, quote chat, vendor dashboard

**DoD:** concurrency test with two users racing for the same vendor and date: exactly one succeeds.

---

## Phase 10: Event Management (Weeks 23-24)

- [ ] **10.1 Events**
  - [ ] Types: birthday, corporate, concert, community, festival, wedding function
  - [ ] Title, banner, date/time/timezone, venue or online, capacity, public/private
- [ ] **10.2 Guests and RSVP**
  - [ ] Invite by link, email, phone, followers
  - [ ] RSVP yes/no/maybe, plus-ones, meal preference
  - [ ] Event wall and event chat
  - [ ] Reminders (7 days, 1 day, 2 hours) from scheduler
- [ ] **10.3 Ticketing (optional)**
  - [ ] Free/paid, QR ticket, check-in scanner, capacity lock
- [ ] **10.4 Vendor link**
  - [ ] Attach booked vendors; vendors see event brief

**DoD:** host invites 50 guests, RSVPs update live, each reminder fires exactly once.

---

## Phase 11: Wedding Management (Weeks 25-27)

- [ ] **11.1 Workspace**
  - [ ] Couple profile, date, city, budget, shared access for partner/family
  - [ ] Functions: Engagement, Mehndi, Haldi, Sangeet, Wedding, Reception (each an Event)
- [ ] **11.2 Planner tools**
  - [ ] 12-month checklist template with due dates and assignees
  - [ ] Budget: planned vs actual per category, payments due, charts
  - [ ] Guest manager: groups (bride/groom side), invite status, table/seat plan, rooms
  - [ ] Vendor shortlist: one-click enquiry to many, compare quotes
  - [ ] Wedding-day run-sheet shared with vendors
  - [ ] Digital invitation page with RSVP and post-event gallery
- [ ] **11.3 Bundles**
  - [ ] Combined vendor packages (photographer + makeup + tent)

**DoD:** couple plans from checklist to run-sheet with live budget tracking.

---

## Phase 12: Search (Weeks 28-29)

- [ ] **12.1 Keyword:** SQL Server Full-Text on posts, businesses, vendors, events (`CONTAINS`/`FREETEXT`)
- [ ] **12.2 Vector**
  - [ ] Worker generates embeddings after outbox event
  - [ ] Store in SQL Server 2025 `vector` column, or Qdrant / Azure AI Search on older versions
  - [ ] Hybrid ranking with Reciprocal Rank Fusion
- [ ] **12.3 Rules**
  - [ ] Visibility and block rules in SQL
  - [ ] Provider switch: fall back to keyword-only when AI is down
  - [ ] Geo filter, category facets

**DoD:** "outdoor wedding photographer Delhi budget" returns relevant vendors; with AI off, search still works.

---

## Phase 13: AI Assistant (RAG) and MCP (Weeks 30-32)

- [ ] **13.1 RAG**
  - [ ] Chunk public business, vendor, event, review content with `OwnerId` and `Visibility`
  - [ ] Retrieval with tenant filter in SQL; DMs and private data never indexed
  - [ ] Prompt with citations, token budget, "I don't know" when no context
  - [ ] Use cases: wedding budget help, find caterer for 300 guests, draft a shop post
- [ ] **13.2 Guardrails**
  - [ ] Label retrieved text as untrusted, strip instructions, output filter
  - [ ] Per-user daily token cap, answer cache, cost dashboard
- [ ] **13.3 Evaluation**
  - [ ] 50+ golden questions; score faithfulness and relevance; nightly CI run
- [ ] **13.4 MCP server**
  - [ ] Tools: `search_vendors`, `get_event`, `draft_post`, `draft_quote_request`
  - [ ] Reuse application services; write tools create drafts only
  - [ ] Scoped per-user token, audit logging

**DoD:** AI answers only from allowed data; injection test suite passes; MCP tool works from Claude Code.

---

## Phase 14: Admin and Moderation (Week 33)

- [ ] Admin panel: users, business approval, vendor KYC, report queue, bans
- [ ] Moderation: report flow, auto-flag, appeals
- [ ] Feature flags, announcements, audit viewer, dead-letter replay
- [ ] Terms, Privacy Policy, data export, account deletion (DPDP Act / GDPR)

---

## Phase 15: Testing and Quality Gates (Week 34, continuous)

- [ ] **Backend:** xUnit unit, Testcontainers integration (real SQL Server + Redis), architecture tests
- [ ] **Frontend:** Vitest + Testing Library, Playwright at 360/768/1280, axe accessibility
- [ ] **Performance:** k6 for feed, search, booking; review plans with Query Store and missing-index DMVs
- [ ] **CI gates (required to merge)**
  - [ ] Build + analyzers
  - [ ] Tests with 60% minimum coverage
  - [ ] OpenAPI diff check
  - [ ] `dotnet list package --vulnerable`, `npm audit`
  - [ ] Secret scan, Docker image scan (Trivy)

---

## Phase 16: Observability (Week 35)

- [ ] OpenTelemetry traces, metrics, logs from Api, Worker, SignalR
- [ ] Sentry for React and .NET, release tracking
- [ ] Dashboards: request rate, p95, error rate, queue depth, outbox lag, booking conflicts
- [ ] SLOs: 99.5% availability, p95 under 500 ms; burn-rate alerts
- [ ] Runbooks in `docs/runbooks/`: slow DB, stuck queue, failing payment webhook, high 5xx

---

## Phase 17: Deployment and Production (Weeks 36-37)

- [ ] **17.1 Containers:** multi-stage Dockerfiles (Api, Worker, Mcp, Realtime, React + nginx), non-root, health checks
- [ ] **17.2 Environments:** Dev, Staging, Production with separate DBs and secrets
- [ ] **17.3 CI/CD**
  - [ ] PR pipeline: build, test, scan
  - [ ] Main pipeline: build images, push, auto-deploy to staging
  - [ ] Production: manual approval, rolling update, smoke test, one-click rollback
  - [ ] DB migrations as a separate job; backward compatible (expand, migrate, contract)
- [ ] **17.4 Hosting path**
  - [ ] Start: one VPS or Azure App Service + Azure SQL, Cloudflare free tier
  - [ ] Grow: separate worker, managed Redis, read replica, Container Apps / Kubernetes
- [ ] **17.5 Edge:** Cloudflare DNS, CDN, WAF, rate limits, R2 policy, Key Vault secrets, TLS
- [ ] **17.6 Data safety:** automated backups, point-in-time restore, **monthly restore drill**
- [ ] **17.7 Go-live checklist**
  - [ ] Load test passed
  - [ ] Security review done
  - [ ] Backup restore verified
  - [ ] Alerts tested
  - [ ] Rollback tested
  - [ ] Legal pages live
  - [ ] Status page and support email ready

---

## Phase 18: Launch and Growth (Week 38+)

- [ ] Beta with 20 local businesses and 5 wedding vendors; weekly feedback
- [ ] Referral/invite system, WhatsApp share, SEO pages per city and category
- [ ] Plans for businesses: Free, Pro, Premium; promoted listings; booking commission
- [ ] PWA first, then React Native on the same API
- [ ] Split into microservices only when metrics demand it (feed, chat, search first)

---

## 3. Per-Phase Learning Loop

1. **Read** official docs for the topic (1-2 hrs).
2. **Build** the smallest working slice (1-2 days).
3. **Test** it: unit + integration, one failure case.
4. **Harden:** logs, metrics, timeouts, error handling.
5. **Document:** short ADR or README note, tag a release.

**Weekly rhythm (about 10-12 hrs)**
- Mon-Tue: learn + design
- Wed-Fri: build
- Weekend: tests, docs, demo to yourself

---

## 4. Definition of Production-Ready (Final Checklist)

- [ ] All endpoints versioned, validated, rate-limited, ProblemDetails errors
- [ ] Auth hardened, secrets in Key Vault, dependencies scanned
- [ ] Idempotent writes and outbox for side effects
- [ ] Indexes reviewed with real query plans
- [ ] Health checks, graceful shutdown, timeouts, circuit breakers
- [ ] Structured logs with correlation IDs, traces, alerts on p95 and error rate
- [ ] CI gates enforced; staging deploys automatic; production approval + rollback
- [ ] Backups tested by restore
- [ ] Runbooks and ADRs written

---

## 5. Risks

| Risk | Mitigation |
|---|---|
| Scope too large | MVP path; strict DoD per phase |
| Slow feed at scale | Fan-out, cache, indexes, early k6 |
| Fake vendors/reviews | Verification, verified-booking reviews, moderation |
| AI cost / prompt injection | Caps, cache, untrusted labelling, draft-only tools |
| Data loss | Backups plus monthly restore drills |
| SQL Server cost | Developer edition locally, Azure SQL serverless in cloud |

---

## 6. Next Step

Start **Phase 0 and Phase 1 this week.** When a phase begins, ask for its detailed micro-steps with code, e.g. "Phase 6 outbox in .NET 10 with SQL Server, step by step".

# Core User Flows

> **Draft for owner review.** Diagrams use Mermaid (they render on GitHub and in VS Code with a Mermaid extension).
> Security-relevant steps are marked 🔒.

## 1. Sign-up and email verification (US-01, US-02)
```mermaid
flowchart TD
    A[Register form: email, password, display name] --> B{Valid input?}
    B -- no --> A
    B -- yes --> C[🔒 Check password against breached list]
    C -- breached --> A
    C -- ok --> D[🔒 Hash with Argon2id, create unverified account]
    D --> E[Send verification email<br/>single-use token, 24h expiry]
    E --> F[Show 'check your inbox'<br/>🔒 same message whether or not the email already exists]
    F --> G[User clicks link]
    G --> H{Token valid and unused?}
    H -- no --> I[Offer resend<br/>🔒 rate limited]
    H -- yes --> J[Mark verified, start session<br/>🔒 HttpOnly cookies]
    J --> K[Onboarding: profile photo, city, interests]
```

## 2. Create a post (US-12, US-13)
```mermaid
flowchart TD
    A[Composer: text, images, visibility] --> B[Request presigned upload URLs]
    B --> C[Browser uploads images directly to storage]
    C --> D[Submit post with media IDs<br/>🔒 Idempotency-Key header]
    D --> E{Server validation}
    E -- media type, size or length invalid --> A
    E -- ok --> F[Save post + outbox message<br/>one transaction]
    F --> G[Return 201; post appears optimistically]
    F -.-> H[Worker: process images<br/>🔒 re-encode, strip EXIF]
    F -.-> I[Worker: fan-out to followers' feeds]
    F -.-> J[Worker: notify @mentions]
```

## 3. Book a vendor (US-30, US-31, US-32)
```mermaid
sequenceDiagram
    actor U as User
    participant API
    actor V as Vendor
    U->>API: Search vendors (category, city, date, budget)
    API-->>U: Vendors available on that date
    U->>API: Send enquiry (date, guests, package)
    API-->>V: Notify new enquiry
    V->>API: Send quote (price, terms, valid until)
    API-->>U: Notify quote received
    U->>API: Accept quote (🔒 Idempotency-Key)
    Note over API: 🔒 One transaction: claim the slot (unique index on VendorId, Date, SlotNo)<br/>and create the booking. A second concurrent claim fails.
    alt slot still free
        API-->>U: Booking confirmed
        API-->>V: Booking confirmed
    else slot taken meanwhile
        API-->>U: 409 Conflict: date no longer available
    end
```

## 4. Create an event and collect RSVPs (US-34 to US-37)
```mermaid
flowchart TD
    A[Host: create event<br/>title, banner, date/time + timezone, venue/online, visibility] --> B[Event page created]
    B --> C[Generate invite link<br/>🔒 unguessable token; private events need it]
    C --> D[Share via WhatsApp, email, copy link]
    D --> E[Guest opens link]
    E --> F{Logged in?}
    F -- no --> G[Login or register, then return to event]
    F -- yes --> H[RSVP: yes / no / maybe + plus-ones]
    G --> H
    H --> I[Host dashboard updates counts live]
    H -.-> J[Scheduler: reminders at T-1 day and T-2 hours<br/>exactly once per guest]
```

## 5. Wedding checklist (post-MVP, Phase 11)
```mermaid
flowchart TD
    A[Couple creates wedding workspace: date, city, budget] --> B[Invite partner and family<br/>🔒 role per member: owner / editor / viewer]
    B --> C[Generate 12-month checklist from template, due dates relative to wedding date]
    C --> D[Assign tasks to members]
    D --> E[Task done]
    E --> F{Task linked to a vendor category?}
    F -- yes --> G[Suggest shortlisted vendors, send enquiries]
    F -- no --> H[Update progress]
    G --> H
    H --> I[Dashboard: % complete, overdue tasks, budget planned vs actual]
```

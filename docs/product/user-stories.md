# MVP User Stories

> **Draft for owner review.** Scope follows the *Fast MVP path* in `plan.md` §2: Phases 0-5, basic 8,
> basic 9 (enquiry + booking, no payment), basic 10, then 17.
> Priority uses MoSCoW: **M** = Must (MVP blocker), **S** = Should (MVP if time allows), **C** = Could (after launch).
> 38 stories (limit: 40).

Format: *As a ‹role›, I want ‹goal› so that ‹benefit›.* Each story's acceptance criteria are written when its phase starts.

## Identity and security (Phase 2)
| ID | Story | Pri |
|---|---|---|
| US-01 | As a visitor, I want to register with email and password so that I can use the platform. | M |
| US-02 | As a new user, I want to verify my email so that my account is trusted and recoverable. | M |
| US-03 | As a user, I want to log in and stay logged in safely across visits so that I don't re-enter my password constantly. | M |
| US-04 | As a user, I want to reset a forgotten password via email so that I can regain access. | M |
| US-05 | As a user, I want to see and revoke my active sessions/devices so that I can kick out a lost phone. | S |
| US-06 | As a visitor, I want to sign in with Google so that registration is one click. | S |

## Profiles and follow graph (Phase 4)
| ID | Story | Pri |
|---|---|---|
| US-07 | As a user, I want to edit my profile (photo, cover, bio, city, interests) so that people recognise me. | M |
| US-08 | As a user, I want to set my profile to public, followers-only or private so that I control who sees my content. | M |
| US-09 | As a user, I want to follow and unfollow people and businesses so that I see their posts. | M |
| US-10 | As a user, I want to block or mute someone so that I'm protected from harassment. | M |
| US-11 | As a user, I want suggestions of people and businesses in my city so that I find relevant accounts. | C |

## Posts and feed (Phase 5)
| ID | Story | Pri |
|---|---|---|
| US-12 | As a user, I want to post text with up to 10 images so that I can share moments. | M |
| US-13 | As a user, I want to choose a post's visibility so that private moments stay private. | M |
| US-14 | As a user, I want a feed of accounts I follow, newest first with infinite scroll, so that I keep up. | M |
| US-15 | As a user, I want to like and unlike posts so that I can react quickly. | M |
| US-16 | As a user, I want to comment and reply (2 levels) so that I can join conversations. | M |
| US-17 | As a user, I want to edit or delete my own posts and comments so that I can fix mistakes. | M |
| US-18 | As a user, I want to report a post or comment so that abusive content is reviewed. | M |
| US-19 | As a user, I want to share or repost with a caption, or copy a link, so that I can spread good content. | S |
| US-20 | As a user, I want hashtags and @mentions so that posts are discoverable and people get notified. | S |
| US-21 | As a user, I want 24-hour stories so that I can share casual updates. | C |

## Business pages, basic (Phase 8)
| ID | Story | Pri |
|---|---|---|
| US-22 | As a business owner, I want a step-by-step setup wizard (category, info, location, hours, photos) so that my page is live in minutes. | M |
| US-23 | As a business owner, I want to list products with price and photo so that customers know what I sell. | M |
| US-24 | As a business owner, I want to post offers with start and end dates so that promotions expire automatically. | S |
| US-25 | As a user, I want to find businesses near me by category so that I discover local shops. | M |
| US-26 | As a user, I want to send an enquiry to a business so that I can ask about products or wholesale orders. | M |
| US-27 | As a business owner, I want basic analytics (page views, enquiries, followers) so that I know if it's working. | S |

## Vendor marketplace and booking, basic (Phase 9, no payments)
| ID | Story | Pri |
|---|---|---|
| US-28 | As a vendor, I want a profile with category, service area, portfolio and packages so that clients can evaluate me. | M |
| US-29 | As a vendor, I want an availability calendar with blocked dates so that I'm only booked when free. | M |
| US-30 | As a user, I want to filter vendors by category, city, date and budget so that I find someone suitable. | M |
| US-31 | As a user, I want to send an enquiry and receive a quote so that I know the price before committing. | M |
| US-32 | As a user, I want to confirm a booking for a date, and the system must **never double-book** a vendor, so that both sides can trust the booking. | M |
| US-33 | As a user with a completed booking, I want to leave a review with photos so that others can trust the vendor. | S |

## Events, basic (Phase 10)
| ID | Story | Pri |
|---|---|---|
| US-34 | As a host, I want to create an event (title, banner, date/time/timezone, venue or online, public or private) so that people know the details. | M |
| US-35 | As a host, I want to invite people by link so that inviting is easy. | M |
| US-36 | As a guest, I want to RSVP yes, no or maybe with plus-ones so that the host can plan. | M |
| US-37 | As a guest, I want reminders 1 day and 2 hours before so that I don't forget; each fires exactly once. | S |

## Admin (Phase 14, minimal for launch)
| ID | Story | Pri |
|---|---|---|
| US-38 | As an admin, I want to review reports and approve businesses and vendors so that the platform stays trustworthy. | M |

### Explicitly out of MVP (post-launch)
Chat, notifications beyond email, wedding workspace, payments, ticketing, AI assistant, MCP server, stories (US-21), people suggestions (US-11).
Note: **in-app notifications** (Phase 7) are probably needed for engagement; consider promoting a minimal version to **S**.

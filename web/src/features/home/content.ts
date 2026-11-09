import type { IconName } from '../../components/Icon'

// Home page copy, kept apart from layout so wording changes never touch components.
// Product scope follows plan.md §0. "Later" items are on the roadmap, not in the MVP.

export interface Pillar {
  icon: IconName
  title: string
  text: string
  points: string[]
  later?: boolean
}

export const PILLARS: Pillar[] = [
  {
    icon: 'users',
    title: 'Social',
    text: 'Follow friends, creators and the places you love. Share moments that stay with the people you choose.',
    points: ['Photo posts, likes and comments', 'Public, followers-only or private', 'Block and mute in one tap'],
  },
  {
    icon: 'calendar',
    title: 'Events',
    text: 'Birthdays, meetups, festivals: create an event, share one link, and watch the RSVPs roll in.',
    points: ['RSVP with plus-ones', 'Automatic reminders', 'An event wall for every gathering'],
  },
  {
    icon: 'heart',
    title: 'Weddings',
    text: 'From roka to reception: checklist, budget, guest list and vendors in one workspace your family shares.',
    points: ['12-month planning checklist', 'Budget: planned vs actual', 'Guest list and seating'],
    later: true,
  },
  {
    icon: 'briefcase',
    title: 'Vendors',
    text: 'Caterers, photographers, decorators, makeup artists, DJs and venues. Compare packages and book with confidence.',
    points: ['Real availability calendars', 'Quotes before you commit', 'Reviews from verified bookings'],
  },
  {
    icon: 'store',
    title: 'Local business',
    text: 'The ice-cream parlour, the boutique, the neighbourhood factory. Discover what’s near you and what’s new.',
    points: ['Products, offers and updates', '“Near me” discovery', 'Ask a question in one tap'],
  },
  {
    icon: 'sparkles',
    title: 'Smart assistant',
    text: 'Ask for a caterer for 300 guests or help with a wedding budget. Answers drawn only from public listings.',
    points: ['Hybrid search that understands intent', 'Never reads your private messages', 'Drafts, never acts, without you'],
    later: true,
  },
]

export interface Audience {
  id: string
  icon: IconName
  title: string
  lead: string
  benefits: string[]
}

export const AUDIENCES: Audience[] = [
  {
    id: 'people',
    icon: 'users',
    title: 'For you and your people',
    lead: 'Keep up with friends, find what’s happening this weekend, and plan the big days together.',
    benefits: ['A feed of people and places you chose', 'Events and invites in one place', 'Find trusted vendors fast'],
  },
  {
    id: 'vendors',
    icon: 'briefcase',
    title: 'For vendors',
    lead: 'Turn enquiries into bookings without juggling WhatsApp chats and paper calendars.',
    benefits: ['Packages and portfolio that sell', 'A calendar that prevents double booking', 'Enquiry → quote → booking, tracked'],
  },
  {
    id: 'business',
    icon: 'store',
    title: 'For local businesses',
    lead: 'Get your shop online in minutes and reach customers right around the corner.',
    benefits: ['Step-by-step page setup', 'Post offers that expire on their own', 'See visits, enquiries and followers'],
  },
]

export interface Step {
  title: string
  text: string
  icon: IconName
}

export const STEPS: Step[] = [
  {
    icon: 'search',
    title: 'Discover',
    text: 'Search by city, date and budget, or browse what your friends and favourite places are sharing.',
  },
  {
    icon: 'message',
    title: 'Ask and compare',
    text: 'Send an enquiry, get clear quotes, and compare vendors side by side before you decide.',
  },
  {
    icon: 'badge',
    title: 'Book with confidence',
    text: 'Confirm a date that’s guaranteed to be yours. Our booking system never double-books a vendor.',
  },
]

export interface TrustPoint {
  icon: IconName
  title: string
  text: string
}

export const TRUST_POINTS: TrustPoint[] = [
  {
    icon: 'lock',
    title: 'You control who sees what',
    text: 'Every profile and post can be public, followers-only or private, and the rules are enforced by the database itself.',
  },
  {
    icon: 'badge',
    title: 'Verified businesses and vendors',
    text: 'Partners are checked before they get a Verified badge, and reviews come only from real, completed bookings.',
  },
  {
    icon: 'shield',
    title: 'Your private life stays private',
    text: 'Direct messages and private posts are never indexed for search or used by the assistant. We don’t sell your data.',
  },
]

export interface Category {
  icon: IconName
  name: string
  examples: string
}

// Vendor categories from plan.md Phase 9.1.
export const CATEGORIES: Category[] = [
  { icon: 'utensils', name: 'Catering', examples: 'Veg & non-veg, live counters, per-plate packages' },
  { icon: 'camera', name: 'Photo & video', examples: 'Candid, traditional, drone, pre-wedding shoots' },
  { icon: 'tent', name: 'Tent & decor', examples: 'Mandaps, stage, lighting, floral themes' },
  { icon: 'brush', name: 'Makeup & mehndi', examples: 'Bridal makeup, hair, mehndi artists' },
  { icon: 'music', name: 'DJ & music', examples: 'DJs, dhol, live bands, sound & light' },
  { icon: 'venue', name: 'Venues', examples: 'Banquet halls, lawns, farmhouses, rooftops' },
  { icon: 'car', name: 'Vehicles', examples: 'Wedding cars, guest transfers, vintage rides' },
  { icon: 'shirt', name: 'Boutiques', examples: 'Lehengas, sherwanis, tailoring, rentals' },
]

export interface RoadmapStage {
  stage: 'Now' | 'Next' | 'Later'
  title: string
  items: string[]
}

// Honest public roadmap, mirrored from plan.md phases. Update as phases complete.
export const ROADMAP: RoadmapStage[] = [
  {
    stage: 'Now',
    title: 'Foundations',
    items: ['Secure, versioned API', 'Accounts with modern login security', 'Privacy rules enforced in the database'],
  },
  {
    stage: 'Next',
    title: 'MVP launch',
    items: ['Profiles, posts and the feed', 'Local business pages and “near me”', 'Vendor enquiries, quotes and bookings', 'Events with RSVPs and reminders'],
  },
  {
    stage: 'Later',
    title: 'After launch',
    items: ['Wedding planner workspace', 'Chat and notifications', 'Smart assistant', 'Payments and ticketing'],
  },
]

export interface Faq {
  question: string
  answer: string
}

export const FAQS: Faq[] = [
  {
    question: 'Is Red Star Vibe free?',
    answer:
      'Yes for people: following, posting, events and finding vendors are free. Businesses and vendors start free too; optional paid plans with extra tools may come later.',
  },
  {
    question: 'When and where does it launch?',
    answer:
      'The beta opens city by city, starting with local businesses and wedding vendors so there’s something great to discover on day one. Everyone else joins right after.',
  },
  {
    question: 'How do you prevent double bookings?',
    answer:
      'Each vendor’s calendar is protected by the database itself: a date can only be confirmed once, even if two people try to book it at the same moment.',
  },
  {
    question: 'How are vendors and businesses verified?',
    answer:
      'Partners verify their phone number and documents before an admin approves them for a Verified badge. Reviews can only be written after a real, completed booking.',
  },
  {
    question: 'What happens to my private messages and posts?',
    answer:
      'They stay private. Direct messages and private posts are never indexed for search, never used by the smart assistant, and never sold.',
  },
  {
    question: 'I run a shop or a vendor business. How do I join?',
    answer:
      'Business and vendor sign-ups open first in the beta. Setup is a short step-by-step wizard: category, details, location, hours, photos, and you’re live.',
  },
]

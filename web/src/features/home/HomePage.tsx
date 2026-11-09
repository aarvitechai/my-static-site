import { Icon } from '../../components/Icon'
import { AUDIENCES, CATEGORIES, FAQS, PILLARS, ROADMAP, STEPS, TRUST_POINTS } from './content'
import './home.css'

const GITHUB_URL = 'https://github.com/aarvitechai/my-static-site'

export function HomePage() {
  return (
    <>
      <Hero />
      <Categories />
      <Features />
      <Audiences />
      <HowItWorks />
      <Trust />
      <Roadmap />
      <FaqSection />
      <JoinCta />
    </>
  )
}

function Hero() {
  return (
    <section className="hero" aria-labelledby="hero-title">
      <div className="container hero-grid">
        <div className="hero-copy">
          <p className="eyebrow">
            <Icon name="star" size={14} filled /> Social · Events · Weddings · Local
          </p>
          <h1 id="hero-title">
            Where your city <span className="highlight">celebrates</span>.
          </h1>
          <p className="hero-lead">
            Share moments with your people, plan events and weddings, and find trusted vendors and local
            businesses, all in one place built for how India celebrates.
          </p>
          <div className="hero-actions">
            <a href="#join" className="btn btn-primary">
              Join the beta <Icon name="arrow" size={18} />
            </a>
            <a href="#features" className="btn btn-ghost">
              See what’s inside
            </a>
          </div>
          <nav className="hero-quick" aria-label="Popular categories">
            <span>Popular:</span>
            {CATEGORIES.slice(0, 4).map((c) => (
              <a key={c.name} href="#categories" className="chip">
                <Icon name={c.icon} size={16} />
                {c.name}
              </a>
            ))}
          </nav>
          <ul className="hero-proof" aria-label="Highlights">
            <li>
              <Icon name="check" size={18} /> Verified vendors
            </li>
            <li>
              <Icon name="check" size={18} /> No double bookings
            </li>
            <li>
              <Icon name="check" size={18} /> Your privacy, your rules
            </li>
          </ul>
        </div>

        {/* Illustrative preview of the product; decorative, so hidden from screen readers. */}
        <div className="hero-visual" aria-hidden="true">
          <div className="preview-card preview-event">
            <div className="preview-banner">
              <Icon name="calendar" size={20} />
            </div>
            <div>
              <p className="preview-kicker">Sat · 7:00 PM</p>
              <p className="preview-title">Sangeet Night</p>
              <p className="preview-meta">
                <span className="avatars">
                  <span />
                  <span />
                  <span />
                </span>
                42 going · 8 maybe
              </p>
            </div>
          </div>

          <div className="preview-card preview-vendor">
            <div className="preview-avatar">LL</div>
            <div>
              <p className="preview-title">
                Lens &amp; Light Studio <Icon name="badge" size={16} className="verified" />
              </p>
              <p className="preview-meta">Wedding photography · Delhi</p>
              <p className="preview-chip">
                <Icon name="calendar" size={14} /> Available 14 Dec
              </p>
            </div>
          </div>

          <div className="preview-card preview-business">
            <div className="preview-avatar alt">MI</div>
            <div>
              <p className="preview-title">Meena’s Ice Creams</p>
              <p className="preview-meta">
                <Icon name="pin" size={14} /> 0.8 km · Open now
              </p>
              <p className="preview-offer">New: Mango Kulfi, 20% off this week</p>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}

function Categories() {
  return (
    <section id="categories" className="section section-tint" aria-labelledby="categories-title">
      <div className="container">
        <div className="section-head">
          <p className="eyebrow">Vendors for every occasion</p>
          <h2 id="categories-title">Everyone you need, in one marketplace</h2>
          <p>
            Compare packages, check real availability and get quotes from local vendors, without forty phone calls.
          </p>
        </div>

        <ul className="category-grid">
          {CATEGORIES.map((category) => (
            <li key={category.name} className="category">
              <div className="card-icon">
                <Icon name={category.icon} />
              </div>
              <div>
                <h3>{category.name}</h3>
                <p>{category.examples}</p>
              </div>
            </li>
          ))}
        </ul>
      </div>
    </section>
  )
}

function Features() {
  return (
    <section id="features" className="section" aria-labelledby="features-title">
      <div className="container">
        <div className="section-head">
          <p className="eyebrow">Everything in one place</p>
          <h2 id="features-title">One app for the moments that matter</h2>
          <p>
            No more switching between five apps and a spreadsheet. Red Star Vibe brings your social life, your events
            and the businesses that make them happen together.
          </p>
        </div>

        <ul className="pillar-grid">
          {PILLARS.map((pillar) => (
            <li key={pillar.title} className="card pillar">
              <div className="card-icon">
                <Icon name={pillar.icon} />
              </div>
              <h3>
                {pillar.title}
                {pillar.later && <span className="tag">Coming later</span>}
              </h3>
              <p>{pillar.text}</p>
              <ul className="check-list">
                {pillar.points.map((point) => (
                  <li key={point}>
                    <Icon name="check" size={16} />
                    {point}
                  </li>
                ))}
              </ul>
            </li>
          ))}
        </ul>
      </div>
    </section>
  )
}

function Audiences() {
  return (
    <section id="for-you" className="section" aria-labelledby="audiences-title">
      <div className="container">
        <div className="section-head">
          <p className="eyebrow">Who it’s for</p>
          <h2 id="audiences-title">Built for everyone in the celebration</h2>
          <p>The guests, the hosts, and the local businesses and vendors who make every occasion special.</p>
        </div>

        <div className="audience-grid">
          {AUDIENCES.map((audience) => (
            <article key={audience.id} className="card audience" aria-labelledby={`aud-${audience.id}`}>
              <div className="card-icon">
                <Icon name={audience.icon} />
              </div>
              <h3 id={`aud-${audience.id}`}>{audience.title}</h3>
              <p>{audience.lead}</p>
              <ul className="check-list">
                {audience.benefits.map((benefit) => (
                  <li key={benefit}>
                    <Icon name="check" size={16} />
                    {benefit}
                  </li>
                ))}
              </ul>
            </article>
          ))}
        </div>
      </div>
    </section>
  )
}

function HowItWorks() {
  return (
    <section id="how-it-works" className="section section-tint" aria-labelledby="how-title">
      <div className="container">
        <div className="section-head">
          <p className="eyebrow">How it works</p>
          <h2 id="how-title">From “we should do something” to “see you there”</h2>
        </div>

        <ol className="steps">
          {STEPS.map((step, index) => (
            <li key={step.title} className="step">
              <span className="step-number" aria-hidden="true">
                {index + 1}
              </span>
              <div className="card-icon">
                <Icon name={step.icon} />
              </div>
              <h3>{step.title}</h3>
              <p>{step.text}</p>
            </li>
          ))}
        </ol>
      </div>
    </section>
  )
}

function Trust() {
  return (
    <section id="trust" className="section" aria-labelledby="trust-title">
      <div className="container trust-grid">
        <div className="section-head">
          <p className="eyebrow">Trust &amp; safety</p>
          <h2 id="trust-title">Safe by design, not as an afterthought</h2>
          <p>
            Privacy rules, verification and booking guarantees are built into the platform’s foundations, so they hold
            up even when things get busy.
          </p>
        </div>

        <ul className="trust-list">
          {TRUST_POINTS.map((point) => (
            <li key={point.title} className="trust-item">
              <div className="card-icon">
                <Icon name={point.icon} />
              </div>
              <div>
                <h3>{point.title}</h3>
                <p>{point.text}</p>
              </div>
            </li>
          ))}
        </ul>
      </div>
    </section>
  )
}

function Roadmap() {
  return (
    <section id="roadmap" className="section" aria-labelledby="roadmap-title">
      <div className="container">
        <div className="section-head">
          <p className="eyebrow">Built in the open</p>
          <h2 id="roadmap-title">What we’re building, and when</h2>
          <p>No vapourware. Here’s exactly where Red Star Vibe is today and what comes next.</p>
        </div>

        <ol className="roadmap">
          {ROADMAP.map((stage) => (
            <li key={stage.stage} className={`roadmap-stage stage-${stage.stage.toLowerCase()}`}>
              <p className="stage-label">{stage.stage}</p>
              <h3>{stage.title}</h3>
              <ul className="check-list">
                {stage.items.map((item) => (
                  <li key={item}>
                    <Icon name={stage.stage === 'Now' ? 'check' : 'arrow'} size={16} />
                    {item}
                  </li>
                ))}
              </ul>
            </li>
          ))}
        </ol>
      </div>
    </section>
  )
}

function FaqSection() {
  return (
    <section id="faq" className="section section-tint" aria-labelledby="faq-title">
      <div className="container faq-grid">
        <div className="section-head">
          <p className="eyebrow">FAQ</p>
          <h2 id="faq-title">Questions, answered</h2>
          <p>Can’t find what you’re looking for? Follow the build on GitHub and open a discussion.</p>
        </div>

        {/* <details> is an accessible accordion with no JavaScript: keyboard and screen-reader support built in. */}
        <div className="faq-list">
          {FAQS.map((faq) => (
            <details key={faq.question} className="faq">
              <summary>
                {faq.question}
                <Icon name="chevron" size={20} className="faq-chevron" />
              </summary>
              <p>{faq.answer}</p>
            </details>
          ))}
        </div>
      </div>
    </section>
  )
}

function JoinCta() {
  return (
    <section id="join" className="section" aria-labelledby="join-title">
      <div className="container">
        <div className="cta-panel">
          <div>
            <h2 id="join-title">Launching city by city</h2>
            <p>
              The beta opens first to local businesses and wedding vendors, then to everyone. Red Star Vibe is being
              built in the open: follow along and be first in line.
            </p>
          </div>
          <div className="cta-actions">
            <a href={GITHUB_URL} className="btn btn-light" target="_blank" rel="noopener noreferrer">
              Follow the build <Icon name="arrow" size={18} />
              <span className="visually-hidden">(opens GitHub in a new tab)</span>
            </a>
            <p className="cta-note">Beta sign-ups open soon.</p>
          </div>
        </div>
      </div>
    </section>
  )
}

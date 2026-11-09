import ApiStatus from '../ApiStatus'
import { Logo } from './SiteHeader'
import './layout.css'

const COLUMNS = [
  {
    title: 'Product',
    links: [
      { href: '#features', label: 'Features' },
      { href: '#how-it-works', label: 'How it works' },
      { href: '#trust', label: 'Trust & safety' },
      { href: '#roadmap', label: 'Roadmap' },
      { href: '#faq', label: 'FAQ' },
    ],
  },
  {
    title: 'For partners',
    links: [
      { href: '#categories', label: 'Vendor categories' },
      { href: '#for-you', label: 'Local businesses' },
      { href: '#join', label: 'Join the beta' },
    ],
  },
]

export function SiteFooter() {
  return (
    <footer className="site-footer">
      <div className="container footer-grid">
        <div className="footer-brand">
          <Logo />
          <p>Social, events, weddings and local businesses: everything your city celebrates, in one place.</p>
        </div>

        {COLUMNS.map((col) => (
          <nav key={col.title} aria-label={col.title}>
            <h2 className="footer-title">{col.title}</h2>
            <ul>
              {col.links.map((link) => (
                <li key={link.label}>
                  <a href={link.href}>{link.label}</a>
                </li>
              ))}
            </ul>
          </nav>
        ))}
      </div>

      <div className="container footer-bottom">
        <p>© {new Date().getFullYear()} Red Star Vibe. Built in the open.</p>
        {/* Developer aid only: never shown on the public site. */}
        {import.meta.env.DEV && <ApiStatus />}
      </div>
    </footer>
  )
}

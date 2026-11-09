import { useEffect, useState } from 'react'
import { Icon } from '../Icon'
import './layout.css'

const NAV_LINKS = [
  { href: '#categories', label: 'Vendors' },
  { href: '#features', label: 'Features' },
  { href: '#how-it-works', label: 'How it works' },
  { href: '#roadmap', label: 'Roadmap' },
  { href: '#faq', label: 'FAQ' },
]

export function Logo() {
  return (
    <a href="#top" className="logo" aria-label="Red Star Vibe, home">
      <span className="logo-mark">
        <Icon name="star" size={18} filled />
      </span>
      <span className="logo-text">
        Red Star <span>Vibe</span>
      </span>
    </a>
  )
}

export function SiteHeader() {
  const [menuOpen, setMenuOpen] = useState(false)

  // Close the mobile menu with Escape, a standard keyboard expectation for disclosure menus.
  useEffect(() => {
    if (!menuOpen) return
    const onKey = (e: KeyboardEvent) => e.key === 'Escape' && setMenuOpen(false)
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [menuOpen])

  const close = () => setMenuOpen(false)

  return (
    <header className="site-header">
      <div className="container header-inner">
        <Logo />

        <nav aria-label="Main" className={`main-nav${menuOpen ? ' is-open' : ''}`} id="main-nav">
          <ul>
            {NAV_LINKS.map((link) => (
              <li key={link.href}>
                <a href={link.href} onClick={close}>
                  {link.label}
                </a>
              </li>
            ))}
          </ul>
          <a href="#join" className="btn btn-primary btn-sm nav-cta" onClick={close}>
            Join the beta
          </a>
        </nav>

        <button
          type="button"
          className="menu-toggle"
          aria-expanded={menuOpen}
          aria-controls="main-nav"
          onClick={() => setMenuOpen((open) => !open)}
        >
          <Icon name={menuOpen ? 'close' : 'menu'} />
          <span className="visually-hidden">{menuOpen ? 'Close menu' : 'Open menu'}</span>
        </button>
      </div>
    </header>
  )
}

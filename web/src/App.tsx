import { SiteFooter } from './components/layout/SiteFooter'
import { SiteHeader } from './components/layout/SiteHeader'
import { HomePage } from './features/home/HomePage'

// One page for now. React Router arrives with the app shell in Phase 3.
export default function App() {
  return (
    <>
      <a href="#main" className="skip-link">
        Skip to content
      </a>
      <SiteHeader />
      <main id="main" tabIndex={-1}>
        <span id="top" />
        <HomePage />
      </main>
      <SiteFooter />
    </>
  )
}

import { useEffect, useState } from 'react'

// Temporary connectivity check until the real app shell arrives in Phase 3.
// In dev, Vite proxies /api to the backend (vite.config.ts); VITE_API_URL points elsewhere when deployed.
const PING_URL = `${import.meta.env.VITE_API_URL ?? ''}/api/v1/ping`

type Status = { state: 'checking' } | { state: 'up'; serverTime: string } | { state: 'down'; reason: string }

interface PingResponse {
  status: string
  serverTimeUtc: string
}

export default function ApiStatus() {
  const [status, setStatus] = useState<Status>({ state: 'checking' })

  useEffect(() => {
    // AbortController cancels the request if the component unmounts (and in React StrictMode's double-run).
    const controller = new AbortController()

    fetch(PING_URL, { signal: controller.signal, headers: { Accept: 'application/json' } })
      .then(async (res) => {
        if (!res.ok) throw new Error(`HTTP ${res.status}`)
        // Static hosts (Cloudflare Pages) answer unknown paths with index.html, so a 200 isn't proof the API exists.
        if (!res.headers.get('content-type')?.includes('application/json')) {
          throw new Error('no API at this address')
        }
        const body = (await res.json()) as PingResponse
        setStatus({ state: 'up', serverTime: new Date(body.serverTimeUtc).toLocaleTimeString() })
      })
      .catch((err: unknown) => {
        if (controller.signal.aborted) return
        setStatus({ state: 'down', reason: err instanceof Error ? err.message : 'unknown error' })
      })

    return () => controller.abort()
  }, [])

  const text =
    status.state === 'checking'
      ? 'Checking API…'
      : status.state === 'up'
        ? `API online (server time ${status.serverTime})`
        : `API offline: ${status.reason}`

  return (
    <p role="status" aria-live="polite" style={{ textAlign: 'center' }}>
      {text}
    </p>
  )
}

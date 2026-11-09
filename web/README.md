# Red Star Vibe web (React 19 + TypeScript + Vite)

## Run
```bash
npm install
npm run dev        # http://localhost:5173
npm run build      # type-check + production build
npm run lint
```
Start the API too (from the repo root): `dotnet run --project src/Hosts/RedStarVibe.Api --launch-profile http`.
In dev, Vite forwards `/api/*` to `http://localhost:5080` (see `vite.config.ts`), so code calls relative paths like `/api/v1/ping`.

## Current state
Still the Vite starter page plus `src/components/ApiStatus.tsx`, a connectivity check against `/api/v1/ping`.
The real app shell (routing, TanStack Query, Tailwind, auth screens, generated API client) is built in **Phase 3** of `plan.md`.

## Pointing at a deployed API
Set `VITE_API_URL` (for example in `.env.production.local`) to the API's base URL. When the API is on a
different origin, it must allow this site's origin in its CORS policy.

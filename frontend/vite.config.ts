import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  base: '/',
  server: {
    // During `npm run dev`, forward /api/* calls to the ASP.NET Core backend.
    proxy: {
      '/api': {
        target: 'http://localhost:5121',
        changeOrigin: true,
      },
    },
  },
})

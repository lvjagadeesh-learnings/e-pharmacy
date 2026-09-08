import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Dev-only: avoids needing CORS on the backend (localhost:5123).
    proxy: {
      '/health': 'http://localhost:5123',
      '/api': 'http://localhost:5123',
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: './src/setupTests.js',
    globals: true,
  },
})

import { defineConfig } from 'vitest/config'
import { fileURLToPath, URL } from 'node:url'
import react from '@vitejs/plugin-react'

/**
 * Vitest configuration for the YSSE Idea Box frontend.
 *
 * Unit and integration tests (Vitest + Testing Library) live under `tests/unit`
 * and `tests/integration`. End-to-end specs (Playwright) live under `tests/e2e`
 * and are excluded here; run them with `npm run test:e2e`.
 */
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./tests/setup.js'],
    include: ['tests/{unit,integration}/**/*.{test,spec}.{js,jsx}'],
    exclude: ['node_modules', 'dist', 'tests/e2e/**'],
    css: false,
    restoreMocks: true,
    coverage: {
      provider: 'v8',
      reportsDirectory: './tests/coverage',
      include: ['src/**/*.{js,jsx}'],
      exclude: ['src/**/*.test.{js,jsx}', 'src/main.jsx'],
    },
  },
})

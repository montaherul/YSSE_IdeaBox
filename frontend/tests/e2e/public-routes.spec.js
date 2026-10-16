import { test, expect } from '@playwright/test'

/**
 * Smoke coverage for public routes. These assert the app boots and the key
 * entry points render; they deliberately avoid backend-dependent data.
 */

test('landing page renders the public site', async ({ page }) => {
  await page.goto('/')
  await expect(page.getByRole('heading', { name: /every great change/i })).toBeVisible()
})

test('sign-in page renders from the login route', async ({ page }) => {
  await page.goto('/login')
  await expect(page.getByRole('heading', { name: /welcome back/i })).toBeVisible()
  await expect(page.getByLabel(/email or phone/i)).toBeVisible()
})

test('forgot-password page renders', async ({ page }) => {
  await page.goto('/forgot-password')
  await expect(page.getByRole('heading', { name: /reset your/i })).toBeVisible()
})

test('login screen links to the forgot-password flow', async ({ page }) => {
  await page.goto('/login')
  await page.getByRole('link', { name: /forgot your password/i }).click()
  await expect(page).toHaveURL(/\/forgot-password$/)
})

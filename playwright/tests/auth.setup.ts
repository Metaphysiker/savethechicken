import { test as setup, expect } from '@playwright/test';

const authFile = 'playwright/.auth/admin.json';

setup('authenticate as admin', async ({ page }) => {
  // Navigate to login page
  await page.goto('/login');

  // Fill in login credentials using accessible role selectors
  await page.getByRole('textbox', { name: 'Email*' }).fill('test@example.com');
  await page.getByRole('textbox', { name: 'Password*' }).fill('testpassword');

  // Click login button
  await page.getByRole('button', { name: 'Login' }).click();

  // Wait for navigation to complete - admin users are redirected to /admin/persons
  await page.waitForURL('/admin/persons');

  // Verify we're logged in
  await expect(page.getByText('Logout')).toBeVisible();

  // Save authentication state
  await page.context().storageState({ path: authFile });
});

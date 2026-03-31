import { test, expect } from '@playwright/test';

test.describe('SaveTheChicken Homepage', () => {
  test('should load the homepage successfully', async ({ page }) => {
    await page.goto('/');

    // Check that the page loaded
    await expect(page).toHaveTitle(/SaveTheChicken/i);
  });

  test('should navigate to new save chicken request page', async ({ page }) => {
    await page.goto('/');

    // Click on new request link/button
    await page.click('text=/new.*request/i');

    // Verify we're on the new request page
    await expect(page).toHaveURL(/\/save-chicken-requests\/new/);
  });
});

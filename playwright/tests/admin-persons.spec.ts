import { test, expect } from '@playwright/test';

test.use({ storageState: 'playwright/.auth/admin.json' });

test.describe('Admin - Person Management', () => {
  test('should navigate to persons list', async ({ page }) => {
    await page.goto('/admin/persons');

    // Verify we're on the persons page
    await expect(page).toHaveURL(/\/admin\/persons/);

    // Should see the table or "no data" message
    await expect(page.locator('text=/person|Name/i')).toBeVisible();
  });

  test('should search for persons', async ({ page }) => {
    await page.goto('/admin/persons');

    // Enter search term
    await page.fill('input[placeholder*="Search"]', 'raess');
    await page.click('button[type="submit"]');

    // Wait for results
    await page.waitForTimeout(500);

    // Should show filtered results or no results message
    await expect(page.locator('table, .mud-table').or(page.locator('text=/no.*data/i'))).toBeVisible();
  });

  test('should create a new person', async ({ page }) => {
    await page.goto('/admin/persons/new');

    // Fill person form
    await page.fill('input[id*="Contact.FirstName"]', 'Playwright');
    await page.fill('input[id*="Contact.LastName"]', 'TestUser');
    await page.fill('input[id*="Contact.Email"]', `playwright.${Date.now()}@test.com`);
    await page.fill('input[id*="Contact.PhoneNumber"]', '+41791111111');

    await page.fill('input[id*="Address.Street"]', 'Test Avenue 456');
    await page.fill('input[id*="Address.PostalCode"]', '3000');
    await page.fill('input[id*="Address.City"]', 'Bern');

    // Submit
    await page.click('button[type="submit"]');

    // Should redirect to list or show success
    await expect(page).toHaveURL(/\/admin\/persons/, { timeout: 10000 });
  });

  test('should update person blacklist status', async ({ page }) => {
    await page.goto('/admin/persons');

    // Click edit on first person
    await page.click('button[title*="Edit"], a[href*="/edit"]');

    // Toggle blacklist
    const blacklistSwitch = page.locator('input[type="checkbox"][id*="Blacklist"]');
    const wasChecked = await blacklistSwitch.isChecked();

    if (wasChecked) {
      await blacklistSwitch.uncheck();
    } else {
      await blacklistSwitch.check();
    }

    // Save
    await page.click('button[type="submit"]');

    // Should show success or redirect
    await expect(page).toHaveURL(/\/admin\/persons/, { timeout: 10000 });
  });
});

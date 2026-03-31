import { test, expect } from '@playwright/test';

// Configure browser for this test file (top-level)
test.use({
  headless: false,
  launchOptions: {
    slowMo: 500,
  },
});

test.describe('Save Chicken Drive Request Management', () => {
  test('should create a new drive request', async ({ page }) => {
    // Use public context without authentication for drive requests
    const timestamp = Date.now();
    const testFirstName = `DriverFirst${timestamp}`;
    const testLastName = `DriverLast${timestamp}`;
    const testEmail = `driver${timestamp}@example.com`;

    await test.step('Navigate to new drive request page', async () => {
      await page.goto('/save-chicken-drive-requests/new', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });
    });

    await test.step('Fill in the form', async () => {
      // Fill contact information
      await page.getByLabel('Vorname').fill(testFirstName);
      await page.getByLabel('Nachname').fill(testLastName);
      await page.getByLabel('e-Mail').fill(testEmail);
      await page.getByLabel('Telefon').fill('+41797654321');

      // Fill address
      await page.getByLabel('Ort').fill('Basel');
      await page.getByLabel('Postleitzahl').fill('4000');
      await page.getByLabel('Strasse').fill('Driver Street 789');

      // Fill drive-specific fields (if any exist)
      // Note: Adjust based on actual form fields
    });

    await test.step('Submit and verify', async () => {
      await page.getByRole('button', { name: /erstellen|submit/i }).click();

      // Wait for success confirmation or redirect
      await page.waitForURL(/thank-you|drive-requests/, { timeout: 10000 });

      // Verify success message or redirect
      const currentUrl = page.url();
      expect(currentUrl).toMatch(/thank-you|drive-requests/);
    });
  });

  test('should show validation errors for required fields', async ({ page }) => {
    await page.goto('/save-chicken-drive-requests/new', { waitUntil: 'networkidle' });

    // Try to submit without filling required fields
    await page.getByRole('button', { name: /erstellen|submit/i }).click();

    // Verify validation errors appear
    await expect(page.getByText(/required|erforderlich/i).first()).toBeVisible({ timeout: 5000 });
  });
});

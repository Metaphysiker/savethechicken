import { test, expect } from '@playwright/test';

// Configure browser for this test file (top-level)
test.use({
  headless: false,
  launchOptions: {
    slowMo: 500,
  },
});

test.describe('Admin - Save Chicken Request Management', () => {
  test('should create a save chicken request as admin', async ({ page }) => {
    const timestamp = Date.now();
    const testFirstName = `AdminFirst${timestamp}`;
    const testLastName = `AdminLast${timestamp}`;
    const testEmail = `admin${timestamp}@example.com`;

    await test.step('Navigate to new save chicken request page', async () => {
      await page.goto('/admin/save-chicken-requests/new', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });
    });

    await test.step('Fill in the form', async () => {
      // Fill contact information
      await page.getByLabel('Vorname').fill(testFirstName);
      await page.getByLabel('Nachname').fill(testLastName);
      await page.getByLabel('e-Mail').fill(testEmail);
      await page.getByLabel('Telefon').fill('+41791234567');

      // Fill address
      await page.getByLabel('Ort').fill('Zurich');
      await page.getByLabel('Postleitzahl').fill('8000');
      await page.getByLabel('Strasse').fill('Main Street 456');

      // Fill chicken information
      await page.getByLabel('Anzahl Hühner').fill('10');
      await page.getByLabel('Anzahl Hähne').fill('1');
      await page.getByLabel('Beschreibung').fill('Test description for admin created request');
    });

    await test.step('Submit and verify', async () => {
      await page.getByRole('button', { name: 'Erstellen' }).click();
      await page.waitForURL('/admin/save-chicken-requests', { timeout: 10000 });

      // Verify redirect to admin list page
      await expect(page.getByText(testFirstName)).toBeVisible();
    });
  });

  test('should show validation errors', async ({ page }) => {
    await page.goto('/admin/save-chicken-requests/new', { waitUntil: 'networkidle' });

    // Try to submit empty form
    await page.getByRole('button', { name: 'Erstellen' }).click();

    // Wait for validation errors
    await expect(page.getByText(/required|erforderlich/i).first()).toBeVisible({ timeout: 5000 });
  });
});

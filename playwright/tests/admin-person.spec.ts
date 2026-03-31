import { test, expect } from '@playwright/test';

// Configure browser for this test file (top-level)
test.use({
  headless: false,
  launchOptions: {
    slowMo: 500,
  },
});

test.describe('Admin - Person Management', () => {
  test('should create a new person', async ({ page }) => {
    const timestamp = Date.now();
    const testFirstName = `TestFirst${timestamp}`;
    const testLastName = `TestLast${timestamp}`;
    const testEmail = `test${timestamp}@example.com`;
    const testPhone = `+41791234${timestamp.toString().slice(-3)}`;

    await test.step('Navigate to new person page', async ({ }) => {
      await page.goto('/admin/persons/new', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3:has-text("Create New Person")', { state: 'visible', timeout: 10000 });
    });

    await test.step('Fill in the form', async () => {
      // Fill contact fields
      await page.getByLabel('Vorname').fill(testFirstName);
      await page.getByLabel('Nachname').fill(testLastName);
      await page.getByLabel('e-Mail').fill(testEmail);
      await page.getByLabel('Telefon').fill(testPhone);

      // Fill address fields
      await page.getByLabel('Ort').fill('Zurich');
      await page.getByLabel('Postleitzahl').fill('8000');
      await page.getByLabel('Strasse').fill('Test Street 123');
    });

    await test.step('Submit and verify', async () => {
      await page.getByRole('button', { name: 'Erstellen' }).click();
      await page.waitForURL('/admin/persons', { timeout: 10000 });

      // Verify person appears in list
      await expect(page.getByText(testFirstName)).toBeVisible();
      await expect(page.getByText(testLastName)).toBeVisible();
    });
  });

  test('should show validation errors for required fields', async ({ page }) => {
    await page.goto('/admin/persons/new', { waitUntil: 'networkidle' });
    await page.waitForSelector('h3:has-text("Create New Person")', { state: 'visible', timeout: 10000 });

    // Try to submit without filling required fields
    await page.getByRole('button', { name: 'Erstellen' }).click();

    // Verify validation errors appear (adjust based on actual validation messages)
    await expect(page.getByText(/required|erforderlich/i)).toBeVisible();
  });
});

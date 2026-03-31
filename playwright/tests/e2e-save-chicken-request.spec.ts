import { test, expect } from '@playwright/test';
import { verifyEmailInMailhog } from './helpers/mailhog.helper';
import { getMultiDateSelector } from './helpers/multi-date-selector.helper';

test.describe('E2E: Save Chicken Request Flow', () => {
  test('public user creates request, receives email, admin sees request', async ({ browser, context }) => {
    // Create a fresh context without authentication for public user
    const publicContext = await browser.newContext();
    const page = await publicContext.newPage();

    const timestamp = Date.now();
    const testEmail = `test${timestamp}@example.com`;
    const testFirstName = 'TestUser';
    const testLastName = `User${timestamp}`;
    const numberOfChickens = '5';
    let submissionTime: Date;

    // ========================================
    // STEP 1: Public user creates a save-chicken-request
    // ========================================
    await test.step('Create save-chicken-request as public user', async () => {
      await page.goto('/save-chicken-requests/new');

      // Wait for form to load using label-based selector (most reliable for MudBlazor)
      await page.getByLabel('Vorname').waitFor({ state: 'visible', timeout: 10000 });
      await page.waitForTimeout(1000); // Give Blazor time to fully initialize

      // Fill contact information using label selectors (MudBlazor fieldset legends)
      await page.getByLabel('Vorname').click();
      await page.getByLabel('Vorname').fill(testFirstName);
      await page.getByLabel('Nachname').click();
      await page.getByLabel('Nachname').fill(testLastName);
      await page.getByLabel('e-Mail').click();
      await page.getByLabel('e-Mail').fill(testEmail);
      await page.getByLabel('Telefon').click();
      await page.getByLabel('Telefon').fill('+41791234567');

      // Fill address
      await page.getByLabel('Ort').click();
      await page.getByLabel('Ort').fill('Zurich');
      await page.getByLabel('Postleitzahl').click();
      await page.getByLabel('Postleitzahl').fill('8000');
      await page.getByLabel('Strasse').click();
      await page.getByLabel('Strasse').fill('Test Street 123');

      // Fill chicken information
      await page.getByLabel('Anzahl Hühner').click();
      await page.getByLabel('Anzahl Hühner').fill(numberOfChickens);
      await page.getByLabel('Anzahl Hähne').click();
      await page.getByLabel('Anzahl Hähne').fill('0');
      await page.getByLabel('Beschreibung').click();
      await page.getByLabel('Beschreibung').fill('Large garden with chicken coop for testing');

      // Select available dates using MultiDateSelector helper
      const dateSelector = getMultiDateSelector(page);
      await dateSelector.selectDates([15, 20, 25]); // Select 3 dates in the current month

      // Check required checkboxes using partial text match
      await page.getByText('Ich bestätige, dass ich die Voraussetzungen erfülle').click();
      await page.getByText('Ich akzeptiere die Datenschutzerklärung').click();

      // PAUSE to inspect the filled form before submitting
      await page.pause();

      // Submit form using role selector (most reliable)
      await page.getByRole('button', { name: 'Erstellen' }).click();

      // Capture submission time for email verification
      submissionTime = new Date();

      // Verify success message or redirect
      await expect(
        page.getByText(/success|erfolgreich|vielen dank|thank you/i)
      ).toBeVisible({ timeout: 10000 });
    });

    // ========================================
    // STEP 2: Check Mailhog for confirmation email
    // ========================================
    await test.step('Verify confirmation email in Mailhog', async () => {
      // Use helper to verify email was sent after submission
      await verifyEmailInMailhog(page.request, {
        toEmail: testEmail,
        submittedAfter: submissionTime,
        maxWaitSeconds: 10,
        expectedContent: {
          body: /Vielen Dank|chicken|Huhn/i,
          bodyContains: [testFirstName, numberOfChickens]
        }
      });
    });

    // ========================================
    // STEP 3: Admin logs in and verifies the request
    // ========================================
    await test.step('Admin sees the save-chicken-request', async () => {
      // Create a fresh page for admin session (already authenticated via auth.setup.ts)
      const adminPage = await context.newPage();

      // Go directly to admin page (user is already logged in from auth.setup.ts)
      await adminPage.goto('/admin/save-chicken-requests', { waitUntil: 'networkidle' });

      // Verify the page loaded
      await expect(adminPage.getByRole('heading', { name: /save chicken requests|anfragen/i })).toBeVisible({ timeout: 10000 });

      // Wait for the table/grid to load (look for any table rows)
      await adminPage.waitForSelector('.mud-table-row, table tr, td', { timeout: 10000 });

      // Wait a bit more for data to populate
      await adminPage.waitForTimeout(2000);

      // Search for the newly created request by first name (email is not displayed in the grid)
      const requestRow = adminPage.locator('text=' + testFirstName);

      // Verify the request appears in the list
      await expect(requestRow.first()).toBeVisible({ timeout: 10000 });

      // Optionally verify other details are visible
      await expect(adminPage.locator('text=' + numberOfChickens).first()).toBeVisible();

      // Clean up
      await adminPage.close();
    });

    // Clean up public context
    await page.close();
    await publicContext.close();
  });
});

import { test, expect } from '@playwright/test';
import { fillSaveChickenRequestForm } from './helpers/save-chicken-request-form.helper';

// Configure browser for this test file (top-level)
test.use({
  headless: false,
  launchOptions: {
    slowMo: 500,
  },
});

test.describe('Save Chicken Request Management', () => {
  test('should create a new save chicken request', async ({ page }) => {
    const timestamp = Date.now();
    const testFirstName = `RequestFirst${timestamp}`;
    const testLastName = `RequestLast${timestamp}`;
    const testEmail = `request${timestamp}@example.com`;

    await test.step('Navigate to new save chicken request page', async () => {
      await page.goto('/save-chicken-requests/new', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });
    });

    await test.step('Fill in the save chicken request form', async () => {
      const today = new Date();
      const dayOfMonth = today.getDate();
      
      const datesToSelect = [dayOfMonth];
      if (dayOfMonth + 1 <= 28) datesToSelect.push(dayOfMonth + 1);

      await fillSaveChickenRequestForm(page, {
        contactFirstName: testFirstName,
        contactLastName: testLastName,
        contactEmail: testEmail,
        contactPhone: '+41798888888',
        addressCity: 'Lausanne',
        addressPostalCode: '1000',
        addressStreet: 'Request Street 123',
        numberOfChickens: '8',
        numberOfRoosters: '1',
        description: 'Large garden with secure chicken coop',
        alreadyReceivedChickensBefore: false,
        datesForHandOver: datesToSelect,
        message: 'Looking forward to helping chickens!',
        confirmCriteria: true,
        acceptTerms: true,
      });
    });

    await test.step('Submit and verify', async () => {
      await page.getByRole('button', { name: /erstellen|submit/i }).click();

      // Wait for success confirmation or redirect
      await page.waitForURL(/thank-you|save-chicken-requests/, { timeout: 10000 });

      // Verify success
      const currentUrl = page.url();
      expect(currentUrl).toMatch(/thank-you|save-chicken-requests/);
    });

    await test.step('Find and verify created request', async () => {
      // Navigate to save chicken requests list
      await page.goto('/save-chicken-requests-list', { waitUntil: 'networkidle' });

      // Search for the request
      await page.getByLabel('Suche').fill(testFirstName);
      await page.getByRole('button', { name: /suchen|search/i }).click();

      // Wait for view button and click it
      const viewButton = page.getByTestId('view-button').first();
      await viewButton.waitFor({ state: 'visible', timeout: 5000 });
      await viewButton.click();

      // Wait for detail page to load
      await page.waitForURL(/\/save-chicken-requests\/\d+/, { timeout: 10000 });

      // Verify all the information is displayed
      await expect(page.getByText(testFirstName)).toBeVisible();
      await expect(page.getByText(testLastName)).toBeVisible();
      await expect(page.getByText(testEmail)).toBeVisible();
      await expect(page.getByText('+41798888888')).toBeVisible();
      await expect(page.getByText('Lausanne')).toBeVisible();
      await expect(page.getByText('1000')).toBeVisible();
      await expect(page.getByText('Request Street 123')).toBeVisible();
      await expect(page.getByText(/Anzahl Hühner:\s*8/)).toBeVisible();
      await expect(page.getByText('Large garden with secure chicken coop')).toBeVisible();
      await expect(page.getByText('Looking forward to helping chickens!')).toBeVisible();
    });
  });

  test('should show validation errors for required fields', async ({ page }) => {
    await page.goto('/save-chicken-requests/new', { waitUntil: 'networkidle' });

    // Try to submit without filling required fields
    await page.getByRole('button', { name: /erstellen|submit/i }).click();

    // Verify validation errors appear
    await expect(page.getByText(/required|erforderlich/i).first()).toBeVisible({ timeout: 5000 });
  });

  test('should create and then update a save chicken request', async ({ page }) => {
    test.setTimeout(60000);
    
    const timestamp = Date.now();
    const originalFirstName = `OriginalReq${timestamp}`;
    const originalEmail = `reqoriginal${timestamp}@example.com`;

    await test.step('Create a new save chicken request', async () => {
      await page.goto('/save-chicken-requests/new', { waitUntil: 'networkidle' });

      const today = new Date();
      const dayOfMonth = today.getDate();

      await fillSaveChickenRequestForm(page, {
        contactFirstName: originalFirstName,
        contactLastName: 'OriginalLast',
        contactEmail: originalEmail,
        contactPhone: '+41793333333',
        addressCity: 'Geneva',
        addressPostalCode: '1200',
        addressStreet: 'Original St 1',
        numberOfChickens: '5',
        numberOfRoosters: '0',
        description: 'Original description',
        message: 'Original message',
        datesForHandOver: [dayOfMonth],
      });

      await page.getByRole('button', { name: /erstellen/i }).click();
      await page.waitForURL(/thank-you|save-chicken-requests/, { timeout: 10000 });
    });

    await test.step('Verify created request', async () => {
      await page.goto('/save-chicken-requests-list', { waitUntil: 'networkidle' });

      await page.getByLabel('Suche').fill(originalFirstName);
      await page.getByRole('button', { name: /suchen|search/i }).click();

      const viewButton = page.getByTestId('view-button').first();
      await viewButton.waitFor({ state: 'visible', timeout: 5000 });
      await viewButton.click();
      await page.waitForURL(/\/save-chicken-requests\/\d+/, { timeout: 10000 });

      await expect(page.getByText(originalFirstName)).toBeVisible();
      await expect(page.getByText(originalEmail)).toBeVisible();
      await expect(page.getByText('Geneva')).toBeVisible();
      await expect(page.getByText('Original description')).toBeVisible();
    });

    await test.step('Update the request', async () => {
      await page.goto('/save-chicken-requests-list', { waitUntil: 'networkidle' });
      
      await page.getByLabel('Suche').fill(originalFirstName);
      await page.getByRole('button', { name: /suchen|search/i }).click();

      const editButton = page.getByTestId('edit-button').first();
      await editButton.waitFor({ state: 'visible', timeout: 5000 });
      await editButton.click();

      // Wait for dialog to open
      await page.waitForSelector('text=Allgemeine Informationen', { state: 'visible', timeout: 10000 });

      // Update only SaveChickenRequest's own fields (NOT Person/Contact/Address)
      await page.getByTestId('chickens-count').fill('12');
      await page.getByTestId('roosters-count').fill('2');
      await page.getByTestId('description').fill('Updated description with more space');
      await page.getByTestId('message').fill('Updated message from admin');

      // Submit the update
      await page.getByRole('button', { name: /aktualisieren|update/i }).click();

      // Wait for dialog to close by waiting for the list to be visible again
      await page.waitForTimeout(2000);
      
      // Close the dialog if still open by navigating back to list
      await page.goto('/save-chicken-requests-list', { waitUntil: 'networkidle' });
    });

    await test.step('Verify updated request', async () => {
      // Search for the request using original name (Person data shouldn't change)
      await page.getByLabel('Suche').clear();
      await page.getByLabel('Suche').fill(originalFirstName);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(2000);

      const viewButton = page.getByTestId('view-button').first();
      await viewButton.waitFor({ state: 'visible', timeout: 5000 });
      await viewButton.click();
      await page.waitForURL(/\/save-chicken-requests\/\d+/, { timeout: 10000 });

      // Verify Person data is UNCHANGED (original values)
      await expect(page.getByText(originalFirstName)).toBeVisible();
      await expect(page.getByText('OriginalLast')).toBeVisible();
      await expect(page.getByText(originalEmail)).toBeVisible();
      await expect(page.getByText('Geneva')).toBeVisible();
      await expect(page.getByText('1200')).toBeVisible();

      // Verify SaveChickenRequest data IS UPDATED
      await expect(page.getByText(/Anzahl Hühner:\s*12/)).toBeVisible();
      await expect(page.getByText(/Anzahl Hähne:\s*2/)).toBeVisible();
      await expect(page.getByText('Updated description with more space')).toBeVisible();
      await expect(page.getByText('Updated message from admin')).toBeVisible();

      // Verify old SaveChickenRequest data is NOT visible
      await expect(page.getByText(/Anzahl Hühner:\s*5/)).not.toBeVisible();
      await expect(page.getByText('Original description')).not.toBeVisible();
      await expect(page.getByText('Original message')).not.toBeVisible();
    });
  });

  test('should create and then delete a save chicken request', async ({ page }) => {
    const timestamp = Date.now();
    const testFirstName = `DeleteReq${timestamp}`;
    const testEmail = `deletereq${timestamp}@example.com`;

    await test.step('Create a new save chicken request', async () => {
      await page.goto('/save-chicken-requests/new', { waitUntil: 'networkidle' });

      const today = new Date();
      const dayOfMonth = today.getDate();

      await fillSaveChickenRequestForm(page, {
        contactFirstName: testFirstName,
        contactLastName: 'DeleteLast',
        contactEmail: testEmail,
        contactPhone: '+41794444444',
        addressCity: 'Luzern',
        addressPostalCode: '6000',
        addressStreet: 'Delete St 1',
        numberOfChickens: '3',
        numberOfRoosters: '0',
        description: 'This request will be deleted',
        datesForHandOver: [dayOfMonth],
      });

      await page.getByRole('button', { name: /erstellen/i }).click();
      await page.waitForURL(/thank-you|save-chicken-requests/, { timeout: 10000 });
    });

    await test.step('Navigate to list and find the created request', async () => {
      await page.goto('/save-chicken-requests-list', { waitUntil: 'networkidle' });

      await page.getByLabel('Suche').fill(testFirstName);
      await page.getByRole('button', { name: /suchen|search/i }).click();

      const requestRow = page.locator('tr').filter({ hasText: testFirstName });
      await requestRow.waitFor({ state: 'visible', timeout: 5000 });
    });

    await test.step('Delete the request', async () => {
      const requestRow = page.locator('tr').filter({ hasText: testFirstName });
      const deleteButton = requestRow.getByTestId('delete-button');

      await deleteButton.click();

      // Wait for confirmation dialog
      await page.waitForSelector('text=wirklich löschen?', { state: 'visible', timeout: 5000 });

      // Verify warning message is visible
      await expect(page.getByText('Diese Aktion kann nicht rückgängig gemacht werden')).toBeVisible();

      // Click the "Löschen" (Delete) button in the dialog
      const confirmDeleteButton = page.getByRole('button', { name: 'Löschen' });
      await confirmDeleteButton.click();

      // Wait for deletion to complete
      await page.waitForTimeout(1000);
    });

    await test.step('Verify request is deleted', async () => {
      await page.getByLabel('Suche').fill(testFirstName);
      await page.getByRole('button', { name: /suchen|search/i }).click();

      await page.waitForTimeout(1000);

      const requestRow = page.locator('tr').filter({ hasText: testFirstName });
      await expect(requestRow).not.toBeVisible();
    });
  });
});

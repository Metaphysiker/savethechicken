import { test, expect } from '@playwright/test';
import { getMultiDateSelector } from './helpers/multi-date-selector.helper';
import { fillFarmForm } from './helpers/farm-form.helper';

// Configure browser for this test file (top-level)
test.use({
  headless: false,
  launchOptions: {
    slowMo: 500,
  },
});

test.describe('Farm Management', () => {
  test('should create a new farm', async ({ page }) => {
    const timestamp = Date.now();
    const testFarmName = `Test Farm ${timestamp}`;
    const testFirstName = `FarmOwner${timestamp}`;
    const testLastName = `Owner${timestamp}`;
    const testEmail = `farm${timestamp}@example.com`;

    await test.step('Navigate to new farm page', async () => {
      await page.goto('/farms/new', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });
    });

    await test.step('Fill in the farm form', async () => {
      const today = new Date();
      const dayOfMonth = today.getDate();
      
      // Select today and two more days (if they exist in the month)
      const datesToSelect = [dayOfMonth];
      if (dayOfMonth + 1 <= 28) datesToSelect.push(dayOfMonth + 1);
      if (dayOfMonth + 2 <= 28) datesToSelect.push(dayOfMonth + 2);

      await fillFarmForm(page, {
        name: testFarmName,
        numberOfChickens: '50',
        numberOfRoosters: '5',
        size: 'Gross',
        color: 'Braun',
        generalInformation: 'Test farm with good conditions for chickens',
        contactFirstName: testFirstName,
        contactLastName: testLastName,
        contactEmail: testEmail,
        contactPhone: '+41798765432',
        addressCity: 'Bern',
        addressPostalCode: '3000',
        addressStreet: 'Farm Road 321',
        datesForRescues: datesToSelect,
      });
    });

    await test.step('Submit and verify', async () => {
      await page.getByRole('button', { name: /erstellen|submit/i }).click();

      // Wait for success confirmation or redirect
      await page.waitForURL(/thank-you|farms/, { timeout: 10000 });

      // Verify success
      const currentUrl = page.url();
      expect(currentUrl).toMatch(/thank-you|farms/);
    });

    await test.step('Find and verify created farm', async () => {
      // Navigate to farms list
      await page.goto('/farms-list', { waitUntil: 'networkidle' });

      // Search for the farm by name
      await page.getByLabel('Suche').fill(testFarmName);
      await page.getByRole('button', { name: /suchen|search/i }).click();

      // Wait for view button and click it
      const viewButton = page.getByTestId('view-button').first();
      await viewButton.waitFor({ state: 'visible', timeout: 5000 });
      await viewButton.click();

      // Wait for detail page to load
      await page.waitForURL(/\/farms\/\d+/, { timeout: 10000 });

      // Verify all the information is displayed
      await expect(page.getByText(testFarmName)).toBeVisible();
      await expect(page.getByText(testFirstName)).toBeVisible();
      await expect(page.getByText(testLastName)).toBeVisible();
      await expect(page.getByText(testEmail)).toBeVisible();
      await expect(page.getByText('+41798765432')).toBeVisible();
      await expect(page.getByText('Bern')).toBeVisible();
      await expect(page.getByText('3000')).toBeVisible();
      await expect(page.getByText('Farm Road 321')).toBeVisible();
      await expect(page.getByText(/Anzahl Hühner:\s*50/)).toBeVisible();
      await expect(page.getByText(/Anzahl Hähne:\s*5/)).toBeVisible();
      await expect(page.getByText('Gross')).toBeVisible();
      await expect(page.getByText('Braun')).toBeVisible();
      await expect(page.getByText('Test farm with good conditions for chickens')).toBeVisible();
    });
  });

  test('should show validation errors for required fields', async ({ page }) => {
    await page.goto('/farms/new', { waitUntil: 'networkidle' });

    // Try to submit without filling required fields
    await page.getByRole('button', { name: /erstellen|submit/i }).click();

    // Verify validation errors appear
    await expect(page.getByText(/required|erforderlich/i).first()).toBeVisible({ timeout: 5000 });
  });

  test('should show validation error when number of chickens is 0', async ({ page }) => {
    const timestamp = Date.now();

    await page.goto('/farms/new', { waitUntil: 'networkidle' });

    const today = new Date();
    const dayOfMonth = today.getDate();

    // Fill form with 0 chickens (but otherwise valid)
    await fillFarmForm(page, {
      name: `Test Farm ${timestamp}`,
      numberOfChickens: '0',
      numberOfRoosters: '2',
      size: 'Klein',
      color: 'Weiss',
      generalInformation: 'Test validation',
      contactFirstName: 'Test',
      contactLastName: 'User',
      contactEmail: `test${timestamp}@example.com`,
      contactPhone: '+41791111111',
      addressCity: 'Bern',
      addressPostalCode: '3000',
      addressStreet: 'Test St 1',
      datesForRescues: [dayOfMonth],
    });

    // Try to submit
    await page.getByRole('button', { name: /erstellen|submit/i }).click();

    // Wait a bit for any validation to trigger
    await page.waitForTimeout(1000);

    // Verify we're still on the form page (not redirected)
    const currentUrl = page.url();
    expect(currentUrl).toContain('/farms/new');

    // Verify validation error appears
    const errorVisible = await page.getByText(/must be greater than 0|greater than zero|grösser als 0/i).isVisible().catch(() => false);
    if (!errorVisible) {
      // If the specific error isn't visible, check for any validation error
      await expect(page.getByText(/error|fehler/i).first()).toBeVisible({ timeout: 2000 });
    }
  });

  test('should create and then update a farm', async ({ page }) => {
    test.setTimeout(60000); // Increase timeout to 60 seconds for this complex test
    
    const timestamp = Date.now();
    const originalName = `Original Farm ${timestamp}`;
    const updatedName = `Updated Farm ${timestamp}`;
    const originalEmail = `farmoriginal${timestamp}@example.com`;
    const updatedEmail = `farmupdated${timestamp}@example.com`;

    await test.step('Create a new farm', async () => {
      await page.goto('/farms/new', { waitUntil: 'networkidle' });

      const today = new Date();
      const dayOfMonth = today.getDate();

      await fillFarmForm(page, {
        name: originalName,
        numberOfChickens: '25',
        numberOfRoosters: '3',
        size: 'Mittel',
        color: 'Weiss',
        generalInformation: 'Original farm info',
        contactFirstName: `OriginalFirst${timestamp}`,
        contactLastName: `OriginalLast${timestamp}`,
        contactEmail: originalEmail,
        contactPhone: '+41791111111',
        addressCity: 'Zurich',
        addressPostalCode: '8000',
        addressStreet: 'Original St 1',
        datesForRescues: [dayOfMonth],
      });

      await page.getByRole('button', { name: /erstellen/i }).click();
      await page.waitForURL(/thank-you|farms/, { timeout: 10000 });
    });

    await test.step('Verify created farm', async () => {
      // Navigate to farms list
      await page.goto('/farms-list', { waitUntil: 'networkidle' });

      // Search for the farm
      await page.getByLabel('Suche').fill(originalName);
      await page.getByRole('button', { name: /suchen|search/i }).click();

      // Wait for view button to appear
      const viewButton = page.getByTestId('view-button').first();
      await viewButton.waitFor({ state: 'visible', timeout: 5000 });
      await viewButton.click();
      await page.waitForURL(/\/farms\/\d+/, { timeout: 10000 });

      // Verify all original data
      await expect(page.getByText(originalName)).toBeVisible();
      await expect(page.getByText(`OriginalFirst${timestamp}`)).toBeVisible();
      await expect(page.getByText(`OriginalLast${timestamp}`)).toBeVisible();
      await expect(page.getByText(originalEmail)).toBeVisible();
      await expect(page.getByText('+41791111111')).toBeVisible();
      await expect(page.getByText('Zurich')).toBeVisible();
      await expect(page.getByText('8000')).toBeVisible();
      await expect(page.getByText('Original St 1')).toBeVisible();
      await expect(page.getByText(/Anzahl Hühner:\s*25/)).toBeVisible();
      await expect(page.getByText(/Anzahl Hähne:\s*3/)).toBeVisible();
      await expect(page.getByText('Mittel')).toBeVisible();
      await expect(page.getByText('Weiss')).toBeVisible();
      await expect(page.getByText('Original farm info')).toBeVisible();
    });

    await test.step('Update the farm', async () => {
      // Go back to list to click edit button (detail page has edit too, but let's use table edit)
      await page.goto('/farms-list', { waitUntil: 'networkidle' });
      
      // Search for the farm again
      await page.getByLabel('Suche').fill(originalName);
      await page.getByRole('button', { name: /suchen|search/i }).click();

      // Wait for edit button to be visible and click it
      const editButton = page.getByTestId('edit-button').first();
      await editButton.waitFor({ state: 'visible', timeout: 5000 });
      await editButton.click();

      // Wait for dialog to open - wait for form sections to be visible
      await page.waitForSelector('text=Allgemeine Informationen', { state: 'visible', timeout: 10000 });
      await page.waitForSelector('text=Kontakt', { state: 'visible', timeout: 10000 });
      await page.waitForSelector('text=Adresse', { state: 'visible', timeout: 10000 });

      // Update farm details - .fill() automatically clears the field first
      await page.getByLabel('Name des Betriebs').fill(updatedName);
      await page.getByLabel('Anzahl Hühner').fill('50');
      await page.getByLabel('Anzahl Hähne').fill('5');
      await page.getByLabel('Grösse').fill('Gross');
      await page.getByLabel('Farbe').fill('Braun');
      await page.getByTestId('farm-general-info').fill('Updated farm info');

      // Update contact information
      await page.getByTestId('contact-firstname').fill(`UpdatedFirst${timestamp}`);
      await page.getByTestId('contact-lastname').fill(`UpdatedLast${timestamp}`);
      await page.getByTestId('contact-email').fill(updatedEmail);

      // Update address
      await page.getByTestId('address-city').fill('Bern');
      await page.getByTestId('address-postalcode').fill('3000');
      await page.getByTestId('address-street').fill('Updated St 99');

      // Submit the update
      await page.getByRole('button', { name: /aktualisieren|update/i }).click();

      // Wait for dialog to close - wait for form sections to disappear
      await page.waitForSelector('text=Allgemeine Informationen', { state: 'hidden', timeout: 10000 });
    });

    await test.step('Verify updated farm', async () => {
      // Search for the updated farm
      await page.getByLabel('Suche').fill(updatedName);
      await page.getByRole('button', { name: /suchen|search/i }).click();

      // Wait for view button and click it
      const viewButton2 = page.getByTestId('view-button').first();
      await viewButton2.waitFor({ state: 'visible', timeout: 5000 });
      await viewButton2.click();
      await page.waitForURL(/\/farms\/\d+/, { timeout: 10000 });

      // Verify all updated data
      await expect(page.getByText(updatedName)).toBeVisible();
      await expect(page.getByText(`UpdatedFirst${timestamp}`)).toBeVisible();
      await expect(page.getByText(`UpdatedLast${timestamp}`)).toBeVisible();
      await expect(page.getByText(updatedEmail)).toBeVisible();
      await expect(page.getByText('Bern')).toBeVisible();
      await expect(page.getByText('3000')).toBeVisible();
      await expect(page.getByText('Updated St 99')).toBeVisible();
      await expect(page.getByText(/Anzahl Hühner:\s*50/)).toBeVisible();
      await expect(page.getByText(/Anzahl Hähne:\s*5/)).toBeVisible();
      await expect(page.getByText('Gross')).toBeVisible();
      await expect(page.getByText('Braun')).toBeVisible();
      await expect(page.getByText('Updated farm info')).toBeVisible();

      // Verify old data is NOT visible
      await expect(page.getByText(originalName)).not.toBeVisible();
      await expect(page.getByText(`OriginalFirst${timestamp}`)).not.toBeVisible();
      await expect(page.getByText(originalEmail)).not.toBeVisible();
      await expect(page.getByText('Original St 1')).not.toBeVisible();
      await expect(page.getByText('Original farm info')).not.toBeVisible();
      await expect(page.getByText('Zurich')).not.toBeVisible();
      await expect(page.getByText('8000')).not.toBeVisible();
    });
  });

  test('should create and then delete a farm', async ({ page }) => {
    const timestamp = Date.now();
    const testFarmName = `Delete Test Farm ${timestamp}`;
    const testEmail = `deletefarm${timestamp}@example.com`;

    await test.step('Create a new farm', async () => {
      await page.goto('/farms/new', { waitUntil: 'networkidle' });

      const today = new Date();
      const dayOfMonth = today.getDate();

      await fillFarmForm(page, {
        name: testFarmName,
        numberOfChickens: '15',
        numberOfRoosters: '2',
        size: 'Klein',
        color: 'Grau',
        generalInformation: 'This farm will be deleted',
        contactFirstName: `DeleteFirst${timestamp}`,
        contactLastName: `DeleteLast${timestamp}`,
        contactEmail: testEmail,
        contactPhone: '+41792222222',
        addressCity: 'Basel',
        addressPostalCode: '4000',
        addressStreet: 'Delete St 1',
        datesForRescues: [dayOfMonth],
      });

      await page.getByRole('button', { name: /erstellen/i }).click();
      await page.waitForURL(/thank-you|farms/, { timeout: 10000 });
    });

    await test.step('Navigate to list and find the created farm', async () => {
      await page.goto('/farms-list', { waitUntil: 'networkidle' });

      // Search for the created farm
      await page.getByLabel('Suche').fill(testFarmName);
      await page.getByRole('button', { name: /suchen|search/i }).click();

      // Wait for the farm row to be visible
      const farmRow = page.locator('tr').filter({ hasText: testFarmName });
      await farmRow.waitFor({ state: 'visible', timeout: 5000 });
    });

    await test.step('Delete the farm', async () => {
      // Find the delete button in the row
      const farmRow = page.locator('tr').filter({ hasText: testFarmName });
      const deleteButton = farmRow.getByTestId('delete-button');

      // Click delete button
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

    await test.step('Verify farm is deleted', async () => {
      // Search for the deleted farm again
      await page.getByLabel('Suche').fill(testFarmName);
      await page.getByRole('button', { name: /suchen|search/i }).click();

      // Wait a bit for search to complete
      await page.waitForTimeout(1000);

      // The farm should not be in the list anymore
      const farmRow = page.locator('tr').filter({ hasText: testFarmName });
      await expect(farmRow).not.toBeVisible();
    });
  });
});

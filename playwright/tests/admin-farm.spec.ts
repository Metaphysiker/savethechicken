import { test, expect } from '@playwright/test';
import { fillFarmForm } from './helpers/farm-form.helper';
import { createSaveChickenAction } from './helpers/save-chicken-action-form.helper';

// Use admin authentication
test.use({ storageState: 'playwright/.auth/admin.json' });

// Configure browser for this test file (top-level)
test.use({
  headless: false,
  launchOptions: {
    slowMo: 100, // Reduced from 500ms to speed up tests
  },
});

test.describe('Admin Farm Management', () => {
  // Run tests serially to avoid resource contention
  test.describe.configure({ mode: 'serial' });

  test('should create and view a farm', async ({ page }) => {
    test.setTimeout(90000); // Increased timeout for farm creation

    const timestamp = Date.now();
    const testFarmName = `Test Farm ${timestamp}`;
    const testFirstName = `FarmOwner${timestamp}`;
    const testLastName = `Owner${timestamp}`;
    const testEmail = `farm${timestamp}@example.com`;

    // Create SaveChickenAction with dates first
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(today.getDate() + 1);
    const dayAfter = new Date(today);
    dayAfter.setDate(today.getDate() + 2);

    const datesToSelect = [today.getDate(), tomorrow.getDate(), dayAfter.getDate()];

    let actionId: number;

    await test.step('Create SaveChickenAction with dates', async () => {
      const action = await createSaveChickenAction(page, {
        title: `Test Action ${timestamp}`,
        description: 'Test action for farms',
        dates: datesToSelect,
        isActive: true,
      });

      actionId = action.actionId;
      expect(actionId).toBeGreaterThan(0);
    });

    await test.step('Admin creates a farm', async () => {
      await page.goto('/admin/farms/new', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      // Wait for page to stabilize
      await page.waitForTimeout(1000);

      console.log(`Attempting to create farm with action ID: ${actionId}`);

      await fillFarmForm(page, {
        saveChickenActionId: actionId,
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
      });

      // Wait a bit and check the current URL before clicking submit
      await page.waitForTimeout(1000);
      console.log('URL before submit:', page.url());

      // Check if button is enabled
      const createButton = page.getByRole('button', { name: /erstellen|create/i });
      const isDisabled = await createButton.isDisabled();
      console.log('Create button disabled:', isDisabled);

      // Check for any visible validation errors
      const errorMessages = await page.locator('.validation-message, .mud-input-error').count();
      console.log('Validation error count:', errorMessages);

      await createButton.click();

      // Wait and check URL after click
      await page.waitForTimeout(1000);
      console.log('URL after submit:', page.url());

      // Admin should be redirected to detail page, not thank-you
      await page.waitForURL(/\/admin\/farms\/\d+/, { timeout: 10000 });
    });

    await test.step('Admin finds and views the farm', async () => {
      await page.goto('/admin/farms', { waitUntil: 'networkidle' });

      // Search for the farm
      await page.getByLabel(/suche|search/i).fill(testFarmName);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(1000);

      // Click view button
      const showButton = page.getByTestId('view-button').first();
      await showButton.waitFor({ state: 'visible', timeout: 5000 });
      await showButton.click();

      // Verify on detail page
      await page.waitForURL(/\/admin\/farms\/\d+/, { timeout: 10000 });

      // Verify farm information using data-testid where available
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

  test('should edit a farm', async ({ page }) => {
    test.setTimeout(90000); // Increased timeout for farm creation + edit

    const timestamp = Date.now();
    const originalName = `Original Farm ${timestamp}`;
    const updatedName = `Updated Farm ${timestamp}`;
    const originalEmail = `farmoriginal${timestamp}@example.com`;
    const updatedEmail = `farmupdated${timestamp}@example.com`;

    // Create SaveChickenAction with dates first
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(today.getDate() + 1);

    const datesToSelect = [today.getDate(), tomorrow.getDate()];

    let actionId: number;

    await test.step('Create SaveChickenAction with dates', async () => {
      const action = await createSaveChickenAction(page, {
        title: `Edit Test Action ${timestamp}`,
        description: 'Test action for farm edit',
        dates: datesToSelect,
        isActive: true,
      });

      actionId = action.actionId;
      expect(actionId).toBeGreaterThan(0);
    });

    await test.step('Admin creates a farm', async () => {
      await page.goto('/admin/farms/new', { waitUntil: 'networkidle' });
      await page.waitForTimeout(1000);

      await fillFarmForm(page, {
        saveChickenActionId: actionId,
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
      });

      await page.getByRole('button', { name: /erstellen/i }).click();

      // Admin should be redirected to detail page
      await page.waitForURL(/\/admin\/farms\/\d+/, { timeout: 10000 });
    });

    await test.step('Verify created farm', async () => {
      await page.goto('/admin/farms', { waitUntil: 'networkidle' });

      await page.getByLabel(/suche|search/i).fill(originalName);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(1000);

      const showButton = page.getByTestId('view-button').first();
      await showButton.waitFor({ state: 'visible', timeout: 5000 });
      await showButton.click();
      await page.waitForURL(/\/admin\/farms\/\d+/, { timeout: 10000 });

      // Verify original data
      await expect(page.getByText(originalName)).toBeVisible();
      await expect(page.getByText(`OriginalFirst${timestamp}`)).toBeVisible();
      await expect(page.getByText(originalEmail)).toBeVisible();
      await expect(page.getByText('Original farm info')).toBeVisible();
    });

    await test.step('Update the farm', async () => {
      await page.goto('/admin/farms', { waitUntil: 'networkidle' });

      await page.getByLabel(/suche|search/i).fill(originalName);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(1000);

      const editButton = page.getByTestId('edit-button').first();
      await editButton.waitFor({ state: 'visible', timeout: 5000 });
      await editButton.click();

      // Wait for edit dialog to open
      await page.waitForSelector('text=Allgemeine Informationen', { state: 'visible', timeout: 10000 });

      // Update farm details
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

      // Wait for dialog to close
      await page.waitForSelector('text=Allgemeine Informationen', { state: 'hidden', timeout: 10000 });
    });

    await test.step('Verify updated farm', async () => {
      // Search for the updated farm
      await page.getByLabel(/suche|search/i).clear();
      await page.getByLabel(/suche|search/i).fill(updatedName);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(2000);

      const showButton = page.getByTestId('view-button').first();
      await showButton.waitFor({ state: 'visible', timeout: 5000 });
      await showButton.click();
      await page.waitForURL(/\/admin\/farms\/\d+/, { timeout: 10000 });

      // Verify updated data
      await expect(page.getByText(updatedName)).toBeVisible();
      await expect(page.getByText(`UpdatedFirst${timestamp}`)).toBeVisible();
      await expect(page.getByText(updatedEmail)).toBeVisible();
      await expect(page.getByText('Updated farm info')).toBeVisible();
      await expect(page.getByText(/Anzahl Hühner:\s*50/)).toBeVisible();
      await expect(page.getByText(/Anzahl Hähne:\s*5/)).toBeVisible();

      // Verify old data is NOT visible
      await expect(page.getByText(originalName)).not.toBeVisible();
      await expect(page.getByText(`OriginalFirst${timestamp}`)).not.toBeVisible();
      await expect(page.getByText(originalEmail)).not.toBeVisible();
      await expect(page.getByText('Original farm info')).not.toBeVisible();
    });
  });

  test('should delete a farm', async ({ page }) => {
    test.setTimeout(90000);

    const timestamp = Date.now();
    const testFarmName = `Delete Test Farm ${timestamp}`;
    const testEmail = `deletefarm${timestamp}@example.com`;

    // Create SaveChickenAction with dates first
    const today = new Date();
    const datesToSelect = [today.getDate()];

    let actionId: number;

    await test.step('Create SaveChickenAction with dates', async () => {
      const action = await createSaveChickenAction(page, {
        title: `Delete Test Action ${timestamp}`,
        description: 'Test action for farm delete',
        dates: datesToSelect,
        isActive: true,
      });

      actionId = action.actionId;
      expect(actionId).toBeGreaterThan(0);
    });

    await test.step('Admin creates a farm', async () => {
      await page.goto('/admin/farms/new', { waitUntil: 'networkidle' });
      await page.waitForTimeout(1000);

      await fillFarmForm(page, {
        saveChickenActionId: actionId,
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
      });

      await page.getByRole('button', { name: /erstellen/i }).click();
      await page.waitForURL(/\/admin\/farms\/\d+/, { timeout: 10000 });
    });

    await test.step('Navigate to list and find the created farm', async () => {
      await page.goto('/admin/farms', { waitUntil: 'networkidle' });

      await page.getByLabel(/suche|search/i).fill(testFarmName);
      await page.getByRole('button', { name: /suchen|search/i }).click();

      const farmRow = page.locator('tr').filter({ hasText: testFarmName });
      await farmRow.waitFor({ state: 'visible', timeout: 5000 });
    });

    await test.step('Delete the farm', async () => {
      const farmRow = page.locator('tr').filter({ hasText: testFarmName });
      const deleteButton = farmRow.getByTestId('delete-button');

      await deleteButton.click();

      // Wait for confirmation dialog
      await page.waitForSelector('text=wirklich löschen?', { state: 'visible', timeout: 5000 });

      // Click the "Löschen" (Delete) button in the dialog
      const confirmDeleteButton = page.getByRole('button', { name: 'Löschen' });
      await confirmDeleteButton.click();

      await page.waitForTimeout(1000);
    });

    await test.step('Verify farm is deleted', async () => {
      // Search for the deleted farm again
      await page.getByLabel(/suche|search/i).fill(testFarmName);
      await page.getByRole('button', { name: /suchen|search/i }).click();

      await page.waitForTimeout(1000);

      const farmRow = page.locator('tr').filter({ hasText: testFarmName });
      await expect(farmRow).not.toBeVisible();
    });
  });

  test('should show validation warnings for required fields', async ({ page }) => {
    test.setTimeout(60000);

    await test.step('Navigate to create form and submit without filling required fields', async () => {
      await page.goto('/admin/farms/new', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      // Try to submit without filling any fields
      await page.getByRole('button', { name: /erstellen|submit|create/i }).click();

      // Wait for validation messages to appear
      await page.waitForTimeout(1000);

      // Should see validation errors for required fields
      const validationMessages = page.locator('.validation-message, .mud-input-error, [class*="error"]');
      await expect(validationMessages.first()).toBeVisible({ timeout: 5000 });
    });

    await test.step('Fill only some fields and verify specific validations', async () => {
      await page.goto('/admin/farms/new', { waitUntil: 'networkidle' });

      // Fill only farm name and chickens count, leave other required fields empty
      await page.getByTestId('farm-name').fill('Test Farm');
      await page.getByTestId('number-of-chickens').fill('10');

      // Try to submit with incomplete form
      await page.getByRole('button', { name: /erstellen|submit|create/i }).click();
      await page.waitForTimeout(1000);

      // Should see validation errors for missing required fields (contact, address, etc.)
      // The form should not navigate away (stays on the create page)
      await expect(page).toHaveURL(/\/admin\/farms\/new/);

      // Check for validation messages (at least one should be visible)
      const validationMessages = page.locator('.validation-message, .mud-input-error, [class*="mud-error"]');
      const validationCount = await validationMessages.count();
      expect(validationCount).toBeGreaterThan(0);
    });
  });
});

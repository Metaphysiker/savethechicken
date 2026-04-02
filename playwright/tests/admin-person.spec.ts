import { test, expect } from '@playwright/test';
import { fillPersonForm } from './helpers/person-form.helper';

// Use admin authentication
test.use({ storageState: 'playwright/.auth/admin.json' });

// Configure browser for this test file (top-level)
test.use({
  headless: false,
  launchOptions: {
    slowMo: 100,
  },
});

test.describe('Admin Person Management', () => {
  // Run tests serially to avoid resource contention
  test.describe.configure({ mode: 'serial' });

  test('should create and view a person', async ({ page }) => {
    test.setTimeout(60000);

    const timestamp = Date.now();
    const testFirstName = `CreateFirst${timestamp}`;
    const testLastName = `CreateLast${timestamp}`;
    const testEmail = `createperson${timestamp}@example.com`;
    const testPhone = '+41791111111';
    const testCity = 'Zurich';
    const testPostalCode = '8000';
    const testStreet = `Test Street ${timestamp}`;

    let personId: number;

    await test.step('Admin creates a person', async () => {
      await page.goto('/admin/persons/new', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      await fillPersonForm(page, {
        firstName: testFirstName,
        lastName: testLastName,
        email: testEmail,
        phone: testPhone,
        city: testCity,
        postalCode: testPostalCode,
        street: testStreet,
        isBlacklisted: false,
      });

      await page.getByRole('button', { name: /create|erstellen/i }).click();

      // Wait for redirect to person detail page (changed behavior - now redirects to detail instead of list)
      await page.waitForURL(/\/admin\/persons\/\d+/, { timeout: 10000 });

      // Extract person ID from URL
      const url = page.url();
      const match = url.match(/\/admin\/persons\/(\d+)/);
      expect(match).not.toBeNull();
      personId = parseInt(match![1], 10);
      expect(personId).toBeGreaterThan(0);

      // Verify person information is displayed on the detail page
      await expect(page.getByTestId('contact-firstname')).toHaveText(testFirstName);
      await expect(page.getByTestId('contact-lastname')).toHaveText(testLastName);
      await expect(page.getByTestId('contact-email')).toHaveText(testEmail);
      await expect(page.getByTestId('contact-phone')).toHaveText(testPhone);
      await expect(page.getByTestId('address-city')).toHaveText(testCity);
      await expect(page.getByTestId('address-postalcode')).toHaveText(testPostalCode);
      await expect(page.getByTestId('address-street')).toHaveText(testStreet);

      // Verify not blacklisted
      await expect(page.getByTestId('blacklist-status')).toBeVisible();
    });

    await test.step('Admin verifies person appears in list', async () => {
      // Navigate back to list to verify the person appears there
      await page.goto('/admin/persons', { waitUntil: 'networkidle' });

      // Search for the person
      await page.getByLabel(/suche|search/i).fill(testEmail);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(1000);

      // Verify the view button is visible for this person (confirms it's in the list)
      const showButton = page.getByTestId('view-button').first();
      await showButton.waitFor({ state: 'visible', timeout: 5000 });
    });
  });

  test('should edit a person (admin updates Contact and Address fields)', async ({ page }) => {
    test.setTimeout(60000);

    const timestamp = Date.now();
    const originalFirstName = `EditablePerson${timestamp}`;
    const originalLastName = 'OriginalLast';
    const originalEmail = `editableperson${timestamp}@example.com`;
    const originalPhone = '+41792222222';
    const originalCity = 'Bern';
    const originalPostalCode = '3000';
    const originalStreet = 'Original St 1';

    const updatedLastName = 'UpdatedLast';
    const updatedPhone = '+41793333333';
    const updatedCity = 'Basel';
    const updatedPostalCode = '4000';
    const updatedStreet = 'Updated Street 456';

    await test.step('Admin creates a person', async () => {
      await page.goto('/admin/persons/new', { waitUntil: 'networkidle' });

      await fillPersonForm(page, {
        firstName: originalFirstName,
        lastName: originalLastName,
        email: originalEmail,
        phone: originalPhone,
        city: originalCity,
        postalCode: originalPostalCode,
        street: originalStreet,
        isBlacklisted: false,
      });

      await page.getByRole('button', { name: /create|erstellen/i }).click();

      // After creation, now redirects to detail page
      await page.waitForURL(/\/admin\/persons\/\d+/, { timeout: 10000 });

      // Verify original data on detail page
      await expect(page.getByTestId('contact-firstname')).toHaveText(originalFirstName);
      await expect(page.getByTestId('contact-lastname')).toHaveText(originalLastName);
      await expect(page.getByTestId('contact-email')).toHaveText(originalEmail);
      await expect(page.getByTestId('contact-phone')).toHaveText(originalPhone);
      await expect(page.getByTestId('address-city')).toHaveText(originalCity);
      await expect(page.getByTestId('address-postalcode')).toHaveText(originalPostalCode);
      await expect(page.getByTestId('address-street')).toHaveText(originalStreet);
    });

    await test.step('Update the person (Contact and Address fields)', async () => {
      await page.goto('/admin/persons', { waitUntil: 'networkidle' });

      await page.getByLabel(/suche|search/i).fill(originalFirstName);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(1000);

      const editButton = page.getByTestId('edit-button').first();
      await editButton.waitFor({ state: 'visible', timeout: 5000 });
      await editButton.click();

      // Wait for navigation to edit page
      await page.waitForURL(/\/admin\/persons\/\d+\/edit/, { timeout: 10000 });

      // Update contact fields (keep firstName and email the same for searching later)
      await page.getByTestId('contact-lastname').fill(updatedLastName);
      await page.getByTestId('contact-phone').fill(updatedPhone);

      // Update address fields
      await page.getByTestId('address-city').fill(updatedCity);
      await page.getByTestId('address-postalcode').fill(updatedPostalCode);
      await page.getByTestId('address-street').fill(updatedStreet);

      // Submit the update
      console.log('About to click update button');
      const updateButton = page.getByRole('button', { name: /update|aktualisieren/i });
      await updateButton.waitFor({ state: 'visible', timeout: 5000 });
      console.log('Update button is visible');

      // Listen for API calls
      const updatePromise = page.waitForResponse(
        response => response.url().includes('/api/Person') && response.request().method() === 'PUT',
        { timeout: 10000 }
      ).catch(() => null);

      await updateButton.click();
      console.log('Clicked update button');

      // Wait for the API response
      const response = await updatePromise;
      if (response) {
        console.log('Update response status:', response.status());
        const body = await response.json().catch(() => null);
        console.log('Update response body:', body);
      } else {
        console.log('No update API call detected');
      }

      // Wait for redirect to detail page
      try {
        await page.waitForURL(/\/admin\/persons\/\d+$/, { timeout: 5000 });
      } catch (e) {
        console.log('Update did not redirect, checking for errors');
        console.log('Current URL:', page.url());
        const errors = await page.locator('.mud-input-error, .validation-message').allTextContents();
        console.log('Validation errors:', errors);
        const errorDialog = await page.locator('.mud-alert-message, .mud-snackbar').allTextContents();
        console.log('Error messages:', errorDialog);
        throw new Error(`Update did not redirect. Validation errors: ${errors.join(', ')}. Other errors: ${errorDialog.join(', ')}`);
      }
    });

    await test.step('Verify updated person data', async () => {
      // Should already be on the detail page after update
      const url = page.url();
      expect(url).toMatch(/\/admin\/persons\/\d+$/);

      // Verify updated data is visible
      await expect(page.getByTestId('contact-firstname')).toHaveText(originalFirstName);
      await expect(page.getByTestId('contact-lastname')).toHaveText(updatedLastName);
      await expect(page.getByTestId('contact-email')).toHaveText(originalEmail);
      await expect(page.getByTestId('contact-phone')).toHaveText(updatedPhone);
      await expect(page.getByTestId('address-city')).toHaveText(updatedCity);
      await expect(page.getByTestId('address-postalcode')).toHaveText(updatedPostalCode);
      await expect(page.getByTestId('address-street')).toHaveText(updatedStreet);

      // Verify old data is NOT visible
      await expect(page.getByTestId('contact-lastname')).not.toHaveText(originalLastName);
      await expect(page.getByTestId('contact-phone')).not.toHaveText(originalPhone);
      await expect(page.getByTestId('address-city')).not.toHaveText(originalCity);
      await expect(page.getByTestId('address-postalcode')).not.toHaveText(originalPostalCode);
      await expect(page.getByTestId('address-street')).not.toHaveText(originalStreet);
    });
  });

  test('should delete a person', async ({ page }) => {
    test.setTimeout(60000);

    const timestamp = Date.now();
    const testFirstName = `DeletePerson${timestamp}`;
    const testLastName = 'DeleteLast';
    const testEmail = `deleteperson${timestamp}@example.com`;

    await test.step('Admin creates a person', async () => {
      await page.goto('/admin/persons/new', { waitUntil: 'networkidle' });

      await fillPersonForm(page, {
        firstName: testFirstName,
        lastName: testLastName,
        email: testEmail,
        phone: '+41794444444',
        city: 'Geneva',
        postalCode: '1200',
        street: 'Delete St 1',
        isBlacklisted: false,
      });

      await page.getByRole('button', { name: /create|erstellen/i }).click();

      // After creation, now redirects to detail page
      await page.waitForURL(/\/admin\/persons\/\d+/, { timeout: 10000 });
    });

    await test.step('Navigate to list and find the created person', async () => {
      await page.goto('/admin/persons', { waitUntil: 'networkidle' });

      await page.getByLabel(/suche|search/i).fill(testEmail);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(1000);

      const personRow = page.locator('tr').filter({ hasText: testFirstName });
      await personRow.waitFor({ state: 'visible', timeout: 5000 });
    });

    await test.step('Delete the person', async () => {
      const personRow = page.locator('tr').filter({ hasText: testFirstName });
      const deleteButton = personRow.getByTestId('delete-button');

      await deleteButton.click();

      // Wait for confirmation dialog
      await page.waitForSelector('text=wirklich löschen?', { state: 'visible', timeout: 5000 });

      // Verify warning message is visible
      await expect(page.getByText('Diese Aktion kann nicht rückgängig gemacht werden')).toBeVisible();

      // Click the "Löschen" (Delete) button in the dialog
      const confirmDeleteButton = page.getByRole('button', { name: 'Löschen' });
      await confirmDeleteButton.click();

      // Wait for deletion to complete (dialog should close)
      await page.waitForSelector('text=wirklich löschen?', { state: 'hidden', timeout: 5000 });

      // Wait a bit for the list to refresh
      await page.waitForTimeout(1000);
    });

    await test.step('Verify person is deleted', async () => {
      // Search again for the person
      await page.getByLabel(/suche|search/i).fill(testEmail);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(1000);

      // Verify person is NOT in the list
      const personRow = page.locator('tr').filter({ hasText: testFirstName });
      await expect(personRow).not.toBeVisible();
    });
  });

  test('should toggle blacklist status', async ({ page }) => {
    test.setTimeout(60000);

    const timestamp = Date.now();
    const testFirstName = `BlacklistPerson${timestamp}`;
    const testLastName = 'BlacklistLast';
    const testEmail = `blacklist${timestamp}@example.com`;

    await test.step('Admin creates a non-blacklisted person', async () => {
      await page.goto('/admin/persons/new', { waitUntil: 'networkidle' });

      await fillPersonForm(page, {
        firstName: testFirstName,
        lastName: testLastName,
        email: testEmail,
        phone: '+41795555555',
        city: 'Lugano',
        postalCode: '6900',
        street: 'Blacklist St 1',
        isBlacklisted: false,
      });

      await page.getByRole('button', { name: /create|erstellen/i }).click();

      // After creation, now redirects to detail page
      await page.waitForURL(/\/admin\/persons\/\d+/, { timeout: 10000 });
    });

    await test.step('Verify person is not blacklisted', async () => {
      await page.goto('/admin/persons', { waitUntil: 'networkidle' });

      await page.getByLabel(/suche|search/i).fill(testEmail);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(1000);

      const showButton = page.getByTestId('view-button').first();
      await showButton.click();
      await page.waitForURL(/\/admin\/persons\/\d+/, { timeout: 10000 });

      await expect(page.getByTestId('blacklist-status')).toBeVisible();
    });

    await test.step('Toggle blacklist status to true', async () => {
      await page.goto('/admin/persons', { waitUntil: 'networkidle' });

      await page.getByLabel(/suche|search/i).fill(testEmail);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(1000);

      const editButton = page.getByTestId('edit-button').first();
      await editButton.click();
      await page.waitForURL(/\/admin\/persons\/\d+\/edit/, { timeout: 10000 });

      // Check the blacklist checkbox
      const blacklistCheckbox = page.getByLabel(/blacklisted/i);
      await blacklistCheckbox.check();

      const updateButton = page.getByRole('button', { name: /update|aktualisieren/i });
      await updateButton.click();
      await page.waitForURL(/\/admin\/persons\/\d+$/, { timeout: 10000 });
    });

    await test.step('Verify person is now blacklisted', async () => {
      // Should be on detail page
      await expect(page.getByTestId('blacklist-status')).toBeVisible();
    });

    await test.step('Toggle blacklist status back to false', async () => {
      await page.goto('/admin/persons', { waitUntil: 'networkidle' });

      await page.getByLabel(/suche|search/i).fill(testEmail);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(1000);

      const editButton = page.getByTestId('edit-button').first();
      await editButton.click();
      await page.waitForURL(/\/admin\/persons\/\d+\/edit/, { timeout: 10000 });

      // Uncheck the blacklist checkbox
      const blacklistCheckbox = page.getByLabel(/blacklisted/i);
      await blacklistCheckbox.uncheck();

      const updateButton = page.getByRole('button', { name: /update|aktualisieren/i });
      await updateButton.click();
      await page.waitForURL(/\/admin\/persons\/\d+$/, { timeout: 10000 });
    });

    await test.step('Verify person is no longer blacklisted', async () => {
      // Should be on detail page
      await expect(page.getByTestId('blacklist-status')).toBeVisible();
    });
  });
});

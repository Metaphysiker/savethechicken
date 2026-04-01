import { test, expect } from '@playwright/test';
import { createPerson } from './helpers/person-form.helper';
import { fillAdminSaveChickenDriveRequestForm } from './helpers/admin-save-chicken-drive-request-form.helper';
import { createSaveChickenAction } from './helpers/save-chicken-action-form.helper';
import { selectPerson } from './helpers/person-selector.helper';

// Use admin authentication
test.use({ storageState: 'playwright/.auth/admin.json' });

// Configure browser for this test file (top-level)
test.use({
  headless: false,
  launchOptions: {
    slowMo: 100, // Reduced from 500ms to speed up tests
  },
});

test.describe('Admin Save Chicken Drive Request Management', () => {
  // Run tests serially to avoid resource contention when creating actions/persons
  test.describe.configure({ mode: 'serial' });

  test('should create and view a save chicken drive request using PersonSelector', async ({ page }) => {
    test.setTimeout(90000); // Increased timeout for action + person + request creation

    const timestamp = Date.now();
    const testFirstName = `AdminDriveFirst${timestamp}`;
    const testLastName = `AdminDriveLast${timestamp}`;
    const testEmail = `admindrive${timestamp}@example.com`;

    let personId: number;
    let availableDates: number[];

    await test.step('Admin creates a SaveChickenAction first', async () => {
      const today = new Date();
      const dayOfMonth = today.getDate();

      // Create dates relative to today (today + next 3 days)
      const datesToCreate = [dayOfMonth];
      if (dayOfMonth + 1 <= 28) datesToCreate.push(dayOfMonth + 1);
      if (dayOfMonth + 2 <= 28) datesToCreate.push(dayOfMonth + 2);
      if (dayOfMonth + 3 <= 28) datesToCreate.push(dayOfMonth + 3);

      const action = await createSaveChickenAction(page, {
        title: `Test Drive Action ${timestamp}`,
        description: 'Test action for save chicken drive requests',
        dates: datesToCreate,
        isActive: true,
      });

      availableDates = action.dates;
      expect(action.actionId).toBeGreaterThan(0);
    });

    await test.step('Admin creates a person first', async () => {
      personId = await createPerson(page, {
        firstName: testFirstName,
        lastName: testLastName,
        email: testEmail,
        phone: '+41798888888',
        city: 'Lausanne',
        postalCode: '1000',
        street: 'Drive Street 123',
      });

      expect(personId).toBeGreaterThan(0);
    });

    await test.step('Admin creates SaveChickenDriveRequest using PersonSelector', async () => {
      await page.goto('/admin/save-chicken-drive-requests/new', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      // Use dates from the created action (select first two)
      const datesToSelect = availableDates.slice(0, 2);

      await fillAdminSaveChickenDriveRequestForm(page, {
        personId: personId,
        personEmail: testEmail,
        carMake: 'Toyota Corolla',
        capacityForChickens: '25',
        availableDates: datesToSelect,
        message: 'Happy to help transport chickens!',
      });

      await page.getByRole('button', { name: /erstellen|submit/i }).click();

      // Admin should be redirected to detail page, not thank-you
      await page.waitForURL(/\/admin\/save-chicken-drive-requests\/\d+/, { timeout: 10000 });
    });

    await test.step('Admin finds and views the drive request', async () => {
      await page.goto('/admin/save-chicken-drive-requests', { waitUntil: 'networkidle' });

      // Wait for the page to load
      await page.waitForSelector('h2', { state: 'visible', timeout: 10000 });

      // Search for the request
      await page.getByLabel('Suche').fill(testFirstName);
      await page.getByTestId('search-button').click();
      await page.waitForTimeout(1000);

      // Click view button
      const showButton = page.getByTestId('view-button').first();
      await showButton.waitFor({ state: 'visible', timeout: 5000 });
      await showButton.click();

      // Verify on detail page
      await page.waitForURL(/\/admin\/save-chicken-drive-requests\/\d+/, { timeout: 10000 });

      // Verify person information using data-testid
      await expect(page.getByTestId('contact-firstname')).toHaveText(testFirstName);
      await expect(page.getByTestId('contact-lastname')).toHaveText(testLastName);
      await expect(page.getByTestId('contact-email')).toHaveText(testEmail);
      await expect(page.getByTestId('contact-phone')).toHaveText('+41798888888');
      await expect(page.getByTestId('address-city')).toHaveText('Lausanne');
      await expect(page.getByTestId('address-postalcode')).toHaveText('1000');
      await expect(page.getByTestId('address-street')).toHaveText('Drive Street 123');

      // Verify save chicken drive request information using data-testid
      await expect(page.getByTestId('drive-request-car')).toContainText('Toyota Corolla');
      await expect(page.getByTestId('drive-request-capacity')).toContainText('25');
      await expect(page.getByTestId('drive-request-message-display')).toContainText('Happy to help transport chickens!');
    });
  });

  test('should edit a save chicken drive request (admin updates SaveChickenDriveRequest fields only)', async ({ page }) => {
    test.setTimeout(90000); // Increased timeout for action + person + request creation

    const timestamp = Date.now();
    const originalFirstName = `EditableDrive${timestamp}`;
    const originalLastName = 'OriginalLast';
    const originalEmail = `editabledrive${timestamp}@example.com`;

    let personId: number;
    let availableDates: number[];

    await test.step('Admin creates a SaveChickenAction first', async () => {
      const today = new Date();
      const dayOfMonth = today.getDate();

      // Create dates relative to today
      const datesToCreate = [dayOfMonth];
      if (dayOfMonth + 1 <= 28) datesToCreate.push(dayOfMonth + 1);
      if (dayOfMonth + 2 <= 28) datesToCreate.push(dayOfMonth + 2);

      const action = await createSaveChickenAction(page, {
        title: `Test Drive Action ${timestamp}`,
        description: 'Test action for edit test',
        dates: datesToCreate,
        isActive: true,
      });

      availableDates = action.dates;
      expect(action.actionId).toBeGreaterThan(0);
    });

    await test.step('Admin creates a person first', async () => {
      personId = await createPerson(page, {
        firstName: originalFirstName,
        lastName: originalLastName,
        email: originalEmail,
        phone: '+41793333333',
        city: 'Geneva',
        postalCode: '1200',
        street: 'Original St 1',
      });

      expect(personId).toBeGreaterThan(0);
    });

    await test.step('Admin creates SaveChickenDriveRequest', async () => {
      await page.goto('/admin/save-chicken-drive-requests/new', { waitUntil: 'networkidle' });

      // Use first date from created action
      const dayOfMonth = availableDates[0];

      await fillAdminSaveChickenDriveRequestForm(page, {
        personId: personId,
        personEmail: originalEmail,
        carMake: 'Honda Civic',
        capacityForChickens: '15',
        availableDates: [dayOfMonth],
        message: 'Original drive message',
      });

      await page.getByRole('button', { name: /erstellen/i }).click();

      // Admin should be redirected to detail page, not thank-you
      await page.waitForURL(/\/admin\/save-chicken-drive-requests\/\d+/, { timeout: 10000 });
    });

    await test.step('Verify created drive request', async () => {
      await page.goto('/admin/save-chicken-drive-requests', { waitUntil: 'networkidle' });

      await page.getByLabel('Suche').fill(originalFirstName);
      await page.getByTestId('search-button').click();
      await page.waitForTimeout(1000);

      const showButton = page.getByTestId('view-button').first();
      await showButton.waitFor({ state: 'visible', timeout: 5000 });
      await showButton.click();
      await page.waitForURL(/\/admin\/save-chicken-drive-requests\/\d+/, { timeout: 10000 });

      // Verify person information using data-testid
      await expect(page.getByTestId('contact-firstname')).toHaveText(originalFirstName);
      await expect(page.getByTestId('contact-email')).toHaveText(originalEmail);
      await expect(page.getByTestId('address-city')).toHaveText('Geneva');

      // Verify drive request information using data-testid
      await expect(page.getByTestId('drive-request-car')).toContainText('Honda Civic');
      await expect(page.getByTestId('drive-request-message-display')).toContainText('Original drive message');
    });

    await test.step('Update the drive request (SaveChickenDriveRequest fields only)', async () => {
      await page.goto('/admin/save-chicken-drive-requests', { waitUntil: 'networkidle' });

      await page.getByLabel('Suche').fill(originalFirstName);
      await page.getByTestId('search-button').click();
      await page.waitForTimeout(1000);

      const editButton = page.getByTestId('edit-button').first();
      await editButton.waitFor({ state: 'visible', timeout: 5000 });
      await editButton.click();

      // Wait for dialog to open
      await page.waitForSelector('[data-testid="car-make"]', { state: 'visible', timeout: 10000 });

      // Update only SaveChickenDriveRequest's own fields (NOT Person/Contact/Address)
      await page.getByTestId('car-make').fill('Tesla Model 3');
      await page.getByTestId('capacity-for-chickens').fill('30');
      await page.getByTestId('drive-request-message').fill('Updated message - can transport more chickens now');

      // Submit the update
      console.log('About to click update button');
      const updateButton = page.getByRole('button', { name: /aktualisieren|update/i });
      await updateButton.waitFor({ state: 'visible', timeout: 5000 });
      console.log('Update button is visible');

      // Check if button is enabled
      const isDisabled = await updateButton.isDisabled();
      console.log('Update button disabled:', isDisabled);

      // Listen for API calls
      const updatePromise = page.waitForResponse(
        response => response.url().includes('/api/SaveChickenDriveRequest') && response.request().method() === 'PUT',
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

      // Wait for dialog to close or check for errors
      try {
        await page.waitForSelector('[data-testid="car-make"]', { state: 'hidden', timeout: 5000 });
      } catch (e) {
        // Dialog didn't close, might be validation error
        console.log('Dialog did not close, checking for validation errors');
        const errors = await page.locator('.mud-input-error, .validation-message').allTextContents();
        console.log('Validation errors:', errors);

        // Check if there's a general error message
        const errorDialog = await page.locator('.mud-alert-message, .mud-snackbar').allTextContents();
        console.log('Error messages:', errorDialog);

        throw new Error(`Update dialog did not close. Validation errors: ${errors.join(', ')}. Other errors: ${errorDialog.join(', ')}`);
      }

      // Navigate back to list to refresh data
      await page.goto('/admin/save-chicken-drive-requests', { waitUntil: 'networkidle' });
    });

    await test.step('Verify updated drive request', async () => {
      // Search for the request using original name (Person data shouldn't change)
      await page.getByLabel('Suche').clear();
      await page.getByLabel('Suche').fill(originalFirstName);
      await page.getByTestId('search-button').click();
      await page.waitForTimeout(2000);

      const showButton = page.getByTestId('view-button').first();
      await showButton.waitFor({ state: 'visible', timeout: 5000 });
      await showButton.click();
      await page.waitForURL(/\/admin\/save-chicken-drive-requests\/\d+/, { timeout: 10000 });

      // Verify Person data is UNCHANGED (original values) using data-testid
      await expect(page.getByTestId('contact-firstname')).toHaveText(originalFirstName);
      await expect(page.getByTestId('contact-lastname')).toHaveText(originalLastName);
      await expect(page.getByTestId('contact-email')).toHaveText(originalEmail);
      await expect(page.getByTestId('address-city')).toHaveText('Geneva');
      await expect(page.getByTestId('address-postalcode')).toHaveText('1200');

      // Verify SaveChickenDriveRequest data IS UPDATED using data-testid
      await expect(page.getByTestId('drive-request-car')).toContainText('Tesla Model 3');
      await expect(page.getByTestId('drive-request-capacity')).toContainText('30');
      await expect(page.getByTestId('drive-request-message-display')).toContainText('Updated message - can transport more chickens now');

      // Verify old SaveChickenDriveRequest data is NOT visible
      await expect(page.getByTestId('drive-request-car')).not.toContainText('Honda Civic');
      await expect(page.getByTestId('drive-request-capacity')).not.toContainText('15');
      await expect(page.getByTestId('drive-request-message-display')).not.toContainText('Original drive message');
    });
  });

  test('should delete a save chicken drive request', async ({ page }) => {
    test.setTimeout(90000); // Increased timeout for action + person + request creation

    const timestamp = Date.now();
    const testFirstName = `DeleteDrive${timestamp}`;
    const testLastName = 'DeleteLast';
    const testEmail = `deletedrive${timestamp}@example.com`;
    const uniqueCarMake = `Ford-${timestamp}`; // Unique CarMake to avoid collisions with previous test runs

    let personId: number;
    let availableDates: number[];

    // NOTE: Backend search doesn't include SaveChickenAction.Title, only searches:
    // CarMake, Person.Contact fields, Person.Address fields
    // Also, Person data appears null in table rows for some reason

    await test.step('Admin creates a SaveChickenAction first', async () => {
      const today = new Date();
      const dayOfMonth = today.getDate();

      // Create dates relative to today
      const datesToCreate = [dayOfMonth];
      if (dayOfMonth + 1 <= 28) datesToCreate.push(dayOfMonth + 1);

      const action = await createSaveChickenAction(page, {
        title: `Test Drive Action ${timestamp}`,
        description: 'Test action for delete test',
        dates: datesToCreate,
        isActive: true,
      });

      availableDates = action.dates;
      expect(action.actionId).toBeGreaterThan(0);
    });

    await test.step('Admin creates a person first', async () => {
      personId = await createPerson(page, {
        firstName: testFirstName,
        lastName: testLastName,
        email: testEmail,
        phone: '+41794444444',
        city: 'Luzern',
        postalCode: '6000',
        street: 'Delete St 1',
      });

      expect(personId).toBeGreaterThan(0);
    });

    await test.step('Admin creates SaveChickenDriveRequest', async () => {
      await page.goto('/admin/save-chicken-drive-requests/new', { waitUntil: 'networkidle' });

      // Use first date from created action
      const dayOfMonth = availableDates[0];

      await fillAdminSaveChickenDriveRequestForm(page, {
        personId: personId,
        personEmail: testEmail,
        carMake: uniqueCarMake,
        capacityForChickens: '10',
        availableDates: [dayOfMonth],
        message: 'This drive request will be deleted',
      });

      await page.getByRole('button', { name: /erstellen/i }).click();

      // Admin should be redirected to detail page, not thank-you
      await page.waitForURL(/\/admin\/save-chicken-drive-requests\/\d+/, { timeout: 10000 });
    });

    await test.step('Navigate to list and find the created drive request', async () => {
      await page.goto('/admin/save-chicken-drive-requests', { waitUntil: 'networkidle' });

      // Wait for the page to load
      await page.waitForSelector('h2', { state: 'visible', timeout: 10000 });

      // Search using unique CarMake
      await page.getByLabel('Suche').fill(uniqueCarMake);
      await page.getByTestId('search-button').click();
      await page.waitForTimeout(1000); // Wait for search to filter results

      // Find the row by unique CarMake
      const requestRow = page.locator('tr').filter({ hasText: uniqueCarMake });
      await requestRow.waitFor({ state: 'visible', timeout: 5000 });
    });

    await test.step('Delete the drive request', async () => {
      const requestRow = page.locator('tr').filter({ hasText: uniqueCarMake });
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

    await test.step('Verify drive request is deleted', async () => {
      await page.getByLabel('Suche').fill(uniqueCarMake);
      await page.getByTestId('search-button').click();
      await page.waitForTimeout(1000); // Wait for search to filter results

      const requestRow = page.locator('tr').filter({ hasText: uniqueCarMake });
      await expect(requestRow).not.toBeVisible();
    });
  });

  test('should show validation warnings for required fields', async ({ page }) => {
    test.setTimeout(60000);

    const timestamp = Date.now();
    let availableDates: number[];

    await test.step('Admin creates a SaveChickenAction', async () => {
      const today = new Date();
      const dayOfMonth = today.getDate();

      const datesToCreate = [dayOfMonth];
      if (dayOfMonth + 1 <= 28) datesToCreate.push(dayOfMonth + 1);

      const action = await createSaveChickenAction(page, {
        title: `Test Drive Action ${timestamp}`,
        description: 'Test action for validation test',
        dates: datesToCreate,
        isActive: true,
      });

      availableDates = action.dates;
      expect(action.actionId).toBeGreaterThan(0);
    });

    await test.step('Navigate to create form and submit without filling required fields', async () => {
      await page.goto('/admin/save-chicken-drive-requests/new', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      // Try to submit without filling any fields
      await page.getByRole('button', { name: /erstellen|submit|create/i }).click();

      // Wait for validation messages to appear
      await page.waitForTimeout(1000);

      // Person selection should be required
      const personValidation = page.locator('.validation-message, .mud-input-error, [class*="error"]').filter({ hasText: /person|required|erforderlich/i });
      await expect(personValidation.first()).toBeVisible({ timeout: 5000 });
    });

    await test.step('Fill only some fields and verify specific validations', async () => {
      // Create a person first
      const testEmail = `drivevalidation${timestamp}@example.com`;
      const personId = await createPerson(page, {
        firstName: `DriveValidationTest${timestamp}`,
        lastName: 'User',
        email: testEmail,
        phone: '+41791111111',
        city: 'Bern',
        postalCode: '3000',
        street: 'Test Street 1',
      });

      await page.goto('/admin/save-chicken-drive-requests/new', { waitUntil: 'networkidle' });

      // Use the helper to select person (this is a known working pattern)
      await selectPerson(page, testEmail);

      // Fill only capacity but leave car make empty (which is required)
      await page.getByTestId('capacity-for-chickens').fill('20');

      // Try to submit with incomplete form
      await page.getByRole('button', { name: /erstellen|submit|create/i }).click();
      await page.waitForTimeout(1000);

      // Should see validation errors for missing required fields
      // The form should not navigate away (stays on the create page)
      await expect(page).toHaveURL(/\/admin\/save-chicken-drive-requests\/new/);

      // Check for validation messages (at least one should be visible)
      const validationMessages = page.locator('.validation-message, .mud-input-error, [class*="mud-error"]');
      const validationCount = await validationMessages.count();
      expect(validationCount).toBeGreaterThan(0);
    });
  });

  test('should manage save chicken drive request from person detail page', async ({ page }) => {
    test.setTimeout(120000); // Increased timeout for multiple operations

    const timestamp = Date.now();
    const testFirstName = `PersonDrive${timestamp}`;
    const testLastName = `PersonDriveLast${timestamp}`;
    const testEmail = `persondrive${timestamp}@example.com`;

    let personId: number;
    let availableDates: number[];
    let actionId: number;

    await test.step('Admin creates a SaveChickenAction first', async () => {
      const today = new Date();
      const dayOfMonth = today.getDate();

      // Create dates relative to today
      const datesToCreate = [dayOfMonth];
      if (dayOfMonth + 1 <= 28) datesToCreate.push(dayOfMonth + 1);
      if (dayOfMonth + 2 <= 28) datesToCreate.push(dayOfMonth + 2);

      const action = await createSaveChickenAction(page, {
        title: `Test Action ${timestamp}`,
        description: 'Test action for person detail page',
        dates: datesToCreate,
        isActive: true,
      });

      availableDates = action.dates;
      actionId = action.actionId;
      expect(action.actionId).toBeGreaterThan(0);
    });

    await test.step('Admin creates a person', async () => {
      personId = await createPerson(page, {
        firstName: testFirstName,
        lastName: testLastName,
        email: testEmail,
        phone: '+41792222222',
        city: 'Zurich',
        postalCode: '8000',
        street: 'Drive Street 789',
      });

      expect(personId).toBeGreaterThan(0);
    });

    await test.step('Navigate to person detail page and add save chicken drive request', async () => {
      // Go to person detail page
      await page.goto(`/admin/persons/${personId}`, { waitUntil: 'networkidle' });
      
      // Verify we're on the correct person page using specific test IDs
      await expect(page.getByTestId('contact-firstname')).toHaveText(testFirstName);
      await expect(page.getByTestId('contact-email')).toHaveText(testEmail);

      // Click "Add Drive Request" button
      await page.getByRole('button', { name: /Add Drive Request/i }).click();

      // Wait for dialog to open
      await page.waitForSelector('[data-testid="drive-request-general-info"]', { state: 'visible', timeout: 10000 });

      // Verify person is pre-selected and disabled
      await expect(page.getByText('Person is pre-selected and cannot be changed')).toBeVisible();
      
      // Use helper to fill the form (person is pre-selected, so don't pass personId/personEmail)
      await fillAdminSaveChickenDriveRequestForm(page, {
        carMake: 'Honda Civic',
        capacityForChickens: '30',
        availableDates: [availableDates[0]],
        message: 'Ready to transport chickens!',
      });

      // Submit
      await page.getByTestId('submit-button').click();

      // Wait for dialog to close
      await page.waitForTimeout(2000);
    });

    await test.step('Verify request appears on person detail page', async () => {
      // Should still be on person detail page
      await expect(page).toHaveURL(`/admin/persons/${personId}`);
      
      // Verify the request appears in the table
      await expect(page.getByText('Drive Requests (1)')).toBeVisible();
      await expect(page.getByText('Honda Civic')).toBeVisible();
      
      // Verify it shows the correct data in the table
      const tableRow = page.locator('tr').filter({ hasText: 'Honda Civic' });
      await expect(tableRow).toBeVisible();
    });

    await test.step('Edit the request from person detail page', async () => {
      // Click edit button in the table
      const editButton = page.getByTestId('edit-button').first();
      await editButton.waitFor({ state: 'visible', timeout: 5000 });
      await editButton.click();

      // Wait for dialog to open
      await page.waitForSelector('[data-testid="drive-request-general-info"]', { state: 'visible', timeout: 10000 });

      // Update the request
      await page.getByTestId('car-make').fill('Toyota Prius');
      await page.getByTestId('capacity-for-chickens').fill('35');
      await page.getByTestId('drive-request-message').fill('Updated: Even more capacity!');

      // Submit update
      await page.getByRole('button', { name: /aktualisieren|update/i }).click();

      // Wait for dialog to close
      await page.waitForTimeout(2000);
    });

    await test.step('Verify updated request appears on person detail page', async () => {
      // Should still be on person detail page
      await expect(page).toHaveURL(`/admin/persons/${personId}`);
      
      // Verify the updated data appears
      await expect(page.getByText('Toyota Prius')).toBeVisible();
      
      // Verify old data is gone
      await expect(page.getByText('Honda Civic')).not.toBeVisible();
      
      // Still shows 1 request
      await expect(page.getByText('Drive Requests (1)')).toBeVisible();
    });

    await test.step('Delete the request from person detail page', async () => {
      // Click delete button in the table
      const deleteButton = page.getByTestId('delete-button').first();
      await deleteButton.waitFor({ state: 'visible', timeout: 5000 });
      await deleteButton.click();

      // Confirm deletion
      await page.getByRole('button', { name: /ja|yes|delete|löschen/i }).click();

      // Wait for deletion to complete
      await page.waitForTimeout(2000);
    });

    await test.step('Verify request no longer appears on person detail page', async () => {
      // Should still be on person detail page
      await expect(page).toHaveURL(`/admin/persons/${personId}`);
      
      // Request section should not be visible anymore (no requests)
      await expect(page.getByText('Drive Requests')).not.toBeVisible();
      
      // The data should not be visible
      await expect(page.getByText('Toyota Prius')).not.toBeVisible();
    });
  });
});

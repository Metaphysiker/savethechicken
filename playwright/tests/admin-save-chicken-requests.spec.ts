import { test, expect } from '@playwright/test';
import { createPerson } from './helpers/person-form.helper';
import { fillAdminSaveChickenRequestForm } from './helpers/admin-save-chicken-request-form.helper';
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

test.describe('Admin Save Chicken Request Management', () => {
  // Run tests serially to avoid resource contention when creating actions/persons
  test.describe.configure({ mode: 'serial' });

  test('should create and view a save chicken request using PersonSelector', async ({ page }) => {
    test.setTimeout(90000); // Increased timeout for action + person + request creation

    const timestamp = Date.now();
    const testFirstName = `AdminReqFirst${timestamp}`;
    const testLastName = `AdminReqLast${timestamp}`;
    const testEmail = `adminreq${timestamp}@example.com`;

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
        title: `Test Action ${timestamp}`,
        description: 'Test action for save chicken requests',
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
        street: 'Request Street 123',
      });

      expect(personId).toBeGreaterThan(0);
    });

    await test.step('Admin creates SaveChickenRequest using PersonSelector', async () => {
      await page.goto('/admin/save-chicken-requests/new', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      // Use dates from the created action (select first two)
      const datesToSelect = availableDates.slice(0, 2);

      await fillAdminSaveChickenRequestForm(page, {
        personId: personId,
        personEmail: testEmail,
        numberOfChickens: '8',
        numberOfRoosters: '1',
        description: 'Large garden with secure chicken coop',
        alreadyReceivedChickensBefore: false,
        datesForHandOver: datesToSelect,
        message: 'Looking forward to helping chickens!',
        confirmCriteria: true,
        acceptTerms: true,
      });

      await page.getByRole('button', { name: /erstellen|submit/i }).click();

      // Admin should be redirected to detail page, not thank-you
      await page.waitForURL(/\/admin\/save-chicken-requests\/\d+/, { timeout: 10000 });
    });

    await test.step('Admin finds and views the request', async () => {
      await page.goto('/admin/save-chicken-requests', { waitUntil: 'networkidle' });

      // Search for the request
      await page.getByLabel(/suche|search/i).fill(testFirstName);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(1000);

      // Click view button
      const showButton = page.getByTestId('view-button').first();
      await showButton.waitFor({ state: 'visible', timeout: 5000 });
      await showButton.click();

      // Verify on detail page
      await page.waitForURL(/\/admin\/save-chicken-requests\/\d+/, { timeout: 10000 });

      // Verify person information using data-testid
      await expect(page.getByTestId('contact-firstname')).toHaveText(testFirstName);
      await expect(page.getByTestId('contact-lastname')).toHaveText(testLastName);
      await expect(page.getByTestId('contact-email')).toHaveText(testEmail);
      await expect(page.getByTestId('contact-phone')).toHaveText('+41798888888');
      await expect(page.getByTestId('address-city')).toHaveText('Lausanne');
      await expect(page.getByTestId('address-postalcode')).toHaveText('1000');
      await expect(page.getByTestId('address-street')).toHaveText('Request Street 123');

      // Verify save chicken request information
      await expect(page.getByText(/Anzahl Hühner:\s*8/)).toBeVisible();
      await expect(page.getByText('Large garden with secure chicken coop')).toBeVisible();
      await expect(page.getByText('Looking forward to helping chickens!')).toBeVisible();
    });
  });

  test('should edit a save chicken request (admin updates SaveChickenRequest fields only)', async ({ page }) => {
    test.setTimeout(90000); // Increased timeout for action + person + request creation

    const timestamp = Date.now();
    const originalFirstName = `EditableReq${timestamp}`;
    const originalLastName = 'OriginalLast';
    const originalEmail = `editablereq${timestamp}@example.com`;

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
        title: `Test Action ${timestamp}`,
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

    await test.step('Admin creates SaveChickenRequest', async () => {
      await page.goto('/admin/save-chicken-requests/new', { waitUntil: 'networkidle' });

      // Use first date from created action
      const dayOfMonth = availableDates[0];

      await fillAdminSaveChickenRequestForm(page, {
        personId: personId,
        personEmail: originalEmail,
        numberOfChickens: '5',
        numberOfRoosters: '0',
        description: 'Original description',
        message: 'Original message',
        datesForHandOver: [dayOfMonth],
      });

      await page.getByRole('button', { name: /erstellen/i }).click();

      // Admin should be redirected to detail page, not thank-you
      await page.waitForURL(/\/admin\/save-chicken-requests\/\d+/, { timeout: 10000 });
    });

    await test.step('Verify created request', async () => {
      await page.goto('/admin/save-chicken-requests', { waitUntil: 'networkidle' });

      await page.getByLabel(/suche|search/i).fill(originalFirstName);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(1000);

      const showButton = page.getByTestId('view-button').first();
      await showButton.waitFor({ state: 'visible', timeout: 5000 });
      await showButton.click();
      await page.waitForURL(/\/admin\/save-chicken-requests\/\d+/, { timeout: 10000 });

      // Verify person information using data-testid
      await expect(page.getByTestId('contact-firstname')).toHaveText(originalFirstName);
      await expect(page.getByTestId('contact-email')).toHaveText(originalEmail);
      await expect(page.getByTestId('address-city')).toHaveText('Geneva');
      await expect(page.getByText('Original description')).toBeVisible();
    });

    await test.step('Update the request (SaveChickenRequest fields only)', async () => {
      await page.goto('/admin/save-chicken-requests', { waitUntil: 'networkidle' });

      await page.getByLabel(/suche|search/i).fill(originalFirstName);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(1000);

      const editButton = page.getByTestId('edit-button').first();
      await editButton.waitFor({ state: 'visible', timeout: 5000 });
      await editButton.click();

      // Wait for dialog to open
      await page.waitForSelector('[data-testid="save-chicken-request-general-info"]', { state: 'visible', timeout: 10000 });

      // Update only SaveChickenRequest's own fields (NOT Person/Contact/Address)
      await page.getByTestId('chickens-count').fill('12');
      await page.getByTestId('roosters-count').fill('2');
      await page.getByTestId('description').fill('Updated description with more space');
      await page.getByTestId('message').fill('Updated message from admin');

      // Submit the update
      await page.getByRole('button', { name: /aktualisieren|update/i }).click();

      // Wait for dialog to close
      await page.waitForTimeout(2000);

      // Navigate back to list
      await page.goto('/admin/save-chicken-requests', { waitUntil: 'networkidle' });
    });

    await test.step('Verify updated request', async () => {
      // Search for the request using original name (Person data shouldn't change)
      await page.getByLabel(/suche|search/i).clear();
      await page.getByLabel(/suche|search/i).fill(originalFirstName);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(2000);

      const showButton = page.getByTestId('view-button').first();
      await showButton.waitFor({ state: 'visible', timeout: 5000 });
      await showButton.click();
      await page.waitForURL(/\/admin\/save-chicken-requests\/\d+/, { timeout: 10000 });

      // Verify Person data is UNCHANGED (original values) using data-testid
      await expect(page.getByTestId('contact-firstname')).toHaveText(originalFirstName);
      await expect(page.getByTestId('contact-lastname')).toHaveText(originalLastName);
      await expect(page.getByTestId('contact-email')).toHaveText(originalEmail);
      await expect(page.getByTestId('address-city')).toHaveText('Geneva');
      await expect(page.getByTestId('address-postalcode')).toHaveText('1200');

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

  test('should delete a save chicken request', async ({ page }) => {
    test.setTimeout(90000); // Increased timeout for action + person + request creation

    const timestamp = Date.now();
    const testFirstName = `DeleteReq${timestamp}`;
    const testLastName = 'DeleteLast';
    const testEmail = `deletereq${timestamp}@example.com`;

    let personId: number;
    let availableDates: number[];

    await test.step('Admin creates a SaveChickenAction first', async () => {
      const today = new Date();
      const dayOfMonth = today.getDate();

      // Create dates relative to today
      const datesToCreate = [dayOfMonth];
      if (dayOfMonth + 1 <= 28) datesToCreate.push(dayOfMonth + 1);

      const action = await createSaveChickenAction(page, {
        title: `Test Action ${timestamp}`,
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

    await test.step('Admin creates SaveChickenRequest', async () => {
      await page.goto('/admin/save-chicken-requests/new', { waitUntil: 'networkidle' });

      // Use first date from created action
      const dayOfMonth = availableDates[0];

      await fillAdminSaveChickenRequestForm(page, {
        personId: personId,
        personEmail: testEmail,
        numberOfChickens: '3',
        numberOfRoosters: '0',
        description: 'This request will be deleted',
        datesForHandOver: [dayOfMonth],
      });

      await page.getByRole('button', { name: /erstellen/i }).click();

      // Admin should be redirected to detail page, not thank-you
      await page.waitForURL(/\/admin\/save-chicken-requests\/\d+/, { timeout: 10000 });
    });

    await test.step('Navigate to list and find the created request', async () => {
      await page.goto('/admin/save-chicken-requests', { waitUntil: 'networkidle' });

      await page.getByLabel(/suche|search/i).fill(testFirstName);
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
        title: `Test Action ${timestamp}`,
        description: 'Test action for validation test',
        dates: datesToCreate,
        isActive: true,
      });

      availableDates = action.dates;
      expect(action.actionId).toBeGreaterThan(0);
    });

    await test.step('Navigate to create form and submit without filling required fields', async () => {
      await page.goto('/admin/save-chicken-requests/new', { waitUntil: 'networkidle' });
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
      const testEmail = `validation${timestamp}@example.com`;
      const personId = await createPerson(page, {
        firstName: `ValidationTest${timestamp}`,
        lastName: 'User',
        email: testEmail,
        phone: '+41791111111',
        city: 'Bern',
        postalCode: '3000',
        street: 'Test Street 1',
      });

      await page.goto('/admin/save-chicken-requests/new', { waitUntil: 'networkidle' });

      // Use the helper to select person (this is a known working pattern)
      await selectPerson(page, testEmail);

      // Fill only numberOfChickens but leave other required fields empty
      await page.getByTestId('chickens-count').fill('5');

      // Try to submit with incomplete form
      await page.getByRole('button', { name: /erstellen|submit|create/i }).click();
      await page.waitForTimeout(1000);

      // Should see validation errors for missing required fields
      // The form should not navigate away (stays on the create page)
      await expect(page).toHaveURL(/\/admin\/save-chicken-requests\/new/);

      // Check for validation messages (at least one should be visible)
      const validationMessages = page.locator('.validation-message, .mud-input-error, [class*="mud-error"]');
      const validationCount = await validationMessages.count();
      expect(validationCount).toBeGreaterThan(0);
    });
  });
});

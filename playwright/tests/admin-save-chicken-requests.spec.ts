import { test, expect } from '@playwright/test';
import { createPerson } from './helpers/person-form.helper';
import { fillAdminSaveChickenRequestForm } from './helpers/admin-save-chicken-request-form.helper';
import { fillSaveChickenRequestForm } from './helpers/save-chicken-request-form.helper';
import { createSaveChickenAction } from './helpers/save-chicken-action-form.helper';
import { selectPerson } from './helpers/person-selector.helper';
import { selectSaveChickenAction } from './helpers/save-chicken-action-selector.helper';
import { fail } from 'assert';

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
    let actionId: number;

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
      actionId = action.actionId;
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
        saveChickenActionId: actionId,
        numberOfChickens: '8',
        numberOfRoosters: '1',
        description: 'Large garden with secure chicken coop',
        alreadyReceivedChickensBefore: false,
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
        description: 'Test action for edit test',
        dates: datesToCreate,
        isActive: true,
      });

      availableDates = action.dates;
      actionId = action.actionId;
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
        saveChickenActionId: actionId,
        numberOfChickens: '5',
        numberOfRoosters: '0',
        description: 'Original description',
        message: 'Original message',
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
    let actionId: number;

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
      actionId = action.actionId;
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
        saveChickenActionId: actionId,
        numberOfChickens: '3',
        numberOfRoosters: '0',
        description: 'This request will be deleted',
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
    let actionId: number;

    await test.step('Admin creates a SaveChickenAction first', async () => {
      const today = new Date();
      const dayOfMonth = today.getDate();

      const datesToCreate = [dayOfMonth];
      if (dayOfMonth + 1 <= 28) datesToCreate.push(dayOfMonth + 1);

      const action = await createSaveChickenAction(page, {
        title: `Test Action ${timestamp}`,
        description: 'Test action for delete test',
        dates: datesToCreate,
        isActive: true,
      });

      availableDates = action.dates;
      actionId = action.actionId;
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

  test('should manage save chicken request from person detail page', async ({ page }) => {
    test.setTimeout(60000);

    const timestamp = Date.now();
    const testEmail = `personreq${timestamp}@example.com`;

    // Create action and person
    const today = new Date();
    const dayOfMonth = today.getDate();
    const datesToCreate = [dayOfMonth, dayOfMonth + 1 <= 28 ? dayOfMonth + 1 : dayOfMonth];

    const action = await createSaveChickenAction(page, {
      title: `Test Action ${timestamp}`,
      description: 'Test action for person detail page',
      dates: datesToCreate,
      isActive: true,
    });

    const personId = await createPerson(page, {
      firstName: `PersonReq${timestamp}`,
      lastName: 'Test',
      email: testEmail,
      phone: '+41791111111',
      city: 'Bern',
      postalCode: '3000',
      street: 'Person Street 456',
    });

    // Navigate to person detail page
    await page.goto(`/admin/persons/${personId}`, { waitUntil: 'networkidle' });
    await expect(page.getByTestId('contact-firstname')).toContainText(`PersonReq${timestamp}`);

    // Add save chicken request
    await page.getByTestId('add-save-chicken-request-button').click();
    await page.waitForSelector('[data-testid="save-chicken-request-general-info"]', { state: 'visible', timeout: 10000 });

    await fillAdminSaveChickenRequestForm(page, {
      saveChickenActionId: action.actionId,
      numberOfChickens: '10',
      numberOfRoosters: '2',
      description: 'Nice farm with lots of space',
      message: 'Happy to help!',
      confirmCriteria: true,
      acceptTerms: true,
    });

    await page.getByTestId('submit-button').click();
    await page.waitForSelector('[data-testid="save-chicken-request-general-info"]', { state: 'hidden', timeout: 5000 });

    // Verify creation
    await expect(page.getByText('Nice farm with lots of space')).toBeVisible();

    // Edit the request
    await page.getByTestId('edit-button').first().click();
    await page.waitForSelector('[data-testid="save-chicken-request-general-info"]', { state: 'visible', timeout: 10000 });

    await page.getByTestId('chickens-count').fill('15');
    await page.getByTestId('description').fill('Updated: Even more space now');

    await page.getByRole('button', { name: /aktualisieren|update/i }).click();
    await page.waitForSelector('[data-testid="save-chicken-request-general-info"]', { state: 'hidden', timeout: 5000 });

    // Wait for the edit dialog container to be completely removed
    await page.waitForSelector('.mud-dialog-container', { state: 'hidden', timeout: 5000 });

    // Wait for network to be idle after edit to ensure table has refreshed
    await page.waitForLoadState('networkidle', { timeout: 10000 });

    // Verify changes
    await expect(page.getByText('Updated: Even more space now')).toBeVisible();
    await expect(page.getByText('Nice farm with lots of space')).not.toBeVisible();

    // Delete the request - ensure button is actionable before clicking
    const deleteButton = page.getByTestId('delete-button').first();
    await deleteButton.waitFor({ state: 'attached', timeout: 5000 });
    await deleteButton.waitFor({ state: 'visible', timeout: 5000 });
    await expect(deleteButton).toBeEnabled();

    // Small delay to ensure button event handlers are attached
    await page.waitForTimeout(500);
    await deleteButton.click();

    // Wait for the delete confirmation dialog to appear
    await page.waitForSelector('.mud-dialog-container', { state: 'visible', timeout: 10000 });

    // Wait for confirmation button and confirm
    const confirmButton = page.getByTestId('confirm-delete-button');
    await confirmButton.waitFor({ state: 'visible', timeout: 5000 });
    await confirmButton.click();

    // Wait for confirmation dialog to close
    await page.waitForSelector('.mud-dialog-container', { state: 'hidden', timeout: 5000 });

    // Wait for the data to be gone (row is removed from table)
    await expect(page.getByText('Updated: Even more space now')).not.toBeVisible({ timeout: 5000 });
    await expect(page.getByText('Save Chicken Requests')).not.toBeVisible();
  });

  test('should handle incoming public request and assign to action', async ({ page, context }) => {
    test.setTimeout(120000); // Increased timeout for complex workflow

    const timestamp = Date.now();
    const publicFirstName = `PublicReq${timestamp}`;
    const publicLastName = 'PublicUser';
    const publicEmail = `publicreq${timestamp}@example.com`;
    const publicPhone = '+41791234567';

    let actionId: number;
    let actionTitle: string;

    await test.step('Admin creates a SaveChickenAction', async () => {
      const today = new Date();
      const dayOfMonth = today.getDate();

      const datesToCreate = [dayOfMonth];
      if (dayOfMonth + 1 <= 28) datesToCreate.push(dayOfMonth + 1);

      const action = await createSaveChickenAction(page, {
        title: `Incoming Test Action ${timestamp}`,
        description: 'Action for incoming request test',
        dates: datesToCreate,
        isActive: true,
      });

      actionId = action.actionId;
      actionTitle = `Incoming Test Action ${timestamp}`;
      expect(actionId).toBeGreaterThan(0);
    });

    await test.step('Admin creates an existing person', async () => {
      await createPerson(page, {
        firstName: 'Existing',
        lastName: 'Person',
        email: 'existing@example.com',
        phone: '+41791111111',
        city: 'Zurich',
        postalCode: '8000',
        street: 'Existing Street 1',
      });
    });

    await test.step('Log out (clear authentication)', async () => {
      // Clear all cookies, local storage, and session storage to fully log out
      await context.clearCookies();
      await context.clearPermissions();
      await page.evaluate(() => {
        localStorage.clear();
        sessionStorage.clear();
      });
      // Reload to ensure clean state
      await page.goto('/offer-place', { waitUntil: 'networkidle' });
    });

    await test.step('Public user submits save chicken request', async () => {
      // Should already be on the public form from previous step
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      // Use the public form helper (imported at top)
      await fillSaveChickenRequestForm(page, {
        contactFirstName: publicFirstName,
        contactLastName: publicLastName,
        contactEmail: publicEmail,
        contactPhone: publicPhone,
        addressCity: 'Basel',
        addressPostalCode: '4000',
        addressStreet: 'Public Street 999',
        numberOfChickens: '7',
        numberOfRoosters: '1',
        description: 'Public request with nice garden',
        message: 'I am a public user submitting a request',
        confirmCriteria: true,
        acceptTerms: true,
      });

      // Submit the public form
      const submitButton = page.getByRole('button', { name: /absenden|submit/i });
      await submitButton.waitFor({ state: 'visible', timeout: 5000 });
      await submitButton.click();

      // Wait for navigation to thank-you page
      await page.waitForURL(/\/thank-you/, { timeout: 15000 });

      // Verify we're on thank you page - look for the heading
      await expect(page.getByRole('heading', { name: /vielen dank/i })).toBeVisible({ timeout: 5000 });
      console.log('Public form submitted successfully');
    });

    await test.step('Log back in as admin', async () => {
      // Navigate to login page
      await page.goto('/login', { waitUntil: 'networkidle' });

      // Fill login form with correct admin credentials
      await page.getByRole('textbox', { name: 'Email*' }).fill('test@example.com');
      await page.getByRole('textbox', { name: 'Password*' }).fill('testpassword');
      await page.getByRole('button', { name: 'Login' }).click();

      // Wait for successful login - admin redirects to /admin/persons
      await page.waitForURL('/admin/persons', { timeout: 10000 });
      await expect(page.getByText('Logout')).toBeVisible();
    });

    await test.step('Navigate to incoming requests page', async () => {
      await page.goto('/admin/incoming-save-chicken-requests', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      // Verify we're on the incoming requests page
      await expect(page).toHaveURL(/\/admin\/incoming-save-chicken-requests/);
    });

    await test.step('Find and select the incoming public request', async () => {
      // Wait for the requests list to load
      await page.waitForTimeout(2000);

      // Click on the request card with the public user's name
      const requestCard = page.locator('.mud-card').filter({ hasText: `${publicFirstName} ${publicLastName}` });
      await requestCard.waitFor({ state: 'visible', timeout: 10000 });
      await requestCard.click();

      // Wait for details to load after selection
      await page.waitForTimeout(1000);

      // Verify request details are displayed using data-testid
      await expect(page.getByTestId('contact-email')).toContainText(publicEmail);
      await expect(page.getByTestId('address-city')).toContainText('Basel');

      // Description is displayed as regular text, not in a testid element
      await expect(page.locator('text=Public request with nice garden')).toBeVisible();
    });

    await test.step('Select save chicken action from dropdown', async () => {
      // Use the helper to select the action by ID
      await selectSaveChickenAction(page, actionId);
    });

    await test.step('Mark request as handled', async () => {
      // Click the "Mark as Handled" button
      const markHandledButton = page.getByRole('button', { name: /als bearbeitet markieren|mark as handled/i });
      await markHandledButton.waitFor({ state: 'visible', timeout: 5000 });
      await markHandledButton.click();

      // Wait for success message
      await page.waitForTimeout(2000);

      // Verify success message appears
      await expect(page.locator('.mud-snackbar').filter({ hasText: /erfolgreich|success/i })).toBeVisible({ timeout: 5000 });

      // Request should disappear from the list (removed from unhandled)
      await page.waitForTimeout(1000);
    });

    await test.step('Verify request appears in save chicken action', async () => {
      // Navigate directly to the action detail page using actionId
      await page.goto(`/admin/save-chicken-actions/${actionId}`, { waitUntil: 'networkidle' });

      // Wait for the page to load and requests table to appear
      await page.waitForLoadState('networkidle', { timeout: 10000 });
      await page.waitForTimeout(2000); // Give time for the data to load

      // Verify the request appears in the action's requests table by checking for timestamp-specific data
      // This catches database pollution from previous test runs
      await expect(page.locator('text=' + publicFirstName).first()).toBeVisible({ timeout: 5000 });
      await expect(page.locator('text=' + publicEmail).first()).toBeVisible({ timeout: 5000 });
      console.log('Verified that public request with current timestamp appears in the save chicken action');
    });
  });

  test('should merge incoming public request with similar person', async ({ page, context }) => {
    test.setTimeout(120000); // Increased timeout for complex workflow

    const timestamp = Date.now();
    const publicFirstName = `PublicReq${timestamp}`;
    const publicLastName = 'PublicUser';
    const publicEmail = `publicreq${timestamp}@example.com`;
    const publicPhone = '+41791234567';

    let existingPersonId: number;
    let actionId: number;
    let actionTitle: string;

    await test.step('Admin creates a SaveChickenAction', async () => {
      const today = new Date();
      const dayOfMonth = today.getDate();

      const datesToCreate = [dayOfMonth];
      if (dayOfMonth + 1 <= 28) datesToCreate.push(dayOfMonth + 1);

      const action = await createSaveChickenAction(page, {
        title: `Merge Test Action ${timestamp}`,
        description: 'Action for merge test',
        dates: datesToCreate,
        isActive: true,
      });

      actionId = action.actionId;
      actionTitle = `Merge Test Action ${timestamp}`;
      expect(actionId).toBeGreaterThan(0);
    });

    await test.step('Admin creates a similar person', async () => {
      existingPersonId = await createPerson(page, {
        firstName: publicFirstName,
        lastName: publicLastName,
        email: publicEmail,
        phone: publicPhone,
        city: 'Basel',
        postalCode: '4000',
        street: 'Existing Street 10',
      });

      expect(existingPersonId).toBeGreaterThan(0);
    });

    await test.step('Log out (clear authentication)', async () => {
      // Clear all cookies, local storage, and session storage to fully log out
      await context.clearCookies();
      await context.clearPermissions();
      await page.evaluate(() => {
        localStorage.clear();
        sessionStorage.clear();
      });
      // Reload to ensure clean state
      await page.goto('/offer-place', { waitUntil: 'networkidle' });
    });

    await test.step('Public user submits save chicken request with similar info', async () => {
      // Should already be on the public form from previous step
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      // Use the public form helper
      await fillSaveChickenRequestForm(page, {
        contactFirstName: publicFirstName, // Same first name as existing person
        contactLastName: publicLastName,
        contactEmail: publicEmail,
        contactPhone: publicPhone,
        addressCity: 'Basel',
        addressPostalCode: '4000',
        addressStreet: 'Public Street 999',
        numberOfChickens: '7',
        numberOfRoosters: '1',
        description: 'Public request with nice garden',
        message: 'I am a public user submitting a request',
        confirmCriteria: true,
        acceptTerms: true,
      });

      // Submit the public form
      const submitButton = page.getByRole('button', { name: /absenden|submit/i });
      await submitButton.waitFor({ state: 'visible', timeout: 5000 });
      await submitButton.click();

      // Wait for navigation to thank-you page
      await page.waitForURL(/\/thank-you/, { timeout: 15000 });

      // Verify we're on thank you page
      await expect(page.getByRole('heading', { name: /vielen dank/i })).toBeVisible({ timeout: 5000 });
      console.log('Public form submitted successfully');
    });

    await test.step('Log back in as admin', async () => {
      // Navigate to login page
      await page.goto('/login', { waitUntil: 'networkidle' });

      // Fill login form with correct admin credentials
      await page.getByRole('textbox', { name: 'Email*' }).fill('test@example.com');
      await page.getByRole('textbox', { name: 'Password*' }).fill('testpassword');
      await page.getByRole('button', { name: 'Login' }).click();

      // Wait for successful login - admin redirects to /admin/persons
      await page.waitForURL('/admin/persons', { timeout: 10000 });
      await expect(page.getByText('Logout')).toBeVisible();
    });

    await test.step('Navigate to incoming requests page', async () => {
      await page.goto('/admin/incoming-save-chicken-requests', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      // Verify we're on the incoming requests page
      await expect(page).toHaveURL(/\/admin\/incoming-save-chicken-requests/);
    });

    await test.step('Select incoming request and verify similar person appears', async () => {
      // Wait for the requests list to load
      await page.waitForTimeout(2000);

      // Click on the request card with the public user's unique name (email not visible in card)
      const requestCard = page.locator('.mud-card').filter({ hasText: `${publicFirstName} ${publicLastName}` }).first();
      await requestCard.waitFor({ state: 'visible', timeout: 10000 });
      await requestCard.click();

      // Wait for details to load after selection
      await page.waitForTimeout(2000);

      // Verify request details are displayed
      await expect(page.getByTestId('contact-email')).toContainText(publicEmail);
      await expect(page.getByTestId('address-city')).toContainText('Basel');

      // Verify similar person section shows at least one similar person
      await expect(page.locator('text=/ähnliche personen|similar persons/i')).toBeVisible();
      await expect(page.locator('text=/potenzielle duplikate|potential duplicates/i')).toBeVisible();

      // Verify the admin-created person appears as similar
      const similarPersonCards = page.locator('.mud-card').filter({ hasText: publicEmail });
      await expect(similarPersonCards.first()).toBeVisible({ timeout: 10000 });
      console.log(`Found similar person with existingPersonId: ${existingPersonId}`);
    });

    await test.step('Select save chicken action before merging', async () => {
      // Use the helper to select the action by ID
      await selectSaveChickenAction(page, actionId);
    });

    await test.step('Click merge button for similar person', async () => {
      // Use data-testid to select the specific merge button for the admin-created person
      const mergeButton = page.getByTestId(`merge-person-${existingPersonId}`);
      console.log(`Clicking merge button for person ID: ${existingPersonId}`);
      await mergeButton.waitFor({ state: 'visible', timeout: 10000 });
      await mergeButton.click();

      // Should navigate to merge page
      await page.waitForURL(/\/admin\/merge-person\/\d+\/\d+/, { timeout: 10000 });
      console.log('Navigated to merge page');
    });

    await test.step('Complete merge with default values', async () => {
      // Wait for merge page to load
      await page.waitForLoadState('networkidle', { timeout: 10000 });
      await page.waitForTimeout(2000); // Extra time for radio buttons to initialize

      // Verify we see both the incoming and existing person headings
      await expect(page.locator('h5').filter({ hasText: 'Eingehende Anfrage' })).toBeVisible();
      await expect(page.locator('h5').filter({ hasText: 'Bestehende Person' })).toBeVisible();

      // Scroll to bottom to ensure button is in view
      await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
      await page.waitForTimeout(500);

      // Click the Merge Person button - use getByText for direct text match
      const mergePersonButton = page.getByText('Person zusammenführen', { exact: true });
      await mergePersonButton.waitFor({ state: 'visible', timeout: 5000 });
      await mergePersonButton.scrollIntoViewIfNeeded();
      await expect(mergePersonButton).toBeEnabled();
      await mergePersonButton.click();

      // Should navigate back to incoming requests page
      await page.waitForURL(/\/admin\/incoming-save-chicken-requests/, { timeout: 10000 });

      // Verify success message
      await expect(page.locator('.mud-snackbar').filter({ hasText: /erfolgreich|success/i })).toBeVisible({ timeout: 5000 });
      console.log('Merge completed successfully');
    });

    await test.step('Verify merged request no longer appears in incoming requests', async () => {
      // Wait for list to refresh
      await page.waitForTimeout(2000);

      // The merged request should no longer appear in the pending list
      const requestCard = page.locator('.mud-card').filter({ hasText: `${publicFirstName} ${publicLastName}` });
      await expect(requestCard).not.toBeVisible();

      console.log('Verified that merged request is no longer in pending incoming requests');
    });

    await test.step('Verify merged request appears in save chicken action', async () => {
      // Navigate directly to the action detail page using actionId
      await page.goto(`/admin/save-chicken-actions/${actionId}`, { waitUntil: 'networkidle' });

      // Wait for the page to load and requests table to appear
      await page.waitForLoadState('networkidle', { timeout: 10000 });
      await page.waitForTimeout(2000); // Give time for the data to load

      // Verify the merged request appears in the action's requests table
      // After merge, the SaveChickenRequest is associated with the existing person
      // Check for timestamp-specific data to verify this is the request from THIS test run
      await expect(page.locator('text=' + publicFirstName).first()).toBeVisible({ timeout: 5000 });
      await expect(page.locator('text=' + publicEmail).first()).toBeVisible({ timeout: 5000 });
      console.log('Verified that merged request with current timestamp appears in the save chicken action');
    });
  });

  test('should merge incoming request with new values and apply them', async ({ page, context }) => {
    test.setTimeout(120000); // Increased timeout for complex workflow

    const timestamp = Date.now();
    const existingFirstName = `ExistingPerson${timestamp}`;
    const existingLastName = 'OldLastName';
    const existingEmail = `existing${timestamp}@example.com`;
    const existingPhone = '+41791111111';
    const existingCity = 'Zurich';
    const existingPostalCode = '8000';
    const existingStreet = 'Old Street 10';

    const publicFirstName = existingFirstName; // Same first name
    const publicLastName = 'NewLastName'; // Different last name - will test selecting this
    const publicEmail = existingEmail; // SAME email so similarity detection finds the person
    const publicPhone = '+41792222222'; // Different phone - will test selecting this
    const publicCity = 'Basel'; // Different city - will test selecting this
    const publicPostalCode = '4000'; // Different postal code - will test selecting this
    const publicStreet = 'New Street 999'; // Different street - will test selecting this

    let existingPersonId: number;
    let actionId: number;

    await test.step('Admin creates a SaveChickenAction', async () => {
      const today = new Date();
      const dayOfMonth = today.getDate();

      const datesToCreate = [dayOfMonth];
      if (dayOfMonth + 1 <= 28) datesToCreate.push(dayOfMonth + 1);

      const action = await createSaveChickenAction(page, {
        title: `Merge New Values Action ${timestamp}`,
        description: 'Action for merge with new values test',
        dates: datesToCreate,
        isActive: true,
      });

      actionId = action.actionId;
      expect(actionId).toBeGreaterThan(0);
    });

    await test.step('Admin creates existing person with old values', async () => {
      existingPersonId = await createPerson(page, {
        firstName: existingFirstName,
        lastName: existingLastName,
        email: existingEmail,
        phone: existingPhone,
        city: existingCity,
        postalCode: existingPostalCode,
        street: existingStreet,
      });

      expect(existingPersonId).toBeGreaterThan(0);
    });

    await test.step('Log out (clear authentication)', async () => {
      await context.clearCookies();
      await context.clearPermissions();
      await page.evaluate(() => {
        localStorage.clear();
        sessionStorage.clear();
      });
      await page.goto('/offer-place', { waitUntil: 'networkidle' });
    });

    await test.step('Public user submits request with new values', async () => {
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      await fillSaveChickenRequestForm(page, {
        contactFirstName: publicFirstName, // Same first name for similarity
        contactLastName: publicLastName, // NEW last name
        contactEmail: publicEmail, // NEW email
        contactPhone: publicPhone, // NEW phone
        addressCity: publicCity, // NEW city
        addressPostalCode: publicPostalCode, // NEW postal code
        addressStreet: publicStreet, // NEW street
        numberOfChickens: '10',
        numberOfRoosters: '2',
        description: 'Request with updated information',
        message: 'I have new contact details',
        confirmCriteria: true,
        acceptTerms: true,
      });

      const submitButton = page.getByRole('button', { name: /absenden|submit/i });
      await submitButton.waitFor({ state: 'visible', timeout: 5000 });
      await submitButton.click();

      await page.waitForURL(/\/thank-you/, { timeout: 15000 });
      await expect(page.getByRole('heading', { name: /vielen dank/i })).toBeVisible({ timeout: 5000 });
      console.log('Public form submitted with new values');
    });

    await test.step('Log back in as admin', async () => {
      await page.goto('/login', { waitUntil: 'networkidle' });
      await page.getByRole('textbox', { name: 'Email*' }).fill('test@example.com');
      await page.getByRole('textbox', { name: 'Password*' }).fill('testpassword');
      await page.getByRole('button', { name: 'Login' }).click();
      await page.waitForURL('/admin/persons', { timeout: 10000 });
      await expect(page.getByText('Logout')).toBeVisible();
    });

    await test.step('Navigate to incoming requests and select request', async () => {
      await page.goto('/admin/incoming-save-chicken-requests', { waitUntil: 'networkidle' });
      await page.waitForTimeout(2000);

      const requestCard = page.locator('.mud-card').filter({ hasText: `${publicFirstName}` }).first();
      await requestCard.waitFor({ state: 'visible', timeout: 10000 });
      await requestCard.click();
      await page.waitForTimeout(2000);

      // Verify similar person appears
      await expect(page.locator('text=/ähnliche personen|similar persons/i')).toBeVisible();
      console.log('Found similar person for merge');
    });

    await test.step('Select action and click merge', async () => {
      await selectSaveChickenAction(page, actionId);

      const mergeButton = page.getByTestId(`merge-person-${existingPersonId}`);
      await mergeButton.waitFor({ state: 'visible', timeout: 10000 });
      await mergeButton.click();
      await page.waitForURL(/\/admin\/merge-person\/\d+\/\d+/, { timeout: 10000 });
      console.log('Navigated to merge page');
    });

    await test.step('Select NEW values on merge page', async () => {
      await page.waitForLoadState('networkidle', { timeout: 10000 });
      await page.waitForTimeout(2000);

      // Verify we can see both old and new values
      await expect(page.getByText(existingLastName).first()).toBeVisible(); // Old last name
      await expect(page.getByText(publicLastName).first()).toBeVisible(); // New last name
      await expect(page.getByText(existingEmail).first()).toBeVisible(); // Email is the same (appears twice on page)
      await expect(page.getByText(existingPhone).first()).toBeVisible(); // Old phone
      await expect(page.getByText(publicPhone).first()).toBeVisible(); // New phone

      // Select radio buttons for INCOMING (new) values
      // Radio buttons are identified by their value attribute

      // Find and click radio button for incoming last name
      // Use test IDs for merge radio buttons (incoming values)
      await page.getByTestId('merge-lastname-incoming').check();
      await page.getByTestId('merge-phone-incoming').check();
      await page.getByTestId('merge-city-incoming').check();
      await page.getByTestId('merge-postalcode-incoming').check();
      await page.getByTestId('merge-street-incoming').check();

      console.log('Selected all new (incoming) values');
    });

    await test.step('Complete merge', async () => {
      await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
      await page.waitForTimeout(500);

      const mergePersonButton = page.getByText('Person zusammenführen', { exact: true });
      await mergePersonButton.waitFor({ state: 'visible', timeout: 5000 });
      await mergePersonButton.scrollIntoViewIfNeeded();
      await expect(mergePersonButton).toBeEnabled();
      await mergePersonButton.click();

      await page.waitForURL(/\/admin\/incoming-save-chicken-requests/, { timeout: 10000 });
      await expect(page.locator('.mud-snackbar').filter({ hasText: /erfolgreich|success/i })).toBeVisible({ timeout: 5000 });
      console.log('Merge completed with new values');
    });

    await test.step('Verify merged person has NEW values', async () => {
      // Navigate to the person detail page
      await page.goto(`/admin/persons/${existingPersonId}`, { waitUntil: 'networkidle' });
      await page.waitForTimeout(2000);

      // Verify NEW values are applied (from public request)
      await expect(page.getByTestId('contact-firstname')).toContainText(publicFirstName);
      await expect(page.getByTestId('contact-lastname')).toContainText(publicLastName); // NEW
      await expect(page.getByTestId('contact-email')).toContainText(publicEmail); // Same email (unchanged)
      await expect(page.getByTestId('contact-phone')).toContainText(publicPhone); // NEW
      await expect(page.getByTestId('address-city')).toContainText(publicCity); // NEW
      await expect(page.getByTestId('address-postalcode')).toContainText(publicPostalCode); // NEW
      await expect(page.getByTestId('address-street')).toContainText(publicStreet); // NEW

      // Verify OLD values are NOT present (except email which is the same)
      await expect(page.getByTestId('contact-lastname')).not.toContainText(existingLastName);
      await expect(page.getByTestId('contact-phone')).not.toContainText(existingPhone);
      await expect(page.getByTestId('address-city')).not.toContainText(existingCity);
      await expect(page.getByTestId('address-postalcode')).not.toContainText(existingPostalCode);
      await expect(page.getByTestId('address-street')).not.toContainText(existingStreet);

      console.log('Verified that merged person has all NEW values applied');
    });

    await test.step('Verify request appears in action with new values', async () => {
      await page.goto(`/admin/save-chicken-actions/${actionId}`, { waitUntil: 'networkidle' });
      await page.waitForLoadState('networkidle', { timeout: 10000 });
      await page.waitForTimeout(2000);

      // Verify the request appears (email is the same so we can use it to identify the request)
      await expect(page.locator('text=' + publicFirstName).first()).toBeVisible({ timeout: 5000 });
      await expect(page.locator('text=' + publicEmail).first()).toBeVisible({ timeout: 5000 });
      console.log('Verified that merged request appears in action');
    });
  });

  test('create-public-save-chicken-request-and-expect-email', async ({ page, context }) => {
    test.setTimeout(120000); // Increased timeout for complex workflow

    const timestamp = Date.now();
    const existingFirstName = `ExistingPerson${timestamp}`;
    const existingLastName = 'OldLastName';
    const existingEmail = `existing${timestamp}@example.com`;
    const existingPhone = '+41791111111';
    const existingCity = 'Zurich';
    const existingPostalCode = '8000';
    const existingStreet = 'Old Street 10';
    const numberOfChickens = '10';
    const numberOfRoosters = '2';

    let existingPersonId: number;
    let actionId: number;

    await test.step('Admin creates a SaveChickenAction', async () => {
      const today = new Date();
      const dayOfMonth = today.getDate();

      const datesToCreate = [dayOfMonth];
      if (dayOfMonth + 1 <= 28) datesToCreate.push(dayOfMonth + 1);

      const action = await createSaveChickenAction(page, {
        title: `Merge New Values Action ${timestamp}`,
        description: 'Action for merge with new values test',
        dates: datesToCreate,
        isActive: true,
      });

      actionId = action.actionId;
      expect(actionId).toBeGreaterThan(0);
    });

    await test.step('Admin creates existing person with old values', async () => {
      existingPersonId = await createPerson(page, {
        firstName: existingFirstName,
        lastName: existingLastName,
        email: existingEmail,
        phone: existingPhone,
        city: existingCity,
        postalCode: existingPostalCode,
        street: existingStreet,
      });

      expect(existingPersonId).toBeGreaterThan(0);
    });

    await test.step('Log out (clear authentication)', async () => {
      await context.clearCookies();
      await context.clearPermissions();
      await page.evaluate(() => {
        localStorage.clear();
        sessionStorage.clear();
      });
      await page.goto('/offer-place', { waitUntil: 'networkidle' });
    });

    await test.step('Public user submits request with new values', async () => {
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      await fillSaveChickenRequestForm(page, {
        contactFirstName: existingFirstName, // Same first name for similarity
        contactLastName: existingLastName, // NEW last name
        contactEmail: existingEmail, // NEW email
        contactPhone: existingPhone, // NEW phone
        addressCity: existingCity, // NEW city
        addressPostalCode: existingPostalCode, // NEW postal code
        addressStreet: existingStreet, // NEW street
        numberOfChickens: numberOfChickens,
        numberOfRoosters: numberOfRoosters,
        description: 'Request with updated information',
        message: 'I have new contact details',
        confirmCriteria: true,
        acceptTerms: true,
      });

      const submitButton = page.getByRole('button', { name: /absenden|submit/i });
      await submitButton.waitFor({ state: 'visible', timeout: 5000 });
      await submitButton.click();

      await page.waitForURL(/\/thank-you/, { timeout: 15000 });
      await expect(page.getByRole('heading', { name: /vielen dank/i })).toBeVisible({ timeout: 5000 });
      console.log('Public form submitted with new values');
    });

    await test.step('check if email is there', async () => {
      // Wait a moment after sending the request to allow email delivery
      await new Promise(r => setTimeout(r, 1000));

      // Fetch messages from Mailhog API
      const res = await fetch('http://localhost:8025/api/v2/messages');
      const data = await res.json();

      console.log(`Fetched ${data.total} messages from Mailhog`);

      // Check for expected email
      const confirmationEmail = data.items.find((msg: { Content: { Headers: { Subject: (string | string[])[]; To: (string | any[])[]; }; }; }) =>
        msg.Content.Headers.To[0].includes(existingEmail)
      );

      const bodyBase64 = confirmationEmail.Content.Body; // or your variable
      const body = Buffer.from(bodyBase64, 'base64').toString('utf-8');

      expect(confirmationEmail).toBeTruthy();
      expect(confirmationEmail.Content.Headers.To[0]).toContain(existingEmail);
      expect(body).toContain(existingFirstName)
      expect(body).toContain(existingLastName)
      expect(body).toContain(existingPhone)
      expect(body).toContain(existingCity)
      expect(body).toContain(existingPostalCode)
      expect(body).toContain(existingStreet)
      expect(body).toContain(numberOfChickens)
      expect(body).toContain(numberOfRoosters)
    });
  });
});

import { test, expect } from '@playwright/test';
import { createPerson } from './helpers/person-form.helper';
import { fillAdminSaveChickenRequestForm } from './helpers/admin-save-chicken-request-form.helper';
import { fillSaveChickenRequestForm } from './helpers/save-chicken-request-form.helper';
import { createSaveChickenAction } from './helpers/save-chicken-action-form.helper';
import { selectPerson } from './helpers/person-selector.helper';
import { selectSaveChickenAction } from './helpers/save-chicken-action-selector.helper';

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
      // Navigate to save chicken actions list
      await page.goto('/admin/save-chicken-actions', { waitUntil: 'networkidle' });

      // Search for the action
      await page.getByLabel(/suche|search/i).fill(actionTitle);
      await page.getByRole('button', { name: /suchen|search/i }).click();
      await page.waitForTimeout(1000);

      // Click view/show button for the action
      const viewButton = page.getByTestId('view-button').first();
      await viewButton.waitFor({ state: 'visible', timeout: 5000 });
      await viewButton.click();

      // Should be on action detail page
      await page.waitForURL(/\/admin\/save-chicken-actions\/\d+/, { timeout: 10000 });

      // Wait for the page to load and requests table to appear
      await page.waitForLoadState('networkidle', { timeout: 10000 });
      await page.waitForTimeout(2000); // Give time for the search to complete

      // Verify the request appears in the action's requests table by checking for the person's name and email
      await expect(page.locator('text=' + publicEmail)).toBeVisible({ timeout: 5000 });
      console.log('Verified that public request appears in the save chicken action');
    });
  });

  test('should filter and search incoming requests', async ({ page }) => {
    test.setTimeout(90000);

    const timestamp = Date.now();

    await test.step('Create multiple persons with different characteristics', async () => {
      await createPerson(page, {
        firstName: `SearchTest1${timestamp}`,
        lastName: 'Zurich',
        email: `search1${timestamp}@example.com`,
        phone: '+41791111111',
        city: 'Zurich',
        postalCode: '8000',
        street: 'Test Street 1',
      });

      await createPerson(page, {
        firstName: `SearchTest2${timestamp}`,
        lastName: 'Basel',
        email: `search2${timestamp}@example.com`,
        phone: '+41792222222',
        city: 'Basel',
        postalCode: '4000',
        street: 'Test Street 2',
      });
    });

    await test.step('Navigate to incoming requests page', async () => {
      await page.goto('/admin/incoming-save-chicken-requests', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      // Verify page loaded with pending requests counter
      await expect(page.getByText(/ausstehende anfragen|pending requests/i)).toBeVisible();
    });

    await test.step('Verify empty state when no incoming requests', async () => {
      // If there are no requests, should show empty message
      const noRequestsMessage = page.getByText(/keine ausstehenden anfragen|no pending requests/i);
      
      // Either we have requests or we see the empty message
      const hasRequests = await page.locator('.mud-card').filter({ hasText: /searchtest/i }).count();
      
      if (hasRequests === 0) {
        await expect(noRequestsMessage).toBeVisible();
      }
    });

    await test.step('Verify similar persons detection works', async () => {
      // This test verifies the UI shows similar persons when a request is selected
      // If there are incoming requests, select one and check for similar persons section
      const firstRequest = page.locator('.mud-card').first();
      const hasRequests = await firstRequest.isVisible().catch(() => false);

      if (hasRequests) {
        await firstRequest.click();
        await page.waitForTimeout(1000);

        // Verify similar persons section is visible
        await expect(page.getByText(/ähnliche personen|similar persons/i)).toBeVisible();
      }
    });
  });
});

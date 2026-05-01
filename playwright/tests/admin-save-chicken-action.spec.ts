import { test, expect } from '@playwright/test';
import { getMultiDateSelector } from './helpers/multi-date-selector.helper';

// Configure browser for this test file (top-level)
test.use({
  headless: false,
  launchOptions: {
    slowMo: 500,
  },
});

test.describe('Admin - Create Save Chicken Action', () => {
  test('admin creates a new save chicken action', async ({ page }) => {
    const timestamp = Date.now();
    const testTitle = `Test Action ${timestamp}`;
    const testDescription = `Test description for action created at ${timestamp}`;

    await test.step('Navigate to new save chicken action page', async () => {
      await page.goto('/admin/save-chicken-actions/new', { waitUntil: 'networkidle' });

      // Wait for the page heading to be visible
      await expect(page.getByText('Neue Rettungsaktion für Hühner')).toBeVisible();
    });

    await test.step('Fill in the form', async () => {
      // Fill in Title using label selector (supports German "Titel" and English "Title")
      await page.getByLabel(/titel|title/i).click();
      await page.getByLabel(/titel|title/i).fill(testTitle);

      // Fill in Description using label selector (supports German "Beschreibung" and English "Description")
      await page.getByLabel(/beschreibung|description/i).click();
      await page.getByLabel(/beschreibung|description/i).fill(testDescription);

      // Select multiple dates using MultiDateSelector helper
      const dateSelector = getMultiDateSelector(page);
      const today = new Date();
      const dayOfMonth = today.getDate();

      // Select today and two more days (if they exist in the month)
      const datesToSelect = [dayOfMonth];
      if (dayOfMonth + 1 <= 28) datesToSelect.push(dayOfMonth + 1);
      if (dayOfMonth + 2 <= 28) datesToSelect.push(dayOfMonth + 2);

      await dateSelector.selectDates(datesToSelect);

      // Set breakpoint here if you want to inspect before submitting
      // await page.pause();
    });

    await test.step('Submit the form', async () => {
      // Click the submit/create button using role selector (supports German "Erstellen" and English "Create")
      await page.getByRole('button', { name: /erstellen|create/i }).click();

      // Wait for navigation to the detail page
      await page.waitForURL(/\/admin\/save-chicken-actions\/\d+/, { timeout: 10000 });

      // Verify we're on the detail page and can see the created action
      await expect(page.getByTestId('action-title')).toHaveText(testTitle);
    });

    await test.step('Verify action was created and displays correct details', async () => {
      // Should be on the detail page now, not thank-you
      expect(page.url()).toMatch(/\/admin\/save-chicken-actions\/\d+/);

      // Navigate to the save chicken actions list page
      await page.goto('/admin/save-chicken-actions-list', { waitUntil: 'networkidle' });

      // Wait for the page to load
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      // Search for the created action by title using test id
      const searchBox = page.getByTestId('search-box');
      await searchBox.fill(testTitle);

      // Click the search button
      await page.getByTestId('search-button').click();
      await page.waitForTimeout(1000); // Wait for search to filter results

      // Click the view button (eye icon) for the newly created action
      const actionRow = page.locator('tr').filter({ hasText: testTitle });
      await actionRow.getByTestId('view-button').click();

      // Wait for the detail page to load
      await page.waitForURL(/\/admin\/save-chicken-actions\/\d+/, { timeout: 10000 });

      // Verify the action details are displayed correctly
      await expect(page.getByTestId('action-title')).toHaveText(testTitle);
      await expect(page.getByTestId('action-description')).toHaveText(testDescription);

      // Verify the selected dates are displayed
      const transferDatesSection = page.getByTestId('action-transfer-dates');
      await expect(transferDatesSection).toBeVisible();

      // Verify at least the dates we selected are shown
      const today = new Date();
      const dayOfMonth = today.getDate();
      const month = String(today.getMonth() + 1).padStart(2, '0');
      const year = today.getFullYear();

      const expectedDate1 = `${String(dayOfMonth).padStart(2, '0')}.${month}.${year}`;
      await expect(transferDatesSection).toContainText(expectedDate1);
    });
  });

  test('should show validation errors for required fields', async ({ page }) => {
    // Navigate to new save chicken action page
    await page.goto('/admin/save-chicken-actions/new', { waitUntil: 'networkidle' });

    // Wait for the form to load
    await page.waitForSelector('h3:has-text("Neue Rettungsaktion")', { state: 'visible', timeout: 10000 });

    // Try to submit without filling required fields
    await page.getByRole('button', { name: /erstellen|create/i }).click();

    // Verify validation errors appear
    await expect(page.getByText('The Title field is required.')).toBeVisible();
    await expect(page.getByText('The Description field is required.')).toBeVisible();
  });

  test('should create and then update a save chicken action', async ({ page }) => {
    const timestamp = Date.now();
    const originalTitle = `Original Action ${timestamp}`;
    const originalDescription = `Original description ${timestamp}`;
    const updatedTitle = `Updated Action ${timestamp}`;
    const updatedDescription = `Updated description ${timestamp}`;

    // Store the dates for verification
    const today = new Date();
    const originalDay = today.getDate();
    const newDay = originalDay === 1 ? 2 : originalDay - 1; // Pick a different day

    const formatDate = (day: number) => {
      const date = new Date(today.getFullYear(), today.getMonth(), day);
      return `${String(date.getDate()).padStart(2, '0')}.${String(date.getMonth() + 1).padStart(2, '0')}.${date.getFullYear()}`;
    };

    const originalDate = formatDate(originalDay);
    const newDate = formatDate(newDay);

    await test.step('Create a new save chicken action', async () => {
      // Navigate to new save chicken action page
      await page.goto('/admin/save-chicken-actions/new', { waitUntil: 'networkidle' });

      // Fill in the form (supports German and English labels)
      await page.getByLabel(/titel|title/i).click();
      await page.getByLabel(/titel|title/i).fill(originalTitle);
      await page.getByLabel(/beschreibung|description/i).click();
      await page.getByLabel(/beschreibung|description/i).fill(originalDescription);

      // Select original date
      const dateSelector = getMultiDateSelector(page);
      await dateSelector.selectDate(originalDay);

      // Submit the form (supports German "Erstellen" and English "Create")
      await page.getByRole('button', { name: /erstellen|create/i }).click();

      // Wait for navigation to detail page
      await page.waitForURL(/\/admin\/save-chicken-actions\/\d+/, { timeout: 10000 });

      // Verify original details
      await expect(page.getByTestId('action-title')).toHaveText(originalTitle);
      await expect(page.getByTestId('action-description')).toHaveText(originalDescription);

      // Verify original date is shown
      const transferDatesSection = page.getByTestId('action-transfer-dates');
      await expect(transferDatesSection).toContainText(originalDate);
    });

    await test.step('Navigate to edit page', async () => {
      // Extract the ID from the current URL
      const url = page.url();
      const match = url.match(/\/admin\/save-chicken-actions\/(\d+)/);
      expect(match).toBeTruthy();
      const actionId = match![1];

      // Navigate to edit page
      await page.goto(`/admin/save-chicken-actions/${actionId}/edit`, { waitUntil: 'networkidle' });

      // Wait for the edit form to load
      await page.waitForSelector('h2:has-text("Edit Save Chicken Request")', { state: 'visible', timeout: 10000 });
    });

    await test.step('Update the save chicken action', async () => {
      // Update title - clear by selecting all and typing
      const titleInput = page.getByLabel(/titel|title/i);
      await titleInput.click();
      await titleInput.press('Control+A');
      await titleInput.fill(updatedTitle);

      // Update description - clear by selecting all and typing
      const descriptionInput = page.getByLabel(/beschreibung|description/i);
      await descriptionInput.click();
      await descriptionInput.press('Control+A');
      await descriptionInput.fill(updatedDescription);

      // Change the date - deselect original and select new
      const dateSelector = getMultiDateSelector(page);
      await dateSelector.deselectDate(originalDay);  // Remove original date
      await dateSelector.selectDate(newDay);         // Add new date

      // Submit the update - wait for the button to be visible and enabled
      const updateButton = page.getByRole('button', { name: /update|aktualisieren/i });
      await updateButton.waitFor({ state: 'visible', timeout: 5000 });
      await updateButton.click();

      // Wait for navigation back to detail page
      await page.waitForURL(/\/admin\/save-chicken-actions\/\d+/, { timeout: 10000 });
    });

    await test.step('Verify updated details', async () => {
      // Verify updated title
      await expect(page.getByTestId('action-title')).toHaveText(updatedTitle);

      // Verify updated description
      await expect(page.getByTestId('action-description')).toHaveText(updatedDescription);

      // Verify date changed - new date should be shown, old date should not
      const transferDatesSection = page.getByTestId('action-transfer-dates');
      await expect(transferDatesSection).toContainText(newDate);
      await expect(transferDatesSection).not.toContainText(originalDate);
    });
  });

  test('should create and then delete a save chicken action', async ({ page }) => {
    const timestamp = Date.now();
    const testTitle = `Delete Test Action ${timestamp}`;
    const testDescription = `This action will be deleted ${timestamp}`;

    await test.step('Create a new save chicken action', async () => {
      // Navigate to new save chicken action page
      await page.goto('/admin/save-chicken-actions/new', { waitUntil: 'networkidle' });

      // Fill in the form (supports German and English labels)
      await page.getByLabel(/titel|title/i).click();
      await page.getByLabel(/titel|title/i).fill(testTitle);
      await page.getByLabel(/beschreibung|description/i).click();
      await page.getByLabel(/beschreibung|description/i).fill(testDescription);

      // Select a date
      const dateSelector = getMultiDateSelector(page);
      const today = new Date();
      await dateSelector.selectDate(today.getDate());

      // Submit the form (supports German "Erstellen" and English "Create")
      await page.getByRole('button', { name: /erstellen|create/i }).click();

      // Wait for navigation to detail page
      await page.waitForURL(/\/admin\/save-chicken-actions\/\d+/, { timeout: 10000 });

      // Verify creation
      await expect(page.getByTestId('action-title')).toHaveText(testTitle);
    });

    await test.step('Navigate to list and find the created action', async () => {
      // Navigate to save chicken actions list page
      await page.goto('/admin/save-chicken-actions-list', { waitUntil: 'networkidle' });

      // Wait for the page to load
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      // Search for the created action by title using placeholder
      const searchBox = page.getByPlaceholder('Nach Titel suchen...');
      await searchBox.fill(testTitle);

      // Click the search button
      await page.getByTestId('search-button').click();
      await page.waitForTimeout(1000); // Wait for search to filter

      // Find the row containing our action
      const actionRow = page.locator('tr').filter({ hasText: testTitle });
      await expect(actionRow).toBeVisible();
    });

    await test.step('Delete the action', async () => {
      // Find the delete button in the row using data-testid
      const actionRow = page.locator('tr').filter({ hasText: testTitle });
      const deleteButton = actionRow.getByTestId('delete-button');

      // Click delete button
      await deleteButton.click();

      // Wait for confirmation dialog with specific text
      await page.waitForSelector('text=wirklich löschen?', { state: 'visible', timeout: 5000 });

      // Wait for "Diese Aktion kann nicht rückgängig gemacht werden" to be visible
      await expect(page.getByText('Diese Aktion kann nicht rückgängig gemacht werden')).toBeVisible();

      // Click the "Löschen" (Delete) button in the dialog
      const confirmDeleteButton = page.getByRole('button', { name: 'Löschen' });
      await confirmDeleteButton.click();

      // Wait a bit for deletion to complete
      await page.waitForTimeout(1000);
    });

    await test.step('Verify action is deleted', async () => {
      // Search for the deleted action again using placeholder
      const searchBox = page.getByPlaceholder('Nach Titel suchen...');
      await searchBox.clear();
      await searchBox.fill(testTitle);

      // Click the search button
      await page.getByTestId('search-button').click();
      await page.waitForTimeout(1000);

      // The action should not be in the list anymore
      const actionRow = page.locator('tr').filter({ hasText: testTitle });
      await expect(actionRow).not.toBeVisible();
    });
  });
});

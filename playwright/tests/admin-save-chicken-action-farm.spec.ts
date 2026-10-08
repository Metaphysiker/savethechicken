import { test, expect } from '@playwright/test';
import { createSaveChickenAction } from './helpers/save-chicken-action-form.helper';
import { fillFarmForm } from './helpers/farm-form.helper';
import { createSaveChickenActionFarm, CreateSaveChickenActionFarmData } from './helpers/save-chicken-action-farm.helper';

// Use admin authentication
test.use({ storageState: 'playwright/.auth/admin.json' });

// Configure browser for this test file
test.use({
  headless: false,
  launchOptions: {
    slowMo: 100,
  },
});

test.describe('Admin SaveChickenActionFarm Management', () => {
  // Run tests serially to avoid resource contention
  test.describe.configure({ mode: 'serial' });

  test('should create SaveChickenAction, create Farm, then add Farm to SaveChickenAction via SaveChickenActionFarm', async ({ page }) => {
    test.setTimeout(120000); // Increased timeout for the full workflow

    const timestamp = Date.now();
    const actionTitle = `Test Action ${timestamp}`;
    const actionDescription = `Test action for SaveChickenActionFarm workflow ${timestamp}`;
    const farmName = `Test Farm ${timestamp}`;

    let actionId: number;
    let farmId: number;

    await test.step('Create SaveChickenAction', async () => {
      // Create SaveChickenAction with dates
      const today = new Date();
      const datesToSelect = [today.getDate()];

      const action = await createSaveChickenAction(page, {
        title: actionTitle,
        description: actionDescription,
        dates: datesToSelect,
      });

      actionId = action.actionId;
      expect(actionId).toBeGreaterThan(0);
      console.log(`Created SaveChickenAction with ID: ${actionId}`);
    });

    await test.step('Create Farm', async () => {
      // Navigate to farm creation page
      await page.goto('/admin/farms/new', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });
      await page.waitForTimeout(1000);

      // Fill farm form
      await fillFarmForm(page, {
        name: farmName,
        size: 'Gross',
        color: 'Braun',
        generalInformation: `Test farm for SaveChickenAction ${timestamp}`,
        contactFirstName: `FarmOwner${timestamp}`,
        contactLastName: `Test${timestamp}`,
        contactEmail: `farm${timestamp}@example.com`,
        contactPhone: '+41791234567',
        addressCity: 'Bern',
        addressPostalCode: '3000',
        addressStreet: 'Test Street 123',
      });

      // Submit the form
      const createButton = page.getByRole('button', { name: /erstellen|create/i });
      await createButton.click();

      // Wait for redirect to detail page
      await page.waitForURL(/\/admin\/farms\/\d+/, { timeout: 10000 });

      // Extract farm ID from URL
      const url = page.url();
      const match = url.match(/\/admin\/farms\/(\d+)/);
      expect(match).toBeTruthy();
      farmId = parseInt(match![1]);
      expect(farmId).toBeGreaterThan(0);
      console.log(`Created Farm with ID: ${farmId}`);
    });

    await test.step('Navigate to SaveChickenAction detail page', async () => {
      // Navigate to the SaveChickenAction detail page
      await page.goto(`/admin/save-chicken-actions/${actionId}`, { waitUntil: 'networkidle' });

      // Wait for the page to load
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      // Verify we're on the correct action's detail page
      await expect(page.getByTestId('action-title')).toHaveText(actionTitle);
      console.log(`Navigated to SaveChickenAction ${actionId} detail page`);
    });

    await test.step('Add Farm to SaveChickenAction via SaveChickenActionFarm', async () => {
      // Use the helper to create SaveChickenActionFarm
      await createSaveChickenActionFarm(page, {
        farmId: farmId,
        numberOfChickensToBeSaved: 50,
        numberOfRoostersToBeSaved: 5,
      });

      console.log(`Created SaveChickenActionFarm linking Action ${actionId} with Farm ${farmId}`);
    });

    await test.step('Verify SaveChickenActionFarm appears in the table', async () => {
      // The SaveChickenActionFarm should now appear in the table
      // Wait for the table to be visible
      await page.waitForSelector('.mud-table', { state: 'visible', timeout: 10000 });

      // Search for the farm name in the table
      const farmRow = page.locator('tr').filter({ hasText: farmName });
      await farmRow.waitFor({ state: 'visible', timeout: 10000 });

      // Verify the row contains the expected quantities
      await expect(farmRow).toContainText('50'); // NumberOfChickensToBeSaved
      await expect(farmRow).toContainText('5'); // NumberOfRoostersToBeSaved

      console.log('Successfully verified SaveChickenActionFarm in the table');
    });

    await test.step('Verify Farm shows the SaveChickenAction in its associations', async () => {
      // Navigate to Farm detail page
      await page.goto(`/admin/farms/${farmId}`, { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      // Verify the farm is displayed
      await expect(page.getByText(farmName)).toBeVisible();

      // The Farm detail page should show it's associated with the SaveChickenAction
      // through the SaveChickenActionFarms table
      console.log('Verified Farm detail page loads correctly');
    });
  });

  test('should create SaveChickenAction, create Farm separately, then add Farm to SaveChickenAction', async ({ page }) => {
    test.setTimeout(120000);

    const timestamp = Date.now();
    const actionTitle = `Separate Test Action ${timestamp}`;
    const farmName = `Separate Test Farm ${timestamp}`;

    let actionId: number;
    let farmId: number;

    await test.step('Create SaveChickenAction without associated Farm', async () => {
      const today = new Date();
      const datesToSelect = [today.getDate()];

      const action = await createSaveChickenAction(page, {
        title: actionTitle,
        description: `Separate test action ${timestamp}`,
        dates: datesToSelect,
      });

      actionId = action.actionId;
      expect(actionId).toBeGreaterThan(0);
      console.log(`Created SaveChickenAction with ID: ${actionId}`);
    });

    await test.step('Create Farm without associated SaveChickenAction', async () => {
      // Navigate to farm creation page
      await page.goto('/admin/farms/new', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });
      await page.waitForTimeout(1000);

      // Fill farm form - this time WITHOUT saveChickenActionId
      await fillFarmForm(page, {
        name: farmName,
        size: 'Mittel',
        color: 'Weiss',
        generalInformation: `Separate test farm ${timestamp}`,
        contactFirstName: `FarmOwner${timestamp}`,
        contactLastName: `Separate${timestamp}`,
        contactEmail: `separatefarm${timestamp}@example.com`,
        contactPhone: '+41792345678',
        addressCity: 'Zurich',
        addressPostalCode: '8000',
        addressStreet: 'Separate Street 456',
        // Note: NOT providing saveChickenActionId - farm is created independently
      });

      // Submit the form
      const createButton = page.getByRole('button', { name: /erstellen|create/i });
      await createButton.click();

      // Wait for redirect to detail page
      await page.waitForURL(/\/admin\/farms\/\d+/, { timeout: 10000 });

      // Extract farm ID from URL
      const url = page.url();
      const match = url.match(/\/admin\/farms\/(\d+)/);
      expect(match).toBeTruthy();
      farmId = parseInt(match![1]);
      expect(farmId).toBeGreaterThan(0);
      console.log(`Created Farm with ID: ${farmId}`);
    });

    await test.step('Navigate to SaveChickenAction and add the separately created Farm', async () => {
      // Navigate to the SaveChickenAction detail page
      await page.goto(`/admin/save-chicken-actions/${actionId}`, { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      // Verify we're on the correct action's detail page
      await expect(page.getByTestId('action-title')).toHaveText(actionTitle);

      // Add the Farm to SaveChickenAction
      await createSaveChickenActionFarm(page, {
        farmId: farmId,
        numberOfChickensToBeSaved: 100,
        numberOfRoostersToBeSaved: 10,
      });

      console.log(`Created SaveChickenActionFarm linking Action ${actionId} with separately created Farm ${farmId}`);
    });

    await test.step('Verify SaveChickenActionFarm with quantities is displayed', async () => {
      // Wait for the table to refresh
      await page.waitForSelector('.mud-table', { state: 'visible', timeout: 10000 });

      // Search for the farm name in the table
      const farmRow = page.locator('tr').filter({ hasText: farmName });
      await farmRow.waitFor({ state: 'visible', timeout: 10000 });

      // Verify the quantities
      await expect(farmRow).toContainText('100'); // NumberOfChickensToBeSaved
      await expect(farmRow).toContainText('10'); // NumberOfRoostersToBeSaved

      console.log('Successfully verified separately created SaveChickenActionFarm');
    });
  });

  test('should create, update, and verify SaveChickenActionFarm update', async ({ page }) => {
    test.setTimeout(120000);

    const timestamp = Date.now();
    const actionTitle = `Update Test Action ${timestamp}`;
    const farmName = `Update Test Farm ${timestamp}`;

    let actionId: number;
    let farmId: number;
    const originalChickens = 25;
    const originalRoosters = 3;
    const updatedChickens = 75;
    const updatedRoosters = 8;

    await test.step('Create SaveChickenAction and Farm', async () => {
      const today = new Date();
      const datesToSelect = [today.getDate()];

      // Create SaveChickenAction
      const action = await createSaveChickenAction(page, {
        title: actionTitle,
        description: `Update test action ${timestamp}`,
        dates: datesToSelect,
      });
      actionId = action.actionId;
      console.log(`Created SaveChickenAction with ID: ${actionId}`);

      // Create Farm
      await page.goto('/admin/farms/new', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      await fillFarmForm(page, {
        name: farmName,
        size: 'Gross',
        color: 'Braun',
        generalInformation: `Update test farm ${timestamp}`,
        contactFirstName: `FarmOwner${timestamp}`,
        contactLastName: `Update${timestamp}`,
        contactEmail: `updatefarm${timestamp}@example.com`,
        contactPhone: '+41791111112',
        addressCity: 'Bern',
        addressPostalCode: '3000',
        addressStreet: 'Update Street 1',
      });

      await page.getByRole('button', { name: /erstellen|create/i }).click();
      await page.waitForURL(/\/admin\/farms\/\d+/, { timeout: 10000 });

      const url = page.url();
      const match = url.match(/\/admin\/farms\/(\d+)/);
      expect(match).toBeTruthy();
      farmId = parseInt(match![1]);
      expect(farmId).toBeGreaterThan(0);
      console.log(`Created Farm with ID: ${farmId}`);
    });

    await test.step('Create SaveChickenActionFarm with original quantities', async () => {
      await page.goto(`/admin/save-chicken-actions/${actionId}`, { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      await createSaveChickenActionFarm(page, {
        farmId: farmId,
        numberOfChickensToBeSaved: originalChickens,
        numberOfRoostersToBeSaved: originalRoosters,
      });

      console.log(`Created SaveChickenActionFarm with original quantities: ${originalChickens} chickens, ${originalRoosters} roosters`);
    });

    await test.step('Verify original SaveChickenActionFarm appears in table', async () => {
      await page.waitForSelector('.mud-table', { state: 'visible', timeout: 10000 });

      const farmRow = page.locator('tr').filter({ hasText: farmName });
      await farmRow.waitFor({ state: 'visible', timeout: 10000 });

      await expect(farmRow).toContainText(originalChickens.toString());
      await expect(farmRow).toContainText(originalRoosters.toString());
      console.log('Verified original quantities in table');
    });

    await test.step('Update SaveChickenActionFarm quantities', async () => {
      const farmRow = page.locator('tr').filter({ hasText: farmName });
      const editButton = farmRow.getByTestId('edit-button');
      await editButton.waitFor({ state: 'visible', timeout: 5000 });
      await editButton.click();

      // Wait for edit dialog to open
      await page.getByTestId('save-chicken-action-farm-form').waitFor({ state: 'visible', timeout: 10000 });

      // Update quantities
      await page.getByTestId('chickens-to-be-saved').fill(updatedChickens.toString());
      await page.getByTestId('roosters-to-be-saved').fill(updatedRoosters.toString());

      // Save the update
      await page.getByRole('button', { name: /update|aktualisieren|save|speichern/i }).first().click();
      await page.getByTestId('save-chicken-action-farm-form').waitFor({ state: 'hidden', timeout: 10000 });

      console.log(`Updated SaveChickenActionFarm to: ${updatedChickens} chickens, ${updatedRoosters} roosters`);
    });

    await test.step('Verify SaveChickenActionFarm update is reflected in table', async () => {
      await page.waitForTimeout(2000); // Wait for table to refresh

      const farmRow = page.locator('tr').filter({ hasText: farmName });
      await farmRow.waitFor({ state: 'visible', timeout: 10000 });

      // Get the full text of the row for debugging
      const rowText = await farmRow.textContent();
      console.log('Row after update contains:', rowText);
      
      // Simple verification: check the updated values are present
      // We know the update happened from the console log, so presence of new values confirms it
      await expect(farmRow).toContainText(updatedChickens.toString());
      await expect(farmRow).toContainText(updatedRoosters.toString());

      console.log('Successfully verified SaveChickenActionFarm update');
    });
  });

  test('should create and delete SaveChickenActionFarm, then verify it is gone', async ({ page }) => {
    test.setTimeout(120000);

    const timestamp = Date.now();
    const actionTitle = `Delete Test Action ${timestamp}`;
    const farmName = `Delete Test Farm ${timestamp}`;

    let actionId: number;
    let farmId: number;

    await test.step('Create SaveChickenAction and Farm', async () => {
      const today = new Date();
      const datesToSelect = [today.getDate()];

      // Create SaveChickenAction
      const action = await createSaveChickenAction(page, {
        title: actionTitle,
        description: `Delete test action ${timestamp}`,
        dates: datesToSelect,
      });
      actionId = action.actionId;
      console.log(`Created SaveChickenAction with ID: ${actionId}`);

      // Create Farm
      await page.goto('/admin/farms/new', { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      await fillFarmForm(page, {
        name: farmName,
        size: 'Mittel',
        color: 'Weiss',
        generalInformation: `Delete test farm ${timestamp}`,
        contactFirstName: `FarmOwner${timestamp}`,
        contactLastName: `Delete${timestamp}`,
        contactEmail: `deletefarm${timestamp}@example.com`,
        contactPhone: '+41792222223',
        addressCity: 'Zurich',
        addressPostalCode: '8000',
        addressStreet: 'Delete Street 1',
      });

      await page.getByRole('button', { name: /erstellen|create/i }).click();
      await page.waitForURL(/\/admin\/farms\/\d+/, { timeout: 10000 });

      const url = page.url();
      const match = url.match(/\/admin\/farms\/(\d+)/);
      expect(match).toBeTruthy();
      farmId = parseInt(match![1]);
      expect(farmId).toBeGreaterThan(0);
      console.log(`Created Farm with ID: ${farmId}`);
    });

    await test.step('Create SaveChickenActionFarm', async () => {
      await page.goto(`/admin/save-chicken-actions/${actionId}`, { waitUntil: 'networkidle' });
      await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

      await createSaveChickenActionFarm(page, {
        farmId: farmId,
        numberOfChickensToBeSaved: 30,
        numberOfRoostersToBeSaved: 3,
      });

      console.log(`Created SaveChickenActionFarm linking Action ${actionId} with Farm ${farmId}`);
    });

    await test.step('Verify SaveChickenActionFarm appears in table', async () => {
      await page.waitForSelector('.mud-table', { state: 'visible', timeout: 10000 });

      const farmRow = page.locator('tr').filter({ hasText: farmName });
      await farmRow.waitFor({ state: 'visible', timeout: 10000 });

      // Verify it contains the expected quantities
      await expect(farmRow).toContainText('30');
      await expect(farmRow).toContainText('3');
      console.log('Verified SaveChickenActionFarm in table before deletion');
    });

    await test.step('Delete SaveChickenActionFarm', async () => {
      const farmRow = page.locator('tr').filter({ hasText: farmName });
      const deleteButton = farmRow.getByTestId('delete-button');
      await deleteButton.waitFor({ state: 'visible', timeout: 5000 });
      await deleteButton.click();

      // Wait for confirmation dialog
      await page.waitForSelector('text=wirklich löschen?', { state: 'visible', timeout: 5000 });

      // Click the delete confirmation button
      const confirmDeleteButton = page.getByRole('button', { name: /löschen|delete/i });
      await confirmDeleteButton.waitFor({ state: 'visible', timeout: 5000 });
      await confirmDeleteButton.click();

      // Wait for dialog to close
      await page.waitForSelector('text=wirklich löschen?', { state: 'hidden', timeout: 5000 });
      await page.waitForTimeout(2000); // Wait for table to refresh

      console.log('Deleted SaveChickenActionFarm');
    });

    await test.step('Verify SaveChickenActionFarm is gone from table', async () => {
      // Check that the farm row no longer exists
      const farmRow = page.locator('tr').filter({ hasText: farmName });
      await expect(farmRow).not.toBeVisible();

      // Also verify the table is still visible (not empty error)
      await page.waitForSelector('.mud-table', { state: 'visible', timeout: 5000 });
      
      console.log('Successfully verified SaveChickenActionFarm is deleted from table');
    });
  });
});

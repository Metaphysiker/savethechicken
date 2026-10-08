import { Page } from '@playwright/test';
import { fillSaveChickenActionFarmForm } from './save-chicken-action-farm-form.helper';

export interface CreateSaveChickenActionFarmData {
  farmId: number;
  numberOfChickensToBeSaved?: number;
  numberOfRoostersToBeSaved?: number;
}

/**
 * Creates a new SaveChickenActionFarm from the SaveChickenAction detail page
 * This assumes we're already on a SaveChickenAction detail page
 * The SaveChickenAction is automatically pre-selected when opening from the detail page
 */
export async function createSaveChickenActionFarm(page: Page, data: CreateSaveChickenActionFarmData): Promise<void> {
  console.log(`Creating SaveChickenActionFarm with Farm ID: ${data.farmId}`);

  // Click the "Add SaveChickenActionFarm" button
  const addButton = page.getByTestId('add-action-farm-button');
  await addButton.waitFor({ state: 'visible', timeout: 10000 });
  await addButton.click();

  // Wait for the dialog to open
  await page.getByTestId('save-chicken-action-farm-form').waitFor({ state: 'visible', timeout: 10000 });

  // Fill the form
  await fillSaveChickenActionFarmForm(page, {
    farmId: data.farmId,
    numberOfChickensToBeSaved: data.numberOfChickensToBeSaved,
    numberOfRoostersToBeSaved: data.numberOfRoostersToBeSaved,
  });

  // Click the create/submit button - supports both English and German
  const createButton = page.getByRole('button', { name: /erstellen|create|speichern|save/i }).first();
  await createButton.waitFor({ state: 'visible', timeout: 5000 });

  // Check if button is enabled
  const isDisabled = await createButton.isDisabled();
  if (isDisabled) {
    console.log('Create button is disabled - checking for validation errors');
    const errors = page.locator('.validation-message, .mud-input-error');
    const errorCount = await errors.count();
    console.log(`Found ${errorCount} validation errors`);
    if (errorCount > 0) {
      const errorTexts = await errors.allTextContents();
      console.log('Validation errors:', errorTexts);
    }
  }

  await createButton.click();

  // Wait for the dialog to close - check that the form is no longer visible
  await page.getByTestId('save-chicken-action-farm-form').waitFor({ state: 'hidden', timeout: 10000 });

  // Wait for the table to refresh
  await page.waitForTimeout(2000);

  console.log('Successfully created SaveChickenActionFarm');
}

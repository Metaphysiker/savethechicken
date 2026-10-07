import { Page } from '@playwright/test';
import { selectFarm } from './farm-selector.helper';

export interface SaveChickenActionFarmFormData {
  // Required: FarmId to associate
  farmId: number;

  // Optional: Quantities
  numberOfChickensToBeSaved?: number;
  numberOfRoostersToBeSaved?: number;
}

/**
 * Fills the SaveChickenActionFarm form in a dialog
 * Note: When opened from SaveChickenAction detail page, the SaveChickenAction is pre-selected and read-only
 */
export async function fillSaveChickenActionFarmForm(page: Page, data: SaveChickenActionFarmFormData): Promise<void> {
  // Wait for the form to be visible
  await page.waitForSelector('text=SaveChickenActionAndFarmSelection', { state: 'visible', timeout: 10000 });

  // Wait for the SaveChickenAction selector to show it's pre-selected (if opened from detail page)
  // The SaveChickenAction should already be selected and read-only
  await page.waitForTimeout(500);

  // Select the farm using the farm selector
  if (data.farmId) {
    await selectFarm(page, data.farmId);
    await page.waitForTimeout(1000);
  }

  // Fill in the quantities if provided
  if (data.numberOfChickensToBeSaved !== undefined) {
    await page.getByTestId('chickens-to-be-saved').click();
    await page.getByTestId('chickens-to-be-saved').clear();
    await page.getByTestId('chickens-to-be-saved').fill(data.numberOfChickensToBeSaved.toString());
  }

  if (data.numberOfRoostersToBeSaved !== undefined) {
    await page.getByTestId('roosters-to-be-saved').click();
    await page.getByTestId('roosters-to-be-saved').clear();
    await page.getByTestId('roosters-to-be-saved').fill(data.numberOfRoostersToBeSaved.toString());
  }

  // Wait for form to stabilize
  await page.waitForTimeout(500);
}

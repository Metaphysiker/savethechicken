import { Page } from '@playwright/test';
import { getMultiDateSelector } from './multi-date-selector.helper';
import { selectSaveChickenAction } from './save-chicken-action-selector.helper';

export interface AdminSaveChickenDriveRequestFormPreselectedData {
  // SaveChickenAction selection (required for date validation)
  saveChickenActionId: number;

  // Drive request details
  carMake: string;
  capacityForChickens: string;

  // Dates
  availableDates?: number[];

  // Additional info
  message?: string;
}

/**
 * Fill admin save chicken drive request form when person is pre-selected (from person detail page)
 */
export async function fillAdminSaveChickenDriveRequestFormPreselected(page: Page, data: AdminSaveChickenDriveRequestFormPreselectedData): Promise<void> {
  // Wait for form to be fully loaded
  await page.getByTestId('car-make').waitFor({ state: 'visible', timeout: 10000 });
  await page.waitForTimeout(500); // Additional wait for all components to load

  // Fill SaveChickenDriveRequest's own fields
  await page.getByTestId('car-make').fill(data.carMake);
  await page.getByTestId('capacity-for-chickens').fill(data.capacityForChickens);

  // Select SaveChickenAction (required before selecting dates)
  await selectSaveChickenAction(page, data.saveChickenActionId);

  // Select dates if provided
  if (data.availableDates && data.availableDates.length > 0) {
    // Wait for calendar to be visible before trying to select dates
    await page.locator('.mud-calendar').first().waitFor({ state: 'visible', timeout: 10000 });
    await page.waitForTimeout(500); // Additional wait for calendar to stabilize

    const dateSelector = getMultiDateSelector(page);
    if (data.availableDates.length === 1) {
      await dateSelector.selectDate(data.availableDates[0]);
    } else {
      await dateSelector.selectDates(data.availableDates);
    }
  }

  // Fill message if provided
  if (data.message) {
    await page.getByTestId('drive-request-message').fill(data.message);
  }
}

import { Page } from '@playwright/test';
import { getMultiDateSelector } from './multi-date-selector.helper';
import { selectPerson } from './person-selector.helper';
import { selectSaveChickenAction } from './save-chicken-action-selector.helper';

export interface AdminSaveChickenDriveRequestFormData {
  // Person selection (optional - not needed when person is pre-selected)
  personId?: number;
  personEmail?: string;

  // SaveChickenAction selection (required for date validation)
  saveChickenActionId?: number;

  // Drive request details (SaveChickenDriveRequest's own fields)
  carMake: string;
  capacityForChickens: string;

  // Dates
  availableDates?: number[];

  // Additional info
  message?: string;
}

/**
 * Fill admin save chicken drive request form - only SaveChickenDriveRequest fields, not Contact/Address
 */
export async function fillAdminSaveChickenDriveRequestForm(page: Page, data: AdminSaveChickenDriveRequestFormData): Promise<void> {
  // Select person using PersonSelector component (only if personEmail is provided)
  if (data.personEmail) {
    await selectPerson(page, data.personEmail);
  }

  // Fill SaveChickenDriveRequest's own fields only
  await page.getByTestId('car-make').fill(data.carMake);
  await page.getByTestId('capacity-for-chickens').fill(data.capacityForChickens);

  // Select SaveChickenAction if provided (required before selecting dates)
  if (data.saveChickenActionId) {
    await selectSaveChickenAction(page, data.saveChickenActionId);
  }

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

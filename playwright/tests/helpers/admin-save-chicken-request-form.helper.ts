import { Page } from '@playwright/test';
import { getMultiDateSelector } from './multi-date-selector.helper';
import { selectPerson } from './person-selector.helper';
import { selectSaveChickenAction } from './save-chicken-action-selector.helper';

export interface AdminSaveChickenRequestFormData {
  // Person selection (optional - not needed when person is pre-selected)
  personId?: number;
  personEmail?: string;

  // SaveChickenAction selection (required for date filtering)
  saveChickenActionId?: number;

  // Chicken details (SaveChickenRequest's own fields)
  numberOfChickens: string;
  numberOfRoosters: string;
  description: string;
  alreadyReceivedChickensBefore?: boolean;

  // Dates
  datesForHandOver?: number[];

  // Additional info
  message?: string;

  // Confirmations (default to true for valid submissions)
  confirmCriteria?: boolean;
  acceptTerms?: boolean;
}

/**
 * Fill admin save chicken request form - only SaveChickenRequest fields, not Contact/Address
 */
export async function fillAdminSaveChickenRequestForm(page: Page, data: AdminSaveChickenRequestFormData): Promise<void> {
  // Select person using PersonSelector component (only if personEmail is provided)
  if (data.personEmail) {
    await selectPerson(page, data.personEmail);
  }

  // Select SaveChickenAction if provided (required for dates to be available)
  if (data.saveChickenActionId) {
    await selectSaveChickenAction(page, data.saveChickenActionId);
  }

  // Fill SaveChickenRequest's own fields only
  await page.getByTestId('chickens-count').fill(data.numberOfChickens);
  await page.getByTestId('roosters-count').fill(data.numberOfRoosters);
  await page.getByTestId('description').fill(data.description);

  // Check "already received chickens before" if specified
  if (data.alreadyReceivedChickensBefore) {
    await page.getByTestId('received-chickens-before').check();
  }

  // Select dates if provided
  if (data.datesForHandOver && data.datesForHandOver.length > 0) {
    const dateSelector = getMultiDateSelector(page);
    if (data.datesForHandOver.length === 1) {
      await dateSelector.selectDate(data.datesForHandOver[0]);
    } else {
      await dateSelector.selectDates(data.datesForHandOver);
    }
  }

  // Fill message if provided
  if (data.message) {
    await page.getByTestId('message').fill(data.message);
  }

  // Check confirmations (default to true if not specified)
  const confirmCriteria = data.confirmCriteria !== false;
  const acceptTerms = data.acceptTerms !== false;

  if (confirmCriteria) {
    await page.getByTestId('confirm-criteria').check();
  }

  if (acceptTerms) {
    await page.getByTestId('accept-terms').check();
  }
}

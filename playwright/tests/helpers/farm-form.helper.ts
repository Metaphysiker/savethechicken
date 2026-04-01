import { Page } from '@playwright/test';
import { getMultiDateSelector } from './multi-date-selector.helper';
import { selectSaveChickenAction } from './save-chicken-action-selector.helper';

export interface FarmFormData {
  // Farm details
  name: string;
  numberOfChickens: string;
  numberOfRoosters: string;
  size: string;
  color: string;
  generalInformation: string;

  // Contact information
  contactFirstName: string;
  contactLastName: string;
  contactEmail: string;
  contactPhone: string;

  // Address
  addressCity: string;
  addressPostalCode: string;
  addressStreet: string;

  // SaveChickenAction (required for admin forms)
  saveChickenActionId?: number;

  // Dates (optional)
  datesForRescues?: number[];
}

export async function fillFarmForm(page: Page, data: FarmFormData): Promise<void> {
  // Wait for all form sections to be visible
  await page.waitForSelector('text=Allgemeine Informationen', { state: 'visible', timeout: 10000 });
  await page.waitForSelector('text=Kontakt', { state: 'visible', timeout: 10000 });
  await page.waitForSelector('text=Adresse', { state: 'visible', timeout: 10000 });

  // Fill contact information
  await page.getByTestId('contact-firstname').fill(data.contactFirstName);
  await page.getByTestId('contact-lastname').fill(data.contactLastName);
  await page.getByTestId('contact-email').fill(data.contactEmail);
  await page.getByTestId('contact-phone').fill(data.contactPhone);

  // Fill address
  await page.getByTestId('address-city').fill(data.addressCity);
  await page.getByTestId('address-postalcode').fill(data.addressPostalCode);
  await page.getByTestId('address-street').fill(data.addressStreet);

  // Fill farm name and details using data-testids
  await page.getByTestId('farm-name').fill(data.name);
  await page.getByTestId('number-of-chickens').fill(data.numberOfChickens);
  await page.getByTestId('number-of-roosters').fill(data.numberOfRoosters);
  await page.getByTestId('farm-size').fill(data.size);
  await page.getByTestId('farm-color').fill(data.color);
  await page.getByTestId('farm-general-info').fill(data.generalInformation);

  // Select SaveChickenAction if provided (required for dates to be selectable)
  if (data.saveChickenActionId) {
    await selectSaveChickenAction(page, data.saveChickenActionId);
    // Wait for action to load and calendar to update
    await page.waitForTimeout(1000);
  }

  // Select dates if provided
  if (data.datesForRescues && data.datesForRescues.length > 0) {
    // Wait a bit for any previous operations to complete
    await page.waitForTimeout(500);

    const dateSelector = getMultiDateSelector(page);

    // Select dates one by one with a small delay between each
    for (const date of data.datesForRescues) {
      await dateSelector.selectDate(date);
      await page.waitForTimeout(300);
    }
  }
}

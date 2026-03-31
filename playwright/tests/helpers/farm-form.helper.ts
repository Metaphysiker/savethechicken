import { Page } from '@playwright/test';
import { getMultiDateSelector } from './multi-date-selector.helper';

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

  // Fill farm name
  await page.getByLabel('Name des Betriebs').fill(data.name);

  // Fill farm-specific information
  await page.getByLabel('Anzahl Hühner').fill(data.numberOfChickens);
  await page.getByLabel('Anzahl Hähne').fill(data.numberOfRoosters);
  await page.getByLabel('Grösse').fill(data.size);
  await page.getByLabel('Farbe').fill(data.color);

  // Fill general information (multi-line field)
  await page.getByTestId('farm-general-info').fill(data.generalInformation);

  // Select dates if provided
  if (data.datesForRescues && data.datesForRescues.length > 0) {
    const dateSelector = getMultiDateSelector(page);
    if (data.datesForRescues.length === 1) {
      await dateSelector.selectDate(data.datesForRescues[0]);
    } else {
      await dateSelector.selectDates(data.datesForRescues);
    }
  }
}

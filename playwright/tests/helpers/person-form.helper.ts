import { Page } from '@playwright/test';

export interface PersonFormData {
  // Contact information
  firstName: string;
  lastName: string;
  email: string;
  phone: string;

  // Address
  city: string;
  postalCode: string;
  street: string;

  // Optional fields
  comment?: string;
  isBlacklisted?: boolean;
}

/**
 * Fill person form with contact and address information
 */
export async function fillPersonForm(page: Page, data: PersonFormData): Promise<void> {
  // Fill contact information
  await page.getByTestId('contact-firstname').fill(data.firstName);
  await page.getByTestId('contact-lastname').fill(data.lastName);
  await page.getByTestId('contact-email').fill(data.email);
  await page.getByTestId('contact-phone').fill(data.phone);

  // Fill address
  await page.getByTestId('address-city').fill(data.city);
  await page.getByTestId('address-postalcode').fill(data.postalCode);
  await page.getByTestId('address-street').fill(data.street);

  // Fill comment if provided
  if (data.comment !== undefined) {
    await page.getByTestId('person-comment').fill(data.comment);
  }

  // Set blacklist status if specified
  if (data.isBlacklisted !== undefined) {
    const checkbox = page.getByLabel('Blacklisted');
    const isChecked = await checkbox.isChecked();
    if (data.isBlacklisted && !isChecked) {
      await checkbox.check();
    } else if (!data.isBlacklisted && isChecked) {
      await checkbox.uncheck();
    }
  }
}

/**
 * Create a person and return the person ID from the URL
 */
export async function createPerson(page: Page, data: PersonFormData): Promise<number> {
  await page.goto('/admin/persons/new', { waitUntil: 'networkidle' });
  await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

  await fillPersonForm(page, data);

  await page.getByRole('button', { name: /create|erstellen/i }).click();

  // Wait for redirect to person detail page (GenericCreate redirects to detail page)
  await page.waitForURL(/\/admin\/persons\/\d+/, { timeout: 10000 });

  // Extract ID from URL
  const url = page.url();
  const match = url.match(/\/admin\/persons\/(\d+)/);

  if (!match) {
    throw new Error('Could not extract person ID from URL: ' + url);
  }

  return parseInt(match[1], 10);
}

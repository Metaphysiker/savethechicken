import { Page } from '@playwright/test';

export interface SaveChickenRequestFormData {
  // Contact information
  contactFirstName: string;
  contactLastName: string;
  contactEmail: string;
  contactPhone: string;

  // Address
  addressCity: string;
  addressPostalCode: string;
  addressStreet: string;

  // Chicken details
  numberOfChickens: string;
  numberOfRoosters: string;
  description: string;
  alreadyReceivedChickensBefore?: boolean;

  // Additional info
  message?: string;

  // Files to upload (optional)
  files?: Array<{ name: string; mimeType: string; buffer: Buffer }>;

  // Confirmations (default to true for valid submissions)
  confirmCriteria?: boolean;
  acceptTerms?: boolean;
}

export async function fillSaveChickenRequestForm(page: Page, data: SaveChickenRequestFormData): Promise<void> {
  // Fill contact information
  await page.getByTestId('contact-firstname').fill(data.contactFirstName);
  await page.getByTestId('contact-lastname').fill(data.contactLastName);
  await page.getByTestId('contact-email').fill(data.contactEmail);
  await page.getByTestId('contact-phone').fill(data.contactPhone);

  // Fill address
  await page.getByTestId('address-city').fill(data.addressCity);
  await page.getByTestId('address-postalcode').fill(data.addressPostalCode);
  await page.getByTestId('address-street').fill(data.addressStreet);

  // Fill chicken details
  await page.getByTestId('chickens-count').fill(data.numberOfChickens);
  await page.getByTestId('roosters-count').fill(data.numberOfRoosters);
  await page.getByTestId('description').fill(data.description);

  // Check "already received chickens before" if specified
  if (data.alreadyReceivedChickensBefore) {
    await page.getByTestId('received-chickens-before').check();
  }

  // Fill message if provided
  if (data.message) {
    await page.getByTestId('message').fill(data.message);
  }

  // Upload files if provided — use the filechooser event so Blazor's change handler fires
  if (data.files && data.files.length > 0) {
    const [fileChooser] = await Promise.all([
      page.waitForEvent('filechooser'),
      page.getByRole('button', { name: /bilder hochladen/i }).click(),
    ]);
    await fileChooser.setFiles(data.files);
    // All files are added in one Blazor render after FileChanged completes.
    // FileChanged can take up to 5s per non-HEIC file (resize timeout) so wait generously.
    for (const file of data.files) {
      await page.getByText(file.name).waitFor({ state: 'visible', timeout: 30000 });
    }
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

import { Page } from '@playwright/test';

/**
 * Select a person from PersonSelector component by email
 * @param page - Playwright page object
 * @param personEmail - Email of the person to select (used to identify in dropdown)
 */
export async function selectPerson(page: Page, personEmail: string): Promise<void> {
  // Click the PersonSelector to open dropdown
  await page.getByTestId('person-selector').click();
  await page.waitForTimeout(1000); // Wait for dropdown to open and render options

  // Get the popover content and find the list item by email text
  // Person options are displayed as "FirstName LastName (email@example.com)"
  const popover = page.locator('.mud-popover-open .mud-list');
  await popover.waitFor({ state: 'visible', timeout: 5000 });

  // Find the list item that contains the person's email
  const option = popover.locator('.mud-list-item').filter({ hasText: personEmail });
  await option.waitFor({ state: 'visible', timeout: 5000 });
  await option.click();
  await page.waitForTimeout(1000); // Wait for dropdown to close and value to be set
}

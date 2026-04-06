import { Page } from '@playwright/test';

/**
 * Select a person from PersonSelector component by email
 * @param page - Playwright page object
 * @param personEmail - Email of the person to select (used to search)
 */
export async function selectPerson(page: Page, personEmail: string): Promise<void> {
  // Click the PersonSelector autocomplete input to focus it
  const autocomplete = page.getByTestId('person-selector');
  await autocomplete.click();
  await page.waitForTimeout(300); // Wait for input to focus

  // Type the email to trigger search (MudAutocomplete requires min 2 characters)
  await autocomplete.fill(personEmail);
  await page.waitForTimeout(500); // Wait for debounce (300ms) + search results

  // Wait for the autocomplete dropdown to appear
  const popover = page.locator('.mud-popover-open .mud-list');
  await popover.waitFor({ state: 'visible', timeout: 5000 });

  // Find and click the list item that contains the person's email
  const option = popover.locator('.mud-list-item').filter({ hasText: personEmail });
  await option.waitFor({ state: 'visible', timeout: 5000 });
  await option.click();
  await page.waitForTimeout(500); // Wait for dropdown to close and value to be set
}

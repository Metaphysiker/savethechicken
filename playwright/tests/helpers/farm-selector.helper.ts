import { Page } from '@playwright/test';

/**
 * Select a Farm from the dropdown by ID
 */
export async function selectFarm(page: Page, farmId: number): Promise<void> {
  console.log(`Selecting Farm with ID: ${farmId}`);

  // Find the Farm selector using data-testid
  const farmSelector = page.getByTestId('farm-selector');
  const selectContainer = farmSelector.locator('..');

  // Wait for the container to be visible
  await selectContainer.waitFor({ state: 'visible', timeout: 10000 });

  // Wait for farms to load
  await page.waitForTimeout(2000);

  // Click to open the dropdown
  console.log('Opening Farm dropdown...');
  await selectContainer.click();

  // Wait for dropdown to open
  await page.waitForTimeout(1000);

  // Find the list item that matches our farm ID pattern (now displays as #5 | FarmName)
  const optionPattern = `#${farmId}`;
  console.log(`Looking for Farm option containing: ${optionPattern}`);

  // Wait for at least one matching option to appear
  const option = page.locator('.mud-list-item').filter({ hasText: optionPattern }).first();
  await option.waitFor({ state: 'visible', timeout: 10000 });

  // Get all options to debug
  const allOptions = page.locator('.mud-list-item');
  const optionCount = await allOptions.count();
  console.log(`Found ${optionCount} Farm options total`);

  // Click the option
  await option.click();

  // Wait for the selection to complete
  await page.waitForTimeout(1000);

  console.log(`Successfully selected Farm ${farmId}`);
}
import { Page } from '@playwright/test';

/**
 * Select a SaveChickenAction from the dropdown by ID
 */
export async function selectSaveChickenAction(page: Page, actionId: number): Promise<void> {
  console.log(`Selecting SaveChickenAction with ID: ${actionId}`);

  // Find the SaveChickenAction selector using data-testid
  // Click on the parent container div which is the clickable area for MudSelect
  const inputElement = page.getByTestId('save-chicken-action-selector');
  const selectContainer = inputElement.locator('..');

  // Wait for the container to be visible
  await selectContainer.waitFor({ state: 'visible', timeout: 10000 });

  // Wait for actions to load - check that the select is not disabled
  await page.waitForTimeout(2000);

  // Click to open the dropdown
  console.log('Opening SaveChickenAction dropdown...');
  await selectContainer.click();

  // Wait for dropdown to open
  await page.waitForTimeout(1000);

  // Find the list items that match our action ID pattern
  // MudSelect renders options as .mud-list-item inside a .mud-popover
  const optionPattern = `#${actionId}`;
  console.log(`Looking for option containing: ${optionPattern}`);

  // Wait for at least one matching option to appear with generous timeout
  const option = page.locator('.mud-list-item').filter({ hasText: optionPattern }).first();
  await option.waitFor({ state: 'visible', timeout: 10000 });

  // Get all options to debug (limit output)
  const allOptions = page.locator('.mud-list-item');
  const optionCount = await allOptions.count();
  console.log(`Found ${optionCount} SaveChickenAction options total`);

  // Click the option
  await option.click();

  // Wait for the selection to complete
  await page.waitForTimeout(1000);

  console.log(`Successfully selected SaveChickenAction ${actionId}`);
}

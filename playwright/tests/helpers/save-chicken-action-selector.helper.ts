import { Page } from '@playwright/test';

/**
 * Select a SaveChickenAction from the dropdown by ID
 */
export async function selectSaveChickenAction(page: Page, actionId: number): Promise<void> {
  console.log(`Selecting SaveChickenAction with ID: ${actionId}`);

  // Find the SaveChickenAction selector using data-testid
  // The data-testid is on the hidden input, but we need to click its visible sibling div with tabindex
  const hiddenInput = page.getByTestId('save-chicken-action-selector');
  const selector = hiddenInput.locator('..').locator('div[tabindex="0"]').first();

  // Wait for the selector to be visible
  await selector.waitFor({ state: 'visible', timeout: 10000 });

  // Wait a bit for actions to load
  await page.waitForTimeout(1000);

  // Click to open the dropdown
  console.log('Opening SaveChickenAction dropdown...');
  await selector.click();

  // Wait for the popover to appear (MudSelect renders options in a popover)
  await page.waitForSelector('.mud-popover', { state: 'visible', timeout: 5000 });
  await page.waitForTimeout(500);

  // Get all options to debug (MudSelect uses .mud-list-item for options)
  const options = await page.locator('.mud-popover .mud-list-item').allTextContents();
  console.log('Available SaveChickenAction options:', options);

  // Select the option that contains the action ID
  // Format is: "#ID | Title | Dates"
  const optionPattern = `#${actionId}`;
  console.log(`Looking for option containing: ${optionPattern}`);

  const option = page.locator('.mud-popover .mud-list-item').filter({ hasText: optionPattern }).first();
  await option.waitFor({ state: 'visible', timeout: 5000 });
  await option.click();

  // Wait for the popover to close (confirms selection)
  await page.waitForSelector('.mud-popover', { state: 'hidden', timeout: 5000 });

  // Wait for the calendar to update
  await page.waitForTimeout(1000);

  console.log(`Successfully selected SaveChickenAction ${actionId}`);
}

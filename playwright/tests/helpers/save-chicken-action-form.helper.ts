import { Page } from '@playwright/test';
import { getMultiDateSelector } from './multi-date-selector.helper';

export interface SaveChickenActionFormData {
  title: string;
  description: string;
  isActive?: boolean;
  dates: number[]; // Day numbers to select (e.g., [27, 28, 29, 30])
}

export interface CreatedSaveChickenAction {
  actionId: number;
  dates: number[]; // The dates that were selected
}

/**
 * Create a SaveChickenAction and return the action ID and dates
 */
export async function createSaveChickenAction(
  page: Page,
  data: SaveChickenActionFormData
): Promise<CreatedSaveChickenAction> {
  await page.goto('/admin/save-chicken-actions/new', { waitUntil: 'networkidle' });
  await page.waitForSelector('h3', { state: 'visible', timeout: 10000 });

  // Fill in Title (supports both German "Titel" and English "Title")
  await page.getByLabel(/titel|title/i).fill(data.title);

  // Fill in Description (supports both German "Beschreibung" and English "Description")
  await page.getByLabel(/beschreibung|description/i).fill(data.description);


  // Select dates using MultiDateSelector
  const dateSelector = getMultiDateSelector(page);
  await dateSelector.selectDates(data.dates);

  // Submit the form (supports both German "Erstellen" and English "Create")
  await page.getByRole('button', { name: /erstellen|create/i }).click();

  // Wait for redirect to detail page
  await page.waitForURL(/\/admin\/save-chicken-actions\/\d+/, { timeout: 10000 });

  // Extract action ID from URL
  const url = page.url();
  const match = url.match(/\/admin\/save-chicken-actions\/(\d+)/);

  if (!match) {
    throw new Error('Could not extract action ID from URL: ' + url);
  }

  return {
    actionId: parseInt(match[1], 10),
    dates: data.dates,
  };
}

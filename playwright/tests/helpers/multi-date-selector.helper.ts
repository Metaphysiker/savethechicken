import { Page, Locator, expect } from '@playwright/test';

/**
 * Helper class for interacting with MultiDateOnlySelector component in Playwright tests
 */
export class MultiDateSelectorHelper {
  private page: Page;
  private container: Locator;

  constructor(page: Page, containerSelector?: string) {
    this.page = page;
    // If no container selector provided, use the first calendar on the page
    this.container = containerSelector
      ? page.locator(containerSelector)
      : page.locator('.mud-calendar').first();
  }

  /**
   * Select a specific date by day number in the current month
   * @param day - The day of the month (1-31)
   */
  async selectDate(day: number) {
    const dayCell = this.container.locator(`.calendar-day:has-text("${day}")`).first();
    await dayCell.waitFor({ state: 'visible' });
    await dayCell.click();
    await this.page.waitForTimeout(200); // Wait for selection to register
  }

  /**
   * Select multiple dates by day numbers in the current month
   * @param days - Array of day numbers to select
   */
  async selectDates(days: number[]) {
    for (const day of days) {
      await this.selectDate(day);
    }
  }

  /**
   * Deselect a date (clicks it again if already selected)
   * @param day - The day of the month to deselect
   */
  async deselectDate(day: number) {
    const dayCell = this.container.locator(`.calendar-day.selected:has-text("${day}")`).first();
    const isSelected = await dayCell.count() > 0;

    if (isSelected) {
      await dayCell.click();
      await this.page.waitForTimeout(200);
    }
  }

  /**
   * Navigate to the next month
   */
  async nextMonth() {
    await this.container.locator('button:has(svg)').nth(1).click(); // Right chevron
    await this.page.waitForTimeout(200);
  }

  /**
   * Navigate to the previous month
   */
  async prevMonth() {
    await this.container.locator('button:has(svg)').nth(0).click(); // Left chevron
    await this.page.waitForTimeout(200);
  }

  /**
   * Navigate to a specific month and year
   * @param month - Month number (1-12)
   * @param year - Year (e.g., 2026)
   */
  async navigateToMonth(month: number, year: number) {
    // Get current displayed month
    const headerText = await this.container.locator('.calendar-header span').textContent();

    // Navigate month by month (simple but reliable)
    // TODO: Could be optimized by calculating exact number of clicks needed
    let maxIterations = 24; // Prevent infinite loop
    while (maxIterations-- > 0) {
      const currentHeader = await this.container.locator('.calendar-header span').textContent();

      // Check if we're at the target month (format: "März 2026")
      const targetMonthName = new Date(year, month - 1).toLocaleDateString('de-DE', { month: 'long' });
      const targetHeader = `${targetMonthName} ${year}`;

      if (currentHeader?.toLowerCase().includes(targetMonthName.toLowerCase()) &&
          currentHeader?.includes(year.toString())) {
        return; // We're at the target month
      }

      // Determine if we need to go forward or backward
      const currentDate = this.parseHeaderToDate(currentHeader || '');
      const targetDate = new Date(year, month - 1);

      if (targetDate > currentDate) {
        await this.nextMonth();
      } else {
        await this.prevMonth();
      }
    }

    throw new Error(`Could not navigate to ${month}/${year} after 24 iterations`);
  }

  /**
   * Parse calendar header text to a Date object
   */
  private parseHeaderToDate(header: string): Date {
    // Header format: "März 2026" or "March 2026"
    const parts = header.trim().split(' ');
    if (parts.length !== 2) return new Date();

    const monthName = parts[0];
    const year = parseInt(parts[1]);

    // Try to parse month name
    const monthIndex = new Date(`${monthName} 1, ${year}`).getMonth();

    return new Date(year, monthIndex);
  }

  /**
   * Verify that specific dates are selected
   * @param expectedDates - Array of date strings in DD.MM.YYYY format
   */
  async verifySelectedDates(expectedDates: string[]) {
    // Wait for the selected dates list to update
    await this.page.waitForTimeout(300);

    if (expectedDates.length === 0) {
      // Check for "Keine" (none) text
      await expect(this.page.locator('text=Keine').first()).toBeVisible();
      return;
    }

    // Check each expected date appears in the list
    for (const date of expectedDates) {
      const dateItem = this.page.locator(`li:has-text("${date}")`);
      await expect(dateItem).toBeVisible();
    }

    // Verify the count matches
    const listItems = this.page.locator('ul li');
    const count = await listItems.count();
    expect(count).toBe(expectedDates.length);
  }

  /**
   * Get the current month/year displayed in the calendar
   */
  async getCurrentMonth(): Promise<string> {
    return await this.container.locator('.calendar-header span').textContent() || '';
  }

  /**
   * Check if a specific day is selected
   * @param day - Day number to check
   */
  async isDateSelected(day: number): Promise<boolean> {
    const selectedDay = this.container.locator(`.calendar-day.selected:has-text("${day}")`).first();
    return await selectedDay.count() > 0;
  }

  /**
   * Check if a specific day is disabled
   * @param day - Day number to check
   */
  async isDateDisabled(day: number): Promise<boolean> {
    const disabledDay = this.container.locator(`.calendar-day.disabled:has-text("${day}")`).first();
    return await disabledDay.count() > 0;
  }

  /**
   * Get all selected dates as formatted strings
   * @returns Array of selected dates in DD.MM.YYYY format
   */
  async getSelectedDates(): Promise<string[]> {
    const listItems = this.page.locator('ul li');
    const count = await listItems.count();

    if (count === 0) {
      return [];
    }

    const dates: string[] = [];
    for (let i = 0; i < count; i++) {
      const text = await listItems.nth(i).textContent();
      if (text) {
        dates.push(text.trim());
      }
    }

    return dates;
  }

  /**
   * Clear all selected dates
   */
  async clearAllDates() {
    const selectedDays = this.container.locator('.calendar-day.selected');
    const count = await selectedDays.count();

    for (let i = count - 1; i >= 0; i--) {
      await selectedDays.nth(i).click();
      await this.page.waitForTimeout(100);
    }
  }
}

/**
 * Convenience function to create a MultiDateSelectorHelper
 */
export function getMultiDateSelector(page: Page, containerSelector?: string): MultiDateSelectorHelper {
  return new MultiDateSelectorHelper(page, containerSelector);
}

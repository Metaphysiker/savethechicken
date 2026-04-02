import { test, expect } from '@playwright/test';
import { getMultiDateSelector } from './helpers/multi-date-selector.helper';

// These tests are obsolete after removing DatesForHandOver from SaveChickenRequest
test.describe.skip('Multi Date Selector Example', () => {
  test('should select and verify multiple dates', async ({ page }) => {
    // Navigate to a page with MultiDateOnlySelector component
    // Replace with your actual page path
    await page.goto('/save-chicken-requests/new');

    // Wait for the calendar to load
    await page.waitForSelector('.mud-calendar', { state: 'visible' });

    // Create helper instance
    const dateSelector = getMultiDateSelector(page);

    // Select multiple dates in the current month
    await dateSelector.selectDates([15, 20, 25]);

    // Verify the dates are selected
    const today = new Date();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const year = today.getFullYear();

    await dateSelector.verifySelectedDates([
      `15.${month}.${year}`,
      `20.${month}.${year}`,
      `25.${month}.${year}`
    ]);

    // Navigate to next month and select a date
    await dateSelector.nextMonth();
    await dateSelector.selectDate(10);

    // Navigate back to previous month
    await dateSelector.prevMonth();

    // Deselect one date
    await dateSelector.deselectDate(20);

    // Verify remaining selected dates
    const selectedDates = await dateSelector.getSelectedDates();
    expect(selectedDates.length).toBe(3); // 15, 25, and 10 from next month

    // Clear all dates
    await dateSelector.clearAllDates();

    // Verify no dates selected
    await dateSelector.verifySelectedDates([]);
  });

  test('should navigate to specific month and select dates', async ({ page }) => {
    await page.goto('/save-chicken-requests/new');
    await page.waitForSelector('.mud-calendar', { state: 'visible' });

    const dateSelector = getMultiDateSelector(page);

    // Navigate to a specific month (e.g., May 2026)
    await dateSelector.navigateToMonth(5, 2026);

    // Verify we're at the correct month
    const currentMonth = await dateSelector.getCurrentMonth();
    expect(currentMonth.toLowerCase()).toContain('mai'); // German for May
    expect(currentMonth).toContain('2026');

    // Select dates in May
    await dateSelector.selectDates([1, 15, 30]);

    // Verify dates are selected
    await dateSelector.verifySelectedDates([
      '01.05.2026',
      '15.05.2026',
      '30.05.2026'
    ]);
  });

  test('should check if dates are disabled', async ({ page }) => {
    await page.goto('/save-chicken-requests/new');
    await page.waitForSelector('.mud-calendar', { state: 'visible' });

    const dateSelector = getMultiDateSelector(page);

    // Check if specific dates are disabled (depends on your MinDate/MaxDate settings)
    const isDisabled = await dateSelector.isDateDisabled(1);
    console.log('Is day 1 disabled?', isDisabled);

    // Check if a date is selected
    await dateSelector.selectDate(10);
    const isSelected = await dateSelector.isDateSelected(10);
    expect(isSelected).toBe(true);
  });
});

/**
 * USAGE EXAMPLES:
 *
 * // Basic usage
 * const dateSelector = getMultiDateSelector(page);
 * await dateSelector.selectDates([5, 10, 15]);
 *
 * // With specific container (if multiple calendars on page)
 * const dateSelector = getMultiDateSelector(page, '[data-testid="availability-calendar"]');
 *
 * // Navigate months
 * await dateSelector.navigateToMonth(6, 2026);
 * await dateSelector.nextMonth();
 * await dateSelector.prevMonth();
 *
 * // Select and verify
 * await dateSelector.selectDate(20);
 * expect(await dateSelector.isDateSelected(20)).toBe(true);
 *
 * // Get all selected dates
 * const dates = await dateSelector.getSelectedDates();
 * console.log('Selected dates:', dates);
 *
 * // Clear selection
 * await dateSelector.clearAllDates();
 */

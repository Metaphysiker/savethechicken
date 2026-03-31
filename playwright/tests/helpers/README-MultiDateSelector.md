# MultiDateSelector Playwright Helper

Helper class for interacting with the custom `MultiDateOnlySelector` component in Playwright tests.

## Installation

The helper is located at `playwright/tests/helpers/multi-date-selector.helper.ts`

## Usage

### Basic Usage

```typescript
import { getMultiDateSelector } from './helpers/multi-date-selector.helper';

test('select dates', async ({ page }) => {
  await page.goto('/save-chicken-requests/new');

  // Create helper instance
  const dateSelector = getMultiDateSelector(page);

  // Select multiple dates in the current month
  await dateSelector.selectDates([15, 20, 25]);

  // Verify the dates are selected
  await dateSelector.verifySelectedDates([
    '15.03.2026',
    '20.03.2026',
    '25.03.2026'
  ]);
});
```

### Working with Multiple Calendars

If your page has multiple `MultiDateOnlySelector` components, specify a container selector:

```typescript
// Select the calendar inside a specific container
const dateSelector = getMultiDateSelector(page, '[data-testid="availability-calendar"]');
```

## API Reference

### Constructor

```typescript
new MultiDateSelectorHelper(page: Page, containerSelector?: string)
```

- `page`: Playwright Page object
- `containerSelector`: Optional CSS selector to target a specific calendar (uses first `.mud-calendar` if omitted)

### Methods

#### `selectDate(day: number)`
Select a single date by day number in the currently displayed month.

```typescript
await dateSelector.selectDate(15);
```

#### `selectDates(days: number[])`
Select multiple dates in the currently displayed month.

```typescript
await dateSelector.selectDates([10, 15, 20]);
```

#### `deselectDate(day: number)`
Deselect a date (clicks it again if already selected).

```typescript
await dateSelector.deselectDate(15);
```

#### `nextMonth()`
Navigate to the next month.

```typescript
await dateSelector.nextMonth();
```

#### `prevMonth()`
Navigate to the previous month.

```typescript
await dateSelector.prevMonth();
```

#### `navigateToMonth(month: number, year: number)`
Navigate to a specific month and year.

```typescript
// Navigate to May 2026
await dateSelector.navigateToMonth(5, 2026);
```

#### `verifySelectedDates(expectedDates: string[])`
Verify that specific dates are selected. Dates should be in DD.MM.YYYY format.

```typescript
await dateSelector.verifySelectedDates([
  '01.05.2026',
  '15.05.2026',
  '30.05.2026'
]);
```

For no selection:
```typescript
await dateSelector.verifySelectedDates([]);
```

#### `getCurrentMonth()`
Get the current month/year displayed in the calendar.

```typescript
const currentMonth = await dateSelector.getCurrentMonth();
console.log(currentMonth); // "März 2026"
```

#### `isDateSelected(day: number)`
Check if a specific day is selected.

```typescript
const isSelected = await dateSelector.isDateSelected(15);
expect(isSelected).toBe(true);
```

#### `isDateDisabled(day: number)`
Check if a specific day is disabled (outside allowed range).

```typescript
const isDisabled = await dateSelector.isDateDisabled(5);
```

#### `getSelectedDates()`
Get all selected dates as an array of formatted strings.

```typescript
const dates = await dateSelector.getSelectedDates();
console.log(dates); // ['15.03.2026', '20.03.2026']
```

#### `clearAllDates()`
Clear all selected dates by clicking them.

```typescript
await dateSelector.clearAllDates();
```

## Complete Examples

### Example 1: E2E Test with Date Selection

```typescript
import { test, expect } from '@playwright/test';
import { getMultiDateSelector } from './helpers/multi-date-selector.helper';

test('user creates save chicken request with availability dates', async ({ page }) => {
  // Navigate to form
  await page.goto('/save-chicken-requests/new');

  // Fill other form fields...
  await page.getByLabel('Vorname').fill('Test');
  await page.getByLabel('Nachname').fill('User');

  // Select available dates
  const dateSelector = getMultiDateSelector(page);
  await dateSelector.selectDates([15, 20, 25]);

  // Verify dates were selected
  const today = new Date();
  const month = String(today.getMonth() + 1).padStart(2, '0');
  const year = today.getFullYear();

  await dateSelector.verifySelectedDates([
    `15.${month}.${year}`,
    `20.${month}.${year}`,
    `25.${month}.${year}`
  ]);

  // Submit form
  await page.getByRole('button', { name: 'Erstellen' }).click();
});
```

### Example 2: Admin Creating Action with Future Dates

```typescript
test('admin creates save chicken action for next month', async ({ page }) => {
  await page.goto('/save-chicken-actions/new');

  // Fill basic info
  await page.getByTestId('action-title').locator('input').fill('Spring 2026 Rescue');
  await page.getByTestId('action-description').locator('textarea').fill('Large rescue operation');

  // Navigate to next month and select dates
  const dateSelector = getMultiDateSelector(page);
  await dateSelector.nextMonth();
  await dateSelector.selectDates([1, 8, 15, 22]);

  // Verify 4 dates are selected
  const selectedDates = await dateSelector.getSelectedDates();
  expect(selectedDates.length).toBe(4);

  // Submit
  await page.getByTestId('submit-button').click();
});
```

### Example 3: Modifying Existing Dates

```typescript
test('admin updates farm rescue dates', async ({ page }) => {
  await page.goto('/farms/123/edit');

  const dateSelector = getMultiDateSelector(page);

  // Clear existing dates
  await dateSelector.clearAllDates();

  // Navigate to specific month
  await dateSelector.navigateToMonth(6, 2026); // June 2026

  // Select new dates
  await dateSelector.selectDates([5, 12, 19, 26]);

  // Verify current month
  const currentMonth = await dateSelector.getCurrentMonth();
  expect(currentMonth).toContain('Juni');
  expect(currentMonth).toContain('2026');

  // Save changes
  await page.getByRole('button', { name: 'Speichern' }).click();
});
```

## Tips

1. **Always wait for calendar visibility** before using the helper:
   ```typescript
   await page.waitForSelector('.mud-calendar', { state: 'visible' });
   ```

2. **Use safe day numbers** (1-28) to avoid month boundary issues:
   ```typescript
   const safeDates = [15, 20, 25]; // Always exist in any month
   ```

3. **Verify selection after actions** to catch timing issues:
   ```typescript
   await dateSelector.selectDate(15);
   expect(await dateSelector.isDateSelected(15)).toBe(true);
   ```

4. **Handle disabled dates** gracefully:
   ```typescript
   if (!(await dateSelector.isDateDisabled(5))) {
     await dateSelector.selectDate(5);
   }
   ```

## Related Components

This helper works with:
- `SaveChickenRequestFormWithFiles.razor` - "Ich bin an diesen Tagen erreichbar"
- `AdminSaveChickenRequestFormWithFiles.razor` - "Ich bin an diesen Tagen erreichbar"
- `SaveChickenActionForm.razor` - "Übergabe an diesen Daten"
- `SaveChickenDriveRequestFormWithFiles.razor` - "Ich kann an diesen Tagen fahren"
- `FarmFormWithFiles.razor` - "Hühner können an diesen Tagen abgeholt werden"

## Troubleshooting

### Calendar won't load
Ensure Blazor has initialized:
```typescript
await page.waitForTimeout(500); // Wait for Blazor
await page.waitForSelector('.mud-calendar');
```

### Dates not selecting
Check if dates are disabled:
```typescript
const isDisabled = await dateSelector.isDateDisabled(15);
if (!isDisabled) {
  await dateSelector.selectDate(15);
}
```

### Multiple calendars on page
Specify a container:
```typescript
const dateSelector = getMultiDateSelector(page, '.my-calendar-container');
```

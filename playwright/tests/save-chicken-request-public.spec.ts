import { test, expect } from '@playwright/test';

test.describe('Save Chicken Request - Public Form', () => {
  test('should display the public request form', async ({ page }) => {
    await page.goto('/save-chicken-requests/new');

    // Verify form title exists
    await expect(page.locator('h3, h4, h6').filter({ hasText: /person information/i })).toBeVisible();

    // Verify contact fields are present
    await expect(page.locator('input').filter({ hasText: /vorname|firstname/i }).first()).toBeVisible();
    await expect(page.locator('input').filter({ hasText: /nachname|lastname/i }).first()).toBeVisible();
  });

  test('should require mandatory fields', async ({ page }) => {
    await page.goto('/save-chicken-requests/new');

    // Try to submit empty form
    await page.click('button[type="submit"]');

    // Should show validation errors
    await expect(page.locator('text=/required|erforderlich/i').first()).toBeVisible();
  });

  test('should create a new save chicken request', async ({ page }) => {
    await page.goto('/save-chicken-requests/new');

    // Fill contact information
    await page.fill('input[id*="Contact.FirstName"]', 'Test');
    await page.fill('input[id*="Contact.LastName"]', 'User');
    await page.fill('input[id*="Contact.Email"]', 'test@example.com');
    await page.fill('input[id*="Contact.PhoneNumber"]', '+41791234567');

    // Fill address information
    await page.fill('input[id*="Address.Street"]', 'Test Street 123');
    await page.fill('input[id*="Address.PostalCode"]', '8000');
    await page.fill('input[id*="Address.City"]', 'Zurich');

    // Fill chicken request details
    await page.fill('input[id*="NumberOfChickensToBeSaved"]', '5');
    await page.fill('textarea[id*="DescriptionOfPlaceForChickens"]', 'Large garden with chicken coop');

    // Accept terms
    await page.check('input[id*="ConfirmThatIFulfillCriteria"]');
    await page.check('input[id*="AcceptTermsAndConditions"]');

    // Submit form
    await page.click('button[type="submit"]');

    // Should redirect or show success message
    await expect(page.locator('text=/success|erfolgreich/i')).toBeVisible({ timeout: 10000 });
  });
});

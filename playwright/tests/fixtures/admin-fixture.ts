import { test as base, expect } from '@playwright/test';

type AdminUser = {
  email: string;
  password: string;
};

export const test = base.extend<{ adminUser: AdminUser }>({
  adminUser: async ({ page }, use) => {
    const admin: AdminUser = {
      email: 's.raess@me.com',
      password: 'password',
    };

    // Login before each test
    await page.goto('/login');
    await page.fill('input[type="email"]', admin.email);
    await page.fill('input[type="password"]', admin.password);
    await page.click('button[type="submit"]');
    await page.waitForURL('/');

    await use(admin);

    // Logout after each test
    await page.click('text=Logout');
  },
});

export { expect };

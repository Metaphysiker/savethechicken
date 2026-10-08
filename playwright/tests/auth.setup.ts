import { test as setup, expect } from '@playwright/test';

const authFile = 'playwright/.auth/admin.json';

setup('authenticate as admin', async ({ page, baseURL, request }) => {
  setup.setTimeout(120000); // 120 second timeout for entire setup test
  
  console.log('=== Starting Admin Authentication ===');
  console.log('Base URL:', baseURL);
  
  // Derive API URL from base URL (switch from web port 8080 to API port 8081)
  const apiUrl = baseURL.replace('8080', '8081');
  console.log('API URL:', apiUrl);
  
  // Pre-flight check: Verify API is reachable
  console.log('Checking API health...');
  try {
    const healthResponse = await request.get(`${apiUrl}/health`, { timeout: 30000 });
    if (!healthResponse.ok()) {
      throw new Error(`API health check failed with status: ${healthResponse.status()}`);
    }
    console.log('✓ API health check passed');
    
    // Check database health
    const dbHealthResponse = await request.get(`${apiUrl}/health/db`, { timeout: 30000 });
    if (dbHealthResponse.ok()) {
      console.log('✓ Database health check passed');
    } else {
      console.log('⚠ Database health check failed, but API is running');
    }
    
    // Verify test user exists, call setup API if needed
    console.log('Verifying test user exists...');
    const loginResponse = await request.post(`${apiUrl}/api/auth/login`, {
      data: {
        email: 'test@example.com',
        password: 'testpassword'
      },
      timeout: 30000
    });
    
    if (loginResponse.ok()) {
      console.log('✓ Test user exists and can authenticate via API');
    } else {
      console.log('⚠ Test user does not exist, calling setup API to create it...');
      
      // Call the setup API to create the test user and seed data
      const setupResponse = await request.get(`${apiUrl}/api/Setup/setup`, { timeout: 60000 });
      if (!setupResponse.ok()) {
        const responseText = await setupResponse.text();
        throw new Error(`Setup API failed: ${setupResponse.status()} ${responseText}`);
      }
      console.log('✓ Setup API called successfully, test user should now exist');
      
      // Verify the user was created
      const verifyResponse = await request.post(`${apiUrl}/api/auth/login`, {
        data: {
          email: 'test@example.com',
          password: 'testpassword'
        },
        timeout: 30000
      });
      
      if (!verifyResponse.ok()) {
        const responseText = await verifyResponse.text();
        throw new Error(`Test user still cannot authenticate after setup: ${responseText}`);
      }
      console.log('✓ Test user verified after setup');
    }
    
  } catch (healthError) {
    console.error('✗ API health check failed:', healthError);
    throw new Error(`API is not reachable. Please ensure Docker containers are running. Error: ${healthError}`);
  }
  
  // Navigate to login page
  await page.goto('/login', { waitUntil: 'networkidle' });
  console.log('✓ Navigated to login page, URL:', page.url());
  
  // Fill credentials
  await page.getByRole('textbox', { name: /email/i }).first().fill('test@example.com');
  await page.getByRole('textbox', { name: /password/i }).first().fill('testpassword');
  console.log('✓ Filled credentials');

  // Click login button
  await page.getByRole('button', { name: /login/i }).first().click();
  console.log('✓ Clicked login button');
  
  // Wait for login to complete
  try {
    await page.waitForURL('/admin/persons', { timeout: 60000 });
    console.log('✓ SUCCESS: Redirected to /admin/persons');
  } catch (error) {
    const currentUrl = page.url();
    console.log('⚠ Login redirect failed. Current URL:', currentUrl);
    
    // Check for error messages if still on login page
    if (currentUrl.includes('/login')) {
      const errorMessages = await page.locator('.mud-alert').allTextContents().catch(() => []);
      if (errorMessages.length > 0) {
        console.log('✗ Login error messages:', errorMessages);
        throw new Error(`Login failed: ${errorMessages.join(' | ')}`);
      }
      
      // No visible errors - this might be a network/API connectivity issue
      console.log('✗ No visible error messages. Possible network or API connectivity issue.');
      throw new Error(`Login failed with no visible error. Current URL: ${currentUrl}, API: ${apiUrl}`);
    }
    
    // Check if we landed somewhere else - verify login success via logout button
    try {
      await page.getByText('Logout', { timeout: 10000 }).waitFor();
      console.log('✓ SUCCESS: Found Logout button, login likely succeeded');
    } catch (fallbackError) {
      console.log('✗ Login verification failed. Unexpected URL:', currentUrl);
      throw new Error(`Login verification failed. URL: ${currentUrl}, No logout button found.`);
    }
  }

  // Final verification
  await expect(page.getByText('Logout')).toBeVisible({ timeout: 10000 });
  console.log('✓ Login verified - Logout button is visible');

  // Save authentication state
  await page.context().storageState({ path: authFile });
  console.log('✓ Authentication state saved to', authFile);
  console.log('=== Admin Authentication Complete ===');
});
# End-to-End Testing with Playwright

This folder contains Playwright end-to-end tests for the SaveTheChicken application.

## Prerequisites

- Node.js 18+ and npm
- Docker and Docker Compose (for running test environment)

## Setup

### 1. Install Dependencies

```bash
npm install
npx playwright install chromium  # Install browser
```

### 2. Start Test Environment

```bash
# From root/infrastructure/testing folder
cd ../infrastructure/testing
docker-compose up -d

# Wait for services to be healthy (check with)
docker-compose ps
```

## Running Tests

### All Tests

```bash
npm test
```

### Headed Mode (visible browser)

```bash
npm run test:headed
```

### Interactive UI Mode

```bash
npm run test:ui
```

### Debug Mode

```bash
npm run test:debug
```

### Specific Test File

```bash
npx playwright test tests/admin-persons.spec.ts
```

### Generate Tests with Codegen

```bash
npm run test:codegen
```

## Project Structure

```
playwright/
├── tests/                          # Test files
│   ├── example.spec.ts            # Basic homepage tests
│   ├── save-chicken-request-public.spec.ts  # Public form tests
│   ├── admin-persons.spec.ts      # Admin person management tests
│   ├── auth.setup.ts              # Authentication setup
│   └── fixtures/                  # Test fixtures
│       └── admin-fixture.ts       # Admin user fixture
├── playwright.config.ts           # Playwright configuration
├── package.json                   # Dependencies
└── .gitignore                     # Git ignore rules
```

## Test Files

- **example.spec.ts** - Basic navigation and homepage tests
- **save-chicken-request-public.spec.ts** - Public-facing form submission tests
- **admin-persons.spec.ts** - Admin person management CRUD tests
- **auth.setup.ts** - Authentication setup (runs before admin tests)

## Configuration

### Base URL

Default: `http://localhost:8080`

Override with environment variable:

```bash
BASE_URL=http://localhost:9000 npm test
```

### Browsers

Configured browsers:
- Chromium (default)
- Firefox
- WebKit (Safari)

Run specific browser:

```bash
npx playwright test --project=firefox
```

## Viewing Test Reports

After running tests:

```bash
npm run test:report
```

## CI/CD

For CI environments:

```bash
CI=true npm test
```

This enables:
- 2 retries on failure
- Single worker (no parallelization)
- JUnit XML output

## Writing New Tests

1. Create a new `.spec.ts` file in `tests/` folder
2. Import test and expect from Playwright
3. Use `test.describe()` to group related tests
4. Use `test()` for individual test cases

Example:

```typescript
import { test, expect } from '@playwright/test';

test.describe('Feature Name', () => {
  test('should do something', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('h1')).toBeVisible();
  });
});
```

## Tips

- Use `page.pause()` to pause test execution and inspect
- Use `--debug` flag for step-by-step debugging
- Use `--headed` to see the browser
- Screenshots and videos are captured on test failures

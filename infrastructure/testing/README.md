# SaveTheChicken - Testing Infrastructure

This folder contains Docker Compose configuration for running end-to-end tests.

## Quick Start

### 1. Start Test Environment

```powershell
# From the infrastructure/testing folder
docker-compose up -d

# Wait for services to be healthy
docker-compose ps
```

### 2. Run Tests

```powershell
# From the playwright folder
cd ../../playwright
npm install
npx playwright install chromium
npm test
```

### 3. Stop Test Environment

```powershell
# From the infrastructure/testing folder
docker-compose down
```

## Services

- **postgres-test** - PostgreSQL test database on port 5433
- **webapi-test** - ASP.NET Core Web API on port 8081
- **web-test** - Blazor Web App on port 8080

## Running Tests

### From Host Machine (Recommended)

```bash
cd playwright
npm test                      # Run all tests
npm run test:headed          # Run with browser visible
npm run test:ui              # Interactive UI mode
npm run test:debug           # Debug mode
npm run test:report          # Show test report
```

### With Different Browsers

```bash
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit
```

### Run Specific Tests

```bash
npx playwright test tests/admin-persons.spec.ts
npx playwright test tests/save-chicken-request-public.spec.ts
```

## Environment Variables

You can override the base URL:

```bash
BASE_URL=http://localhost:8080 npm test
```

## Cleanup Test Data

To reset the test database:

```bash
docker-compose down -v
docker-compose up -d
```

## CI/CD Integration

Set `CI=true` environment variable to enable CI-optimized settings:
- Retries on failure
- Single worker
- JUnit reporter for test results

```bash
CI=true npm test
```

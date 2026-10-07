# Test Automation Scripts

This directory contains scripts to automate the test environment setup and teardown for SaveTheChicken Playwright tests.

## Problem

When running tests, data from previous test runs persists in the Docker containers. This requires manually:
1. Running `docker compose down -v` to remove containers and volumes
2. Running `docker compose up -d` to start fresh containers
3. Calling the `/api/Setup/setup` endpoint to create the admin user

## Solution

We provide multiple ways to automate this:

### Option 1: Use the Full Test Runner Script (Recommended)

The `test-runner.sh` script automates the entire workflow:

```bash
# Run full test suite with automatic setup and cleanup
npm run test:full

# Run tests without cleanup on exit (for debugging)
npm run test:full:no-cleanup

# Run tests skipping Docker setup (use existing containers)
npm run test:full:skip-setup
```

The script:
1. Runs `docker compose down -v` to clean up
2. Runs `docker compose up -d` to start containers
3. Waits for database and API to be ready
4. Calls the setup API to initialize admin user
5. Runs Playwright tests
6. Runs `docker compose down -v` on exit (unless --no-cleanup)

### Option 2: Manual Docker Control with Automated Test Setup

If you want to manually control Docker but have automated setup:

```bash
# Clean and start Docker manually
cd ../infrastructure/development
docker compose down -v
docker compose up -d

# Then run tests with global setup (waits for services and calls setup API)
npm run test
```

The global setup (configured in `playwright.config.ts`) will:
- Wait for database to be ready
- Wait for API to be ready
- Call the setup API to create admin user

### Option 3: Individual Commands

For complete manual control:

```bash
# Clean up
npm run test:cleanup

# Setup environment
npm run test:setup

# Run tests
npm run test

# Clean up after
npm run test:cleanup
```

## Configuration

The scripts use the following environment variables (with defaults):
- `API_BASE_URL`: http://localhost:8091
- `PORT_FRONTEND`: 8090
- `PORT_WEBAPI`: 8091

You can override these:
```bash
API_BASE_URL=http://my-api:8080 npm run test:full
```

## Requirements

- Docker and Docker Compose installed
- Node.js installed (for npm scripts)
- curl installed (for health checks in test-runner.sh)
- Docker containers must be able to start successfully

## Notes

- The `--no-cleanup` flag is useful when you want to inspect the database state after tests fail
- The `--skip-setup` flag is useful when you want to reuse existing containers (e.g., when iterating on test development)
- Cleanup always runs on script exit, even if tests fail
- If you manually stop the script with Ctrl+C, cleanup will still run

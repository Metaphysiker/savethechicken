# Docker Test Automation

This document describes how to automate Docker setup and teardown for Playwright tests.

## Problem

When running tests, data from previous test runs persists in Docker containers. This requires manually:
1. Running `docker compose down -v` to remove containers and volumes
2. Running `docker compose up -d` to start fresh containers  
3. Calling `/api/Setup/setup` to create the admin user

## Solutions

We provide **three approaches** to automate this, from simplest to most comprehensive:

---

## Approach 1: Use Global Setup (Recommended for CI/CD)

Playwright's built-in `globalSetup` automatically runs before all tests and can execute Docker commands.

### How it works

The `global-setup.ts` file:
1. Runs `docker compose down -v` (ignoring errors if containers don't exist)
2. Runs `docker compose up -d` to start containers
3. Waits for database to be healthy (`/health/db`)
4. Waits for API to be ready (`/health`)
5. Calls the setup API (`/api/Setup/setup`) to create admin user

### Usage

```bash
# Just run tests - global setup runs automatically
npm run test

# Or run with headed mode for debugging
npm run test:headed
```

The global setup is configured in `playwright.config.ts` and runs **once** before all test projects.

### Requirements
- Docker must be installed and accessible from the test environment
- The `child_process` module must have permission to run Docker commands
- Node.js must have network access to call the API

### Configuration

Override the API base URL:
```bash
API_BASE_URL=http://my-api:8080 npm run test
```

---

## Approach 2: Use the Test Runner Script (Recommended for Local Development)

For more control, use the `test-runner.sh` bash script.

### Features
- Full Docker lifecycle management
- Health checks before running tests
- Automatic cleanup on exit (even on test failure)
- Configurable via command-line flags

### Usage

```bash
# Full workflow with setup and cleanup (RECOMMENDED)
npm run test:full

# Run without cleanup on exit (for debugging)
npm run test:full:no-cleanup

# Skip Docker setup (reuse existing containers)
npm run test:full:skip-setup
```

### Command-line Options

| Option | Description |
|--------|-------------|
| `--no-cleanup` | Don't run `docker compose down -v` on exit |
| `--skip-setup` | Skip Docker setup, use existing containers |

### Requirements
- Bash shell
- Docker and Docker Compose
- curl (for health checks)

---

## Approach 3: Manual Control with Individual Scripts

For maximum flexibility, use individual npm scripts.

### Scripts Available

```bash
# Clean up Docker containers and volumes
npm run test:cleanup

# Setup environment (clean, start, initialize)
npm run test:setup

# Run tests (requires containers to be running)
npm run test
```

### Example Workflow

```bash
# Clean up old data
npm run test:cleanup

# Start fresh environment
npm run test:setup

# Run tests
npm run test

# Clean up when done
npm run test:cleanup
```

---

## Files Added/Modified

### New Files

1. **`tests/global-setup.ts`** - Playwright global setup that automates Docker operations
2. **`scripts/test-runner.sh`** - Comprehensive bash script for full test workflow
3. **`scripts/README.md`** - Documentation for the test runner

### Modified Files

1. **`playwright.config.ts`** - Added `globalSetup: './tests/global-setup.ts'`
2. **`package.json`** - Added new npm scripts:
   - `test:full` - Run full workflow with test-runner.sh
   - `test:full:no-cleanup` - Run without cleanup on exit
   - `test:full:skip-setup` - Run skipping Docker setup
   - `test:setup` - Manual setup command
   - `test:cleanup` - Manual cleanup command
   - `test:global-setup` - Run global setup directly
3. **`package.json`** - Added `ts-node` dependency for running TypeScript setup files

---

## Environment Variables

All scripts respect these environment variables:

| Variable | Default | Description |
|----------|---------|-------------|
| `API_BASE_URL` | http://localhost:8091 | Base URL for the API |
| `PORT_WEBAPI` | 8091 | API port |
| `PORT_FRONTEND` | 8090 | Frontend port |

---

## CI/CD Integration

For CI/CD pipelines, use the global setup approach:

```yaml
# GitHub Actions example
- name: Install dependencies
  run: npm ci

- name: Run tests
  run: npm run test
  env:
    API_BASE_URL: http://localhost:8091
```

The global setup will automatically:
- Clean up any existing containers
- Start fresh containers
- Initialize the database
- Run tests

---

## Debugging Tips

### If tests fail with "Database not ready"

1. Check if Docker is running: `docker ps`
2. Check container logs: `docker compose logs`
3. Manually verify API health: `curl http://localhost:8091/health`
4. Increase timeout in `global-setup.ts` (currently 30 retries x 2 seconds = 60 seconds)

### If you need to inspect database state after test failure

Use `--no-cleanup` flag:
```bash
npm run test:full:no-cleanup
```

Then you can:
- Connect to the database directly
- Inspect via API endpoints
- Manually clean up when done: `npm run test:cleanup`

### If Docker commands fail with permission errors

Ensure your user has permission to run Docker commands. You may need to:
```bash
sudo usermod -aG docker $USER
newgrp docker
```

---

## Known Limitations

1. **Global Setup runs once per test run** - If you need to reset the database between individual tests, you'll need a different approach (e.g., transaction rollback or test-specific setup).

2. **No automatic cleanup with globalSetup** - Playwright doesn't have a built-in `globalTeardown`. Use the bash script approach if you need guaranteed cleanup.

3. **Docker must be installed** - These scripts require Docker to be available on the test machine.

4. **Cross-platform considerations** - The bash script may need adjustments for Windows (use Git Bash or WSL).

---

## Migration from Manual Setup

Before:
```bash
# Manual process
docker compose down -v
cd infrastructure/development
docker compose up -d
# Wait for services...
curl http://localhost:8091/api/Setup/setup
cd ../MauiBlazorWeb
npm run test
```

After (using global setup):
```bash
# Automatic
npm run test
```

Or using the full script:
```bash
# Most comprehensive
npm run test:full
```

import { FullConfig } from '@playwright/test';
import { execSync, spawnSync } from 'child_process';
import * as path from 'path';
import * as os from 'os';

// Configuration
const DOCKER_COMPOSE_DIR = path.join(__dirname, '../../infrastructure/testing');
const API_BASE_URL = process.env.API_BASE_URL || 'http://localhost:8081';
const MAX_RETRIES = 60;
const RETRY_DELAY_MS = 2000;

// Debug logging
console.log('DOCKER_COMPOSE_DIR:', DOCKER_COMPOSE_DIR);
console.log('API_BASE_URL:', API_BASE_URL);
console.log('Using TESTING infrastructure');

/**
 * Execute a shell command
 */
function runCommand(command: string, cwd?: string, ignoreError: boolean = false): void {
  console.log(`  → Running: ${command}`);
  try {
    // On Windows, explicitly use cmd.exe with /c flag
    if (os.platform() === 'win32') {
      const result = spawnSync('cmd.exe', ['/c', command], {
        cwd: cwd || DOCKER_COMPOSE_DIR,
        stdio: 'inherit',
        shell: false
      });
      
      if (result.error) {
        throw result.error;
      }
      
      if (result.status !== 0) {
        throw new Error(`Command failed with exit code ${result.status}`);
      }
    } else {
      // On Unix-like systems, use execSync with shell
      execSync(command, {
        cwd: cwd || DOCKER_COMPOSE_DIR,
        encoding: 'utf-8',
        stdio: 'inherit',
        shell: true
      });
    }
    
    console.log(`  ✓ Command succeeded: ${command}`);
  } catch (error) {
    if (ignoreError) {
      console.log(`  ⚠ Command failed (ignored): ${command}`);
    } else {
      console.error(`  ✗ Command failed: ${command}`);
      throw error;
    }
  }
}

/**
 * Wait for a URL to be accessible
 */
async function waitForUrl(url: string, timeout: number = 60000): Promise<void> {
  const startTime = Date.now();
  let attempt = 0;
  
  while (Date.now() - startTime < timeout) {
    attempt++;
    try {
      const response = await fetch(url);
      if (response.ok) {
        console.log(`  ✓ URL ${url} is accessible (attempt ${attempt})`);
        return;
      }
      console.log(`  ⚠ URL ${url} returned status ${response.status} (attempt ${attempt})`);
    } catch (error) {
      console.log(`  ⚠ URL ${url} not accessible yet (attempt ${attempt}): ${error}`);
    }
    await new Promise(resolve => setTimeout(resolve, RETRY_DELAY_MS));
  }
  throw new Error(`Timeout waiting for URL: ${url} after ${attempt} attempts`);
}

/**
 * Call the setup API endpoint
 */
async function callSetupApi(): Promise<void> {
  console.log('Calling setup API...');
  
  const setupUrl = `${API_BASE_URL}/api/Setup/setup`;
  
  const response = await fetch(setupUrl, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
    },
  });
  
  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`Setup API failed: ${response.status} ${errorText}`);
  }
  
  console.log('✓ Setup API called successfully');
}

async function globalSetup(config: FullConfig): Promise<void> {
  console.log('\n=== Global Test Setup ===\n');
  
  // Step 1: Clean up existing containers and volumes
  console.log('Step 1: Cleaning up existing Docker containers and volumes...');
  runCommand('docker compose down -v', DOCKER_COMPOSE_DIR, true);
  
  // Step 2: Start containers
  console.log('\nStep 2: Starting Docker containers...');
  runCommand('docker compose up -d', DOCKER_COMPOSE_DIR);
  console.log('✓ Containers started in detached mode');
  
  // Wait a moment for containers to start initializing
  console.log('  → Waiting 5 seconds for containers to initialize...');
  await new Promise(resolve => setTimeout(resolve, 5000));
  
  // Step 3: Wait for database to be ready
  console.log('\nStep 3: Waiting for database to be ready...');
  await waitForUrl(`${API_BASE_URL}/health/db`);
  console.log('✓ Database is ready');
  
  // Step 4: Wait for API to be ready
  console.log('\nStep 4: Waiting for API to be ready...');
  await waitForUrl(`${API_BASE_URL}/health`);
  console.log('✓ API is ready');
  
  // Step 5: Initialize database with setup API
  console.log('\nStep 5: Calling setup API to initialize database...');
  await callSetupApi();
  console.log('✓ Database initialized with admin user');
  
  console.log('\n=== Global Test Setup Complete ===\n');
}

export default globalSetup;

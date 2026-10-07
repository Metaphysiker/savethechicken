#!/bin/bash

# SaveTheChicken Test Runner
# This script automates the full test workflow including Docker setup and teardown

set -e

# Configuration
DOCKER_COMPOSE_DIR="../infrastructure/development"
API_BASE_URL="${API_BASE_URL:-http://localhost:8091}"
PORT_FRONTEND="${PORT_FRONTEND:-8090}"
PORT_WEBAPI="${PORT_WEBAPI:-8091}"
MAX_RETRIES=30
RETRY_DELAY=2

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
  echo -e "${YELLOW}[TEST RUNNER]${NC} $1"
}

print_success() {
  echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_error() {
  echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if a URL is accessible
wait_for_url() {
  local url="$1"
  local max_retries="$2"
  local retry_delay="$3"
  local attempt=1

  while [ $attempt -le $max_retries ]; do
    if curl -s -f -o /dev/null "$url"; then
      return 0
    fi
    print_status "Waiting for $url (attempt $attempt/$max_retries)..."
    sleep $retry_delay
    ((attempt++))
  done
  return 1
}

# Function to call setup API
call_setup_api() {
  local url="${API_BASE_URL}/api/Setup/setup"
  print_status "Calling setup API at $url"
  
  local response
  response=$(curl -s -w "%{http_code}" -o /dev/null "$url")
  
  if [ "$response" -eq 200 ]; then
    print_success "Setup API called successfully"
    return 0
  else
    print_error "Setup API failed with status code: $response"
    return 1
  fi
}

# Cleanup function (runs on script exit)
cleanup() {
  if [ "$CLEANUP" != "false" ]; then
    print_status "Cleaning up Docker containers..."
    cd "$DOCKER_COMPOSE_DIR"
    docker compose down -v || print_error "Failed to clean up Docker containers"
    cd - > /dev/null
  fi
}

# Trap EXIT signal to run cleanup
trap cleanup EXIT

# Parse command line arguments
CLEANUP="true"
SKIP_SETUP="false"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --no-cleanup)
      CLEANUP="false"
      shift
      ;;
    --skip-setup)
      SKIP_SETUP="true"
      shift
      ;;
    *)
      # Unknown option, pass through to npm test
      break
      ;;
  esac
done

# Step 1: Clean up existing containers
if [ "$SKIP_SETUP" == "false" ]; then
  print_status "Step 1: Cleaning up existing Docker containers and volumes..."
  cd "$DOCKER_COMPOSE_DIR"
  docker compose down -v || print_error "Failed to clean up (containers may not exist)"
  cd - > /dev/null
  print_success "Containers and volumes cleaned up"
fi

# Step 2: Start containers
if [ "$SKIP_SETUP" == "false" ]; then
  print_status "Step 2: Starting Docker containers..."
  cd "$DOCKER_COMPOSE_DIR"
  docker compose up -d
  cd - > /dev/null
  print_success "Containers started in detached mode"
fi

# Step 3: Wait for services to be ready
if [ "$SKIP_SETUP" == "false" ]; then
  print_status "Step 3: Waiting for database to be ready..."
  wait_for_url "${API_BASE_URL}/health/db" $MAX_RETRIES $RETRY_DELAY
  print_success "Database is ready"

  print_status "Step 4: Waiting for API to be ready..."
  wait_for_url "${API_BASE_URL}/health" $MAX_RETRIES $RETRY_DELAY
  print_success "API is ready"

  print_status "Step 5: Initializing database with setup API..."
  call_setup_api
  print_success "Database initialized with admin user"
fi

# Step 6: Run Playwright tests
print_status "Step 6: Running Playwright tests..."
npm run test "$@"
TEST_EXIT_CODE=$?

# Exit with the test exit code
exit $TEST_EXIT_CODE

#!/bin/bash

# Setup Only Script
# Cleans Docker, starts containers, initializes database, then exits
# Perfect for running tests manually in VS Code afterward

set -e

DOCKER_COMPOSE_DIR="../infrastructure/development"
API_BASE_URL="${API_BASE_URL:-http://localhost:8091}"
MAX_RETRIES=30
RETRY_DELAY=2

echo "=== Setting up test environment ==="
echo ""

# Step 1: Clean up
echo "Step 1: Cleaning Docker containers and volumes..."
cd "$DOCKER_COMPOSE_DIR"
docker compose down -v || echo "  (containers may not have existed)"
cd - > /dev/null
echo "  ✓ Cleaned up"

# Step 2: Start containers
echo ""
echo "Step 2: Starting Docker containers..."
cd "$DOCKER_COMPOSE_DIR"
docker compose up -d
cd - > /dev/null
echo "  ✓ Containers started"

# Step 3: Wait for database
echo ""
echo "Step 3: Waiting for database..."
attempt=1
while [ $attempt -le $MAX_RETRIES ]; do
  if curl -s -f -o /dev/null "${API_BASE_URL}/health/db"; then
    echo "  ✓ Database ready"
    break
  fi
  echo "  Waiting... (attempt $attempt/$MAX_RETRIES)"
  sleep $RETRY_DELAY
  ((attempt++))
done

# Step 4: Wait for API
echo ""
echo "Step 4: Waiting for API..."
attempt=1
while [ $attempt -le $MAX_RETRIES ]; do
  if curl -s -f -o /dev/null "${API_BASE_URL}/health"; then
    echo "  ✓ API ready"
    break
  fi
  echo "  Waiting... (attempt $attempt/$MAX_RETRIES)"
  sleep $RETRY_DELAY
  ((attempt++))
done

# Step 5: Initialize database
echo ""
echo "Step 5: Initializing database..."
curl -X GET "${API_BASE_URL}/api/Setup/setup"
echo "  ✓ Database initialized"

echo ""
echo "=== Setup Complete ==="
echo ""
echo "✅ Docker containers are running"
echo "✅ Database is initialized with admin user"
echo ""
echo "You can now run tests in VS Code!"
echo "  - Open the Playwright Test Explorer"
echo "  - Or run: npm run test:ui"
echo ""
echo "To clean up when done: npm run test:cleanup"

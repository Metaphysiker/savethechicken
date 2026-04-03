#!/bin/bash
# Clean up unused Docker resources to free disk space

echo "=================================================="
echo "Docker Cleanup Script - SaveTheChicken Server"
echo "=================================================="
echo ""

echo "Current disk usage:"
df -h / | grep -v Filesystem
echo ""

echo "Current Docker disk usage:"
docker system df
echo ""

echo "=================================================="
echo "WARNING: This will remove:"
echo "  - Stopped containers"
echo "  - Unused networks"
echo "  - Dangling images"
echo "  - Unused volumes"
echo "  - Build cache"
echo ""
echo "Active containers and their volumes will be preserved."
echo "=================================================="
echo ""

read -p "Continue with cleanup? (yes/no): " confirm

if [ "$confirm" != "yes" ]; then
    echo "Cleanup cancelled."
    exit 0
fi

echo ""
echo "Starting cleanup..."
echo ""

# Remove stopped containers
echo "Removing stopped containers..."
docker container prune -f
echo ""

# Remove unused networks
echo "Removing unused networks..."
docker network prune -f
echo ""

# Remove dangling images
echo "Removing dangling images..."
docker image prune -f
echo ""

# Remove unused volumes
echo "Removing unused volumes..."
docker volume prune -f
echo ""

# Remove build cache
echo "Removing build cache..."
docker builder prune -f
echo ""

echo "=================================================="
echo "Cleanup complete!"
echo "=================================================="
echo ""

echo "New disk usage:"
df -h / | grep -v Filesystem
echo ""

echo "New Docker disk usage:"
docker system df
echo ""

echo "Disk space freed:"
echo "(Compare the 'Used' values above with the initial values)"
echo ""

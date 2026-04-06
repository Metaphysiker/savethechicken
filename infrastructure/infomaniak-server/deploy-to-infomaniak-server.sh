#!/bin/bash
set -e

echo "=== Pre-Deployment Checks ==="

# Check for uncommitted changes
if ! git diff-index --quiet HEAD --; then
    echo "WARNING: You have uncommitted changes:"
    git status --short
    read -p "Continue anyway? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Deployment cancelled."
        exit 0
    fi
fi

# Get current deployment info
current_branch=$(git rev-parse --abbrev-ref HEAD)
commit_hash=$(git rev-parse HEAD)
commit_short=$(git rev-parse --short HEAD)
commit_message=$(git log -1 --pretty=%B)

echo ""
echo "Current branch: $current_branch"
echo "Commit: $commit_short - $commit_message"

# Check what's currently deployed
echo ""
echo "Checking current deployment on server..."
current_deployment=$(ssh deploy@84.234.19.192 "cat /home/deploy/savethechicken/deployment.txt 2>/dev/null || echo 'No deployment info found'")
if [ "$current_deployment" != "No deployment info found" ]; then
    echo "Currently deployed:"
    echo "$current_deployment"
else
    echo "No previous deployment info found on server"
fi

# Create deployment tracking
timestamp=$(date +"%Y-%m-%d_%H-%M-%S")
deployment_branch="deployment/$timestamp"
deployment_tag="deploy-$timestamp"

echo ""
echo "Creating deployment tracking:"
echo "  Branch: $deployment_branch"
echo "  Tag: $deployment_tag"

git branch $deployment_branch
git tag -a $deployment_tag -m "Deployment at $timestamp from $current_branch ($commit_short)"

# Create deployment info file
cat > deployment.txt << EOF
Deployment Date: $timestamp
Branch: $current_branch
Commit: $commit_hash
Commit Short: $commit_short
Message: $commit_message
Deployed By: $USER
Deployed From: $(hostname)
EOF

echo ""
echo "Deployment info created"
echo ""
echo "Building Docker images..."

docker compose --file docker-compose.build.yml build

docker save savethechicken-production-webapi | bzip2 | pv | ssh deploy@84.234.19.192 docker load

docker save savethechicken-production-web | bzip2 | pv | ssh deploy@84.234.19.192 docker load

scp docker-compose.remote.yml deploy@84.234.19.192:/home/deploy/savethechicken

scp .env deploy@84.234.19.192:/home/deploy/savethechicken

scp appsettings.Production.json deploy@84.234.19.192:/home/deploy/savethechicken

scp deployment.txt deploy@84.234.19.192:/home/deploy/savethechicken

ssh deploy@84.234.19.192 << EOF
    cd /home/deploy/savethechicken
    docker compose --file docker-compose.remote.yml down
    docker compose --file docker-compose.remote.yml up -d
    docker system prune -f
EOF

echo ""
echo "Deployment completed successfully!"
echo "Deployment tracked in:"
echo "  Branch: $deployment_branch"
echo "  Tag: $deployment_tag"
echo "  Commit: $commit_short"
echo ""
echo "Cleaning up..."
rm -f deployment.txt

git checkout $current_branch

echo ""
echo "To check what's deployed on server, run:"
echo "  ssh deploy@84.234.19.192 cat /home/deploy/savethechicken/deployment.txt"
echo ""
echo "To view deployment history:"
echo "  git branch --list 'deployment/*'"
echo "  git tag --list 'deploy-*'"

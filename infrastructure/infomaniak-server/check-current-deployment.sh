#!/bin/bash

echo "Checking current deployment on Infomaniak server..."
echo ""

deployment_info=$(ssh deploy@84.234.19.192 "cat /home/deploy/savethechicken/deployment.txt 2>/dev/null")

if [ $? -eq 0 ] && [ -n "$deployment_info" ]; then
    echo "Currently Deployed:"
    echo "$deployment_info"

    echo ""
    echo "Quick Summary:"
    echo "$deployment_info" | grep "Commit Short:" | sed 's/Commit Short:/  Commit:/'
    echo "$deployment_info" | grep "Deployment Date:" | sed 's/Deployment Date:/  Deployed:/'
    echo "$deployment_info" | grep "^Branch:" | sed 's/Branch:/  From Branch:/'
else
    echo "No deployment info found on server"
    echo "This might be the first deployment, or the server doesn't have deployment tracking yet."
fi

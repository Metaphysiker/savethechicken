#!/bin/bash
# Script to help update nginx with rate limiting configuration

echo "=================================================="
echo "Nginx Rate Limiting Setup Helper"
echo "=================================================="
echo ""

# Find nginx config file
if [ -f /etc/nginx/sites-available/default ]; then
    CONFIG_FILE="/etc/nginx/sites-available/default"
elif [ -f /etc/nginx/nginx.conf ]; then
    CONFIG_FILE="/etc/nginx/nginx.conf"
else
    echo "Could not find nginx config file."
    echo "Please specify the path manually"
    exit 1
fi

echo "Found nginx config: $CONFIG_FILE"
echo ""

# Backup current config
BACKUP_FILE="$CONFIG_FILE.backup.$(date +%Y%m%d_%H%M%S)"
echo "Creating backup: $BACKUP_FILE"
sudo cp "$CONFIG_FILE" "$BACKUP_FILE"
echo "✅ Backup created"
echo ""

echo "=================================================="
echo "NEXT STEPS:"
echo "=================================================="
echo ""
echo "1. Edit your nginx config:"
echo "   sudo nano $CONFIG_FILE"
echo ""
echo "2. Add these lines at the TOP of the http block:"
echo "   limit_req_zone \$binary_remote_addr zone=general:10m rate=10r/s;"
echo "   limit_req_zone \$binary_remote_addr zone=api:10m rate=5r/s;"
echo "   limit_req_zone \$binary_remote_addr zone=auth:10m rate=2r/s;"
echo "   limit_conn_zone \$binary_remote_addr zone=addr:10m;"
echo ""
echo "3. In your server block, add:"
echo "   limit_conn addr 20;"
echo "   limit_req zone=general burst=20 nodelay;"
echo ""
echo "4. Test the configuration:"
echo "   sudo nginx -t"
echo ""
echo "5. If test passes, reload nginx:"
echo "   sudo systemctl reload nginx"
echo ""
echo "6. Monitor for rate limiting:"
echo "   sudo tail -f /var/log/nginx/error.log | grep limiting"
echo ""
echo "=================================================="
echo "See nginx-rate-limiting-config.conf for full example"
echo "=================================================="
echo ""

# Show current nginx status
echo "Current nginx status:"
sudo systemctl status nginx --no-pager | head -10
echo ""

# Test if nginx config is valid
echo "Testing current nginx config:"
sudo nginx -t
echo ""

if [ $? -eq 0 ]; then
    echo "✅ Current config is valid"
    echo ""
    echo "Ready to make changes!"
else
    echo "⚠️  Current config has errors - fix these first"
    exit 1
fi

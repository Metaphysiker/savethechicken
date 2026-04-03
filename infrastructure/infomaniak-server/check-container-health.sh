#!/bin/bash
# Monitor container health and detect if they're hitting resource limits

echo "=================================================="
echo "Container Health Check - SaveTheChicken"
echo "=================================================="
echo ""

# Check if any containers have restarted recently (sign of OOM kills)
echo "=== CONTAINER RESTART COUNT ==="
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.RestartCount}}" | grep -E "(savethechicken|stinah)" || echo "No SaveTheChicken containers found"
echo ""

# Show containers that have restarted
echo "=== RECENTLY RESTARTED CONTAINERS ==="
docker ps -a --filter "name=savethechicken" --format "table {{.Names}}\t{{.Status}}" | grep -v "Up [0-9]* month" || echo "All containers running stable"
echo ""

# Real-time memory usage vs limits
echo "=== MEMORY USAGE vs LIMITS ==="
echo "Checking which containers are close to their limits..."
echo ""

for container in $(docker ps --filter "name=savethechicken" --format "{{.Names}}"); do
    # Get memory usage
    stats=$(docker stats --no-stream --format "{{.MemUsage}}" "$container")
    used=$(echo $stats | awk '{print $1}')
    limit=$(echo $stats | awk '{print $3}')
    percent=$(docker stats --no-stream --format "{{.MemPerc}}" "$container" | sed 's/%//')
    
    # Alert if over 80%
    if (( $(echo "$percent > 80" | bc -l) )); then
        echo "⚠️  $container: $used / $limit ($percent%) - DANGER!" 
    elif (( $(echo "$percent > 60" | bc -l) )); then
        echo "⚠️  $container: $used / $limit ($percent%) - Warning"
    else
        echo "✅  $container: $used / $limit ($percent%) - OK"
    fi
done
echo ""

# Check Docker logs for OOM kills
echo "=== CHECKING FOR OOM KILLS (Last 100 lines) ==="
if dmesg 2>/dev/null | tail -100 | grep -i "oom" | grep -i "docker" | tail -5; then
    echo ""
    echo "⚠️  WARNING: OOM kills detected! Containers were killed due to memory limits."
    echo "Consider increasing memory limits or optimizing your application."
else
    echo "✅ No OOM kills detected in recent logs"
fi
echo ""

# Check for errors in container logs
echo "=== RECENT CONTAINER ERRORS ==="
for container in $(docker ps --filter "name=savethechicken" --format "{{.Names}}"); do
    errors=$(docker logs "$container" --since 1h 2>&1 | grep -iE "error|exception|fatal" | wc -l)
    if [ "$errors" -gt 0 ]; then
        echo "⚠️  $container: $errors errors in last hour"
        docker logs "$container" --since 1h 2>&1 | grep -iE "error|exception|fatal" | tail -3
        echo ""
    else
        echo "✅  $container: No errors in last hour"
    fi
done
echo ""

echo "=================================================="
echo "RECOMMENDATIONS:"
echo "=================================================="
echo "- If containers restart frequently: increase memory limits"
echo "- If memory usage > 80%: monitor closely or upgrade"
echo "- If OOM kills detected: increase limits immediately"
echo "- Run this check after traffic spikes or deployments"
echo ""
echo "Monitor live: docker stats"
echo ""

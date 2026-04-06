#!/bin/bash
# Check server resources for SaveTheChicken production environment

echo "=================================================="
echo "SERVER RESOURCES CHECK - SaveTheChicken Production"
echo "=================================================="
echo ""

# System Information
echo "=== SYSTEM INFORMATION ==="
echo "Hostname: $(hostname)"
echo "OS: $(cat /etc/os-release | grep PRETTY_NAME | cut -d= -f2 | tr -d '\"')"
echo "Kernel: $(uname -r)"
echo "Uptime: $(uptime -p)"
echo ""

# CPU Information
echo "=== CPU INFORMATION ==="
echo "CPU Model: $(lscpu | grep 'Model name' | cut -d: -f2 | xargs)"
echo "CPU Cores: $(nproc) cores"
echo "CPU Usage (last minute):"
top -bn1 | grep "Cpu(s)" | sed "s/.*, *\([0-9.]*\)%* id.*/\1/" | awk '{print "  Used: " 100 - $1 "%"}'
echo ""

# Memory Information
echo "=== MEMORY INFORMATION ==="
free -h | awk 'NR==1{print "  " $0} NR==2{printf "  %-10s %s total, %s used, %s free, %s available\n", $1, $2, $3, $4, $7}'
echo ""
echo "Memory Usage Details:"
free -m | awk 'NR==2{printf "  Total: %s MB\n  Used: %s MB (%.1f%%)\n  Free: %s MB\n  Available: %s MB\n", $2, $3, $3*100/$2, $4, $7}'
echo ""

# Disk Space
echo "=== DISK SPACE ==="
df -h / | awk 'NR==1{print "  " $0} NR==2{printf "  %-20s %s total, %s used (%s), %s available\n", $1, $2, $3, $5, $4}'
echo ""
echo "Disk Usage by Directory:"
du -sh /home/deploy/* 2>/dev/null | sort -hr | head -10 | awk '{printf "  %s\t%s\n", $1, $2}'
echo ""

# Docker Information
echo "=== DOCKER RESOURCES ==="
if command -v docker &> /dev/null; then
    echo "Docker Version: $(docker --version)"
    echo ""
    
    echo "Docker Disk Usage:"
    docker system df -v 2>/dev/null | head -20
    echo ""
    
    echo "=== RUNNING CONTAINERS ==="
    docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" 2>/dev/null
    echo ""
    
    echo "=== CONTAINER RESOURCE USAGE ==="
    docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}\t{{.NetIO}}\t{{.BlockIO}}" 2>/dev/null
    echo ""
    
    # Docker volume sizes
    echo "=== DOCKER VOLUMES ==="
    echo "Volume sizes:"
    docker volume ls -q | while read vol; do
        size=$(docker system df -v 2>/dev/null | grep "$vol" | awk '{print $3}')
        if [ ! -z "$size" ]; then
            echo "  $vol: $size"
        fi
    done
    echo ""
else
    echo "Docker is not installed or not in PATH"
    echo ""
fi

# Network Information
echo "=== NETWORK INFORMATION ==="
echo "Network Connections:"
netstat -an 2>/dev/null | grep ESTABLISHED | wc -l | awk '{print "  Active connections: " $1}'
echo ""

echo "Listening Ports:"
netstat -tuln 2>/dev/null | grep LISTEN | awk '{print "  " $4}' | sort -u | head -10
echo ""

# Load Average
echo "=== LOAD AVERAGE ==="
uptime | awk -F'load average:' '{print "  Load Average:" $2}'
echo ""

# Top Processes by Memory
echo "=== TOP 5 PROCESSES BY MEMORY ==="
ps aux --sort=-%mem | head -6 | awk 'NR==1{print "  " $0} NR>1{printf "  %-10s %5s%% %5s%% %s\n", $1, $3, $4, $11}'
echo ""

# Top Processes by CPU
echo "=== TOP 5 PROCESSES BY CPU ==="
ps aux --sort=-%cpu | head -6 | awk 'NR==1{print "  " $0} NR>1{printf "  %-10s %5s%% %5s%% %s\n", $1, $3, $4, $11}'
echo ""

# PostgreSQL Database Size (if accessible)
if docker ps | grep -q postgres; then
    echo "=== DATABASE SIZE ==="
    docker exec savethechicken-production-postgres-1 psql -U savethechicken -d savethechicken -c "
        SELECT 
            pg_size_pretty(pg_database_size('savethechicken')) as database_size,
            pg_size_pretty(pg_total_relation_size('public.chicken')) as chicken_table_size;
    " 2>/dev/null || echo "  Could not connect to database"
    echo ""
fi

echo "=================================================="
echo "RESOURCE CHECK COMPLETE"
echo "=================================================="
echo ""
echo "RECOMMENDATIONS:"
echo "  - If memory usage is > 80%, consider upgrading or adding limits"
echo "  - If disk usage is > 85%, clean up Docker images/volumes"
echo "  - If CPU consistently > 70%, consider optimization or upgrade"
echo "  - Check docker stats regularly to monitor container usage"
echo ""

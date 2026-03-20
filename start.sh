#!/bin/bash
set -e

echo "═══════════════════════════════════════════════════════"
echo "  Starting Multi-Application Container"
echo "═══════════════════════════════════════════════════════"
echo ""
echo "  → .NET Web App: MaschinenDataein (Port 8080)"
echo "  → Python Pipeline: Market Data Collector"
echo ""
echo "═══════════════════════════════════════════════════════"

# Start Supervisor
exec /usr/bin/supervisord -c /etc/supervisor/conf.d/supervisord.conf

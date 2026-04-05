#!/bin/bash
# QSS Platform — Local Development Launcher
# Starts both the API (port 5000) and Web (port 5001) concurrently

set -e

echo "=========================================="
echo "  QSS Platform — Local Development"
echo "=========================================="

# Check .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not found. Install from https://dot.net"
    exit 1
fi

echo ".NET version: $(dotnet --version)"
echo ""

# Kill anything running on those ports
kill_port() {
    pid=$(lsof -ti tcp:$1 2>/dev/null)
    if [ -n "$pid" ]; then
        echo "Stopping process on port $1 (PID $pid)..."
        kill -9 $pid 2>/dev/null || true
    fi
}

kill_port 5000
kill_port 5001

echo "Starting QSS API on http://localhost:5000 ..."
cd src/QSS.API
ASPNETCORE_URLS="http://localhost:5000" dotnet run --no-build &
API_PID=$!
cd ../..

# Wait a moment for API to start
sleep 3

echo "Starting QSS Web on http://localhost:5001 ..."
cd src/QSS.Web
ASPNETCORE_URLS="http://localhost:5001" dotnet run --no-build &
WEB_PID=$!
cd ../..

echo ""
echo "=========================================="
echo "  QSS Platform is running!"
echo "  API:     http://localhost:5000/swagger"
echo "  Web UI:  http://localhost:5001"
echo ""
echo "  Default credentials:"
echo "  superadmin@qss.com / Admin@1234!"
echo "  admin@qss.com / Admin@1234!"
echo "  dentist@qss.com / Admin@1234!"
echo "=========================================="
echo ""
echo "Press Ctrl+C to stop all services"

# Wait for either process to exit
wait $API_PID $WEB_PID

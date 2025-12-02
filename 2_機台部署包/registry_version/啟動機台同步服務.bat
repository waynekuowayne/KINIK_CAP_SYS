@echo off
echo ========================================
echo Machine Sync Service Startup
echo ========================================
echo.

echo Starting Registry Sync Service...
echo.

cd /d %~dp0

echo Checking Python packages...
pip install -q pymysql

echo.
echo [Starting] Registry Sync Service
echo Log file: C:\Temp\RegistrySync_Log.txt
echo.

python registry_sync.py

pause

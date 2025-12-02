@echo off
echo ========================================
echo Registry Sync Service
echo ========================================
echo.
echo 正在啟動 Registry 同步程式...
echo 此程式會每 3 秒同步一次資料到資料庫
echo.
echo 按 Ctrl+C 可停止程式
echo ========================================
echo.

cd /d %~dp0
python registry_sync.py

pause

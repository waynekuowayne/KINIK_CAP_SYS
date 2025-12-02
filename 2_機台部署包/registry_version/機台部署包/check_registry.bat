@echo off
chcp 65001 >nul
echo ========================================
echo Check Registry Settings
echo ========================================
echo.
echo Registry Path: HKEY_CURRENT_USER\Software\ZHAOI\VALUE
echo.
echo Checking machine config...
echo.

reg query "HKCU\Software\ZHAOI\VALUE\MachineConfig" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [OK] Machine config exists
    echo.
    reg query "HKCU\Software\ZHAOI\VALUE\MachineConfig" /v MachineID 2>nul
    reg query "HKCU\Software\ZHAOI\VALUE\MachineConfig" /v MachineName 2>nul
    reg query "HKCU\Software\ZHAOI\VALUE\MachineConfig" /v MachineIP 2>nul
) else (
    echo [ERROR] Machine config does not exist!
    echo.
    echo Please import .reg file first:
    echo 1. Double-click CAM-001.reg in the folder
    echo 2. Click Yes to confirm
    echo.
)

echo.
echo ========================================
pause

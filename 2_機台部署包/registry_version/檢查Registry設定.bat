@echo off
echo ========================================
echo 檢查 Registry 設定
echo ========================================
echo.
echo Registry 路徑: HKEY_CURRENT_USER\Software\ZHAOI\VALUE
echo.
echo 正在查詢機台設定...
echo.

reg query "HKCU\Software\ZHAOI\VALUE\MachineConfig" 2>nul
if %ERRORLEVEL% EQU 0 (
    echo ✓ 機台設定已存在
    echo.
    reg query "HKCU\Software\ZHAOI\VALUE\MachineConfig" /v MachineID
    reg query "HKCU\Software\ZHAOI\VALUE\MachineConfig" /v MachineName
    reg query "HKCU\Software\ZHAOI\VALUE\MachineConfig" /v MachineIP
) else (
    echo ✗ 機台設定不存在！
    echo.
    echo 請先執行以下步驟：
    echo 1. 雙擊 各機台設定範例\CAM-001.reg
    echo 2. 點選「是」確認匯入
    echo.
)

echo.
echo ========================================
pause

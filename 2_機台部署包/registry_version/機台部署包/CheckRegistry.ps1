# Registry Check Script
# PowerShell version - supports Chinese characters properly

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Registry Settings Check" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Registry Path: HKEY_CURRENT_USER\Software\ZHAOI\VALUE" -ForegroundColor Yellow
Write-Host ""

$regPath = "HKCU:\Software\ZHAOI\VALUE\MachineConfig"

if (Test-Path $regPath) {
    Write-Host "[OK] Machine config exists" -ForegroundColor Green
    Write-Host ""

    try {
        $machineId = Get-ItemProperty -Path $regPath -Name "MachineID" -ErrorAction Stop
        Write-Host "MachineID = $($machineId.MachineID)" -ForegroundColor Green
    } catch {
        Write-Host "MachineID = (not set)" -ForegroundColor Yellow
    }

    try {
        $machineName = Get-ItemProperty -Path $regPath -Name "MachineName" -ErrorAction Stop
        Write-Host "MachineName = $($machineName.MachineName)" -ForegroundColor Green
    } catch {
        Write-Host "MachineName = (not set)" -ForegroundColor Yellow
    }

    try {
        $machineIP = Get-ItemProperty -Path $regPath -Name "MachineIP" -ErrorAction Stop
        Write-Host "MachineIP = $($machineIP.MachineIP)" -ForegroundColor Green
    } catch {
        Write-Host "MachineIP = (not set)" -ForegroundColor Yellow
    }

    Write-Host ""
    Write-Host "Checking CurrentTask..." -ForegroundColor Cyan
    $taskPath = "HKCU:\Software\ZHAOI\VALUE\CurrentTask"
    if (Test-Path $taskPath) {
        Write-Host "[OK] CurrentTask exists" -ForegroundColor Green

        try {
            $progress = Get-ItemProperty -Path $taskPath -Name "Progress" -ErrorAction Stop
            Write-Host "Progress = $($progress.Progress)" -ForegroundColor Green
        } catch {
            Write-Host "Progress = (not set)" -ForegroundColor Yellow
        }

        try {
            $status = Get-ItemProperty -Path $taskPath -Name "Status" -ErrorAction Stop
            Write-Host "Status = $($status.Status)" -ForegroundColor Green
        } catch {
            Write-Host "Status = (not set)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "[INFO] CurrentTask not created yet (will be created when VB.NET runs)" -ForegroundColor Yellow
    }

} else {
    Write-Host "[ERROR] Machine config does not exist!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please follow these steps:" -ForegroundColor Yellow
    Write-Host "1. Go to folder: 各機台設定" -ForegroundColor White
    Write-Host "2. Double-click the .reg file (e.g., CAM-001.reg)" -ForegroundColor White
    Write-Host "3. Click 'Yes' to confirm import" -ForegroundColor White
    Write-Host "4. You should see 'Successfully added to registry' message" -ForegroundColor White
    Write-Host ""
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

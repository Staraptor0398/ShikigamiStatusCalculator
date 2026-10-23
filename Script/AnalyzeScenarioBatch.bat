@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0AnalyzeScenarioBatch.ps1"
if errorlevel 1 (
    echo.
    echo Scenario Runner Batch analysis failed.
    pause
    exit /b 1
)
echo.
echo Scenario Runner Batch analysis completed.
pause

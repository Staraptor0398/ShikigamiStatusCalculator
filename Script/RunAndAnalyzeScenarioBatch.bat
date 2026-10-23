@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0RunAndAnalyzeScenarioBatch.ps1"
if errorlevel 1 (
    echo.
    echo Scenario Runner Batch execution or analysis failed.
    pause
    exit /b 1
)
echo.
echo Scenario Runner Batch execution and analysis completed.
pause

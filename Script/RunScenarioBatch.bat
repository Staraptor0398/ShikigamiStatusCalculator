@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0RunScenarioBatch.ps1"
if errorlevel 1 (
    echo.
    echo Scenario Runner Batch dispatch failed.
    pause
    exit /b 1
)
echo.
echo Scenario Runner Batch dispatch completed.
pause

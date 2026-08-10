@echo off
setlocal

set "WORK_DRIVE=W:"
set "REPOSITORY_ROOT=%~dp0"

echo Setting workspace drive...
echo   %WORK_DRIVE% = %REPOSITORY_ROOT%

subst %WORK_DRIVE% /D >nul 2>&1
subst %WORK_DRIVE% "%REPOSITORY_ROOT%"

if errorlevel 1 (
   echo Failed to set workspace drive.
   exit /b 1
)

echo Workspace drive was set successfully.
echo   %WORK_DRIVE%\

endlocal

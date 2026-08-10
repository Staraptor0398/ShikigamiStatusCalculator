@echo off
setlocal

set "WORK_DRIVE=W:"

echo Removing workspace drive...
subst %WORK_DRIVE% /D

if errorlevel 1 (
   echo Failed to remove workspace drive.
   exit /b 1
)

echo Workspace drive was removed successfully.
endlocal

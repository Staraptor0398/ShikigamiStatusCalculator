@echo off
setlocal

cd /d "%~dp0.."

echo Step1. MSBuild Search

set "MSBUILD="

call :FindMSBuild "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe"
call :FindMSBuild "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
call :FindMSBuild "C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"

if "%MSBUILD%"=="" (
   echo MSBuild.exe not found.
   pause
   exit /b 1
)

echo MSBuild: %MSBUILD%

echo.
echo Step2. Solution Rebuild

"%MSBUILD%" "ShikigamiApp.slnx" /t:Rebuild /m /p:Configuration=Release /p:Platform=x64 /v:minimal

if errorlevel 1 (
   echo Build failed.
   pause
   exit /b 1
)

echo Build succeeded.

echo.
echo Step3. Git tag Search

git describe --tags --exact-match > "%TEMP%\shikigami_tag.txt" 2>nul

set "VERSION_TAG="

for /f "usebackq delims=" %%i in ("%TEMP%\shikigami_tag.txt") do (
   set "VERSION_TAG=%%i"
)

del "%TEMP%\shikigami_tag.txt"

if "%VERSION_TAG%"=="" (
   echo Git tag not found.
   pause
   exit /b 1
)

set "APP_NAME=ShikigamiStatusCalculator"
set "PACKAGE_NAME=%APP_NAME%_%VERSION_TAG%"
set "PACKAGE_ROOT=ReleasePackage"
set "PACKAGE_DIR=%PACKAGE_ROOT%\%PACKAGE_NAME%"

set "ZIP_PATH=%PACKAGE_ROOT%\%PACKAGE_NAME%.zip"

echo.
echo Step4. Package Directory Setup

if exist "%PACKAGE_DIR%" rmdir /s /q "%PACKAGE_DIR%"

mkdir "%PACKAGE_DIR%"
mkdir "%PACKAGE_DIR%\Data"

echo.
echo Step5. Copy Release Files

copy /Y "Gui\bin\x64\Release\Gui.exe" "%PACKAGE_DIR%\"
copy /Y "Gui\bin\x64\Release\Gateway.dll" "%PACKAGE_DIR%\"
copy /Y "Gui\bin\x64\Release\Newtonsoft.Json.dll" "%PACKAGE_DIR%\"
xcopy /E /I /Y "Gui\bin\x64\Release\Data" "%PACKAGE_DIR%\Data"

echo.
echo Step6. Create Zip

if exist "%ZIP_PATH%" del "%ZIP_PATH%"
pushd "%PACKAGE_DIR%"
"..\..\Tools\7za.exe" a -tzip "..\%PACKAGE_NAME%.zip" "*"
popd

if errorlevel 1 (
  echo Zip failed.
  pause
  exit /b 1
)

echo.
echo Step7. Cleanup Package Directory

rmdir /s /q "%PACKAGE_DIR%"

if exist "%PACKAGE_DIR%" (
	echo Cleanup failed.
	pause
	exit /b 1
)

echo.
echo Release package created: %ZIP_PATH%
pause
exit /b 0

:FindMSBuild
if not "%MSBUILD%"=="" exit /b 0
if exist "%~1" set "MSBUILD=%~1"
exit /b 0

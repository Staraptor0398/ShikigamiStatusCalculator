@echo off
setlocal EnableExtensions EnableDelayedExpansion

rem ============================================================
rem  1. パス設定
rem ============================================================
set "SCRIPT_DIR=%~dp0"
set "GENERATOR=%SCRIPT_DIR%generate_pdf.bat"
set "PROJECT_NAME=%~1"

for %%D in ("%SCRIPT_DIR%..") do set "DOCUMENT_DIR=%%~fD"

if "%PROJECT_NAME%"=="" (
    echo [ERROR] プロジェクト名が指定されていません。
    pause
    exit /b 1
)

if "%~2"=="" (
    echo [ERROR] Diagram フォルダが指定されていません。
    pause
    exit /b 1
)

if not exist "%GENERATOR%" (
    echo [ERROR] generate_pdf.bat が見つかりません。
    echo         %GENERATOR%
    pause
    exit /b 1
)

rem ============================================================
rem  2. .wsd ファイル一覧取得
rem ============================================================
set /a FILE_COUNT=0

call :COLLECT_FILES "%~2"
if errorlevel 1 exit /b 1

if not "%~3"=="" (
    call :COLLECT_FILES "%~3"
    if errorlevel 1 exit /b 1
)

if !FILE_COUNT! EQU 0 (
    echo [ERROR] .wsd ファイルが見つかりません。
    pause
    exit /b 1
)

rem ============================================================
rem  3. 一覧表示
rem ============================================================
:MENU
cls
echo ========================================
echo %PROJECT_NAME% Diagram Manager
echo ========================================
echo.

for /l %%I in (1,1,!FILE_COUNT!) do echo [%%I] !DISPLAY_%%I!

echo.
echo [A] 全て更新
echo [Q] 終了
echo.

set "SELECT="
set /p "SELECT=更新する図を選択してください: "

if /i "!SELECT!"=="Q" exit /b 0
if /i "!SELECT!"=="A" goto GENERATE_ALL

rem ============================================================
rem  4. 番号入力チェック
rem ============================================================
set "TARGET_FILE="
for /l %%I in (1,1,!FILE_COUNT!) do (
    if "!SELECT!"=="%%I" set "TARGET_FILE=!FILE_%%I!"
)

if not defined TARGET_FILE (
    echo.
    echo [ERROR] 1～!FILE_COUNT!、A、Q のいずれかを入力してください。
    pause
    goto MENU
)

rem ============================================================
rem  5. 選択された図を更新
rem ============================================================
echo.
echo [INFO] Generating:
echo        !TARGET_FILE!
echo.

call "%GENERATOR%" "!TARGET_FILE!" /nopause
if errorlevel 1 (
    echo.
    echo [ERROR] 図の生成に失敗しました。
    pause
    goto MENU
)

echo.
echo [SUCCESS] 更新しました。
pause
goto MENU

rem ============================================================
rem  6. 全て更新
rem ============================================================
:GENERATE_ALL
echo.
echo [INFO] 全ての図を更新します。
echo.

for /l %%I in (1,1,!FILE_COUNT!) do (
    echo ----------------------------------------
    echo [%%I/!FILE_COUNT!] !DISPLAY_%%I!
    echo ----------------------------------------

    call "%GENERATOR%" "!FILE_%%I!" /nopause
    if errorlevel 1 (
        echo.
        echo [ERROR] 図の生成に失敗しました:
        echo         !FILE_%%I!
        pause
        goto MENU
    )
    echo.
)

echo ========================================
echo [SUCCESS] 全ての図を更新しました。
echo ========================================
echo.
pause
goto MENU

rem ============================================================
rem  7. .wsd ファイル収集
rem ============================================================
:COLLECT_FILES

rem FOR /Rの探索ルートを明示的にカレントディレクトリへ変更する。
pushd "%~1" 2>nul
if errorlevel 1 (
    echo [ERROR] Diagram フォルダに移動できません。
    echo         %~1
    pause
    exit /b 1
)

set "COLLECT_ROOT=%CD%"
set /a BEFORE_COUNT=FILE_COUNT

for /r %%F in (*.wsd) do (
    if exist "%%~fF" (
        set /a FILE_COUNT+=1
        set "FILE_!FILE_COUNT!=%%~fF"
        set "RELATIVE_PATH=%%~fF"
        set "RELATIVE_PATH=!RELATIVE_PATH:%DOCUMENT_DIR%\=!"
        set "DISPLAY_!FILE_COUNT!=!RELATIVE_PATH!"
    )
)

popd

if !FILE_COUNT! EQU !BEFORE_COUNT! (
    echo [WARN] .wsd ファイルが見つかりません: !COLLECT_ROOT!
)

exit /b 0

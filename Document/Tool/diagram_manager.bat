@echo off
setlocal enabledelayedexpansion

rem ============================================================
rem  1. パス設定
rem ============================================================
set "SCRIPT_DIR=%~dp0"
set "DIAGRAM_DIR=%SCRIPT_DIR%..\Diagram"
set "GENERATOR=%SCRIPT_DIR%generate_pdf.bat"

if not exist "%DIAGRAM_DIR%" (
   echo [ERROR] Diagram フォルダが見つかりません。
   echo         %DIAGRAM_DIR%
   echo.
   pause
   exit /b 1
)

if not exist "%GENERATOR%" (
   echo [ERROR] generate_pdf.bat が見つかりません。
   echo         %GENERATOR%
   echo.
   pause
   exit /b 1
)

rem ============================================================
rem  2. .wsd ファイル一覧取得
rem ============================================================
set /a FILE_COUNT=0

for /r "%DIAGRAM_DIR%" %%F in (*.wsd) do (
   set /a FILE_COUNT+=1

   set "FILE_!FILE_COUNT!=%%~fF"

   set "RELATIVE_PATH=%%~fF"
   set "RELATIVE_PATH=!RELATIVE_PATH:%DIAGRAM_DIR%\=!"

   set "DISPLAY_!FILE_COUNT!=!RELATIVE_PATH!"
)

if !FILE_COUNT! EQU 0 (
   echo [ERROR] .wsd ファイルが見つかりません。
   echo.
   pause
   exit /b 1
)

rem ============================================================
rem  3. 一覧表示
rem ============================================================
:MENU

cls

echo ========================================
echo ShikigamiApp Diagram Manager
echo ========================================
echo.

for /l %%I in (1,1,!FILE_COUNT!) do (
   echo [%%I] !DISPLAY_%%I!
)

echo.
echo [A] 全て更新
echo [Q] 終了
echo.

set /p "SELECT=更新する図を選択してください: "

if /i "!SELECT!"=="Q" (
   exit /b 0
)

if /i "!SELECT!"=="A" (
   goto GENERATE_ALL
)


rem ============================================================
rem  4. 番号入力チェック
rem ============================================================
for /f "delims=0123456789" %%A in ("!SELECT!") do (
   echo.
   echo [ERROR] 1～!FILE_COUNT!、A、Q のいずれかを入力してください。
   pause
   goto MENU
)

if "!SELECT!"=="" (
   echo.
   echo [ERROR] 入力されていません。
   pause
   goto MENU
)

if !SELECT! LSS 1 (
   echo.
   echo [ERROR] 範囲外の番号です。
   pause
   goto MENU
)

if !SELECT! GTR !FILE_COUNT! (
   echo.
   echo [ERROR] 範囲外の番号です。
   pause
   goto MENU
)

rem ============================================================
rem  5. 選択された図を更新
rem ============================================================
set "TARGET_FILE=!FILE_%SELECT%!"

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
       echo.
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

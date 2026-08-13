@echo off
setlocal enabledelayedexpansion

rem --- 1. 設定ファイルの読み込み ---
set "SETTING_FILE=%~dp0_diagram_settings.bat"

if not exist "%SETTING_FILE%" (
    echo [ERROR] 設定ファイルが見つかりません。
    echo         「_diagram_settings.bat.template」をコピーして
    echo         「_diagram_settings.bat」を作成し、パスを編集してください。
    echo.
    pause
    exit /b 1
)

call "%SETTING_FILE%"

rem --- 2. 設定されたパスの有効性チェック ---
if not defined PLANTUML_JAR (
    echo [ERROR] PLANTUML_JAR が設定ファイルで定義されていません。
    pause
    exit /b 1
)
set "CHECK_PATH=!PLANTUML_JAR!"
if exist "!CHECK_PATH!" (
    echo [OK] PlantUML found.
) else (
    echo [ERROR] PlantUML のパスが無効です: !CHECK_PATH!
    pause
    exit /b 1
)

if not defined GRAPHVIZ_DOT (
    echo [ERROR] GRAPHVIZ_DOT が設定ファイルで定義されていません。
    pause
    exit /b 1
)
set "CHECK_PATH=!GRAPHVIZ_DOT!"
if exist "!CHECK_PATH!" (
    echo [OK] Graphviz found.
) else (
    echo [ERROR] Graphviz のパスが無効です: !CHECK_PATH!
    pause
    exit /b 1
)

if not defined INKSCAPE_EXE (
    echo [ERROR] INKSCAPE_EXE が設定ファイルで定義されていません。
    pause
    exit /b 1
)
set "CHECK_PATH=!INKSCAPE_EXE!"
if exist "!CHECK_PATH!" (
    echo [OK] Inkscape found.
) else (
    echo [ERROR] Inkscape のパスが無効です: !CHECK_PATH!
    pause
    exit /b 1
)

rem --- 3.処理対象ファイルのチェック ---
set "TARGET_FILE=%~1"
if "!TARGET_FILE!"=="" (
    echo [ERROR] .wsd ファイルをこのバッチファイルにドラッグ＆ドロップしてください。
    echo.
    pause
    exit /b 1
)
if not exist "!TARGET_FILE!" (
    echo [ERROR] 指定されたファイルが見つかりません: !TARGET_FILE!
    pause
    exit /b 1
)

set "SVG_FILE=%~dpn1.svg"
set "PDF_FILE=%~dpn1.pdf"

rem --- 4. SVGファイルの生成 ---
echo [INFO] Step 1/2: Generating SVG file...
java -jar "!PLANTUML_JAR!" -graphvizdot "!GRAPHVIZ_DOT!" -tsvg "!TARGET_FILE!"
if errorlevel 1 (
    echo [ERROR] SVGファイルの生成に失敗しました。
    echo         Javaの実行環境やPlantUMLのログを確認してください。
    pause
    exit /b 1
)
if not exist "!SVG_FILE!" (
    echo [ERROR] SVGファイルが生成されませんでした。
    pause
    exit /b 1
)
echo [SUCCESS] SVG generated: !SVG_FILE!
echo.

rem --- 5. PDFファイルへの変換 ---
echo [INFO] Step 2/2: Converting SVG to PDF...
"!INKSCAPE_EXE!" "!SVG_FILE!" --export-type=pdf --export-filename="!PDF_FILE!"
if errorlevel 1 (
    echo [ERROR] PDFへの変換に失敗しました。
    echo         Inkscapeが正しく動作するか確認してください。
    pause
    exit /b 1
)
if exist "!PDF_FILE!" (
    echo [SUCCESS] PDF generated: !PDF_FILE!
    del "!SVG_FILE!"
) else (
    echo [ERROR] PDFファイルが生成されませんでした。
)
echo.

if /i not "%~2"=="/nopause" (
    pause
)

endlocal

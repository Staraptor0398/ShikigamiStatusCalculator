@echo off

call "%~dp0diagram_manager_core.bat" ^
    "ScenarioRunner" ^
    "%~dp0..\ScenarioRunner\Diagram" ^
    "%~dp0..\ScenarioRunner\Optimization\Diagram"

exit /b %errorlevel%

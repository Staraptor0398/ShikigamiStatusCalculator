@echo off

call "%~dp0diagram_manager_core.bat" ^
    "ShikigamiDataAuditor" ^
    "%~dp0..\ShikigamiDataAuditor\Diagram"

exit /b %errorlevel%

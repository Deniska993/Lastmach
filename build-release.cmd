@echo off
setlocal

set "SCRIPT=%~dp0build-release.ps1"
powershell -ExecutionPolicy Bypass -File "%SCRIPT%"
if errorlevel 1 exit /b 1

echo Build script finished successfully.
endlocal

@echo off
setlocal
pushd "%~dp0.." || exit /b 1

set "MODE=%~1"
if not defined MODE set "MODE=build"

where pwsh >nul 2>&1
if not errorlevel 1 (
    pwsh -NoProfile -ExecutionPolicy Bypass -File "%~dp0architect.ps1" -Mode "%MODE%"
) else (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0architect.ps1" -Mode "%MODE%"
)

set "EXIT_CODE=%ERRORLEVEL%"
popd
exit /b %EXIT_CODE%

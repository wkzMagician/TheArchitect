@echo off
setlocal

pushd "%~dp0.." || exit /b 1

where pwsh >nul 2>&1
if not errorlevel 1 (
    pwsh -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-combat-tests.ps1"
) else (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-combat-tests.ps1"
)

set "EXIT_CODE=%ERRORLEVEL%"
popd
exit /b %EXIT_CODE%

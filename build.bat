@echo off
setlocal
pushd "%~dp0" || exit /b 1

dotnet build "%~dp0TheArchitect.csproj" -p:ExportMod=true %*
set "BUILD_EXIT_CODE=%ERRORLEVEL%"

popd
exit /b %BUILD_EXIT_CODE%

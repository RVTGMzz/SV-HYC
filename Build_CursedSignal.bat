@echo off
setlocal
cd /d "%~dp0"

echo ==========================================
echo   Hey! You're Cursed! v0.0.6-alpha.2
echo   Stabilize Core - Debug Build
echo ==========================================
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
    echo ERROR: dotnet was not found.
    echo Install the .NET SDK or build once through Visual Studio.
    echo.
    pause
    exit /b 1
)

echo Cleaning old build output...
dotnet clean CursedSignal.csproj -c Debug --nologo
if errorlevel 1 goto :failed

echo.
echo Building...
dotnet build CursedSignal.csproj -c Debug --nologo
if errorlevel 1 goto :failed

echo.
echo ==========================================
echo   BUILD SUCCESS
echo   ModBuildConfig should deploy CursedSignal
echo   into your Stardew Valley Mods folder.
echo ==========================================
echo.
pause
exit /b 0

:failed
echo.
echo ==========================================
echo   BUILD FAILED
echo   Copy the error lines above and send them
echo   back to ChatGPT.
echo ==========================================
echo.
pause
exit /b 1

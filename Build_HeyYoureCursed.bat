@echo off
setlocal
cd /d "%~dp0"

echo ==========================================
echo   Hey! You're Cursed! v0.0.7-alpha.2.2.1
echo   ChaCha-like Ghost Hover Hotfix + Test Shortcuts
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
dotnet clean HeyYoureCursed.csproj -c Debug --nologo
if errorlevel 1 goto :failed

echo.
echo Building...
dotnet build HeyYoureCursed.csproj -c Debug --nologo
if errorlevel 1 goto :failed

echo.
echo ==========================================
echo   BUILD SUCCESS
echo   Deploy folder: Mods\HeyYoureCursed
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

@echo off
rem SavePathCopier self-extracting package install script.
rem Runs from the IExpress extraction temp folder. Copies the app to
rem %LOCALAPPDATA%\SavePathCopier, registers logon auto-start, starts the app.

setlocal
set "DEST=%LOCALAPPDATA%\SavePathCopier"
set "SRCDIR=%~dp0"

if not exist "%DEST%" mkdir "%DEST%"

copy /y "%SRCDIR%SavePathCopier.exe" "%DEST%\" >nul
copy /y "%SRCDIR%uninstall.cmd" "%DEST%\" >nul
copy /y "%SRCDIR%README.txt" "%DEST%\" >nul

rem Preserve the user's watched-folders.txt if they already have one.
if not exist "%DEST%\watched-folders.txt" copy /y "%SRCDIR%watched-folders.txt" "%DEST%\" >nul

reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v SavePathCopier /t REG_SZ /d "%DEST%\SavePathCopier.exe" /f >nul

rem Stop an older copy first (single-instance mutex), then start the new one.
taskkill /im SavePathCopier.exe /f >nul 2>&1
start "" "%DEST%\SavePathCopier.exe"

echo SavePathCopier installed to %DEST% and started. It now runs at logon.
echo Save a file in Desktop, Documents, Downloads or C:\tmp and press Ctrl+V to paste its path.
exit /b 0

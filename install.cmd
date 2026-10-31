@echo off
rem Install: build (if needed) and register auto-start at logon for the current user.
rem Run install.cmd again after any rename/move of the exe.
setlocal
set EXE=C:\Projects\SavePathCopier\SavePathCopier.exe
if not exist "%EXE%" (
  echo %EXE% not found. Run build.cmd first.
  exit /b 1
)
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v SavePathCopier /t REG_SZ /d "%EXE%" /f
echo Installed. SavePathCopier will start at logon. Start it now with: start "" "%EXE%"

@echo off
rem Removes SavePathCopier: stops it, deletes the logon auto-start entry,
rem and deletes %LOCALAPPDATA%\SavePathCopier.
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v SavePathCopier /f >nul 2>&1
taskkill /im SavePathCopier.exe /f >nul 2>&1
timeout /t 1 /nobreak >nul
rd /s /q "%LOCALAPPDATA%\SavePathCopier"
echo SavePathCopier removed.

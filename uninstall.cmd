@echo off
rem Remove the logon auto-start entry.
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v SavePathCopier /f
echo Removed. The app will no longer start at logon (kill a running copy in the tray menu).

@echo off
rem Builds the self-extracting installer SavePathCopier-Setup.exe.
rem Prerequisites: build.cmd has produced SavePathCopier.exe.
setlocal
set EXE=%~dp0SavePathCopier.exe
if not exist "%EXE%" (
  echo SavePathCopier.exe not found. Run build.cmd first.
  exit /b 1
)
copy /y "%EXE%" "%~dp0package\" >nul
iexpress /N /Q "%~dp0SavePathCopier.sed"
if errorlevel 1 (echo IExpress build failed & exit /b 1)
if not exist "%~dp0SavePathCopier-Setup.exe" (
  echo IExpress reported success but no output found & exit /b 1
)
echo Built %~dp0SavePathCopier-Setup.exe

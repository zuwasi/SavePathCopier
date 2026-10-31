@echo off
rem Build SavePathCopier.exe with the .NET Framework 4 compiler (no SDK needed).
setlocal
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" (
  echo .NET Framework 4 compiler not found at %CSC%
  exit /b 1
)
"%CSC%" /nologo /target:winexe /out:"%~dp0SavePathCopier.exe" ^
  /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll ^
  "%~dp0SavePathCopier.cs"
if errorlevel 1 (echo Build failed & exit /b 1)
echo Built %~dp0SavePathCopier.exe

# SavePathCopier

A tiny Windows tray utility. Every time a file is saved in a watched folder,
the file's **full path** is copied to the clipboard as plain text, so:

- **Ctrl+V** in Notepad pastes a line like `C:\tmp\sample.docx`
- **Win+V** shows the path in clipboard history (enable history once in
  Settings > System > Clipboard if you have not already)

## Install (recommended: self-extracting installer)

1. Download `SavePathCopier-Setup.exe` from the
   [latest release](https://github.com/zuwasi/SavePathCopier/releases/latest)
   (full URL: https://github.com/zuwasi/SavePathCopier/releases/latest)
2. Run it. It installs to `%LOCALAPPDATA%\SavePathCopier`, registers
   logon auto-start for your user, starts the app, and shows a tray icon.
3. Save a file in Desktop, Documents, Downloads or `C:\tmp`, then press
   **Ctrl+V** anywhere: the file's full path is pasted.

Uninstall: run `uninstall.cmd` in `%LOCALAPPDATA%\SavePathCopier`, or just
delete the folder and remove the `SavePathCopier` value from
`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`.

## Why not a Windows service?

A real Windows service runs in session 0 and cannot access your desktop
clipboard by design. This app is the standard equivalent: a per-user tray
application registered to auto-start at logon.

## Watched folders (defaults)

- your Desktop (OneDrive-redirected if applicable)
- your Documents folder
- your Downloads folder
- `C:\tmp`

Edit `watched-folders.txt` next to the exe to add or remove folders
(subfolders are always included). Right-click the tray icon and choose
**Reload watched folders** after editing.

## Skipped files

Lock/temp files are ignored so autosave noise does not flood the clipboard:
`~$*` (Office), `.*` dotfiles, and `.tmp`, `.temp`, `.crdownload`, `.part`,
`.partial` extensions. A save is only registered after the file has been
quiet for ~0.9 seconds, so multi-write saves copy the path exactly once.
Note that Word/Excel autosave counts as a save and will update the clipboard.

## Building from source

| File | Purpose |
|---|---|
| `SavePathCopier.cs` | C# source (.NET Framework 4, no dependencies) |
| `build.cmd` | Compiles `SavePathCopier.exe` with the built-in csc compiler |
| `make-setup.cmd` | Builds `SavePathCopier-Setup.exe` (IExpress SFX installer) |
| `package/` | Payload for the SFX: installer, uninstaller, readme |
| `SavePathCopier.sed` | IExpress package definition |

1. Run `build.cmd` (uses `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe`)
2. Run `make-setup.cmd` to produce `SavePathCopier-Setup.exe`

## License

MIT, see [LICENSE](LICENSE).

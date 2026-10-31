SavePathCopier
==============

Every time you save a file in a watched folder, the file's full path is
copied to the clipboard as plain text.

  - Ctrl+V in Notepad pastes a line like: C:\tmp\sample.docx
  - Win+V shows the path in clipboard history
    (enable once in Settings > System > Clipboard)

Watched folders by default (subfolders included):
  - your Desktop (including OneDrive Desktop)
  - your Documents folder
  - your Downloads folder
  - C:\tmp

To change the list, edit watched-folders.txt in this folder, then
right-click the tray icon and choose "Reload watched folders".

This app runs at logon (tray icon). A real Windows service cannot touch
your clipboard, so logon auto-start is used instead.

Uninstall: run uninstall.cmd in this folder.

Source: https://github.com/zuwasi/SavePathCopier
License: MIT

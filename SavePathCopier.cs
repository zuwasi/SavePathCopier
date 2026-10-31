// SavePathCopier
// Watches configured folders for saved files and copies the saved file's
// full path to the Windows clipboard, so Ctrl+V (and Win+V history) gets
// a line like: C:\tmp\sample.docx
//
// Runs in the user session as a tray application (a session-0 Windows
// service cannot access the user clipboard). Auto-starts at logon via a
// HKCU Run registry entry created by install.cmd.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SavePathCopier
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool createdNew;
            using (new Mutex(true, "SavePathCopier_SingleInstance", out createdNew))
            {
                if (!createdNew) return; // already running
                Application.EnableVisualStyles();
                Application.Run(new TrayContext());
            }
        }
    }

    internal class TrayContext : ApplicationContext
    {
        // How long to wait after the last write event before treating the
        // file as finished saving. Apps write files in several chunks and
        // we only want to copy the path once, after the save completes.
        private const int QuietPeriodMs = 900;

        private readonly string _configPath;
        private readonly string[] _skipExtensions = { ".tmp", ".temp", ".crdownload", ".part", ".partial" };

        private readonly object _gate = new object();
        private readonly Dictionary<string, DateTime> _pending = new Dictionary<string, DateTime>();
        private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
        private readonly System.Windows.Forms.Timer _pollTimer;
        private readonly NotifyIcon _icon;

        public TrayContext()
        {
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            _configPath = Path.Combine(exeDir, "watched-folders.txt");

            EnsureConfigExists();

            _pollTimer = new System.Windows.Forms.Timer { Interval = 400 };
            _pollTimer.Tick += PollTimerTick;
            _pollTimer.Start();

            var menu = new ContextMenu();
            menu.MenuItems.Add(new MenuItem("Reload watched folders", delegate { ReloadFolders(); }));
            menu.MenuItems.Add(new MenuItem("Edit watched folders...", delegate
            {
                try { Process.Start("notepad.exe", _configPath); }
                catch { }
            }));
            menu.MenuItems.Add(new MenuItem("Exit", delegate
            {
                _icon.Visible = false;
                Application.Exit();
            }));

            _icon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Text = "Save Path Copier",
                ContextMenu = menu,
                Visible = true
            };

            StartWatching();
        }

        // ---- folder watching ------------------------------------------------

        private void ReloadFolders()
        {
            foreach (FileSystemWatcher w in _watchers)
            {
                try { w.EnableRaisingEvents = false; w.Dispose(); } catch { }
            }
            _watchers.Clear();
            StartWatching();
        }

        private List<string> ReadWatchedFolders()
        {
            var folders = new List<string>();
            foreach (string raw in File.ReadAllLines(_configPath, Encoding.Default))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                string expanded = Environment.ExpandEnvironmentVariables(line);
                try
                {
                    if (Directory.Exists(expanded) && !folders.Contains(expanded))
                        folders.Add(expanded);
                }
                catch { }
            }
            return folders;
        }

        private void StartWatching()
        {
            List<string> folders = ReadWatchedFolders();
            _icon.Text = folders.Count == 1
                ? "Save Path Copier (1 folder)"
                : "Save Path Copier (" + folders.Count + " folders)";

            foreach (string folder in folders)
            {
                var watcher = new FileSystemWatcher
                {
                    Path = folder,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
                    InternalBufferSize = 64 * 1024
                };
                watcher.Created += FileEvent;
                watcher.Changed += FileEvent;
                watcher.Renamed += FileRenamed; // apps like Word save by renaming a temp file over the target
                watcher.Error += delegate { };  // buffer overflow: ignore, next event cycle catches up
                watcher.EnableRaisingEvents = true;
                _watchers.Add(watcher);
            }
        }

        private void FileEvent(object sender, FileSystemEventArgs e)
        {
            RecordPending(e.FullPath);
        }

        private void FileRenamed(object sender, RenamedEventArgs e)
        {
            RecordPending(e.FullPath);
        }

        private void RecordPending(string path)
        {
            lock (_gate) { _pending[path] = DateTime.UtcNow; }
        }

        // ---- deferred processing (debounce) ---------------------------------

        private void PollTimerTick(object sender, EventArgs e)
        {
            List<string> ready = new List<string>();
            DateTime now = DateTime.UtcNow;

            lock (_gate)
            {
                List<string> stale = new List<string>();
                foreach (KeyValuePair<string, DateTime> kv in _pending)
                {
                    if ((now - kv.Value).TotalMilliseconds >= QuietPeriodMs)
                    {
                        ready.Add(kv.Key);
                        stale.Add(kv.Key);
                    }
                }
                foreach (string s in stale) _pending.Remove(s);
            }

            foreach (string path in ready) ProcessSavedFile(path);
        }

        private void ProcessSavedFile(string path)
        {
            try
            {
                if (!File.Exists(path)) return; // ignore directories and deleted files

                FileInfo fi = new FileInfo(path);
                FileAttributes attrs = fi.Attributes;
                if ((attrs & FileAttributes.Hidden) != 0 || (attrs & FileAttributes.System) != 0) return;

                string name = fi.Name;
                if (name.StartsWith("~$") || name.StartsWith(".")) return; // Office/LibreOffice lock files, dotfiles
                if (_skipExtensions.Contains(fi.Extension.ToLowerInvariant())) return;

                SetClipboardText(fi.FullName);
            }
            catch { }
        }

        // ---- clipboard -------------------------------------------------------

        private void SetClipboardText(string text)
        {
            Thread t = new Thread(delegate()
            {
                try { Clipboard.SetText(text); }
                catch { }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join(1000);
        }

        // ---- config -----------------------------------------------------------

        private void EnsureConfigExists()
        {
            if (File.Exists(_configPath)) return;

            var lines = new List<string>();
            lines.Add("# Save Path Copier: one folder per line. Lines starting with # are ignored.");
            lines.Add("# Saving any file in these folders (including subfolders) puts the file's");
            lines.Add("# full path on the clipboard. Edit, then use the tray icon menu to reload.");
            lines.Add(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            lines.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            lines.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"));
            if (Directory.Exists(@"C:\tmp")) lines.Add(@"C:\tmp");

            File.WriteAllLines(_configPath, lines.ToArray(), Encoding.Default);
        }
    }
}

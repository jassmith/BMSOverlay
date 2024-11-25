using System.Reflection;
using System.Runtime.InteropServices;
using BMSOverlay.Input;
using BMSOverlay.Menu;
using Microsoft.Win32;

namespace BMSOverlay
{
    internal static class Program
    {
        private static NotifyIcon? notifyIcon;
        private static InputManager? inputManager;
        private static MenuManager? menuManager;
        private static OverlayWindow? overlayWindow;

        static bool IsDarkMode()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                if (key?.GetValue("AppsUseLightTheme") is int value)
                {
                    return value == 0; // 0 = Dark mode, 1 = Light mode
                }
            }
            return false; // Default to light mode if the key doesn't exist
        }

        static void MonitorThemeChanges()
        {
            SystemEvents.UserPreferenceChanged += (sender, args) =>
            {
                if (args.Category == UserPreferenceCategory.General)
                {
                    UpdateNotifyIcon();
                }
            };
        }

        static void UpdateNotifyIcon()
        {
            string iconResourceName = IsDarkMode() ? "BMSOverlay.Resources.bmsoverlay-dark.ico" : "BMSOverlay.Resources.bmsoverlay.ico";

            using (Stream? iconStream = Assembly.GetEntryAssembly()?.GetManifestResourceStream(iconResourceName))
            {
                if (iconStream != null)
                {
                    Icon newIcon = new Icon(iconStream);
                    if (notifyIcon != null)
                        notifyIcon.Icon = newIcon;
                }
                else
                {
                    MessageBox.Show($"Icon resource '{iconResourceName}' not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        private const int ATTACH_PARENT_PROCESS = -1;

        [STAThread]
        static void Main()
        {
            AttachConsole(ATTACH_PARENT_PROCESS);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            inputManager = new InputManager();
            menuManager = new MenuManager();

            menuManager.LoadMenu("menu.json");

            overlayWindow = new OverlayWindow(menuManager);

            overlayWindow.Start();

            inputManager.OnUp += () => menuManager.NavigateUp();
            inputManager.OnDown += () => menuManager.NavigateDown();
            inputManager.OnLeft += () => menuManager.NavigateLeft();
            inputManager.OnRight += () => menuManager.NavigateRight();
            inputManager.OnSelect += () => menuManager.Select();

            inputManager.Initialize();

            string iconResourceName = IsDarkMode() ? "BMSOverlay.Resources.bmsoverlay-dark.ico" : "BMSOverlay.Resources.bmsoverlay.ico";

            Icon trayIcon;
            using (Stream? iconStream = Assembly.GetEntryAssembly()?.GetManifestResourceStream(iconResourceName))
            {
                if (iconStream == null)
                {
                    MessageBox.Show($"Icon resource '{iconResourceName}' not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                trayIcon = new Icon(iconStream);
            }

            notifyIcon = new NotifyIcon
            {
                Icon = trayIcon,
                Visible = true,
                Text = "BMS Overlay"
            };

            var contextMenu = new ContextMenuStrip();
            var exitMenuItem = new ToolStripMenuItem("Exit", null, Exit);

            contextMenu.Items.Add(exitMenuItem);

            notifyIcon.ContextMenuStrip = contextMenu;

            MonitorThemeChanges();

            Application.Run();

            notifyIcon.Dispose();
            inputManager.Dispose();
            overlayWindow.Dispose();
        }

        private static void Exit(object? sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

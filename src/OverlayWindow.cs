using GameOverlay.Drawing;
using GameOverlay.Windows;
using BMSOverlay.Menu;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace BMSOverlay
{
    public class OverlayWindow : IDisposable
    {
        private readonly GameOverlay.Drawing.Graphics _graphics;
        private GraphicsWindow _window;
        private readonly MenuManager _menuManager;

        private GameOverlay.Drawing.SolidBrush _backgroundBrush;
        private GameOverlay.Drawing.SolidBrush _menuItemBrush;
        private GameOverlay.Drawing.SolidBrush _selectedItemBrush;
        private GameOverlay.Drawing.SolidBrush _selectedItemBGBrush;
        private GameOverlay.Drawing.Font _font;

        private Thread? _windowThread;

        public OverlayWindow(MenuManager menuManager)
        {
            _menuManager = menuManager;

            _graphics = new GameOverlay.Drawing.Graphics()
            {
                VSync = true,
                MeasureFPS = false,
                PerPrimitiveAntiAliasing = true,
                TextAntiAliasing = true
            };

            CreateOverlayWindow();

            menuManager.OnMenuVisibilityChanged += (isVisible) =>
            {
                if (_window != null)
                    _window.IsVisible = isVisible;
            };

            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        }

        private void CreateOverlayWindow()
        {
            int screenWidth = GetPrimaryMonitorWidth();
            int screenHeight = GetPrimaryMonitorHeight();

            _window = new GraphicsWindow(0, 0, screenWidth, screenHeight, _graphics)
            {
                FPS = 60,
                IsTopmost = true,
                IsVisible = _menuManager.IsMenuVisible
            };

            // Set up event handlers
            _window.SetupGraphics += SetupGraphics;
            _window.DestroyGraphics += DestroyGraphics;
            _window.DrawGraphics += DrawGraphics;
        }

        public void Start()
        {
            _windowThread = new Thread(() =>
            {
                _window.Create();
                _window.Join();
            })
            {
                IsBackground = true
            };
            _windowThread.SetApartmentState(ApartmentState.STA);
            _windowThread.Start();
        }

        private void SetupGraphics(object? sender, SetupGraphicsEventArgs e)
        {
            var gfx = e.Graphics;

            _backgroundBrush = gfx.CreateSolidBrush(0, 0, 0, 0); // Transparent background
            _menuItemBrush = gfx.CreateSolidBrush(255, 255, 255); // White color
            _selectedItemBrush = gfx.CreateSolidBrush(230, 221, 124); // Yellow color
            _selectedItemBGBrush = gfx.CreateSolidBrush(0, 0, 0, 128); // Semi-transparent black background
            _font = gfx.CreateFont("Cascadia Code", 18);
        }

        private void DestroyGraphics(object? sender, DestroyGraphicsEventArgs e)
        {
            _font?.Dispose();
            _backgroundBrush?.Dispose();
            _menuItemBrush?.Dispose();
            _selectedItemBrush?.Dispose();
            _selectedItemBGBrush?.Dispose();
        }

        private void DrawGraphics(object? sender, DrawGraphicsEventArgs e)
        {
            var gfx = e.Graphics;

            gfx.ClearScene(_backgroundBrush);

            var currentMenu = _menuManager.GetCurrentMenu();

            if (currentMenu == null || !_menuManager.IsMenuVisible)
                return;

            float startX = 20;
            float startY = 20;
            float lineHeight = 24;

            for (int i = 0; i < currentMenu.Submenu.Count; i++)
            {
                var item = currentMenu.Submenu[i];

                if (i == _menuManager.CurrentSelection)
                {
                    gfx.DrawTextWithBackground(_font, _selectedItemBrush, _selectedItemBGBrush, startX, startY + (i * lineHeight), item.Label);
                }
                else
                {
                    gfx.DrawText(_font, _menuItemBrush, startX, startY + (i * lineHeight), item.Label);
                }
            }
        }

        private void OnDisplaySettingsChanged(object? sender, EventArgs e)
        {
            UpdateWindowSizeAndPosition();
        }

        private void UpdateWindowSizeAndPosition()
        {
            if (_window == null)
                return;

            int screenWidth = GetPrimaryMonitorWidth();
            int screenHeight = GetPrimaryMonitorHeight();

            _window.Resize(screenWidth, screenHeight);
            _window.Move(0, 0);

            _window.Recreate();
        }

        private int GetPrimaryMonitorWidth()
        {
            return GetSystemMetrics(SystemMetric.SM_CXSCREEN);
        }

        private int GetPrimaryMonitorHeight()
        {
            return GetSystemMetrics(SystemMetric.SM_CYSCREEN);
        }

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(SystemMetric smIndex);

        private enum SystemMetric
        {
            SM_CXSCREEN = 0,
            SM_CYSCREEN = 1
        }

        public void Dispose()
        {
            SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;

            _window?.Dispose();
            _graphics?.Dispose();

            if (_windowThread != null && _windowThread.IsAlive)
            {
                _windowThread.Join();
                _windowThread = null;
            }
        }
    }
}

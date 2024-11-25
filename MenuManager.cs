using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using InputSimulatorStandard;
using InputSimulatorStandard.Native;

namespace BMSOverlay.Menu
{
    public class MenuManager
    {
        public MenuItem? RootMenu { get; private set; }
        public Stack<MenuItem> MenuStack { get; private set; }
        public int CurrentSelection { get; private set; }

        private bool _isMenuVisible;
        public bool IsMenuVisible
        {
            get
            {
                return _isMenuVisible;
            }
            private set
            {
                _isMenuVisible = value;
                OnMenuVisibilityChanged?.Invoke(value);
            }
        }

        private InputSimulator inputSimulator;

        public event Action<bool>? OnMenuVisibilityChanged;

        public MenuManager()
        {
            MenuStack = new Stack<MenuItem>();
            inputSimulator = new InputSimulator();
            IsMenuVisible = false; // Menu is closed by default
        }

        public void LoadMenu(string configFileName)
        {
            string configPath = ConfigFileUtils.GetConfigPath(configFileName);

            if (!File.Exists(configPath))
            {
                Console.WriteLine($"Menu configuration file not found at {configPath}");
                return;
            }

            var json = File.ReadAllText(configPath);
            if (string.IsNullOrEmpty(json))
            {
                Console.WriteLine("Failed to read menu configuration file");
                return;
            }
            RootMenu = JsonConvert.DeserializeObject<MenuItem>(json);
            CurrentSelection = 0;
        }

        public void OpenMenu()
        {
            if (!IsMenuVisible)
            {
                IsMenuVisible = true;
                MenuStack.Clear();
                if (RootMenu != null)
                    MenuStack.Push(RootMenu);
                CurrentSelection = 0;
            }
        }

        public void CloseMenu()
        {
            if (IsMenuVisible)
            {
                IsMenuVisible = false;
            }
        }

        public MenuItem? GetCurrentMenu()
        {
            if (MenuStack.Count > 0)
                return MenuStack.Peek();
            else
                return null;
        }

        private bool EnsureOpen()
        {
            if (!IsMenuVisible)
            {
                OpenMenu();
                return true;
            }
            return false;
        }

        public void NavigateUp()
        {
            if (EnsureOpen()) return;

            var currentMenu = GetCurrentMenu();
            if (currentMenu != null)
                CurrentSelection = (CurrentSelection - 1 + currentMenu.Submenu.Count) % currentMenu.Submenu.Count;
        }

        public void NavigateDown()
        {
            if (EnsureOpen()) return;

            var currentMenu = GetCurrentMenu();
            if (currentMenu != null)
                CurrentSelection = (CurrentSelection + 1) % currentMenu.Submenu.Count;
        }

        public void NavigateLeft()
        {
            if (EnsureOpen()) return;

            GoBack();
        }

        public void NavigateRight()
        {
            if (EnsureOpen()) return;

            Select();
        }

        public void Select()
        {
            var currentMenu = GetCurrentMenu();
            if (currentMenu == null || currentMenu.Submenu == null || currentMenu.Submenu.Count == 0)
                return;

            var selectedItem = currentMenu.Submenu[CurrentSelection];
            if (selectedItem.Submenu != null && selectedItem.Submenu.Count > 0)
            {
                // Navigate into submenu
                MenuStack.Push(selectedItem);
                CurrentSelection = 0;
            }
            ExecuteAction(selectedItem);
        }

        public void GoBack()
        {
            if (MenuStack.Count > 1)
            {
                var currentMenu = GetCurrentMenu();
                if (currentMenu != null && currentMenu.ExitKey != null)
                {
                    inputSimulator.Keyboard.KeyPress((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), currentMenu.ExitKey));
                }
                MenuStack.Pop();
                CurrentSelection = 0;
            }
            else
            {
                CloseMenu();
            }
        }

        private async void ExecuteAction(MenuItem item)
        {
            if (item.Key != null || item.Keys != null)
            {
                if (item.Keys != null && item.Keys.Count > 0)
                {
                    // Handle sequence of keys
                    foreach (var keyStr in item.Keys)
                    {
                        if (Enum.TryParse(keyStr, true, out VirtualKeyCode keyCode))
                        {
                            inputSimulator.Keyboard.KeyPress(keyCode);
                            await Task.Delay(20); // 20ms delay between keys
                        }
                        else
                        {
                            Console.WriteLine($"Invalid key: {keyStr}");
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(item.Key))
                {
                    // Handle single key
                    if (Enum.TryParse<VirtualKeyCode>(item.Key, true, out VirtualKeyCode keyCode))
                    {
                        inputSimulator.Keyboard.KeyPress(keyCode);
                    }
                    else
                    {
                        Console.WriteLine($"Invalid key: {item.Key}");
                    }
                }
            }

            if (item.CloseMenuAfterAction)
            {
                CloseMenu();
            }
        }
    }
}

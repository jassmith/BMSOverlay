using System.Text.Json;
using SharpDX.DirectInput;

namespace BMSOverlay.Input
{
    public class InputManager : IDisposable
    {
        private DirectInput directInput;
        private Joystick? joystick;
        private Thread? inputThread;
        private bool isRunning;
        private JoystickConfig? joystickConfig;

        // Events for navigation
        public event Action? OnUp;
        public event Action? OnDown;
        public event Action? OnLeft;
        public event Action? OnRight;
        public event Action? OnSelect;

        public InputManager()
        {
            directInput = new DirectInput();
        }

        public void Initialize()
        {
            // Load joystick configuration
            string configPath = ConfigFileUtils.GetConfigPath("joystick.json");
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    joystickConfig = JsonSerializer.Deserialize<JoystickConfig>(json);
                }
            }

            if (joystickConfig == null || string.IsNullOrWhiteSpace(joystickConfig.JoystickGUID))
            {
                // No valid config, list available joysticks
                Console.WriteLine("No valid joystick configuration found.");
                Console.WriteLine("Available joysticks:");

                // Define a list of device types to include
                var deviceTypes = new DeviceType[]
                {
                    DeviceType.Gamepad,
                    DeviceType.Joystick,
                    DeviceType.FirstPerson,
                    DeviceType.Flight,
                    DeviceType.ControlDevice,
                    DeviceType.Remote,
                    DeviceType.Supplemental
                };

                var devicesListed = false;

                foreach (var deviceType in deviceTypes)
                {
                    var devices = directInput.GetDevices(deviceType, DeviceEnumerationFlags.AttachedOnly);
                    foreach (var deviceInstance in devices)
                    {
                        devicesListed = true;
                        Console.WriteLine($"Name: {deviceInstance.InstanceName}, GUID: {deviceInstance.InstanceGuid}, Type: {deviceInstance.Type}");
                    }
                }

                if (!devicesListed)
                {
                    Console.WriteLine("No joysticks or game controllers found.");
                }

                return;
            }

            // Find the joystick specified in the config
            if (!Guid.TryParse(joystickConfig.JoystickGUID, out Guid joystickGuid))
            {
                Console.WriteLine("Invalid JoystickGUID in configuration.");
                return;
            }

            try
            {
                joystick = new Joystick(directInput, joystickGuid);
            }
            catch
            {
                Console.WriteLine("Joystick with specified GUID not found.");
                return;
            }

            Console.WriteLine("Using Joystick: " + joystick.Information.ProductName);

            // Acquire the joystick
            joystick.Properties.BufferSize = 128;
            joystick.Acquire();

            // Start the input reading thread
            isRunning = true;
            inputThread = new Thread(new ThreadStart(PollInput))
            {
                IsBackground = true
            };
            inputThread.Start();
        }

        private void PollInput()
        {
            while (isRunning)
            {
                if (joystick == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                joystick.Poll();
                var datas = joystick.GetBufferedData();
                foreach (var state in datas)
                {
                    HandleInput(state);
                }

                Thread.Sleep(10); // Adjust as needed
            }
        }

        private void HandleInput(JoystickUpdate state)
        {
            // Map the joystick input to actions based on the config
            if (joystickConfig != null && joystickConfig.ButtonMappings != null)
            {
                foreach (var mapping in joystickConfig.ButtonMappings)
                {
                    if (IsMappingMatched(mapping.Value, state))
                    {
                        InvokeAction(mapping.Key);
                    }
                }
            }
        }

        private Dictionary<JoystickOffset, int> previousButtonStates = new Dictionary<JoystickOffset, int>();
        private Dictionary<JoystickOffset, int> previousPovStates = new Dictionary<JoystickOffset, int>();

        private bool IsMappingMatched(string mappingValue, JoystickUpdate state)
        {
            if (mappingValue.StartsWith("Button"))
            {
                // Mapping to a button
                if (int.TryParse(mappingValue.Substring(6), out int buttonIndex))
                {
                    JoystickOffset buttonOffset = JoystickOffset.Buttons0 + buttonIndex;
                    if (state.Offset == buttonOffset)
                    {
                        int previousValue = previousButtonStates.ContainsKey(buttonOffset) ? previousButtonStates[buttonOffset] : 0;

                        // Button pressed (transition from 0 to 128)
                        if (previousValue == 0 && state.Value != 0)
                        {
                            previousButtonStates[buttonOffset] = state.Value;
                            return true;
                        }

                        previousButtonStates[buttonOffset] = state.Value;
                    }
                }
            }
            else if (mappingValue.StartsWith("POV"))
            {
                // Mapping to a POV direction
                // Format: POV0_Up, POV0_Down, POV0_Left, POV0_Right
                string[] parts = mappingValue.Split('_');
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[0].Substring(3), out int povIndex))
                    {
                        string direction = parts[1];

                        JoystickOffset povOffset = JoystickOffset.PointOfViewControllers0 + povIndex;
                        if (state.Offset == povOffset)
                        {
                            int previousValue = previousPovStates.ContainsKey(povOffset) ? previousPovStates[povOffset] : -1;

                            // Transition from not pressed to pressed
                            if (previousValue != GetPovValue(direction) && state.Value == GetPovValue(direction))
                            {
                                previousPovStates[povOffset] = state.Value;
                                return true;
                            }

                            previousPovStates[povOffset] = state.Value;
                        }
                    }
                }
            }

            return false;
        }

        private int GetPovValue(string direction)
        {
            return direction switch
            {
                "Up" => 0,
                "Right" => 9000,
                "Down" => 18000,
                "Left" => 27000,
                _ => -1
            };
        }
        private void InvokeAction(string actionName)
        {
            switch (actionName)
            {
                case "Up":
                    OnUp?.Invoke();
                    break;
                case "Down":
                    OnDown?.Invoke();
                    break;
                case "Left":
                    OnLeft?.Invoke();
                    break;
                case "Right":
                    OnRight?.Invoke();
                    break;
                case "Select":
                    OnSelect?.Invoke();
                    break;
                default:
                    Console.WriteLine($"Unknown action: {actionName}");
                    break;
            }
        }

        public void Dispose()
        {
            isRunning = false;
            inputThread?.Join();

            joystick?.Unacquire();
            joystick?.Dispose();
            directInput?.Dispose();
        }
    }
}

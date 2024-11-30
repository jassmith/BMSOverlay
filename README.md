# BMSOverlay

BMSOverlay is a customizable joystick menu overlay application designed to enhance your simulation or gaming experience.
It allows you to create hierarchical menus that can be navigated using a joystick or game controller, triggering actions
such as key presses or sequences of keys. The overlay is displayed on top of all applications and can be customized to
suit your needs.

## Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
  - [Menu Configuration (`menu.json`)](#menu-configuration-menujson)
  - [Joystick Configuration (`joystick.json`)](#joystick-configuration-joystickjson)
- [Usage](#usage)
- [Building from Source](#building-from-source)
- [License](#license)
- [Acknowledgements](#acknowledgements)

---

## Features

- **Customizable Menus**: Define your own menu structure and actions using JSON configuration files.
- **Joystick Integration**: Navigate menus and trigger actions using joystick buttons and hats.
- **Overlay Display**: Menus are displayed as an overlay on your screen without interrupting your current application.
- **System Tray Icon**: Runs quietly in the system tray with options to exit the application.
- **Self-Contained Executable**: Easy to distribute and run without the need for additional installations.

---

## Prerequisites

- **Windows 10 or higher**: The application is designed for Windows systems.
- **.NET Runtime**: Not required if using the self-contained executable.

---

## Installation

1. **Download the Application**:

   - [Download the latest release](#) of `BMSOverlay.exe`.

2. **Place Configuration Files**:

   - Create a directory at `%LocalAppData%\BMSOverlay`.
   - Place your `menu.json` and `joystick.json` configuration files in this directory. (See [Configuration](#configuration) for details.)

3. **Run the Application**:

   - Double-click `BMSOverlay.exe` to start the application.
   - The application will run in the background and display a system tray icon.

---

## Configuration

BMSOverlay uses two configuration files to customize the menu and joystick settings:

- `menu.json`: Defines the menu structure and actions.
- `joystick.json`: Configures the joystick buttons and axes used for navigation.

Both files should be placed in `%AppData%\BMSOverlay`.

### Menu Configuration (`menu.json`)

The `menu.json` file defines your menu's structure, labels, and actions. Below is an example configuration:

```json
{
  "Label": "Main Menu",
  "Submenu": [
    {
      "Label": "Repeatable Action",
      "Action": "key_press",
      "Keys": ["A", "B", "C"],
      "CloseMenuAfterAction": false
    },
    {
      "Label": "Single Action",
      "Action": "key_press",
      "Key": "SPACE",
      "CloseMenuAfterAction": true
    }
  ]
}
```

#### Menu Item Properties

- **Label**: The text displayed for the menu item.
- **Action**: The action to perform (e.g., `"key_press"`).
- **Key**: A single key to press (uses `VirtualKeyCode` enumeration).
- **Keys**: A sequence of keys to press with a 20ms delay between each.
- **CloseMenuAfterAction**: Determines if the menu closes after the action (`true` or `false`).
- **Submenu**: An array of child menu items for nested menus.

### Joystick Configuration (`joystick.json`)

The `joystick.json` file configures which joystick buttons and axes correspond to menu navigation and selection.

```json
{
  "JoystickGUID": "YOUR-JOYSTICK-GUID",
  "UpButton": 1,
  "DownButton": 2,
  "LeftButton": 3,
  "RightButton": 4,
  "SelectButton": 5,
  "MenuToggleButton": 6
}
```

#### Joystick Configuration Properties

- **JoystickGUID**: The unique identifier for your joystick. You can obtain this by running the application without a `joystick.json` file, and it will list available joysticks.
- **UpButton / DownButton / LeftButton / RightButton**: The button numbers for navigating the menu.
- **SelectButton**: The button number for selecting a menu item.
- **MenuToggleButton**: The button number to toggle the menu's visibility.

---

## Usage

1. **Start the Application**:

   - Run `BMSOverlay.exe`. The application will appear in the system tray.

2. **Toggle the Menu**:

   - Press any of the bound buttons to toggle the menu.

3. **Navigate the Menu**:

   - Use the configured joystick buttons to navigate up, down, left, or right through the menu.

4. **Select a Menu Item**:

   - Press the right button to activate the highlighted menu item.

5. **Trigger Actions**:

   - Menu items will perform the configured actions, such as pressing keys or sequences of keys.

6. **Exit the Application**:

   - Right-click the system tray icon and select **Exit** to close the application.

---

## Building from Source

If you wish to build the application from source, follow these steps:

### Prerequisites

- **.NET 6 SDK**: [Download and install](https://dotnet.microsoft.com/download/dotnet/6.0).
- **Visual Studio Code** or **Visual Studio 2022**: Optional, for editing and building the project.
- **Git**: To clone the repository.

### Steps

1. **Clone the Repository**:

   ```bash
   git clone https://github.com/yourusername/BMSOverlay.git
   ```

2. **Navigate to the Project Directory**:

   ```bash
   cd BMSOverlay
   ```

3. **Restore Dependencies**:

   ```bash
   dotnet restore
   ```

4. **Build the Application**:

   ```bash
   dotnet build -c Release
   ```

5. **Publish as a Self-Contained Executable**:

   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true -o publish
   ```

   - The executable will be located in the `publish` directory.

---

## License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## Acknowledgements

- **[GameOverlay.Net](https://github.com/michel-pi/GameOverlay.Net)**: For providing the overlay functionality.
- **[SharpDX](https://github.com/sharpdx/SharpDX)**: Used for joystick input handling.
- **[InputSimulatorStandard](https://github.com/GregsStack/InputSimulatorStandard)**: For simulating keyboard input.
- **[Newtonsoft.Json](https://www.newtonsoft.com/json)**: For JSON serialization and deserialization.

---

## Future Enhancements

- **Configuration UI**: Develop a user interface for editing the configuration files.
- **Additional Actions**: Support more action types, such as launching applications or sending mouse inputs.

---

**Note**: This application is provided as-is without any guarantees. Use it at your own risk.

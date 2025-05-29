# Checkers for WPF

A fully working 1 or 2 player beginners checkers game written in for WPF in C# for .NET Core.  This app includes examples of using animations, sounds, drag and drop, events, dependency injection, data binding and general project structure.

![Screenshot](/assets/screenshot1.png)

## Getting Started

Before running the application, ensure you have the following prerequisites:

- .NET Core SDK (`9.0+`) although this should work if retargetted to any version of .NET Core after `3.1`.
- Git installed and available on your PATH.

```powershell
# Clone the repository
git clone https://github.com/blakepell/Checkers.git
cd Checkers

# Build the solution
dotnet build Checkers.sln

# Run the application
dotnet run --project src/Checkers.csproj
```

## Project Structure

The project is organized into the following directories and components:

- `src/`: main WPF application code
  - `App.xaml` / `App.xaml.cs`: application startup, resource dictionaries, and global event handlers.
  - `MainWindow.xaml` / `MainWindow.xaml.cs`: the main game window hosting the checkers board and user interactions.
- `src/Common/`:
  - `AppSettings.cs`: configuration class for persisting user preferences and application settings.
  - `GameMode.cs`: enumeration defining available game modes (single-player, two-player).
- `src/Controls/`:
  - `CheckersPiece.cs`: custom control representing individual checker pieces with drag-and-drop support.
- `src/Dialogs/`:
  - `GameModeDialog.xaml` / `GameModeDialog.xaml.cs`: modal dialog for selecting the desired game mode.
- `src/Managers/`:
  - `GameManager.cs`: core game logic, including move validation, turn management, and win detection.
  - `SoundManager.cs`: handles audio playback for moves, captures, and game events.
- `src/Assets/Audio/`: contains sound assets used by `SoundManager` (e.g., jump and capture effects).

Each component follows a clear separation of concerns to support maintainability and extensibility.

## Commentary

There are a lot of people out there using WPF but the web has lost some of its best resources over the years.  This is my attempt to put some working WPF content out there that might help someone else in putting together a WPF app.
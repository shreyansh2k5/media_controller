MiniFlyout 🎵

MiniFlyout is a sleek, native-feeling media controller widget for Windows 11. Built with C# and .NET 10, it integrates seamlessly into your taskbar, providing beautiful and non-intrusive controls for your system's currently playing media (Spotify, YouTube, Windows Media Player, etc.).

✨ Features

Ambient Artwork Glow: Extracts the dominant colors from the currently playing album art and projects a beautiful, glowing acrylic blur behind the widget (similar to YouTube's ambient mode).

Smart Visibility: The widget automatically hides itself to save memory and CPU when no media is playing, and instantly reappears when you start a track.

Native Windows 11 Aesthetics: Uses the native DWM API for hardware-accelerated rounded corners and seamless drop shadows.

Taskbar Integration: Sits flush on the bottom-left of your screen, perfectly tailored to match the height and feel of the Windows 11 taskbar.

Unobtrusive: Runs as a true background overlay (WS_EX_NOACTIVATE). Clicking the media controls will never steal focus from your active game, browser, or full-screen app.

Clean Architecture: Codebase is separated into Core, Infrastructure, and UI layers for high maintainability.

👥 How to Use (For Regular Users)

Once MiniFlyout is running, there is no setup required! It works completely automatically in the background:

Open your favorite media app (Spotify, Apple Music, Edge, Chrome, YouTube, etc.).

Start playing a song or video.

MiniFlyout will automatically appear on the bottom left of your taskbar, displaying the album artwork, track name, artist, and playback controls.

If you close your media player, MiniFlyout will gracefully vanish until you need it again.

🚀 Installation & Running (For Developers)

Prerequisites

Windows 10 or Windows 11

.NET 10 SDK

Build from Source

Clone the repository:

git clone [https://github.com/yourusername/media_controller.git](https://github.com/yourusername/media_controller.git)
cd media_controller/MiniFlyout


Build and run the project using the .NET CLI:

dotnet run


(Note: If Windows SmartScreen displays a "Windows protected your PC" warning when running the compiled .exe for the first time, simply click More info -> Run anyway. This happens because the open-source executable is not signed with an Extended Validation developer certificate).

🏗️ Architecture

The project follows a pragmatic Clean Architecture approach to separate UI from Windows OS dependencies:

Core: Contains domain models (TrackInfo) and interfaces (IMediaService).

Infrastructure: Handles the heavy lifting with the Windows OS. Contains P/Invoke wrappers (NativeMethods) and the implementation for Windows Media Transport Controls (WindowsMediaService).

UI: Contains the WinForms visual elements, native window styling (WindowEffects), dynamic polling, image color extraction (ImageUtils), and custom Fluent buttons (MainForm, StyledButton).

🛠️ Built With

C# / WinForms

.NET 10.0

Windows 10/11 SDK (net10.0-windows10.0.19041.0)
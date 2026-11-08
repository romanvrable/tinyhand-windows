# TinyHand for Windows

TinyHand puts a hand pointer on your screen so you can point things out during a
presentation or screen share. This repository is a Windows-only port of
[nilbuild/tinyhand](https://github.com/nilbuild/tinyhand), which was originally
written for macOS.

## Download

Download the latest Windows x64 or ARM64 ZIP from
[Releases](https://github.com/romanvrable/tinyhand-windows/releases/latest),
extract it, and run `TinyHand.exe`. The app appears in the notification area.
No administrator access or .NET installation is required.

## Use

- Choose a hand style in Settings, then turn the pointer on from the tray icon.
- Click and drag to draw. While the pointer is on, left clicks are captured
  rather than sent to the apps underneath.
- **Ctrl+Alt+H** toggles the pointer, **Ctrl+Alt+X** clears drawings, and
  **Esc** turns it off.
- Right-click the tray icon for Settings or Exit.

TinyHand uses a Windows input hook to capture clicks and global shortcuts. The
pointer and drawing overlay work across the virtual desktop.

## Build

Install the .NET 8 SDK and run:

```powershell
dotnet build TinyHand.Windows/TinyHand.Windows.csproj -c Release
dotnet publish TinyHand.Windows/TinyHand.Windows.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

GitHub Actions also builds self-contained Windows x64 and ARM64 downloads.

## License and attribution

MIT licensed; see [LICENSE](LICENSE). The pointer artwork and original project
are from [Tinyhand by Kamran Ahmed](https://github.com/nilbuild/tinyhand).

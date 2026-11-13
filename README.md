# SideKicker

A sliding sidebar for Windows with quick shortcuts and time tools, a quick-access helper software.

![screenshot](docs/screenshot.png)

## Features
- **Opening:** move the mouse into the screen zone, or press the global shortcut
  `AltGr` + `<`. Hover opening can be turned off in the settings.
- **Pinned commands:** save a name, a file or program, and a working directory,
  then launch it from the sidebar. You can edit and reorder the list. A command
  can open an app, a file or a folder, or start a tool in a chosen directory
- **Alarms:** a single alarm or one that repeats on chosen weekdays, with a
  message.
- **Timers and stopwatch:** timers with pause and resume, and a stopwatch with
  start, pause and stop.
- **Desktop file finder:** searches the Desktop folder and opens its contained folder.
- **Settings panel:** hover-open toggle, sidebar position, and panel order.
- **Light on resources:** ~50 MB idle, ~75 MB in use

## Requirements
- .NET 10.0
- MonoGame 3.8.0 or above (required by [PopUpUI](https://github.com/alpakca/PopUpUI))

## Build & run
1. Reference [PopUpUI](https://github.com/alpakca/PopUpUI) as a project reference.
2. Add `Icon.ico` and `Icon.bmp` to the project folder. They are not tracked in
   this repository, but the project file uses them.
3. Compile the solution as it appears in the repo.
4. Before running the executable, copy the `assets` folder into the same
   folder as the compiled executable.

## Platform
Originally built for Windows. OS-specific code sits behind a Native Services
abstraction layer, so a port is possible.

## Notes
Built solo in about 30 days (summer 2026) for personal use.
# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

iRDCav (iRacing Data Companion) is a set of transparent, always-on-top overlay windows for the iRacing sim: a standings/results view, a relative view with a fuel calculator, and a throttle/brake/clutch input trace. It is built with C# / .NET 8, Avalonia 11, CommunityToolkit.Mvvm, OxyPlot (input graph), and IRSDKSharper (iRacing SDK wrapper). The relative-gap logic is adapted from [iRon](https://github.com/lespalt/iRon/).

The app targets Windows only at runtime. `WindowTransparency.cs` P/Invokes `user32.dll` to make windows click-through and topmost, and the iRacing SDK reads shared memory that exists only while iRacing runs on Windows. The project still builds on Linux, but the overlays won't connect there.

## Commands

The solution is at `IRDCav/IRDCav.sln` and has one project, `IRDCav/IRDCav/IRDCav.csproj`.

```bash
dotnet build IRDCav/IRDCav.sln
dotnet run --project IRDCav/IRDCav/IRDCav.csproj
```

The repo has no test project and no linter configuration.

## Architecture

Data flows one way: `Telemetry/` → `Services/` → `Presentation/` (state store) ← `ViewModels/` → `Views/`. The telemetry side never calls the UI; the UI pulls.

**Composition root** (`App.axaml.cs`): this is the only place where objects get created and wired together: view models (`OverlayViewModels`), windows and their `OverlayWindowBehavior`, the state store, the input buffer, the refresher, and the `TelemetryProcessor` with its services. Everything is passed in through constructors. `ShutdownMode` is `OnExplicitShutdown`; the app exits through `ApplicationViewModel.ExitCommand` in the tray menu.

**Telemetry** (`Telemetry/`): `ITelemetrySource` wraps IRSDKSharper (`IRacingTelemetrySource`, `UpdateInterval = 2`, i.e. 30 frames/s). For each tick it raises `FrameReceived` with an `ITelemetryFrame`, whose `Read*` methods read values only on request. `IRacingTelemetryFrame` looks up the datum handles once per connection and reuses its read buffers: the list from `ReadCars()` is overwritten on the next call. `SessionInfoVersion` changes whenever the session info YAML changes.

**Services** (`Services/`), all running on the SDK thread:
- `TelemetryProcessor` pushes inputs into the `IInputSink` on every frame. Every 4th frame (`RaceDataFrameInterval`) it processes race data and publishes an immutable `OverlayState`. Anything derived from the session info YAML (driver data, the session header) is rebuilt only when `SessionInfoVersion` or the session changes.
- `RaceDataController` holds the mutable `RaceDataModel[MaxNumCars]` indexed by carIdx and merges telemetry, session results and drivers into it. `Advance(sessionTime)` runs `IntervalCalculator`, then marks the fastest lap per class. `Cars` must never leave the telemetry thread.
- `RelativeListBuilder` and `StandingsListBuilder` turn the cars into immutable `RaceRow`s; `RaceRow.Empty` is an empty display row.
- `SessionInfoBuilder` builds the header `SessionInfoModel`. `FuelMonitor` detects stints and completed laps; `FuelCalculator` computes the statistics.

**Interval calculation** (`IntervalCalculator` + `Models/MicroSectorModel.cs`): each car splits a lap into 20 microsectors. Elapsed `SessionTime` is added to the sector being driven. When a tick crosses a sector boundary, the time is split by the distance driven on each side of it. The best time of each sector is kept, and their sum is the optimal lap. Teleports, driving backwards and large time steps discard the sector in progress. Once all sectors are recorded, the interval is the time this car needs, on its optimal lap, to drive from the player's position to its own (positive = ahead). Before that, and while the car is approaching or in the pits or has taken the checkered flag, `EstimateInterval` scales the lap-distance difference by the best or estimated lap time. `LapDelta` comes from `CarIdxLap + CarIdxLapDistPct`.

**Decoupling and threading** (`Presentation/`): the producer side writes into two thread-safe structures and never blocks on the UI:
- `OverlayStateStore`: a slot that keeps only the latest `OverlayState`, written with `Volatile`. A state the UI misses is simply replaced, so nothing can queue up.
- `InputTraceBuffer`: a fixed-size ring buffer that takes a short lock and has a version counter.

`OverlayRefresher` runs a `DispatcherTimer` on the UI thread at background priority (~30 Hz). It pulls the latest values and does nothing when neither the state reference nor the input version changed. Keep it this way: never touch view models from the telemetry thread, and publish only immutable data.

**View models** use CommunityToolkit `[ObservableProperty]`. The list views bind to a `RaceRowCollection` of reused `RaceRowViewModel`s that are updated in place, so only the cells that changed re-render. Don't replace the collections. Compiled bindings are on by default (`AvaloniaUseCompiledBindingsByDefault`), so XAML bindings and `DataTemplate`s need `x:DataType`. Value converters are in `Converters/` and derive from `OneWayConverter`. The input graph is drawn by `Views/InputTraceControl`, which renders polylines directly; there's no chart library.

**Overlay windows and layout** (`Layout/`, `Views/OverlayWindowBehavior.cs`):
- **Window setup:** each window calls `WindowTransparency.ToTransparentWindow(this)` (Win32 click-through and topmost) before `InitializeComponent()`. Its content must be wrapped in a `LayoutTransformControl`, with the design size set on the inner element (`SizeToContent="WidthAndHeight"`).
- **Placement:** `OverlayPlacement` anchors a window point (start, center or end on each axis) to the same relative point on the screen, plus an offset as a fraction of the screen size. `PlacementCalculator` holds the pure math.
- **Scaling:** content is scaled by `screenHeightDIP / 1440 * userScale`, so an overlay covers the same share of the screen on any resolution or DPI setting.
- **Move mode:** F6 toggles it. You can drag a window (also to another monitor) and change its scale with the mouse wheel. When you leave move mode, the anchor is picked from the screen third the window sits in, and the placement is saved by `JsonOverlayLayoutStore` to `%APPDATA%/iRDCav/layout.json`.
- **Re-applying:** placements are applied again when the window size, its DPI or the monitor setup changes. `ScreenWatcher` polls for monitor changes because Avalonia 11.1 has no event for that. The defaults (`DefaultLayouts`) reproduce the original 2560x1440 layout.

## Rules

**Core Philosophy**: Code is read 10x more than written. Optimize for readability and maintainability, not cleverness.

### Functions

- Keep functions small (< 20 lines ideal)
- Keep the cyclomatic complexity low (< 20 ideal)
- Do one thing only - Single Responsibility Principle
- One level of abstraction per function
- Limit arguments: 0-2 ideal, 3 maximum, avoid flag arguments
- No side effects - function should do what its name says
- Separate commands (change state) from queries (return info)
- Prefer exceptions over error codes

### Comments

- Code should be self-explanatory - avoid comments when possible
- Good comments: warnings, TODOs, public API documentation
- Bad comments: redundant, misleading, or explaining bad code
- Never comment out code - delete it (version control preserves history)
- If you need a comment, consider refactoring the code instead

### Formatting

- Keep files small and focued (below 500 lines where possible)
- Use consistent indentation and team style
- Group related functions together

### Error Handling

- Use exceptions, not return codes or error flags
- Write `try-catch-finally` first when code might fail
- Provide context in exception messages
- Don't return `null` - return empty collections or use Optional/Maybe
- Don't pass `null` as arguments


### Classes

- Small classes: measured by responsibilities, not lines
- Single Responsibility Principle: one reason to change
- High cohesion: class variables used by many methods
- Low coupling: minimal dependencies between classes
- Open/Closed Principle: open for extension, closed for modification


### Concurrency

- Keep concurrent code separate from other code
- Limit scope of synchronized/locked data
- Use thread-safe collections
- Keep synchronized sections small
- Know your execution models and primitives


### System Design

- Separate construction from use (dependency injection)
- Use factories, builders for complex object creation
- Program to interfaces, not implementations
- Favor composition over inheritance
- Apply design patterns when they simplify, not to show off

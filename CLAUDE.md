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

Data flows one way: `Telemetry/` → `Services/` → `Presentation/` → `ViewModels/` → `Views/`.

**Composition root** (`App.axaml.cs`): this is the only place where objects get created and wired together. It builds the view models (grouped in `OverlayViewModels`), the three windows, every service, and the `TelemetryProcessor`, using constructor injection throughout. `ShutdownMode` is `OnExplicitShutdown`; the app exits through `ApplicationViewModel.ExitCommand` in the tray menu.

**Telemetry** (`Telemetry/`): `ITelemetrySource` wraps IRSDKSharper (`IRacingTelemetrySource`, `UpdateInterval = 2`). For each tick it raises `FrameReceived` with an `ITelemetryFrame`, whose `Read*` methods read values only on request. `IRacingTelemetryFrame` looks up the `IRacingSdkDatum` handles once per connection. `CarTelemetry` holds the per-car `CarIdx*` values.

**Services** (`Services/`), all running on the SDK thread:
- `TelemetryProcessor` routes frames. Inputs are processed on every frame, race data only on every 4th frame (`RaceDataFrameInterval`).
- `RaceDataController` holds the `RaceDataModel[MaxNumCars]` indexed by carIdx and merges live telemetry, session results (`PositionModel`) and drivers (`DriverModel`) into it. `Advance(sessionTime)` runs `IntervalCalculator`, then marks the fastest lap per class. `Snapshot()` returns copies that are safe to hand to the UI.
- `RelativeListBuilder` and `StandingsListBuilder` turn a snapshot into display rows. A default `RaceDataModel` is used as an empty row.
- `SessionInfoBuilder` builds the header `SessionInfoModel`. `FuelMonitor` detects stints and completed laps; `FuelCalculator` computes the statistics.

**Interval calculation** (`IntervalCalculator` + `Models/MicroSectorModel.cs`): each car splits a lap into 20 microsectors. Elapsed `SessionTime` is added to the sector being driven. When a tick crosses a sector boundary, the time is split by the distance driven on each side of it. The best time of each sector is kept, and their sum is the optimal lap. Teleports, driving backwards and large time steps discard the sector in progress. Once all sectors are recorded, the interval is the time this car needs, on its optimal lap, to drive from the player's position to its own (positive = ahead). Before that, and while the car is approaching or in the pits or has taken the checkered flag, `EstimateInterval` scales the lap-distance difference by the best or estimated lap time. `LapDelta` comes from `CarIdxLap + CarIdxLapDistPct`.

**Threading**: `OverlayPresenter` (implementing `IOverlayPresenter`) is the only code that touches view models from outside the UI thread, and it marshals every update through `IUiDispatcher`. Keep it that way, and pass the UI only snapshots or immutable records (`FuelDataModel` and `SessionInfoModel` are records).

**View models** use CommunityToolkit `[ObservableProperty]`. Models don't notify on change, so the UI refreshes when a whole object or list is replaced. Compiled bindings are on by default (`AvaloniaUseCompiledBindingsByDefault`), so XAML bindings need `x:DataType`. Value converters are in `Converters/` and derive from `OneWayConverter`.

**Overlay windows**: each window calls `WindowTransparency.ToTransparentWindow(this)` (Win32 click-through and topmost) before `InitializeComponent()`. It then calls `OverlayWindowBehavior.Attach(this, initialPosition)` *after* it, so the XAML background gets overridden. The behavior handles F6 move mode, dragging, staying topmost and clamping to the screen. The screen size is hard-coded to 2560x1440.

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

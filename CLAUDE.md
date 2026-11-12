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

**Startup** (`App.axaml.cs`): this file creates the three view models and the three windows (`ResultsWindow`, `RelativesWindow`, `InputGraphWindow`), plus a single `DataCollector` that holds references to all three view models. `ShutdownMode` is `OnExplicitShutdown`, so closing a window doesn't exit the app. Exiting goes through `ApplicationViewModel.ExitCommand`, and `OnExit` stops the SDK.

**Data flow**: data moves in one direction, from the SDK through `DataCollector` and the controllers into the view models.
- `DataCollector` subscribes to `IRacingSdk` events (`OnConnected`, `OnDisconnected`, `OnTelemetryData`). The SDK runs with `UpdateInterval = 2`. On the first telemetry tick, `IRacingSdkDatum` handles are looked up by name and cached.
- The input trace (throttle, brake, clutch) updates on every tick. The heavier race and session processing runs only once every `_refreshInterval + 1` ticks, gated by `_calculateCounter`.
- Each processing cycle fills per-car `CarIdx*` arrays, wraps them in `LiveDataModel`, and merges them into `RaceDataController` together with the session-info `DriverModel`/`PositionModel` data. It also builds a `SessionInfoModel` (SOF, temperatures, incidents, time remaining).
- `RaceDataController` holds a fixed `RaceDataModel[MaxNumCars]` indexed by carIdx. `Update(timeRemain)` computes intervals and the per-class fastest laps, then raises `OnDataReady`. `DataCollector` handles that event by pulling `GetRelativeViewRaceData(...)` and `GetResultsViewRaceData(...)` and assigning **new** `ObservableCollection`s to the view models.
- `FuelDataController` uses lap-completion detection (a change in the player's `CarIdxLap`) plus a stopwatch to compute per-lap fuel usage. It pushes results through its `OnUpdated` event into `RelativesViewModel.FuelData`.

**Interval calculation** (`RaceDataController.CalculateIntervals` + `Models/MicroSectorModel.cs`): each car splits a lap into `MICROSECTOR_COUNT` (20) microsectors. The time elapsed between updates goes into those buckets, and when a sector boundary falls inside one update, the time is split between the two sectors. Once a car has recorded all 20 sectors, the interval comes from summing sector times between the car and the player. Until then, or when the sector method returns 0, `EstimateInterval` is the fallback: the difference in lap-distance percentage scaled by the best or estimated lap time, with handling for gaps that wrap across the start/finish line. Elapsed time is derived from changes in `SessionTimeRemain`, not wall-clock time.

**View models** extend `ViewModelBase` (CommunityToolkit `ObservableObject`) but mostly use hand-written properties that call `OnPropertyChanged`. Models are plain POCOs with no change notification, so the UI refreshes only when a whole object or collection is replaced. Value converters for display formatting (lap times, positions, names, car numbers) sit at the project root. Compiled bindings are on by default (`AvaloniaUseCompiledBindingsByDefault`), so XAML bindings need `x:DataType`.

**Threading caveat**: `OnTelemetryData` runs on IRSDKSharper's background thread and sets view-model properties directly, without dispatching to the UI thread.

**Overlay windows**: each window calls `WindowTransparency.ToTransparentWindow(this)` before `InitializeComponent()`. Pressing F6 toggles "move mode", which changes the background and allows dragging. Window positioning and clamping use a hard-coded 2560x1440 screen size.

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

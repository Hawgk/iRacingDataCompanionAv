# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

iRDCav (iRacing Data Companion) is a set of transparent, always-on-top overlay windows for the iRacing sim: a standings view, a relative view, a fuel window with the weather, a throttle/brake input trace, and the sectors of the best lap. It is built with C# / .NET 8, Avalonia 11, CommunityToolkit.Mvvm and IRSDKSharper (iRacing SDK wrapper). The relative-gap logic is adapted from [iRon](https://github.com/lespalt/iRon/).

The app targets Windows only at runtime. `WindowTransparency.cs` P/Invokes `user32.dll` to make windows click-through and topmost, and the iRacing SDK reads shared memory that exists only while iRacing runs on Windows. The project still builds and runs on Linux, but there the windows aren't click-through and only the UI testing mode has data.

## Commands

The solution is at `IRDCav/IRDCav.sln` and has one project, `IRDCav/IRDCav/IRDCav.csproj`.

```bash
dotnet build IRDCav/IRDCav.sln
dotnet run --project IRDCav/IRDCav/IRDCav.csproj
dotnet run --project IRDCav/IRDCav/IRDCav.csproj -- --ui-testing-mode
```

`--ui-testing-mode` replaces iRacing with a simulated multi-class race, so you can check the overlays without the sim. See **UI testing mode** below.

The repo has no test project and no linter configuration.

## Architecture

Data flows one way: `Telemetry/` → `Services/` → `Presentation/` (state store) ← `ViewModels/` → `Views/`. The telemetry side never calls the UI; the UI pulls.

**Composition root** (`App.axaml.cs`): this is the only place where objects get created and wired together: view models (`OverlayViewModels`), windows and their `OverlayWindowBehavior`, the state store, the input buffer, the refresher, and the data feed (`IOverlayDataFeed`): the `TelemetryProcessor` with its services, or the `UiTestFeed` when started with `--ui-testing-mode`. Everything is passed in through constructors. `ShutdownMode` is `OnExplicitShutdown`; the app exits through `ApplicationViewModel.ExitCommand` in the tray menu.

**Telemetry** (`Telemetry/`): `ITelemetrySource` wraps IRSDKSharper (`IRacingTelemetrySource`, `UpdateInterval = 2`, i.e. 30 frames/s). For each tick it raises `FrameReceived` with an `ITelemetryFrame`, whose `Read*` methods read values only on request. `IRacingTelemetryFrame` looks up the datum handles once per connection and reuses its read buffers: the list from `ReadCars()` is overwritten on the next call. `SessionInfoVersion` changes whenever the session info YAML changes.

**Services** (`Services/`), all running on the SDK thread:
- `TelemetryProcessor` pushes inputs into the `IInputSink` on every frame. Every 4th frame (`RaceDataFrameInterval`) it processes race data and publishes an immutable `OverlayState`. Anything derived from the session info YAML (driver data, the session header) is rebuilt only when `SessionInfoVersion` or the session changes.
- `RaceDataController` holds the mutable `RaceDataModel[MaxNumCars]` indexed by carIdx and merges telemetry, session results and drivers into it. `Advance(sessionTime)` runs `IntervalCalculator`, then marks the fastest lap per class. `Cars` must never leave the telemetry thread.
- `RelativeListBuilder` and `StandingsListBuilder` turn the cars into immutable `RaceRow`s; `RaceRow.Empty` is an empty display row.
- `IRatingEstimator` estimates each car's iRating change if a race ended in the current class positions, with the community formula in `IRatingFormula` (not official, every driver counted as a starter). It is shown inside the iRating badge of both lists.
- `SessionInfoBuilder` builds the header `SessionInfoModel`. `FuelMonitor` detects stints and completed laps; `FuelCalculator` computes the statistics.
- `LapSectorTracker` splits the player's laps into 5 sectors of 4 microsectors each (`SectorTimes`) and compares them with `PersonalBest`, the all-time best of the car at this track. A lap counts only once iRacing reports it as a new session best (`LapBestLapTime`), so laps iRacing doesn't count are ignored; the lap that ended within 5 s of that report is offered to `PersonalBest`, which keeps and saves it if it is faster. Each finished sector gets a delta, which stays until the car enters that sector again (`MicroSectorModel.DrivenMicroSector`). A new session or connection resets the session state, a new car or track (`CarAtTrackReader`, from the session info) loads the matching personal best.

**Best laps** (`BestLaps/`): `JsonBestLapStore` (`IBestLapStore`) keeps one file per track and car, `%APPDATA%/iRDCav/tracks/{track - config}/{car}.json`, with iRacing's names made safe for file names. A `BestLapRecord` holds the lap time, the 5 sector times and every class the car was driven in; the class best is the fastest file at the track that lists the current class. Unreadable files count as missing and failed saves are logged, so the overlays never stop over a file.

**Interval calculation** (`IntervalCalculator` + `Models/MicroSectorModel.cs`): each car splits a lap into 20 microsectors. Elapsed `SessionTime` is added to the sector being driven. When a tick crosses a sector boundary, the time is split by the distance driven on each side of it. The best time of each sector is kept, and their sum is the optimal lap. Teleports, driving backwards and large time steps discard the sector in progress and the lap in progress. The model also keeps the microsector times of the lap in progress (`CurrentLap`) and of the last lap across the line (`LastLap`, `CompletedLaps`) for the sector view. Once all sectors are recorded, the interval is the time this car needs, on its optimal lap, to drive from the player's position to its own (positive = ahead). Before that, and while the car is approaching or in the pits or has taken the checkered flag, `EstimateInterval` scales the lap-distance difference by the best or estimated lap time. `LapDelta` comes from `CarIdxLap + CarIdxLapDistPct`.

**Decoupling and threading** (`Presentation/`): the producer side writes into two thread-safe structures and never blocks on the UI:
- `OverlayStateStore`: a slot that keeps only the latest `OverlayState`, written with `Volatile`. A state the UI misses is simply replaced, so nothing can queue up.
- `InputTraceBuffer`: a fixed-size ring buffer that takes a short lock and has a version counter.

`OverlayRefresher` runs a `DispatcherTimer` on the UI thread at background priority (~30 Hz). It pulls the latest values and does nothing when neither the state reference nor the input version changed. The data views (standings, relatives, fuel) are updated at most 4 times a second (`RefreshCadence`) to keep them calm, visibility on every tick and the input trace with every frame. This only throttles the views; race data and intervals are still computed on every 4th telemetry frame. Keep it this way: never touch view models from the telemetry thread, and publish only immutable data.

**UI testing mode** (`UiTesting/`): `UiTestFeed` stands in for iRacing. It runs its own background thread at 30 frames/s, pushes an `InputPattern` sample every frame, and publishes an `OverlayState` every 4th frame through the same store and buffer as live data. `SimulatedRace` moves a fictional field (`DemoRoster`: three classes, a long name, a three-digit number, a driver without a license color) by distance driven, and derives positions, intervals, lap times and pit stops from it. `UiTestScenario` shapes the rows with the real `RelativeListBuilder` and `StandingsListBuilder`; `SimulatedLapSectors` makes up a best lap with changing deltas. The interval calculation itself is not exercised.

**View models** use CommunityToolkit `[ObservableProperty]`. The list views bind to a `RaceRowCollection` of reused `RaceRowViewModel`s that are updated in place, so only the cells that changed re-render. Don't replace the collections. Compiled bindings are on by default (`AvaloniaUseCompiledBindingsByDefault`), so XAML bindings and `DataTemplate`s need `x:DataType`. Text converters are in `Converters/` and derive from `OneWayConverter`. `TintConverter` turns iRacing's class and license colors into pastel brushes, and `RotationConverter` turns the wind angle into a rotation. The input graph is drawn by `Views/InputTraceControl`, which renders the trace polylines and the current pedal bars directly; there's no chart library.

**Configuration and theme** (`Configuration/`, `Views/Theme/`, `Views/Table/`):
- **The file:** `JsonOverlayConfigurationFile` reads `%APPDATA%/iRDCav/config.json` at startup; on first start it writes the defaults. `OverlayConfigurationValidator` holds the rules for both the file and the settings window. The file holds the font, the color scheme, the three font sizes, the columns of the standings and relative lists (one `columns` list sets both order and visibility), column titles, which of the three refuel estimates (minimum, average, maximum consumption) is enlarged, and the gap and interval decimals. Keys may be left out, and comments are allowed. An invalid file falls back to the defaults and the error goes to `Trace`.
- **Colors, sizes and formats:** `OverlayTheme.Apply` publishes the colors as `Overlay.*` brushes, the font sizes `Overlay.FontSize.Small/Normal/Large` with row heights, line heights, icon size and column-width scale derived from them, the `GapConverter` and `IntervalConverter`, and tabular digits (added once by `AddTabularDigits`). It must run before the windows are created and may run again at any time.
- **Live changes:** `LiveOverlayConfiguration` (`IOverlayConfigurationStore`) holds the configuration in use. `Load` applies the startup file; `Save` validates, writes `config.json` and applies. `OverlayConfigurationApplier` applies the theme first and then the view models, whose `ApplyTable` always sets a new column list so tables rebuild with the new font measurements.
- **Settings window** (`Views/Settings`, `ViewModels/Settings`): opened with F7 on a focused overlay (`SettingsHotkey`) or from the tray menu, one instance at a time (`SettingsWindowLauncher`). It edits every setting; colors are found by reflection over `ColorScheme`, so a new color needs no change there. Overlay text styles are scoped to `Window.overlay` so they don't affect this window. XAML uses `DynamicResource Overlay.*` for every color and font size; don't hard-code either, and don't add a fourth font size.
- **Examples:** `config-examples/` holds sample files and a README that explains every setting. Update them when a setting changes.
- **Columns:** `RaceTable` builds its rows from one `DataTemplate` per `RaceColumn`, keyed `RaceColumn.{name}` in `RaceColumnTemplates.axaml`. Title, alignment and width are in `RaceColumnLayout`; widths are given in digits of the longest expected content and turned into pixels with the font's measured digit width (`FontMeasurement.DigitAdvance`), so they fit any font and size. A new column needs an enum value, a layout entry and a template.
- **Font and alignment:** `OverlayFonts` maps the configured name to a bundled font (Roboto Mono, the default, and Lekton in `Assets/Fonts` with their OFL licenses, or Inter) or to an installed font. Texts of different sizes in one row share a line height (put them flat in a `StackPanel.headerRow` or give them a common `LineHeight`), and every text is shifted by `Overlay.TextShift.Small/Normal/Large`: `FontMeasurement.CapitalOffset` reads from the font's metrics how far its capitals sit off the line box center, so all sizes end up on one center line for any font. Give new texts one of the three size styles so they get the matching shift. Icons are stroked `StreamGeometry` resources in `App.axaml` on a 24 unit grid; wrap them in a `Viewbox.icon` so they are not stretched to their outline.

**Overlay windows and layout** (`Layout/`, `Views/OverlayWindowBehavior.cs`):
- **Window setup:** each window calls `WindowTransparency.ToTransparentWindow(this)` (Win32 click-through) before `InitializeComponent()`. Overlays never take the focus (`ShowActivated = false`), so they are made topmost explicitly: `Topmost = true`, and `WindowTransparency.BringToTop` (`SetWindowPos`) whenever a window opens or reappears. The `WS_EX_TOPMOST` style alone has no effect. Its content must be wrapped in a `LayoutTransformControl`, aligned top-left, so the panel keeps its content size even where a window manager ignores `SizeToContent="WidthAndHeight"` (some Linux desktops do).
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
- Favor composition over inheritance (adapt to external APIs and frameworks that are included in the project)
- Apply design patterns when they simplify, not to show off

### Commit rules

- Always create meaningful granular commits with small incremental steps
- Bundle based on behaviour changes
- Never push commits
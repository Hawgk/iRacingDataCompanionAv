# Example configurations

To use one, copy it to `%APPDATA%\iRDCav\config.json` and restart iRDCav. The app creates that file with the defaults on its first start.

You can leave out any key; whatever is missing keeps its default. If the file can't be read or has an invalid value, the app uses the complete defaults.

| File | What it shows |
|---|---|
| `default.json` | Every setting with its default value |
| `classic-colors.json` | The original iRDCav colors; only the color scheme is set |
| `compact.json` | Smaller fonts and only the essential columns |
| `large-text.json` | Larger fonts in Inter and fewer columns for small or distant screens |
| `multiclass-endurance.json` | Class and pit columns, gaps with 3 decimals, refuel amount for the highest consumption |

## Settings

**`font`**: the font of all overlays. `RobotoMono` (the default) and `Lekton`, both monospace, and `Inter` ship with iRDCav. Any other name must be a font installed on your system, for example `"Consolas"`. A name that isn't installed silently falls back to a system font.

**`colorScheme`**: colors as `#RRGGBB` or `#AARRGGBB` (`AA` is the opacity, `00` transparent to `FF` opaque).

| Key | Used for |
|---|---|
| `background` | Panels |
| `surface` | Header bars |
| `text`, `textMuted` | Values, and labels and captions |
| `playerHighlight` | Your row |
| `lapAhead`, `lapBehind` | Cars a lap or more ahead of or behind you |
| `fastestLap` | Fastest lap of each class |
| `pitRoad` | Pit badge, and the refuel amount when you have enough fuel |
| `warning` | Incident count |
| `throttle`, `brake` | Input trace |

**`fontSizes`**: the three font sizes the overlays use, in pixels on a 1440p screen. They must grow from `small` to `normal` to `large` and lie between 6 and 60. Row heights, icons and column widths follow them.

| Key | Used for |
|---|---|
| `small` | Labels, captions, column titles, badges and class headers (default 11) |
| `normal` | All values and list rows (default 13) |
| `large` | Time remaining and the selected refuel amount (default 22) |

**`fuel.refuelEstimate`**: which of the three refuel amounts is shown large: `Minimum`, `Average` or `Maximum` consumption. All three are always shown.

**`decimals`**: digits after the decimal point for `gap` and `interval`, from 0 to 3.

**`standings`, `relatives`**: `columns` is the only setting for which columns appear and in which order. List the columns you want, left to right; any column not listed is hidden. `showColumnTitles` shows a title row above the list.

| Column | Shows |
|---|---|
| `Position` | Position in class |
| `CarNumber` | Car number with a class color stripe |
| `CarClass` | Class name in the class color |
| `Driver` | Driver name |
| `Pit` | Badge while the car is on pit road |
| `IRating` | iRating in a badge colored by the safety class |
| `Laps` | Laps completed |
| `FastestLap` | Fastest lap, highlighted when it's the class's fastest |
| `LastLap` | Last lap |
| `Gap` | Gap to you in races, or to the class's fastest lap in practice and qualifying (standings only) |
| `Interval` | Time to you on track |

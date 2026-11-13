# iRDCav - iRacing Data Companion
This is a tool.

It's written in C# using .NET 8.0 and AvaloniaUI. Was kinda an experiment to get to know Avalonia a bit better. Might port it to OpenGL/DirectX at some later stage.

One of those things it can do is read data from iRacing SDK.

Also it paints pixels.

All those pixels combined resemble a standings view, relative view and fuel calculator.

~Don't look into the code please.~ I don't care now. Claude was used starting from 12.11.2026.

## Setup
1. Download [install.ps1](install.ps1) and run it in PowerShell on Windows:
   ```
   powershell -ExecutionPolicy Bypass -File install.ps1
   ```
   Or do both in one go:
   ```
   powershell -ExecutionPolicy Bypass -Command "irm https://raw.githubusercontent.com/Hawgk/iRacingDataCompanionAv/main/install.ps1 | iex"
   ```
   The script downloads iRDCav, installs a .NET 8 SDK just for building it if none is installed, builds the app and installs it to `%LOCALAPPDATA%\Programs\iRDCav` with an `iRDCav` entry in the Start menu. Run it again to update, or with `-Uninstall` to remove the app.
2. Start iRDCav from the Start menu, then start iRacing. The overlays appear once you're in a session.
3. Arrange the overlays: with an overlay focused, press F6, drag it into place (also to another monitor), and scroll the mouse wheel to resize it. Press F6 again to save the layout.
4. Optional: change the colors, font, font sizes and columns in the settings window. Open it with F7 on a focused overlay or with "Settings" in the tray menu; saving applies the changes right away and stores them in `%APPDATA%\iRDCav\config.json`. The `config-examples` folder (in this repository and next to the installed app) explains every setting and has ready-made examples.

Quit through the tray icon.

For development, run the app straight from a clone with `dotnet run -c Release --project IRDCav/IRDCav/IRDCav.csproj`. Add `-- --ui-testing-mode` to check the overlays without iRacing.

## Special Thanks
[iRon](https://github.com/lespalt/iRon/) - Thanks for those relatives mate.

[IRSDKSharper](https://github.com/mherbold/IRSDKSharper) - Thanks for the awesome C# wrapper.

iRacing Team - Thanks for giving us a sim that is freaking awesome. (but come on we shouldn't have to pay monthly(?????) for 3rd party overlays)

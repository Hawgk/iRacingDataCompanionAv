# iRDCav - iRacing Data Companion
This is a tool.

It's written in C# using .NET 8.0 and AvaloniaUI. Was kinda an experiment to get to know Avalonia a bit better. Might port it to OpenGL/DirectX at some later stage.

One of those things it can do is read data from iRacing SDK.

Also it paints pixels.

All those pixels combined resemble a standings view, relative view and fuel calculator.

~Don't look into the code please.~ I don't care now. Claude was used starting from 12.11.2026.

## Setup
1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) on Windows. It's only needed to build the app.
2. Clone this repository and run the install script in its folder:
   ```
   powershell -ExecutionPolicy Bypass -File install.ps1
   ```
   It builds iRDCav, installs it to `%LOCALAPPDATA%\Programs\iRDCav` and adds an `iRDCav` entry to the Start menu. Run it again to update, or with `-Uninstall` to remove the app.
3. Start iRDCav from the Start menu, then start iRacing. The overlays appear once you're in a session.
4. Arrange the overlays: with an overlay focused, press F6, drag it into place (also to another monitor), and scroll the mouse wheel to resize it. Press F6 again to save the layout.
5. Optional: change the colors, font, font sizes and columns in `%APPDATA%\iRDCav\config.json`, then restart. The app creates the file on its first start. The `config-examples` folder (in this repository and next to the installed app) explains every setting and has ready-made examples.

Quit through the tray icon.

To build without installing, run `dotnet run -c Release --project IRDCav/IRDCav/IRDCav.csproj`. Add `-- --ui-testing-mode` to check the overlays without iRacing.

## Special Thanks
[iRon](https://github.com/lespalt/iRon/) - Thanks for those relatives mate.

[IRSDKSharper](https://github.com/mherbold/IRSDKSharper) - Thanks for the awesome C# wrapper.

iRacing Team - Thanks for giving us a sim that is freaking awesome. (but come on we shouldn't have to pay monthly(?????) for 3rd party overlays)

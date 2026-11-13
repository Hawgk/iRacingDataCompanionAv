# iRDCav - iRacing Data Companion
This is a tool.

It's written in C# using .NET 8.0 and AvaloniaUI. Was kinda an experiment to get to know Avalonia a bit better. Might port it to OpenGL/DirectX at some later stage.

One of those things it can do is read data from iRacing SDK.

Also it paints pixels.

All those pixels combined resemble a standings view, relative view and fuel calculator.

Don't look into the code please.

## Setup
1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) on Windows.
2. Clone this repository and start the app:
   ```
   dotnet run -c Release --project IRDCav/IRDCav/IRDCav.csproj
   ```
3. Start iRacing. The overlays appear once you're in a session and hide when iRacing closes.
4. Arrange the overlays: with an overlay focused, press F6, drag it into place (also to another monitor), and scroll the mouse wheel to resize it. Press F6 again to save the layout.
5. Optional: change the colors, font, font sizes and columns in `%APPDATA%\iRDCav\config.json`, then restart. The app creates the file on its first start. [config-examples](config-examples/) explains every setting and has ready-made examples.

Quit through the tray icon. To check the overlays without iRacing, add `-- --ui-testing-mode` to the run command.

## Special Thanks
[iRon](https://github.com/lespalt/iRon/) - Thanks for those relatives mate.

[IRSDKSharper](https://github.com/mherbold/IRSDKSharper) - Thanks for the awesome C# wrapper.

iRacing Team - Thanks for giving us a sim that is freaking awesome. (but come on we shouldn't have to pay monthly(?????) for 3rd party overlays)

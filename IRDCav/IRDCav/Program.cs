using Avalonia;
using System;
using System.Globalization;

namespace IRDCav
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            UseInvariantNumberFormat();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(SoftwareRendering())
                .WithInterFont()
                .LogToTrace();

        /// <summary>
        /// Draws the overlays on the CPU. For small transparent windows this costs less CPU than the GPU
        /// path, whose per-frame setup and hand-over to the Windows compositor outweighs the drawing
        /// itself: with iRacing running, about 7 % of one core instead of 13 %. The CPU is what iRacing
        /// needs in crowded races; the GPU share was negligible either way.
        /// </summary>
        private static Win32PlatformOptions SoftwareRendering() => new()
        {
            RenderingMode = new[] { Win32RenderingMode.Software },
        };

        /// <summary>
        /// Numbers always use a decimal point ("2.1k", "12.5 L"), whatever the system's region setting.
        /// Covers the UI thread and every thread started later, such as the telemetry thread.
        /// </summary>
        private static void UseInvariantNumberFormat()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }
    }
}

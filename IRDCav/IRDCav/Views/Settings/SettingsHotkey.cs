using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace IRDCav.Views.Settings
{
    /// <summary>F7 on a focused overlay opens the settings.</summary>
    internal static class SettingsHotkey
    {
        private const Key OpenKey = Key.F7;

        public static void Attach(Window window, Action openSettings) =>
            window.KeyDown += (_, args) =>
            {
                if (args.Key == OpenKey)
                {
                    openSettings();
                }
            };
    }
}

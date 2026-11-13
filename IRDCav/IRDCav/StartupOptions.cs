using System;
using System.Collections.Generic;
using System.Linq;

namespace IRDCav
{
    /// <summary>Command line options given at startup.</summary>
    internal sealed record StartupOptions(bool IsUiTestingMode)
    {
        public const string UiTestingModeArgument = "--ui-testing-mode";

        public static StartupOptions Parse(IEnumerable<string> args) =>
            new(args.Contains(UiTestingModeArgument, StringComparer.OrdinalIgnoreCase));
    }
}

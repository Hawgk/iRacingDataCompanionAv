using System.Collections.Generic;

namespace IRDCav.UiTesting
{
    internal sealed record DemoCarClass(string Name, string Color, float LapTime);

    internal sealed record DemoDriver(string Name, string CarNumber, DemoCarClass CarClass, int IRating, string LicenseColor);

    /// <summary>
    /// Fictional multi-class field. It contains edge cases the overlays must handle:
    /// a very long name, a three-digit car number and a driver without a license color.
    /// </summary>
    internal static class DemoRoster
    {
        public const string PlayerName = "Test Player";

        private const string LicenseA = "#600153DB";
        private const string LicenseB = "#6000C702";
        private const string LicenseC = "#60FEEC04";
        private const string LicenseD = "#60FC8A27";
        private const string LicenseR = "#60FC0706";
        private const string LicenseUndefined = "#60FF7247";

        private static readonly DemoCarClass Prototype = new("LMP2", "#6033CEFF", 98.5f);
        private static readonly DemoCarClass Gt3 = new("GT3", "#60FFDA59", 105.2f);
        private static readonly DemoCarClass Gt4 = new("GT4", "#60AE6BFF", 112.8f);

        public static IReadOnlyList<DemoDriver> Drivers { get; } = new[]
        {
            new DemoDriver("Jonas Brennecke", "22", Prototype, 4210, LicenseA),
            new DemoDriver("Lucia Ferrante", "8", Prototype, 3890, LicenseA),
            new DemoDriver("Mateo Alvarez", "31", Prototype, 3120, LicenseB),
            new DemoDriver("Hannah Okafor", "5", Prototype, 2950, LicenseB),
            new DemoDriver("Pieter de Wit", "38", Prototype, 2470, LicenseC),
            new DemoDriver("Sofia Lindgren", "11", Prototype, 2210, LicenseC),

            new DemoDriver("Marek Novak", "3", Gt3, 3540, LicenseA),
            new DemoDriver("Elena Rossi", "44", Gt3, 3310, LicenseA),
            new DemoDriver("Tomás Iglesias", "911", Gt3, 2980, LicenseB),
            new DemoDriver("Kenji Watanabe", "7", Gt3, 2760, LicenseB),
            new DemoDriver("Maximilian Alexander Vandenberghe-Lindqvist", "63", Gt3, 2640, LicenseB),
            new DemoDriver(PlayerName, "17", Gt3, 2510, LicenseC),
            new DemoDriver("Chloé Dubois", "88", Gt3, 2390, LicenseC),
            new DemoDriver("Oskar Halvorsen", "21", Gt3, 2200, LicenseC),
            new DemoDriver("Ayesha Rahman", "12", Gt3, 1980, LicenseD),
            new DemoDriver("Ben Carter", "69", Gt3, 1850, LicenseD),
            new DemoDriver("Ivan Petrov", "33", Gt3, 1620, LicenseD),
            new DemoDriver("Nora Fischer", "2", Gt3, 1350, LicenseR),

            new DemoDriver("Liam O'Connor", "51", Gt4, 2050, LicenseC),
            new DemoDriver("Mia Schulz", "74", Gt4, 1890, LicenseC),
            new DemoDriver("Rafael Costa", "58", Gt4, 1720, LicenseD),
            new DemoDriver("Yuki Tanaka", "66", Gt4, 1600, LicenseD),
            new DemoDriver("Emma Novak", "90", Gt4, 1480, LicenseD),
            new DemoDriver("Lars Berg", "77", Gt4, 1310, LicenseR),
            new DemoDriver("Zoe Martin", "99", Gt4, 1150, LicenseR),
            new DemoDriver("Omar Haddad", "102", Gt4, 1000, LicenseUndefined),
        };
    }
}

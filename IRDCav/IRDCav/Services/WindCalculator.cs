using System;

namespace IRDCav.Services
{
    /// <summary>Converts the track's wind into what the driver feels in the car.</summary>
    internal static class WindCalculator
    {
        private const double MetersPerSecondToKmh = 3.6;

        /// <summary>Where the wind comes from relative to the car, clockwise in degrees within 0..360. 0 is a headwind.</summary>
        public static double RelativeAngle(float windDirection, float heading)
        {
            double degrees = (windDirection - heading) * 180 / Math.PI;
            return (degrees % 360 + 360) % 360;
        }

        public static double ToKmh(float metersPerSecond) => metersPerSecond * MetersPerSecondToKmh;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace IRDCav.Services
{
    /// <summary>
    /// The iRating change formula that the iRacing community reverse-engineered and that common
    /// iRating calculators use. It is not official, so iRacing's own result can differ by a few points.
    /// Every driver is assumed to have started the race.
    /// </summary>
    internal static class IRatingFormula
    {
        private static readonly double RatingScale = 1600 / Math.Log(2);

        // Ratings of 0 (e.g. AI drivers) would make the win chance between two of them 0 / 0.
        private const int MinRating = 1;

        /// <param name="iRatings">iRatings of all drivers of one class.</param>
        /// <param name="positions">Positions in the class (1 is first), in the same order.</param>
        /// <returns>The estimated change for each driver, in the same order.</returns>
        public static int[] Changes(IReadOnlyList<int> iRatings, IReadOnlyList<int> positions)
        {
            double[] strengths = iRatings.Select(rating => Math.Exp(-Math.Max(rating, MinRating) / RatingScale)).ToArray();
            int count = strengths.Length;
            var changes = new int[count];
            for (int i = 0; i < count; i++)
            {
                double driversBeaten = count - positions[i];
                double fudge = (count / 2.0 - positions[i]) / 100;
                changes[i] = (int)Math.Round((driversBeaten - ExpectedDriversBeaten(strengths, i) - fudge) * 200 / count);
            }
            return changes;
        }

        private static double ExpectedDriversBeaten(double[] strengths, int driver)
        {
            double expected = 0;
            for (int other = 0; other < strengths.Length; other++)
            {
                if (other != driver)
                {
                    expected += WinChance(strengths[driver], strengths[other]);
                }
            }
            return expected;
        }

        /// <summary>Chance that driver a finishes ahead of driver b, from their exp(-iRating / scale) terms.</summary>
        private static double WinChance(double a, double b) => (1 - a) * b / ((1 - b) * a + (1 - a) * b);
    }
}

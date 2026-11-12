using IRDCav.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IRDCav.Services
{
    /// <summary>
    /// Builds the standings view grouped by class. Other classes show their leaders only;
    /// the player's class shows its leaders plus a window of cars around the player.
    /// </summary>
    internal sealed class StandingsListBuilder
    {
        private const string RaceSessionType = "R";

        private readonly int _playerWindowSize;
        private readonly int _leadersPerClass;

        public StandingsListBuilder(int playerWindowSize, int leadersPerClass)
        {
            _playerWindowSize = playerWindowSize;
            _leadersPerClass = leadersPerClass;
        }

        // TODO: At race start the order is random. Would be great if it got sorted by interval.
        public IReadOnlyList<RaceDataModel> Build(IReadOnlyCollection<RaceDataModel> cars, string sessionType)
        {
            string playerClass = cars.FirstOrDefault(car => car.IsMe)?.ClassStr ?? string.Empty;
            var rows = new List<RaceDataModel>();

            foreach (var classCars in ClassifiedCarsByClass(cars))
            {
                List<RaceDataModel> classRows = classCars.Key == playerClass
                    ? BuildPlayerClassRows(classCars.ToList())
                    : BuildOtherClassRows(classCars);

                ApplyGaps(classRows, sessionType);
                rows.AddRange(classRows);
            }
            return rows;
        }

        /// <summary>Classes are ordered by their best placed car, cars within a class by position.</summary>
        private static IEnumerable<IGrouping<string, RaceDataModel>> ClassifiedCarsByClass(IEnumerable<RaceDataModel> cars) =>
            cars.Where(car => car.Name != string.Empty && !car.IsPaceCar && car.Position > 0)
                .OrderBy(car => car.Position)
                .GroupBy(car => car.ClassStr);

        private List<RaceDataModel> BuildOtherClassRows(IEnumerable<RaceDataModel> classCars)
        {
            List<RaceDataModel> rows = classCars.Take(_leadersPerClass).ToList();
            rows.Add(new RaceDataModel());
            return rows;
        }

        /// <summary>
        /// The leaders are separated from the window around the player by an empty row
        /// whenever the window does not start at the class leader.
        /// </summary>
        private List<RaceDataModel> BuildPlayerClassRows(List<RaceDataModel> classCars)
        {
            int playerIndex = Math.Max(0, classCars.FindIndex(car => car.IsMe));
            int windowStart = Math.Max(0, playerIndex - _playerWindowSize / 2);
            int leaderCount = Math.Min(_leadersPerClass, windowStart);

            List<RaceDataModel> rows = classCars.Take(leaderCount).ToList();
            if (leaderCount > 0)
            {
                rows.Add(new RaceDataModel());
            }
            rows.AddRange(classCars.Skip(windowStart).Take(_playerWindowSize));
            return rows;
        }

        private static void ApplyGaps(List<RaceDataModel> classRows, string sessionType)
        {
            if (sessionType == RaceSessionType)
            {
                ApplyIntervalGaps(classRows);
            }
            else
            {
                ApplyLapTimeGaps(classRows);
            }
        }

        private static void ApplyIntervalGaps(IEnumerable<RaceDataModel> rows)
        {
            foreach (RaceDataModel row in rows)
            {
                row.Gap = row.Interval;
            }
        }

        /// <summary>Gap of each fastest lap to the class's fastest lap, which is listed first outside of races.</summary>
        private static void ApplyLapTimeGaps(IEnumerable<RaceDataModel> rows)
        {
            float fastestLapTime = 0;
            foreach (RaceDataModel row in rows)
            {
                if (row.FastestLapTime <= 0)
                {
                    row.Gap = row.Interval;
                }
                else if (row.IsFastest)
                {
                    fastestLapTime = row.FastestLapTime;
                    row.Gap = 0;
                }
                else
                {
                    row.Gap = row.FastestLapTime - fastestLapTime;
                }
            }
        }
    }
}

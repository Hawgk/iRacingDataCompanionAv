using IRDCav.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IRDCav.Services
{
    /// <summary>
    /// Builds the relative view: cars ordered by interval with the player in the middle row.
    /// </summary>
    internal sealed class RelativeListBuilder
    {
        private readonly int _rowCount;

        public RelativeListBuilder(int rowCount)
        {
            _rowCount = rowCount;
        }

        public IReadOnlyList<RaceRow> Build(IEnumerable<RaceDataModel> cars)
        {
            List<RaceDataModel> byInterval = cars
                .Where(car => car.ConsiderForRelative && !car.IsPaceCar)
                .OrderByDescending(car => car.Interval)
                .ToList();

            int playerIndex = Math.Max(0, byInterval.FindIndex(car => car.IsMe));
            int firstIndex = playerIndex - _rowCount / 2;

            return Enumerable.Range(firstIndex, _rowCount)
                .Select(index => index >= 0 && index < byInterval.Count ? RaceRow.From(byInterval[index]) : RaceRow.Empty)
                .ToList();
        }
    }
}

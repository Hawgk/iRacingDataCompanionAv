using IRDCav.Models;
using System.Collections.Generic;
using System.Linq;

namespace IRDCav.Services
{
    /// <summary>
    /// Estimates each car's iRating change if the race ended in the current positions.
    /// iRating is calculated per class and only changes in races.
    /// </summary>
    internal sealed class IRatingEstimator
    {
        public void Update(IReadOnlyCollection<RaceDataModel> cars, bool isRace)
        {
            foreach (RaceDataModel car in cars)
            {
                car.HasIRatingChange = false;
            }
            if (!isRace)
            {
                return;
            }
            foreach (var classCars in cars.Where(IsClassified).GroupBy(car => car.ClassStr))
            {
                ApplyChanges(classCars.ToList());
            }
        }

        private static bool IsClassified(RaceDataModel car) =>
            car.Name != string.Empty && !car.IsPaceCar && car.ClassPosition > 0;

        private static void ApplyChanges(List<RaceDataModel> classCars)
        {
            int[] changes = IRatingFormula.Changes(
                classCars.Select(car => car.IRating).ToList(),
                classCars.Select(car => car.ClassPosition).ToList());

            for (int i = 0; i < classCars.Count; i++)
            {
                classCars[i].IRatingChange = changes[i];
                classCars[i].HasIRatingChange = true;
            }
        }
    }
}

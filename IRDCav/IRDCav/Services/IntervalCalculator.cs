using IRDCav.Models;
using System;
using System.Collections.Generic;
using static IRSDKSharper.IRacingSdkEnum;

namespace IRDCav.Services
{
    /// <summary>
    /// Calculates each car's time interval and lap delta to the player. Uses the car's
    /// microsectors when available and falls back to a lap-time based estimate.
    /// </summary>
    internal sealed class IntervalCalculator
    {
        public void Update(IReadOnlyList<RaceDataModel> cars, RaceDataModel player, double elapsedTime)
        {
            foreach (RaceDataModel car in cars)
            {
                if (IsTracked(car))
                {
                    UpdateCar(car, player, elapsedTime);
                }
                else
                {
                    ResetCar(car);
                }
            }
        }

        private static bool IsTracked(RaceDataModel car) =>
            car.SessionFlags != 0 &&
            car.TrackSurface != TrkSurf.SurfaceNotInWorld &&
            car.TrackSurface != TrkSurf.UndefinedMaterial &&
            car.LapsCompleted >= 0 &&
            !car.IsPaceCar;

        private static void UpdateCar(RaceDataModel car, RaceDataModel player, double elapsedTime)
        {
            car.MicroSectors.Record(elapsedTime, car.LapDistPct, car.LastLapDistPct);
            car.Interval = MathF.Round(CalculateInterval(car, player), 3);
            car.LapDelta = car.LapsCompleted - player.LapsCompleted;
            car.LastLapDistPct = car.LapDistPct;
            car.ConsiderForRelative = true;
        }

        private static void ResetCar(RaceDataModel car)
        {
            car.Interval = 0;
            car.LapDelta = 0;
            car.LastLapDistPct = car.LapDistPct;
            car.ConsiderForRelative = car.IsMe;
        }

        private static float CalculateInterval(RaceDataModel car, RaceDataModel player)
        {
            if (CanUseMicroSectors(car) &&
                car.MicroSectors.TryGetInterval(car.LapDistPct, player.LapDistPct, out float interval))
            {
                return interval;
            }
            return EstimateInterval(car, player);
        }

        private static bool CanUseMicroSectors(RaceDataModel car) =>
            car.TrackLocation != TrkLoc.AproachingPits &&
            car.TrackLocation != TrkLoc.InPitStall &&
            !car.SessionFlags.HasFlag(Flags.Checkered);

        /// <summary>Lap distance difference scaled by a reference lap time.</summary>
        private static float EstimateInterval(RaceDataModel car, RaceDataModel player)
        {
            float distance = car.LapDistPct - player.LapDistPct;
            float lapTime = distance > 0 ? ReferenceLapTime(player) : ReferenceLapTime(car);
            float interval = distance * lapTime;

            bool spansStartFinishLine = Math.Abs(distance) >= 0.5f;
            if (spansStartFinishLine)
            {
                interval += distance < 0 ? lapTime : -lapTime;
            }
            return interval;
        }

        private static float ReferenceLapTime(RaceDataModel car) =>
            car.FastestLapTime > 0 ? car.FastestLapTime : car.EstLapTime;
    }
}

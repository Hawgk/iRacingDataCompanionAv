using IRDCav.Models;
using System;
using System.Collections.Generic;
using static IRSDKSharper.IRacingSdkEnum;

namespace IRDCav.Services
{
    /// <summary>
    /// Calculates each car's time interval and lap delta to the player. The interval is measured
    /// the way race timing does it, from when both cars passed the same point (see
    /// <see cref="MicroSectorModel"/>): for a car ahead, how long ago it passed the player's
    /// position; for a car behind, how long ago the player passed its position. Where no pass time
    /// is known yet, e.g. right after a tow, a lap-time based estimate stands in.
    /// </summary>
    internal sealed class IntervalCalculator
    {
        public void Update(IReadOnlyList<RaceDataModel> cars, RaceDataModel player, TimingTick tick)
        {
            // Every car first, so the player's pass times are current when the cars behind are measured.
            foreach (RaceDataModel car in cars)
            {
                RecordPassTimes(car, tick);
            }
            foreach (RaceDataModel car in cars)
            {
                UpdateInterval(car, player, tick.SessionTime);
            }
        }

        private static bool IsTracked(RaceDataModel car) =>
            car.SessionFlags != 0 &&
            car.TrackSurface != TrkSurf.SurfaceNotInWorld &&
            car.TrackSurface != TrkSurf.UndefinedMaterial &&
            car.LapsCompleted >= 0 &&
            !car.IsPaceCar;

        private static void RecordPassTimes(RaceDataModel car, TimingTick tick)
        {
            if (IsTracked(car))
            {
                car.MicroSectors.Record(tick, car.LapDistPct, car.LastLapDistPct);
            }
            car.LastLapDistPct = car.LapDistPct;
        }

        private static void UpdateInterval(RaceDataModel car, RaceDataModel player, double sessionTime)
        {
            if (!IsTracked(car))
            {
                car.Interval = 0;
                car.LapDelta = 0;
                car.ConsiderForRelative = car.IsMe;
                return;
            }
            car.Interval = MathF.Round(CalculateInterval(car, player, sessionTime), 3);
            car.LapDelta = CalculateLapDelta(car, player);
            car.ConsiderForRelative = true;
        }

        private static float CalculateInterval(RaceDataModel car, RaceDataModel player, double sessionTime) =>
            TryMeasureInterval(car, player, sessionTime, out float interval) ? interval : EstimateInterval(car, player);

        /// <summary>Positive for a car ahead of the player, negative for one behind.</summary>
        private static bool TryMeasureInterval(RaceDataModel car, RaceDataModel player, double sessionTime, out float interval)
        {
            if (IsAhead(car, player))
            {
                bool isKnown = car.MicroSectors.TrySecondsSincePassing(player.LapDistPct, sessionTime, out float sinceCar);
                interval = sinceCar;
                return isKnown;
            }
            bool isMeasured = player.MicroSectors.TrySecondsSincePassing(car.LapDistPct, sessionTime, out float sincePlayer);
            interval = -sincePlayer;
            return isMeasured;
        }

        /// <summary>Ahead on track within half a lap, whatever the lap count.</summary>
        private static bool IsAhead(RaceDataModel car, RaceDataModel player)
        {
            float distance = car.LapDistPct - player.LapDistPct;
            distance -= MathF.Round(distance);
            return distance >= 0;
        }

        /// <summary>
        /// Whole laps between the cars based on total distance driven, so a car that just
        /// crossed the line ahead of the player is not counted as a lap ahead.
        /// </summary>
        private static int CalculateLapDelta(RaceDataModel car, RaceDataModel player)
        {
            float carDistance = car.Lap + car.LapDistPct;
            float playerDistance = player.Lap + player.LapDistPct;
            return (int)MathF.Round(carDistance - playerDistance);
        }

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

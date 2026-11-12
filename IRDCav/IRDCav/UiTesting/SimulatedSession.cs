using IRDCav.Models;
using System;
using System.Linq;

namespace IRDCav.UiTesting
{
    /// <summary>Session header and fuel data of the simulated race, derived from the race time.</summary>
    internal static class SimulatedSession
    {
        private const double SessionSeconds = 60 * 60;
        private const double SecondsPerIncident = 5 * 60;
        private const int IncidentLimit = 17;

        private const float TankCapacity = 60f;
        private const double StintSeconds = 30 * 60;
        private const float MinConsumption = 2.71f;
        private const float AvgConsumption = 2.85f;
        private const float MaxConsumption = 3.02f;

        public static SessionInfoModel Info(SimulatedRace race)
        {
            DemoDriver[] playerClass = PlayerClassDrivers();
            return new SessionInfoModel
            {
                AirTemp = "24°C",
                SurfaceTemp = "37°C",
                Precipitation = "0%",
                Humidity = "48%",
                LapsString = $"{race.Player.Lap + 1}",
                DriverClassName = playerClass[0].CarClass.Name,
                IncidentCount = $"{(int)(race.RaceSeconds / SecondsPerIncident)}/{IncidentLimit}",
                SessionType = "R",
                SOF = (int)playerClass.Average(driver => driver.IRating),
                DriverCount = playerClass.Length,
                TimeRemain = TimeRemain(race),
            };
        }

        public static FuelDataModel Fuel(SimulatedRace race)
        {
            float lapTime = race.Player.EstLapTime;
            float minutesRemaining = (float)(TimeRemain(race) / 60);
            float minutesPerLap = (float)(lapTime / 60);
            float level = FuelLevel(race.RaceSeconds, minutesPerLap);
            return new FuelDataModel
            {
                Level = level,
                MinConsumption = MinConsumption,
                AvgConsumption = AvgConsumption,
                MaxConsumption = MaxConsumption,
                LastConsumption = race.Player.LastLapTime / lapTime * AvgConsumption,
                FuelPerMinuteMin = MinConsumption / minutesPerLap,
                FuelPerMinuteAvg = AvgConsumption / minutesPerLap,
                FuelPerMinuteMax = MaxConsumption / minutesPerLap,
                RefuelMin = level - minutesRemaining * MinConsumption / minutesPerLap,
                RefuelAvg = level - minutesRemaining * AvgConsumption / minutesPerLap,
                RefuelMax = level - minutesRemaining * MaxConsumption / minutesPerLap,
            };
        }

        private static double TimeRemain(SimulatedRace race) => Math.Max(0, SessionSeconds - race.RaceSeconds);

        /// <summary>The tank is filled at the start of every stint.</summary>
        private static float FuelLevel(double raceSeconds, float minutesPerLap)
        {
            float stintMinutes = (float)(raceSeconds % StintSeconds / 60);
            return TankCapacity - stintMinutes * AvgConsumption / minutesPerLap;
        }

        private static DemoDriver[] PlayerClassDrivers()
        {
            DemoCarClass playerClass = DemoRoster.Drivers.First(driver => driver.Name == DemoRoster.PlayerName).CarClass;
            return DemoRoster.Drivers.Where(driver => driver.CarClass == playerClass).ToArray();
        }
    }
}

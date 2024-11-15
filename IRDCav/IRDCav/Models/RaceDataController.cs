using IRSDKSharper;
using System;
using System.Collections.Generic;
using System.Linq;
using static IRSDKSharper.IRacingSdkSessionInfo.DriverInfoModel;
using static IRSDKSharper.IRacingSdkSessionInfo.SessionInfoModel.SessionModel;

namespace IRDCav.Models
{
    public class RaceDataController
    {
        private int _playerId = 0;
        private RaceDataModel[] _raceData = new RaceDataModel[IRacingSdkConst.MaxNumCars];

        public RaceDataController()
        {
            Clear();
        }

        public void Clear()
        {
            for (int i = 0; i < IRacingSdkConst.MaxNumCars; i++)
            {
                _raceData[i] = new RaceDataModel();
            }
        }

        public void SetPlayerId(int id)
        {
            _playerId = id;
            _raceData[_playerId].IsMe = true;
            _raceData[_playerId].IsActive = true;
        }

        public void SetFastestLap(int id)
        {
            if (id < IRacingSdkConst.MaxNumCars)
            {
                _raceData[id].IsFastest = true;
            }
        }

        public void SetFromLiveDataModel(int id, LiveDataModel raceData)
        {
            _raceData[id].SetFromLiveDataModel(raceData);
        }

        public void SetFromResultsModel(int id, ResultsModel results)
        {
            _raceData[id].SetFromResultsModel(results);
        }

        public void SetFromDriverModel(int id, DriverModel driver)
        {
            _raceData[id].SetFromDriverModel(driver);
        }

        public void SetFromPositionModel(int id, PositionModel position)
        {
            _raceData[id].SetFromPositionModel(position);
        }

        public void CalculateBestLaps()
        {
            foreach (RaceDataModel rdm in _raceData)
            {
                rdm.IsFastest = false;
            }

            // Only calculate for active cars and get groups sorted by class
            var calculatedRaceData = _raceData.ToList().Where(x => x.ClassStr != string.Empty).GroupBy(x => x.ClassStr);

            foreach (var raceData in calculatedRaceData)
            {
                // First in order is the fastest lap in class
                try
                {
                    RaceDataModel raceDataModel = raceData.Where(x => x.FastestLapTime > 0).OrderBy(x => x.FastestLapTime).ToArray()[0];
                    if (raceDataModel != null)
                    {
                        raceDataModel.IsFastest = true;
                        _raceData[raceDataModel.Id] = raceDataModel;
                    }
                } catch { }
            }
        }

        public List<RaceDataModel> GetRelativeViewRaceData(int totalCount)
        {
            int idx = 0;
            int localPlayerId = 0;
            int lowerBound = 0;
            int upperBound = 0;
            int offset = 0;
            int middleIdx = totalCount / 2;

            RaceDataModel[] trimmedRaceData = new RaceDataModel[totalCount];
            for (int i = 0; i < trimmedRaceData.Length; i++)
            {
                trimmedRaceData[i] = new RaceDataModel();
            }

            // Only take active cars into consideration and sort by interval
            RaceDataModel[] sortedRaceData = _raceData.ToList()
                .Where(x => x.ConsiderForRelative && !x.IsPaceCar)
                .OrderBy(x => x.Interval)
                .Reverse()
                .ToArray();

            if (sortedRaceData.Length > 0)
            {
                // Get relative player ID in the new sorted list
                for (int i = 0; i < sortedRaceData.Length; i++)
                {
                    if (sortedRaceData[i].IsMe)
                    {
                        localPlayerId = i;
                        break;
                    }
                }
                // Set bounds that are determined with totalCount
                lowerBound = localPlayerId - totalCount / 2;

                if (lowerBound < 0)
                {
                    offset = -lowerBound;
                    lowerBound = 0;
                }

                upperBound = lowerBound + totalCount;

                if (upperBound > sortedRaceData.Length)
                {
                    upperBound = sortedRaceData.Length;
                }
                
                for (int carId = lowerBound; carId < upperBound; carId++)
                {
                    // Set player position in the middle of the trimmedRaceData array
                    if (idx + offset >= 0 && idx + offset < trimmedRaceData.Length)
                    {
                        trimmedRaceData[idx + offset] = sortedRaceData[carId];
                        trimmedRaceData[idx + offset].Interval = Math.Abs(trimmedRaceData[idx + offset].Interval);
                    }
                    idx++;
                }
            }

            return trimmedRaceData.ToList();
        }

        public List<RaceDataModel> GetResultsViewRaceData(int totalCount)
        {
            int idx = 0;
            int localPlayerId = 0;
            int lowerBound = 0;
            int upperBound = 0;

            // Only take cars with drivers into consideration and sort by class position
            // If no class position is available append to the end.
            RaceDataModel[] sortedRaceData = _raceData.ToList()
                .Where(x => x.Name != string.Empty && !x.IsPaceCar)
                .OrderByDescending(x => x.Position > 0)
                .ThenBy(x => x.Position)
                .ToArray();

            // Get relative player ID in the new sorted list
            for (int i = 0; i < sortedRaceData.Length; i++)
            {
                if (sortedRaceData[i].IsMe)
                {
                    localPlayerId = i;
                    break;
                }
            }
            // Set bounds that are determined with totalCount
            lowerBound = localPlayerId - totalCount / 2;
            if (lowerBound < 0) lowerBound = 0;
            upperBound = lowerBound + totalCount;

            if (sortedRaceData.Length < upperBound)
            {
                upperBound = sortedRaceData.Length;
            }

            RaceDataModel[] trimmedRaceData = new RaceDataModel[upperBound - lowerBound];

            for (int carId = lowerBound; carId < upperBound; carId++)
            {
                trimmedRaceData[idx] = sortedRaceData[carId];
                idx++;
            }

            return trimmedRaceData.ToList();
        }
    }
}

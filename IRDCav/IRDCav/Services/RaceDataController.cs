using IRDCav.Models;
using IRSDKSharper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IRSDKSharper.IRacingSdkEnum;
using static IRSDKSharper.IRacingSdkSessionInfo.DriverInfoModel;
using static IRSDKSharper.IRacingSdkSessionInfo.SessionInfoModel.SessionModel;

namespace IRDCav.Services
{
    public delegate void RaceDataReadyHandler();

    internal class RaceDataController
    {
        public static int MICROSECTOR_COUNT = 20;
        public static float MICROSECTOR_LENGTH = 1.0f / MICROSECTOR_COUNT;

        public event RaceDataReadyHandler? OnDataReady;

        private int _playerId = 0;
        private double _lastTimeRemain = 0;
        private RaceDataModel[] _raceData = new RaceDataModel[IRacingSdkConst.MaxNumCars];

        public RaceDataController()
        {
            Clear();
        }

        public void Update(double timeRemain)
        {
            if (_lastTimeRemain != 0)
            {
                Task t = new Task(() =>
                {
                    double elapsedTime = _lastTimeRemain - timeRemain;
                    _lastTimeRemain = timeRemain;

                    CalculateIntervals(elapsedTime);
                    CalculateBestLaps();

                    OnDataReady?.Invoke();
                });

                t.Start();
            }
            else
            {
                _lastTimeRemain = timeRemain;
            }
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
            _raceData[_playerId].ConsiderForRelative = true;
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

        // Check if range between microsectors is valid for the interval calculation
        // If any element in the range is zero for both last and best microsectors this returns false.
        public float CalculateIntervalFromSectors(int carId, int floor, int ceil)
        {
            float interval = 0;

            for (int sectorId = floor; sectorId < ceil; sectorId++)
            {
                if (_raceData[carId].MicroSectors[sectorId] == 0.0f &&
                    _raceData[carId].BestMicroSectors[sectorId] == 0.0f)
                {
                    interval = 0;
                    break;
                }
                else
                {
                    float sectorToAdd = _raceData[carId].MicroSectors[sectorId] > _raceData[carId].BestMicroSectors[sectorId] ? _raceData[carId].BestMicroSectors[sectorId] : _raceData[carId].MicroSectors[sectorId];
                    float sectorStart = sectorId * MICROSECTOR_LENGTH;
                    float sectorEnd = (sectorId + 1) * MICROSECTOR_LENGTH;

                    // Lower sector bound. Interpolate between first and second point.
                    if (sectorId == floor)
                    {
                        // Wrap if car went over the finish line
                        int sid = sectorId + 1;
                        if (sid >= MICROSECTOR_COUNT)
                        {
                            sid = 0;
                        }
                        // Select whichever is quicker
                        float sectorToAddNext = _raceData[carId].BestMicroSectors[sid];
                        if (sectorToAddNext == 0.0f || sectorToAddNext > _raceData[carId].MicroSectors[sid])
                        {
                            sectorToAddNext = _raceData[carId].MicroSectors[sid];
                        }
                        sectorToAdd = Interpolation.LinearInterpolate(_raceData[carId].LapDistPct, sectorStart, sectorEnd, sectorToAdd, sectorToAddNext);
                    }
                    // Upper sector bound. Interpolate between last and second to last point.
                    else if (sectorId == ceil - 1)
                    {
                        // Wrap if car went over the finish line
                        int sid = sectorId - 1;
                        if (sid < 0)
                        {
                            sid = MICROSECTOR_COUNT - 1;
                        }
                        float sectorToAddPrev = _raceData[carId].BestMicroSectors[sid];
                        if (sectorToAddPrev == 0.0f || sectorToAddPrev > _raceData[carId].MicroSectors[sid])
                        {
                            sectorToAddPrev = _raceData[carId].MicroSectors[sid];
                        }
                        sectorToAdd = Interpolation.LinearInterpolate(_raceData[carId].LapDistPct, sectorStart, sectorEnd, sectorToAddPrev, sectorToAdd);
                    }

                    interval += sectorToAdd;
                }
            }

            return interval;
        }

        private int GetSectorId(float lapDist, bool isFloor)
        {
            float normalizedId = (float)(lapDist * 100 / (100.0f / MICROSECTOR_COUNT));
            if (isFloor)
            {
                return (int)Math.Floor(normalizedId);
            }
            else
            {
                if (lapDist < 1.0f)
                    return (int)Math.Ceiling(normalizedId);
                return (int)Math.Floor(normalizedId);
            }
        }

        private void CalculateIntervals(double elapsedTime)
        {
            for (int carId = 0; carId < IRacingSdkConst.MaxNumCars; carId++)
            {
                if ((_raceData[carId].SessionFlags & Flags.Checkered) != Flags.Checkered &&
                    _raceData[carId].SessionFlags != 0 &&
                    _raceData[carId].TrackSurface != TrkSurf.SurfaceNotInWorld &&
                    _raceData[carId].TrackSurface != TrkSurf.UndefinedMaterial)
                {
                    int lapcountS = _raceData[_playerId].LapsCompleted;
                    int lapcountC = _raceData[carId].LapsCompleted;

                    if (lapcountC >= 0)
                    {
                        ////////////////////////////////
                        // First interval calculation //
                        ////////////////////////////////
                        // Detects when a new sector is started.
                        // Elapsed time is then added into buckets.
                        // If a sector switch happened in last cycle the elapsed time is split
                        // into both buckets depending on the overlap.
                        float sectorStart;
                        float sectorEnd;
                        float correction;
                        int sectorIdx = GetSectorId(_raceData[carId].LapDistPct, true);
                        int lastSectorIdx = GetSectorId(_raceData[carId].LastLapDistPct, true);

                        if (sectorIdx > lastSectorIdx ||
                            (sectorIdx == 0 && sectorIdx < lastSectorIdx) &&
                            _raceData[carId].LastLapDistPct != _raceData[carId].LapDistPct)
                        {
                            sectorEnd = (lastSectorIdx + 1) * MICROSECTOR_LENGTH;
                            correction = sectorEnd - _raceData[carId].LastLapDistPct;

                            _raceData[carId].MicroSectors[lastSectorIdx] += (float)Math.Round(elapsedTime * correction, 3);

                            // No best sector time exists yet or best sector time was slower.
                            if ((_raceData[carId].MicroSectors[sectorIdx] - _raceData[carId].BestMicroSectors[sectorIdx]) >= (_raceData[carId].MicroSectors[sectorIdx] / 2) ||
                                _raceData[carId].BestMicroSectors[sectorIdx] > _raceData[carId].MicroSectors[sectorIdx] &&
                                _raceData[carId].MicroSectors[sectorIdx] != 0)
                            {
                                _raceData[carId].BestMicroSectors[sectorIdx] = _raceData[carId].MicroSectors[sectorIdx];
                            }

                            _raceData[carId].MicroSectors[sectorIdx] = 0;

                            sectorStart = sectorIdx * MICROSECTOR_LENGTH;
                            correction = _raceData[carId].LapDistPct - sectorStart;

                            _raceData[carId].MicroSectors[sectorIdx] += (float)Math.Round(elapsedTime * correction, 3);
                        }

                        int floor = 0;
                        int ceil = 0;
                        float calculatedInterval = 0;
                        if (_raceData[carId].LapDistPct > _raceData[_playerId].LapDistPct)
                        {
                            floor = GetSectorId(_raceData[_playerId].LapDistPct, true);
                            ceil = GetSectorId(_raceData[carId].LapDistPct, false);
                        }
                        else
                        {
                            floor = GetSectorId(_raceData[carId].LapDistPct, true);
                            ceil = GetSectorId(_raceData[_playerId].LapDistPct, false);
                        }

                        if (_raceData[carId].TrackLocation != TrkLoc.AproachingPits &&
                            _raceData[carId].TrackLocation != TrkLoc.InPitStall &&
                            _raceData[carId].OnPitRoad == false)
                        {
                            calculatedInterval = CalculateIntervalFromSectors(carId, floor, ceil);
                        }

                        /////////////////////////////////
                        // Second interval calculation //
                        /////////////////////////////////
                        // More straight forward approach.
                        // This method is always available and is used as a fallback
                        // when the above method yields no result.
                        float delta = 0;
                        int lapDelta = lapcountC - lapcountS;
                        float lapTimeRef = _raceData[carId].FastestLapTime;
                        float lapTimePlayer = _raceData[_playerId].FastestLapTime;
                        float L = 0;
                        float timeDiff = 0;

                        if (lapTimeRef <= 0)
                        {
                            lapTimeRef = _raceData[carId].EstLapTime;
                        }

                        if (lapTimePlayer <= 0)
                        {
                            lapTimePlayer = _raceData[_playerId].EstLapTime;
                        }

                        float C = _raceData[carId].LapDistPct;
                        float S = _raceData[_playerId].LapDistPct;

                        // Does the delta between us and the other car span across the start/finish line?
                        bool wrap = Math.Abs(_raceData[carId].LapDistPct - _raceData[_playerId].LapDistPct) > 0.5f;

                        timeDiff = _raceData[carId].LapDistPct - _raceData[_playerId].LapDistPct;

                        if (timeDiff > 0)
                        {
                            timeDiff *= lapTimePlayer;
                            L = lapTimePlayer;
                        }
                        else
                        {
                            timeDiff *= lapTimeRef;
                            L = lapTimeRef;
                        }

                        if (wrap)
                        {
                            delta = S > C ? timeDiff + L : timeDiff - L;
                            lapDelta += S > C ? -1 : 1;
                        }
                        else
                        {
                            delta = timeDiff;
                        }

                        if (sectorIdx == lastSectorIdx &&
                            _raceData[carId].LastLapDistPct != _raceData[carId].LapDistPct)
                        {
                            _raceData[carId].MicroSectors[sectorIdx] += (float)Math.Round(elapsedTime, 3);
                        }

                        _raceData[carId].Interval = Math.Round(calculatedInterval, 1) != 0 && delta > calculatedInterval ? calculatedInterval : delta;
                        _raceData[carId].LapDelta = lapDelta;
                        _raceData[carId].LastLapDistPct = _raceData[carId].LapDistPct;

                        if (_raceData[carId].Interval != 0 || _raceData[carId].IsMe && ((_raceData[carId].SessionFlags & Flags.Checkered) != Flags.Checkered))
                        {
                            _raceData[carId].ConsiderForRelative = true;
                        }
                    }
                }
                else
                {
                    _raceData[carId].ConsiderForRelative = false;
                }
            }
        }

        private void CalculateBestLaps()
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
                RaceDataModel[] raceDataModels = raceData.Where(x => x.FastestLapTime > 0).OrderBy(x => x.FastestLapTime).ToArray();
                if (raceDataModels != null)
                {
                    if (raceDataModels.Length > 0)
                    {
                        RaceDataModel raceDataModel = raceDataModels[0];
                        if (raceDataModel != null)
                        {
                            raceDataModel.IsFastest = true;
                            _raceData[raceDataModel.Id] = raceDataModel;
                        }
                    }
                }
            }
        }

        //************************************************************************************************
        // MOVE TO SEPERATE CLASS
        //************************************************************************************************
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
                .OrderByDescending(x => x.Interval)
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
                        //trimmedRaceData[idx + offset].Interval = Math.Abs(trimmedRaceData[idx + offset].Interval);
                    }
                    idx++;
                }
            }

            return trimmedRaceData.ToList();
        }

        //************************************************************************************************
        // MOVE TO SEPERATE CLASS
        //************************************************************************************************
        public List<RaceDataModel> GetResultsViewRaceData(int totalCount, int perClassCount, int numCarClasses, string sessionType)
        {
            int localPlayerId = 0;
            int lowerBound = 0;
            int upperBound = 0;
            int carClassId = 0;
            string previousClass = string.Empty;

            RaceDataModel[][] sortedRaceDataNew = new RaceDataModel[numCarClasses][];
            RaceDataModel[] raceDataCopy = new RaceDataModel[_raceData.Length];

            List<string> classOrder = new List<string>();
            List<RaceDataModel> returnList = new List<RaceDataModel>();

            // Only take cars with drivers into consideration and sort by class and position
            // TODO: At race start the order is random. Would be great if it got sorted by interval.
            for (int i = 0; i < _raceData.Length; i++)
            {
                raceDataCopy[i] = _raceData[i];
            }
            Array.Sort(raceDataCopy, delegate (RaceDataModel x, RaceDataModel y) {
                return x.Position.CompareTo(y.Position);
            });

            // Get list of classes ordered by position. Fastest should come first.
            foreach (RaceDataModel rdm in raceDataCopy)
            {
                if (rdm.Name != string.Empty && !rdm.IsPaceCar && rdm.Position > 0)
                {
                    if (rdm.ClassStr != previousClass && classOrder.Contains(rdm.ClassStr) == false)
                    {
                        classOrder.Add(rdm.ClassStr);

                        if (classOrder.Count == numCarClasses)
                        {
                            break;
                        }
                    }
                    previousClass = rdm.ClassStr;
                }
            }

            // Sort by class
            carClassId = 0;
            foreach (string cls in classOrder)
            {
                int id = 0;
                RaceDataModel[] rdc = new RaceDataModel[perClassCount + 1];

                if (_raceData[_playerId].ClassStr == cls)
                {
                    rdc = new RaceDataModel[IRacingSdkConst.MaxNumCars];
                }

                foreach (RaceDataModel rdm in raceDataCopy)
                {
                    if (rdm.Name != string.Empty && !rdm.IsPaceCar && rdm.Position > 0)
                    {
                        if (id < rdc.Length)
                        {
                            if (rdm.ClassStr == cls)
                            {
                                if (rdm.IsMe) localPlayerId = id;
                                rdc[id] = rdm;
                                id++;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (_raceData[_playerId].ClassStr == cls)
                {
                    // Set bounds that are determined with totalCount
                    lowerBound = localPlayerId - totalCount / 2;
                    if (lowerBound < 0) lowerBound = 0;
                    if (lowerBound < perClassCount) perClassCount = lowerBound;

                    upperBound = lowerBound + totalCount;

                    if (rdc.Length < upperBound)
                    {
                        upperBound = rdc.Length;
                    }

                    int idx = 0;
                    // Check if extra line needs to be inserted. White line is inserted
                    // if leaderboard of same class has to be split. This happens when
                    // the center position of the player is so far down, that the perClassCount
                    // wouldn't be fully shown.
                    int arrayLength = upperBound - lowerBound + perClassCount;
                    if (perClassCount > 0) arrayLength++;
                    sortedRaceDataNew[carClassId] = new RaceDataModel[arrayLength];

                    for (int carId = 0; carId < perClassCount; carId++)
                    {
                        sortedRaceDataNew[carClassId][idx] = rdc[carId];
                        idx++;
                    }

                    if (perClassCount > 0)
                    {
                        sortedRaceDataNew[carClassId][idx] = new RaceDataModel();
                        idx++;
                    }

                    for (int carId = lowerBound; carId < upperBound; carId++)
                    {
                        sortedRaceDataNew[carClassId][idx] = rdc[carId];
                        idx++;
                    }
                }
                else
                {
                    // Add empty line
                    rdc[rdc.Length - 1] = new RaceDataModel();
                    sortedRaceDataNew[carClassId] = rdc;
                }
                carClassId++;
            }

            foreach (RaceDataModel[] crdm in sortedRaceDataNew)
            {
                if (crdm != null)
                {
                    float fastestLapTime = 0;
                    foreach (RaceDataModel rdm in crdm)
                    {
                        if (rdm != null)
                        {
                            if (rdm.FastestLapTime > 0 && sessionType != "R")
                            {
                                if (rdm.IsFastest)
                                {
                                    rdm.Gap = 0;
                                    fastestLapTime = rdm.FastestLapTime;
                                }
                                else
                                {
                                    rdm.Gap = rdm.FastestLapTime - fastestLapTime;
                                }
                            }
                            else
                            {
                                rdm.Gap = rdm.Interval;
                            }

                            returnList.Add(rdm);
                        }
                    }
                }
            }

            return returnList;
        }
    }
}

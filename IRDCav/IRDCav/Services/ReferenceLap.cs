using IRDCav.LapFiles;
using System;
using System.Linq;

namespace IRDCav.Services
{
    /// <summary>
    /// The lap the sectors are compared with: the player's all-time best with this car at this track,
    /// from iRacing's lap file, and the best lap of the car's class there. iRacing writes a new best to
    /// its file only later, e.g. when the session ends; until then the tracker hands it over. Runs on the
    /// telemetry thread.
    /// </summary>
    internal sealed class ReferenceLap
    {
        private readonly ILapFileLibrary _library;
        private LapFileKey _key = LapFileKey.Unknown;
        private int _sessionCarClassId;
        private int _sectorCount;
        private DateTime _loadedFileTime = DateTime.MinValue;

        public ReferenceLap(ILapFileLibrary library)
        {
            _library = library;
        }

        /// <summary>Seconds; 0 when there is none.</summary>
        public float LapTime { get; private set; }

        /// <summary>Seconds per sector; 0 for an unknown sector.</summary>
        public float[] SectorTimes { get; private set; } = Array.Empty<float>();

        /// <summary>The fastest best lap of the class at this track, this car's included; 0 when there is none.</summary>
        public float ClassBest { get; private set; }

        public void Load(LapFileKey key, int sessionCarClassId, int sectorCount)
        {
            _key = key;
            _sessionCarClassId = sessionCarClassId;
            _sectorCount = sectorCount;
            LapTime = 0;
            SectorTimes = new float[sectorCount];
            ClassBest = 0;
            _loadedFileTime = DateTime.MinValue;
            if (key.IsKnown)
            {
                LoadFiles();
            }
        }

        /// <summary>Picks up a lap file iRacing has rewritten since it was loaded.</summary>
        public void ReloadIfChanged()
        {
            if (_key.IsKnown && _library.LastWritten(_key) != _loadedFileTime)
            {
                Load(_key, _sessionCarClassId, _sectorCount);
            }
        }

        /// <summary>A new best iRacing already compares with, before it is in the file.</summary>
        public void Adopt(float lapTime, float[] sectorTimes)
        {
            LapTime = lapTime;
            SectorTimes = sectorTimes.Length == _sectorCount ? sectorTimes : new float[_sectorCount];
            ClassBest = Faster(ClassBest, lapTime);
        }

        /// <summary>The class comes from the lap file when it names one, so all the player's files of the class match.</summary>
        private void LoadFiles()
        {
            _loadedFileTime = _library.LastWritten(_key);
            int carClassId = _sessionCarClassId;
            if (_library.TryLoadBest(_key, out IRacingLapFile lap))
            {
                LapTime = lap.LapTime;
                SectorTimes = lap.SectorTimes.Count == _sectorCount ? lap.SectorTimes.ToArray() : new float[_sectorCount];
                carClassId = lap.CarClassId > 0 ? lap.CarClassId : carClassId;
            }
            ClassBest = Faster(_library.ClassBest(_key, carClassId), LapTime);
        }

        private static float Faster(float lapTime, float other) =>
            lapTime <= 0 ? other : other <= 0 ? lapTime : Math.Min(lapTime, other);
    }
}

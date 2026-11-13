using System;
using System.Collections.Generic;
using System.Text;

namespace IRDCav.LapFiles
{
    /// <summary>
    /// A best lap as iRacing saves it in Documents/iRacing/lapfiles (".blap"). The format is not
    /// documented; the layout below was read from version 3 files, and every value is checked
    /// before use, so a file of another layout is ignored rather than misread.
    /// </summary>
    /// <param name="CarClassId">iRacing's class of the car; 0 when the file doesn't name one.</param>
    /// <param name="LapTime">Seconds.</param>
    /// <param name="SectorTimes">Seconds per sector, at iRacing's sector boundaries of the track.</param>
    public sealed record IRacingLapFile(int CarClassId, float LapTime, IReadOnlyList<float> SectorTimes)
    {
        private const string Magic = "BLAP";
        private const int SupportedVersion = 3;
        private const int VersionOffset = 4;
        private const int CarClassIdOffset = 136;
        private const int LapTimeOffset = 1460;
        private const int SectorCountOffset = 1468;
        private const int FirstSectorOffset = 1472;
        private const int SectorRecordSize = 32;
        private const int SectorTimeInRecord = 24;
        private const int MaxSectors = 64;
        private const float MaxLapTime = 3600;

        public static bool TryParse(byte[] bytes, out IRacingLapFile lap)
        {
            lap = new IRacingLapFile(0, 0, Array.Empty<float>());
            if (!HasSupportedHeader(bytes))
            {
                return false;
            }

            float lapTime = BitConverter.ToSingle(bytes, LapTimeOffset);
            int sectorCount = BitConverter.ToInt32(bytes, SectorCountOffset);
            if (!IsTime(lapTime) || sectorCount < 1 || sectorCount > MaxSectors ||
                bytes.Length < FirstSectorOffset + sectorCount * SectorRecordSize)
            {
                return false;
            }

            float[] sectors = ReadSectorTimes(bytes, sectorCount);
            if (Array.Exists(sectors, time => !IsTime(time)))
            {
                return false;
            }
            lap = new IRacingLapFile(Math.Max(BitConverter.ToInt32(bytes, CarClassIdOffset), 0), lapTime, sectors);
            return true;
        }

        private static bool HasSupportedHeader(byte[] bytes) =>
            bytes.Length > FirstSectorOffset &&
            Encoding.ASCII.GetString(bytes, 0, Magic.Length) == Magic &&
            BitConverter.ToInt32(bytes, VersionOffset) == SupportedVersion;

        private static float[] ReadSectorTimes(byte[] bytes, int sectorCount)
        {
            var sectors = new float[sectorCount];
            for (int sector = 0; sector < sectorCount; sector++)
            {
                sectors[sector] = BitConverter.ToSingle(bytes, FirstSectorOffset + sector * SectorRecordSize + SectorTimeInRecord);
            }
            return sectors;
        }

        private static bool IsTime(float seconds) => float.IsFinite(seconds) && seconds > 0 && seconds < MaxLapTime;
    }
}

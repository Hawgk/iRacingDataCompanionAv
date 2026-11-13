using System;
using System.Diagnostics;
using System.IO;

namespace IRDCav.LapFiles
{
    /// <summary>
    /// Reads iRacing's best lap files: "{lapfiles}/{track}/{customer id}_{car}.blap". The files
    /// belong to iRacing and are only read. An unreadable file counts as missing.
    /// </summary>
    public sealed class IRacingLapFileLibrary : ILapFileLibrary
    {
        private const string BestLapExtension = ".blap";

        private readonly string _rootDirectory;

        public IRacingLapFileLibrary(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
        }

        public static string DefaultDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "iRacing", "lapfiles");

        public bool TryLoadBest(LapFileKey key, out IRacingLapFile lap) => TryRead(BestLapFile(key), out lap);

        public DateTime LastWritten(LapFileKey key)
        {
            string filePath = BestLapFile(key);
            return File.Exists(filePath) ? File.GetLastWriteTimeUtc(filePath) : DateTime.MinValue;
        }

        public float ClassBest(LapFileKey key, int carClassId)
        {
            string trackDirectory = Path.Combine(_rootDirectory, key.TrackName);
            if (carClassId <= 0 || !Directory.Exists(trackDirectory))
            {
                return 0;
            }

            float best = 0;
            foreach (string filePath in Directory.EnumerateFiles(trackDirectory, $"{key.CustomerId}_*{BestLapExtension}"))
            {
                if (TryRead(filePath, out IRacingLapFile lap) && lap.CarClassId == carClassId && (best == 0 || lap.LapTime < best))
                {
                    best = lap.LapTime;
                }
            }
            return best;
        }

        private string BestLapFile(LapFileKey key) =>
            Path.Combine(_rootDirectory, key.TrackName, $"{key.CustomerId}_{key.CarPath}{BestLapExtension}");

        private static bool TryRead(string filePath, out IRacingLapFile lap)
        {
            lap = new IRacingLapFile(0, 0, Array.Empty<float>());
            try
            {
                return IRacingLapFile.TryParse(File.ReadAllBytes(filePath), out lap);
            }
            catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
            {
                return false;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                Trace.TraceWarning($"Could not read iRacing's lap file '{filePath}': {exception.Message}");
                return false;
            }
        }
    }
}

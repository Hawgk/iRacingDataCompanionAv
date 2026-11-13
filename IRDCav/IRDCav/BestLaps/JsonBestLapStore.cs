using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace IRDCav.BestLaps
{
    /// <summary>
    /// Keeps one JSON file per car in a folder per track: "tracks/{track - config}/{car}.json" next
    /// to config.json. Names come from iRacing, with characters a file name can't hold replaced.
    /// An unreadable file counts as missing and a failed save is logged; neither interrupts the overlays.
    /// </summary>
    public sealed class JsonBestLapStore : IBestLapStore
    {
        private const string FileExtension = ".json";
        private const char Replacement = '_';

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly string _rootDirectory;

        public JsonBestLapStore(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
        }

        public static string DefaultDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "iRDCav", "tracks");

        public bool TryLoad(CarAtTrack car, [NotNullWhen(true)] out BestLapRecord? record) =>
            TryRead(CarFile(car.Track, car.TrackConfig, car.Car), out record);

        public float ClassBest(CarAtTrack car)
        {
            string trackDirectory = TrackDirectory(car.Track, car.TrackConfig);
            if (!Directory.Exists(trackDirectory))
            {
                return 0;
            }

            float best = 0;
            foreach (string filePath in Directory.EnumerateFiles(trackDirectory, "*" + FileExtension))
            {
                if (TryRead(filePath, out BestLapRecord? record) && IsFasterInClass(record, car.CarClass, best))
                {
                    best = record.LapTime;
                }
            }
            return best;
        }

        private static bool IsFasterInClass(BestLapRecord record, string carClass, float best) =>
            record.LapTime > 0 && record.CarClasses.Contains(carClass) && (best == 0 || record.LapTime < best);

        public void Save(BestLapRecord record)
        {
            string filePath = CarFile(record.Track, record.TrackConfig, record.Car);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                // Written next to the file first, so a crash never leaves a half-written best lap behind.
                string temporaryPath = filePath + ".tmp";
                File.WriteAllText(temporaryPath, JsonSerializer.Serialize(record, SerializerOptions));
                File.Move(temporaryPath, filePath, overwrite: true);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                Trace.TraceWarning($"Could not save the best lap to '{filePath}': {exception.Message}");
            }
        }

        private static bool TryRead(string filePath, [NotNullWhen(true)] out BestLapRecord? record)
        {
            record = null;
            try
            {
                record = JsonSerializer.Deserialize<BestLapRecord>(File.ReadAllText(filePath), SerializerOptions);
            }
            catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
            {
                // No best lap saved yet for this car and track.
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
            {
                Trace.TraceWarning($"Could not read the best lap from '{filePath}', ignoring it: {exception.Message}");
            }
            return record != null;
        }

        private string TrackDirectory(string track, string trackConfig)
        {
            string name = trackConfig.Length > 0 ? $"{track} - {trackConfig}" : track;
            return Path.Combine(_rootDirectory, SafeName(name));
        }

        private string CarFile(string track, string trackConfig, string car) =>
            Path.Combine(TrackDirectory(track, trackConfig), SafeName(car) + FileExtension);

        private static string SafeName(string name)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            string safe = new(name.Select(character => invalid.Contains(character) ? Replacement : character).ToArray());
            return safe.Trim().TrimEnd('.');
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IRDCav.Layout
{
    /// <summary>
    /// Stores placements in a JSON file. A missing or unreadable file falls back to the default
    /// placements; a failed save is logged and does not interrupt the overlay.
    /// </summary>
    public sealed class JsonOverlayLayoutStore : IOverlayLayoutStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
        };

        private readonly string _filePath;
        private readonly Dictionary<string, OverlayPlacement> _placements;

        public JsonOverlayLayoutStore(string filePath)
        {
            _filePath = filePath;
            _placements = ReadFile(filePath);
        }

        public static string DefaultFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "iRDCav", "layout.json");

        public OverlayPlacement Load(OverlayDefinition overlay) =>
            _placements.TryGetValue(overlay.Id, out OverlayPlacement? placement) ? placement : overlay.DefaultPlacement;

        public void Save(string overlayId, OverlayPlacement placement)
        {
            _placements[overlayId] = placement;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
                File.WriteAllText(_filePath, JsonSerializer.Serialize(_placements, SerializerOptions));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                Trace.TraceWarning($"Could not save overlay layout to '{_filePath}': {exception.Message}");
            }
        }

        private static Dictionary<string, OverlayPlacement> ReadFile(string filePath)
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, OverlayPlacement>>(File.ReadAllText(filePath), SerializerOptions)
                    ?? new Dictionary<string, OverlayPlacement>();
            }
            catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
            {
                return new Dictionary<string, OverlayPlacement>();
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
            {
                Trace.TraceWarning($"Could not read overlay layout from '{filePath}', using defaults: {exception.Message}");
                return new Dictionary<string, OverlayPlacement>();
            }
        }
    }
}

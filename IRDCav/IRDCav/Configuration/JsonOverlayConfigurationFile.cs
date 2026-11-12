using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IRDCav.Configuration
{
    /// <summary>
    /// The user's config.json. A missing file is created with the defaults so there is always
    /// something to edit; an unreadable or invalid file falls back to the defaults.
    /// </summary>
    public sealed class JsonOverlayConfigurationFile
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters = { new JsonStringEnumConverter(), new ColorJsonConverter() },
        };

        private readonly string _filePath;

        public JsonOverlayConfigurationFile(string filePath)
        {
            _filePath = filePath;
        }

        public static string DefaultFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "iRDCav", "config.json");

        public void CreateIfMissing()
        {
            if (File.Exists(_filePath))
            {
                return;
            }
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
                File.WriteAllText(_filePath, JsonSerializer.Serialize(new OverlayConfiguration(), SerializerOptions));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                Trace.TraceWarning($"Could not create '{_filePath}': {exception.Message}");
            }
        }

        public OverlayConfiguration Read()
        {
            try
            {
                return Validated(JsonSerializer.Deserialize<OverlayConfiguration>(File.ReadAllText(_filePath), SerializerOptions));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
            {
                Trace.TraceWarning($"Using the default configuration, '{_filePath}' could not be read: {exception.Message}");
                return new OverlayConfiguration();
            }
        }

        // Explicit nulls in the file bypass the defaults, so they are rejected here.
        private static OverlayConfiguration Validated(OverlayConfiguration? configuration)
        {
            if (configuration?.ColorScheme == null || configuration.Fuel == null || configuration.Decimals == null
                || configuration.Standings?.Columns == null || configuration.Relatives?.Columns == null)
            {
                throw new InvalidDataException("A section is empty or set to null.");
            }
            int[] decimals = { configuration.Decimals.Gap, configuration.Decimals.Interval };
            if (decimals.Any(value => value < 0 || value > DecimalsConfiguration.Max))
            {
                throw new InvalidDataException($"Decimals must be between 0 and {DecimalsConfiguration.Max}.");
            }
            return configuration;
        }
    }
}

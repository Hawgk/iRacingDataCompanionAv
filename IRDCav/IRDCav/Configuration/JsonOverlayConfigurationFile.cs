using System;
using System.Diagnostics;
using System.IO;
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
                Write(new OverlayConfiguration());
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                Trace.TraceWarning($"Could not create '{_filePath}': {exception.Message}");
            }
        }

        /// <exception cref="IOException">The file could not be written.</exception>
        /// <exception cref="UnauthorizedAccessException">The file or its folder is not writable.</exception>
        public void Write(OverlayConfiguration configuration)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(configuration, SerializerOptions));
        }

        public OverlayConfiguration Read()
        {
            try
            {
                OverlayConfiguration configuration = JsonSerializer.Deserialize<OverlayConfiguration>(File.ReadAllText(_filePath), SerializerOptions)
                    ?? throw new InvalidDataException("The file is empty.");
                OverlayConfigurationValidator.Validate(configuration);
                return configuration;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
            {
                Trace.TraceWarning($"Using the default configuration, '{_filePath}' could not be read: {exception.Message}");
                return new OverlayConfiguration();
            }
        }
    }
}

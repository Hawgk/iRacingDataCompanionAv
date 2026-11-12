using Avalonia.Media;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IRDCav.Configuration
{
    /// <summary>Reads #RRGGBB or #AARRGGBB, writes #AARRGGBB.</summary>
    internal sealed class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string text = reader.GetString() ?? string.Empty;
            if (!text.StartsWith('#') || !Color.TryParse(text, out Color color))
            {
                throw new JsonException($"'{text}' is not a color. Use #RRGGBB or #AARRGGBB.");
            }
            return color;
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options) =>
            writer.WriteStringValue($"#{value.ToUInt32():X8}");
    }
}

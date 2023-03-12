using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataLibrary.JsonConverters;

public class IntJsonConverter : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        if (int.TryParse(value, out var intValue))
        {
            return intValue;
        };
        return null;
    }
    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
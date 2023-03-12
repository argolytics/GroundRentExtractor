using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataLibrary.JsonConverters;
public class BoolJsonConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value.StartsWith('f')) { return false; }
        return true;
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

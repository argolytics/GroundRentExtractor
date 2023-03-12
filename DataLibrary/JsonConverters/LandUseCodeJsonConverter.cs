using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataLibrary.JsonConverters;

public class LandUseCodeJsonConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        if (value.Contains("(R)"))
        {
            return "R";
        };
        if (value.Contains("(E)"))
        {
            return "E";
        };
        if (value.Contains("(C)"))
        {
            return "C";
        };
        if (value.Contains("(EC)"))
        {
            return "EC";
        };
        if (value.Contains("(CR)"))
        {
            return "CR";
        };
        if (value.Contains("(TH)"))
        {
            return "TH";
        };
        if (value.Contains("(RA)"))
        {
            return "RA";
        };
        if (value.Contains("(U)"))
        {
            return "U";
        };
        if (value.Contains("(RC)"))
        {
            return "RC";
        };
        if (value.Contains("(I)"))
        {
            return "I";
        };
        if (value.Contains("(A)"))
        {
            return "A";
        };
        if (value.Contains("(CA)"))
        {
            return "CA";
        };
        if (value.Contains("(CC)"))
        {
            return "CC";
        };
        if (value.Contains("(IC)"))
        {
            return "IC";
        };
        if (value.Contains("(M)"))
        {
            return "M";
        };
        if (value.Contains("(MA)"))
        {
            return "MA";
        };
        if (value.Contains("(NC)"))
        {
            return "NC";
        };
        if (value.Contains("(NP)"))
        {
            return "NP";
        };
        if (value.Contains("(O)"))
        {
            return "O";
        };
        if (value.Contains("(P)"))
        {
            return "P";
        };
        if (value.Contains("(S)"))
        {
            return "S";
        };
        if (value.Contains("(TS)"))
        {
            return "TS";
        };
        return null;
    }
    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

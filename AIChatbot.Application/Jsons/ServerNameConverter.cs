using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIChatbot.Application.Json;

public class ServerNameConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (string.IsNullOrEmpty(value))
            return value!;

        // Normalize server name
        return value.Replace("\\\\", "\\");
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
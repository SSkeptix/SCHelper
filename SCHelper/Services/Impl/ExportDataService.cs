using Microsoft.Extensions.Options;
using SCHelper.Dtos;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace SCHelper.Services.Impl
{
    public class ExportDataService : IExportDataService
    {
        public Task Export(string filePath, object data)
        {
            var serializationOptions = new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true,
            };
            serializationOptions.Converters.Add(new JsonStringEnumConverter());
            serializationOptions.Converters.Add(new DoubleConverter());

            var jsonData = JsonSerializer.Serialize(data, serializationOptions);
            return File.WriteAllTextAsync(filePath, jsonData);
        }
    }

    public class DoubleConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.GetDouble();

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
            => writer.WriteNumberValue(Math.Round(value, 2));
    }
}

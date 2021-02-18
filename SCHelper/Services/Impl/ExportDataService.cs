using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace SCHelper.Services.Impl
{
    public class ExportDataService : IExportDataService
    {
        public void ExportJson(string filePath, object data)
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
            File.WriteAllText(filePath, jsonData);
        }

        public void ExportCsv(string filePath, object[] data)
        {
            var configuration = new CsvConfiguration(CultureInfo.CurrentCulture) { MissingFieldFound = null };
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, configuration))
            {
                csv.WriteRecords(data);
            }
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

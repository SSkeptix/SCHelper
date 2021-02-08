using Microsoft.Extensions.Options;
using SCHelper.Dtos;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SCHelper.Services.Impl
{
    public class ExportDataService : IExportDataService
    {
        private readonly ConfigModel configModel;

        public ExportDataService(IOptions<ConfigModel> configModel)
        {
            this.configModel = configModel.Value;
        }

        public Task Export(ShipParameters data)
        {
            var userData = data with 
            {
                FireRate = 60 * data.FireRate,
                CriticalChance = 100 * data.CriticalChance
            };

            var serializationOptions = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true,
            };
            serializationOptions.Converters.Add(new JsonStringEnumConverter());
            serializationOptions.Converters.Add(new DoubleConverter());

            var jsonData = JsonSerializer.Serialize(userData, serializationOptions);
            return File.WriteAllTextAsync(this.configModel.OutputFile, jsonData);
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

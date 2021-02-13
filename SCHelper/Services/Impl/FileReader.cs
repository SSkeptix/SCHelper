using CsvHelper;
using System.Collections.Generic;
using System.Linq;

namespace SCHelper.Services.Impl
{
    public class FileReader : IFileReader
    {
        private readonly CsvFileReader csvFileReader;

        public FileReader()
        {
            this.csvFileReader = new CsvFileReader(new()
            {
                new ModificationTypeDictionaryHandler()
            });
        }

        public T[] Read<T>(string filePath)
            where T : class, new()
            => this.csvFileReader.Read<T>(filePath);
    }

    public class ModificationTypeDictionaryHandler : TypeHandler<Dictionary<ModificationType, double?>>
    {
        public override Dictionary<ModificationType, double?> ReadItem(CsvReader reader, string propertyName)
            => Utils.GetEnumValues<ModificationType>()
                .ToDictionary(x => x, x => reader.GetField<double?>(x.ToString()));
    }
}

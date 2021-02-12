using CsvHelper;
using SCHelper.Dtos;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SCHelper.Services.Impl
{
    public class FileReader : IFileReader
    {
        public SeedChipConfigModel[] ReadSeedChips(string filePath)
        {
            var records = new List<SeedChipConfigModel>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    records.Add(new SeedChipConfigModel
                    {
                        Name = csv.GetField("Name"),
                        Level = csv.GetField<int?>("Level"),
                        Parameters = Utils.GetEnumValues<ModificationType>()
                            .ToDictionary(
                                x => x,
                                x => csv.GetField<double?>(x.ToString())),
                    });
                }
            }
            return records.ToArray();
        }
    }
}

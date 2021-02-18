﻿using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SCHelper.Services.Impl
{
    public class FileReader : IFileReader
    {
        public T[] Read<T>(string filePath)
            where T : class, new()
        {
            var configuration = new CsvConfiguration(CultureInfo.CurrentCulture) { MissingFieldFound = null };
            using (var streamReader = new StreamReader(filePath))
            using (var csvReader = new CsvReader(streamReader, configuration))
            {
                return csvReader.GetRecords<T>()
                    .ToArray();
            }
        }
    }
}

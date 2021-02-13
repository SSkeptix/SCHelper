using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SCHelper.Services.Impl
{
    public class CsvFileReader
    {
        private List<ITypeHandler> types = new();

        public CsvFileReader(List<ITypeHandler> types = null)
        {
            this.types = types;
        }

        public T[] Read<T>(string filePath)
            where T : class, new()
        {
            var props = CsvFileReader.GetProperties<T>();

            var records = new List<T>();
            using (var streamReader = new StreamReader(filePath))
            using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
            {
                csvReader.Read();
                csvReader.ReadHeader();
                while (csvReader.Read())
                {
                    var item = new T();
                    foreach (var prop in props)
                    {
                        var handler = this.types?.FirstOrDefault(x => x.Type == prop.Type);
                        var value = handler != null
                            ? handler.Read(csvReader, prop.Name)
                            : csvReader.GetField(prop.Type, prop.Name);

                        prop.SetValue(item, value);
                    }
                    records.Add(item);
                }
            }
            return records.ToArray();
        }

        private static PropertyDescription[] GetProperties<T>() => CsvFileReader.GetProperties(typeof(T));

        private static PropertyDescription[] GetProperties(Type type)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.IsPublic)
                .Select(x => new PropertyDescription(
                    Name: x.Name,
                    Type: x.FieldType,
                    SetValue: new Action<object?, object?>((item, value) => x.SetValue(item, value))
                ));
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.CanRead && x.CanWrite)
                .Select(x => new PropertyDescription(
                    Name: x.Name,
                    Type: x.PropertyType,
                    SetValue: new Action<object?, object?>((item, value) => x.SetValue(item, value))
                ));
            return fields.Concat(props).ToArray();
        }

        private record PropertyDescription(
            string Name,
            Type Type,
            Action<object?, object?> SetValue);
    }

    public interface ITypeHandler
    {
        Type Type { get; }
        object? Read(CsvReader reader, string propertyName);
    }

    public abstract class TypeHandler<T> : ITypeHandler
    {
        public Type Type { get; } = typeof(T);

        public object? Read(CsvReader reader, string propertyName)
            => this.ReadItem(reader, propertyName);

        public abstract T ReadItem(CsvReader reader, string propertyName);
    }
}

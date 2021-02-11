using System;
using System.Collections.Generic;
using System.Linq;

namespace SCHelper
{
    public static class Utils
    {
        public static IEnumerable<T> GetEnumValues<T>()
            where T : Enum
            => Enum.GetValues(typeof(T)).Cast<T>();

        public static Dictionary<TKey, TValue> GetEmptyDictionary<TKey, TValue>(TValue defaultValue = default)
            where TKey : Enum
            => Utils.GetEnumValues<TKey>().ToDictionary(x => x, x => defaultValue);

        public static Dictionary<TKey, TValue> Override<TKey, TValue>(this Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> newData)
            => source.Concat(newData)
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Last().Value);

        public static IEnumerable<T[]> GetAllCombinations<T>(T[] data, int count)
        {
            if (count >= data.Length)
                yield return data;
            else
            {
                var items = Enumerable.Range(0, count).ToArray();
                yield return items.Select(x => data[x]).ToArray();

                int index = count - 1;
                while (items[0] < data.Length - count)
                {
                    if (items[index] < data.Length - count + index)
                    {
                        items[index]++;
                        while (index + 1 < count)
                        {
                            index++;
                            items[index] = items[index - 1] + 1;
                        }
                        yield return items.Select(x => data[x]).ToArray();
                    }
                    else
                    {
                        while (index < 0 || items[index] == data.Length - count + index)
                        {
                            index--;
                        }
                    }
                }
            }
        }
    }
}

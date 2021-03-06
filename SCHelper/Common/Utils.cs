﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public static async Task<T> RepeatOnBackground<T>(Task<T> mainTask, Action repetitiveAction, TimeSpan actionDelay)
        {
            var backgroundTokenSource = new CancellationTokenSource();
            var backgroundTask = Task.Run(async () =>
            {
                while (true)
                {
                    repetitiveAction();
                    await Task.Delay(actionDelay);
                }
            }, backgroundTokenSource.Token);

            await mainTask;
            backgroundTokenSource.Cancel();

            return mainTask.Result;
        }
    }
}

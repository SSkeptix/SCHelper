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
    }
}

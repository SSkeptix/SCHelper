using System;
using System.Collections.Generic;
using System.Linq;

namespace SCHelper.Dtos
{
    public static class ModificationsExtensions
    {
        public static Dictionary<ModificationType, double> ToDomainModel(this Dictionary<ModificationType, double?> data)
            => data.ToDictionary(x => x.Key, x => (x.Value ?? 0) / 100);

        public static Dictionary<ModificationType, double[]> ToDomainModel(this Dictionary<ModificationType, double?[]> data)
            => data
            .Where(x => x.Value?.Any(y => y.HasValue) == true)
            .ToDictionary(x => x.Key,
                x => x.Value.Where(y => y.HasValue).Select(y => y.Value / 100).ToArray());

        public static Dictionary<ModificationType, double?> ToUserModel(this Dictionary<ModificationType, double> data)
            => data.ToDictionary(x => x.Key, x => (double?)x.Value * 100);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace SCHelper.Dtos
{
    public static class ModificationsExtensions
    {
        public static Dictionary<ModificationType, double> ToDomainModel(this Dictionary<ModificationType, double?> data)
            => data.ToDictionary(x => x.Key, x => x.Key switch
            {
                ModificationType.DecreaseResistance => Math.Abs(x.Value ?? 0) / 100,
                _ => (x.Value ?? 0) / 100,
            });

        public static Dictionary<ModificationType, double?> ToUserModel(this Dictionary<ModificationType, double> data)
            => data.ToDictionary(x => x.Key, x => (double?)x.Value * 100);
    }
}

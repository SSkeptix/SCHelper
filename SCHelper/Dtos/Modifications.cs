using System;
using System.Collections.Generic;
using System.Linq;

namespace SCHelper.Dtos
{
    public static class ModificationsExtensions
    {
        public static Dictionary<ModificationType, double> ToDomainModel(this Dictionary<ModificationType, double?> data)
            => Utils.GetEmptyDictionary<ModificationType, double?>()
                .Override(data)
                .ToDictionary(x => x.Key, x =>
                {
                    switch (x.Key)
                    {
                        case ModificationType.DecreaseResistance:
                            return Math.Abs(x.Value ?? 0) / 100;
                        default:
                            return (x.Value ?? 0) / 100;
                    }
                });

        public static Dictionary<ModificationType, double?> ToUserModel(this Dictionary<ModificationType, double> data)
            => data.ToDictionary(x => x.Key, x => (double?)x.Value * 100);
    }
}

using SCHelper.Dtos;
using System.Collections.Generic;

namespace SCHelper.Services
{
    public interface IConversionService
    {
        Ship ToDomainModel(ShipConfigModel ship);
        Weapon ToDomainModel(WeaponConfigModel weapon);
        Dictionary<ModificationType, double> ToDomainModel(Dictionary<ModificationType, double?> modifications);

        CalculationResult ToUserDataModel(CalculationResult data);
    }
}

using SCHelper.Dtos;

namespace SCHelper.Services
{
    public interface ICalculationService
    {
        ShipParameters CalcShipParameters(Weapon weapon, Ship ship, SeedChip[] seedChip);
    }
}

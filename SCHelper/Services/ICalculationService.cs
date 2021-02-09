using SCHelper.Dtos;

namespace SCHelper.Services
{
    public interface ICalculationService
    {
        ShipParameters CalcShipParameters(CalculationCommand command);
    }
}

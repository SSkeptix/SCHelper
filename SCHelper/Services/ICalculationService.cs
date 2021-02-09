using SCHelper.Dtos;

namespace SCHelper.Services
{
    public interface ICalculationService
    {
        CalculationResult Calc(CalculationCommand command);
    }
}

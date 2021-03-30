using SCHelper.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCHelper.Services
{
    public interface ICalculationService
    {
        Task<CalculationResult[]> Calc(CalculationCommand[] commands, SeedChip[] seedChips);
    }
}

using SCHelper.Dtos;
using System.Collections.Generic;

namespace SCHelper.Services
{
    public interface ICalculationService
    {
        CalculationResult[] Calc(IEnumerable<CalculationCommand> commands, IEnumerable<SeedChip> seedChips);
    }
}

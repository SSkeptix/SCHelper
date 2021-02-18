using SCHelper.Dtos;
using System.Collections.Generic;

namespace SCHelper.Services
{
    public interface IConversionService
    {
        CalculationResult ToUserDataModel(CalculationResult data);
    }
}

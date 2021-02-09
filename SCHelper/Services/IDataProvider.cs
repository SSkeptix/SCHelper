using SCHelper.Dtos;

namespace SCHelper.Services
{
    public interface IDataProvider
    {
        Weapon[] GetWeapons();
        Ship[] GetShips();
        SeedChip[] GetSeedChips();
        CalculationCommand[] GetCalculationCommands();
    }
}

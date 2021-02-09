namespace SCHelper.Dtos
{
    public record CalculationCommand(
        Ship Ship,
        Weapon Weapon,
        SeedChip[] SeedChips);
}

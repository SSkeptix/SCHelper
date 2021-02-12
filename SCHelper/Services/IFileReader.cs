using SCHelper.Dtos;

namespace SCHelper.Services
{
    public interface IFileReader
    {
        SeedChipConfigModel[] ReadSeedChips(string filePath);
    }
}

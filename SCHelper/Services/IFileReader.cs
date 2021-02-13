namespace SCHelper.Services
{
    public interface IFileReader
    {
        T[] Read<T>(string filePath) where T : class, new();
    }
}

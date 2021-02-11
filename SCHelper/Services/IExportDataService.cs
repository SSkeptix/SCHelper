using System.Threading.Tasks;

namespace SCHelper.Services
{
    public interface IExportDataService
    {
        Task Export(string filePath, object data);
    }
}

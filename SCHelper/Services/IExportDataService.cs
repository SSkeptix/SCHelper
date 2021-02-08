using SCHelper.Dtos;
using System.Threading.Tasks;

namespace SCHelper.Services
{
    public interface IExportDataService
    {
        Task Export(string filePath, ShipParameters data);

        Task ExportData(string filePath, object data);
    }
}

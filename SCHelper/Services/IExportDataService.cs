namespace SCHelper.Services
{
    public interface IExportDataService
    {
        void ExportJson(string filePath, object data);
        void ExportCsv(string filePath, object[] data);
    }
}

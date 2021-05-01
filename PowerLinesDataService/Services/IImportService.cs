using System.Threading.Tasks;

namespace PowerLinesDataService.Services
{
    public interface IImportService
    {
        Task RunImports(string[] args);
    }
}

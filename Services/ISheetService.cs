namespace CreateFromASheet.Services
{
    using CreateFromASheet.Models;
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;

    public  interface ISheetService
    {
        Task<SheetModel> CreateTableFromCsvAsync(IFormFile file);
    }
}
namespace CreateFromASheet.Services
{
    using CreateFromASheet.Models;
    using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public  interface ISheetService
    {
        Task<List<SheetModel>> CreateTableFromCsvAsync(IFormFile file);
        
        Task<List<SheetModel>> DeleteColumnAsync(string columnName, string tableName);

        Task<List<SheetModel>> EditColumnAsync(string columnName, string tableName);
    }
}
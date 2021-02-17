
namespace CreateFromASheet.Services
{
    using CreateFromASheet.Models;
    using CsvHelper;
    using CsvHelper.Configuration;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class SheetService : ISheetService
    {
        public async Task<SheetModel> CreateTableFromCsvAsync(IFormFile file)
        {
            var table = new DataTable();
            var model = new SheetModel();

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                memoryStream.Position = 0;

                using (var sheetReader = new StreamReader(memoryStream))
                using (var csvReader = new CsvReader(sheetReader, CultureInfo.CurrentCulture))
                {

                    var records = csvReader.GetRecords<object>().ToList();
                    var headers = csvReader.Context.Reader.HeaderRecord;

                    if (records.Count == 0)
                    {
                        return null;
                        //return ResponseResult.Failed<SheetModel>(ErrorCode.EmptyFileError);
                    }

                    foreach (var item in headers)
                    {
                        table.Columns.Add(item);
                    }

                    foreach (var record in records)
                    {
                        var row = table.NewRow();
                        var json = JsonConvert.SerializeObject(record);
                        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        
                        foreach (KeyValuePair<string, string> entry in dictionary)
                        {
                            row[entry.Key] = entry.Value;

                            model.Columns.Add(entry.Key, entry.Value);
                        }

                        table.Rows.Add(row);
                    }
                }
            }

            return model;
        }
    }
}

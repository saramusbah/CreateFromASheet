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
    using System.Data.SqlClient;
    using System.Text;

    public class SheetService : ISheetService
    {
        public async Task<List<SheetModel>> CreateTableFromCsvAsync(IFormFile file)
        {
            var data = new List<SheetModel>();
            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var table = CreateTable(fileName);

            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                BadDataFound = null
            };

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                memoryStream.Position = 0;

                using (var sheetReader = new StreamReader(memoryStream))
                using (var csvReader = new CsvReader(sheetReader, config))
                {
                    var records = csvReader.GetRecords<dynamic>().ToList();
                    var headers = csvReader.Context.Reader.HeaderRecord;
                    InsertHeadersToTable(table, headers);
                    InsertDataToTable(data, table, records);
                }
            }
            var ds = new DataSet("MyDataSet");
            ds.Tables.Add(table);
            AutoSqlBulkCopy(ds);
            return data;
        }


        public async Task<List<SheetModel>> DeleteColumnAsync(string columnName, string tableName)
        {
            var data = new List<SheetModel>();

            var table = GetTableByName(tableName);
            var lst = table.Columns.Cast<DataColumn>()
                 .Where(x => x.ColumnName.StartsWith(columnName))
                 .ToList();

            foreach (DataColumn col in lst)
            {
                table.Columns.Remove(col);
            }

            //data.Add(new SheetModel { Columns = GetDataFromRows(table.Rows) });
            //data.AddRange()
            return data;
        }

        public Task<List<SheetModel>> EditColumnAsync(string columnName, string tableName)
        {
            throw new NotImplementedException();
        }

        private DataTable GetTableByName(string tableName)
        {
            throw new NotImplementedException();
        }

        private static void InsertDataToTable(List<SheetModel> data, DataTable table, List<dynamic> records)
        {
            foreach (var record in records)
            {
                var row = table.NewRow();
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    JsonConvert.SerializeObject(record));

                foreach (KeyValuePair<string, object> entry in dictionary)
                {
                    row[entry.Key.Replace("\"", "").Trim()] = entry.Value.ToString().Replace("\"", "").Trim();
                }

                data.Add(new SheetModel { Columns = dictionary });
                table.Rows.Add(row);
            }
        }

        private static void InsertHeadersToTable(DataTable table, string[] headers)
        {
            foreach (var item in headers)
            {
                var dc = new DataColumn
                {
                    ColumnName = item.Replace("\"", "").Trim(),
                    Unique = false,
                    AllowDBNull = true,
                    DataType = item.GetType()
                };

                table.Columns.Add(dc);
            }
        }

        private static DataTable CreateTable(string filename)
        {
            var table = new DataTable(filename);
            var idColumn = new DataColumn("id", Type.GetType("System.Int32"));
            idColumn.AutoIncrement = true;
            idColumn.AutoIncrementSeed = 1;
            DataColumn[] pk = new DataColumn[1] { idColumn };
            table.Columns.AddRange(pk);
            table.PrimaryKey = pk;
            return table;
        }

        /// <summary>
        ///refrence:https://stackoverflow.com/questions/24046680/how-to-create-a-table-before-using-sqlbulkcopy
        /// </summary>
        private static void AutoSqlBulkCopy(DataSet dataSet)
        {
            var sqlConnection = new SqlConnection(@"Server=(localdb)\mssqllocaldb; Initial Catalog=SheetsDb");
            sqlConnection.Open();
            foreach (DataTable dataTable in dataSet.Tables)
            {
                // checking whether the table selected from the dataset exists in the database or not
                var checkTableIfExistsCommand = new SqlCommand("IF EXISTS (SELECT 1 FROM sysobjects WHERE name =  '" + dataTable.TableName + "') SELECT 1 ELSE SELECT 0", sqlConnection);
                var exists = checkTableIfExistsCommand.ExecuteScalar().ToString().Equals("1");

                // if does not exist
                if (!exists)
                {
                    var createTableBuilder = new StringBuilder("CREATE TABLE [" + dataTable.TableName + "]");
                    createTableBuilder.AppendLine("(");

                    // selecting each column of the datatable to create a table in the database
                    foreach (DataColumn dc in dataTable.Columns)
                    {
                        createTableBuilder.AppendLine("  [" + dc.ColumnName + "] VARCHAR(MAX),");
                    }

                    createTableBuilder.Remove(createTableBuilder.Length - 1, 1);
                    createTableBuilder.AppendLine(")");

                    var createTableCommand = new SqlCommand(createTableBuilder.ToString(), sqlConnection);
                    createTableCommand.ExecuteNonQuery();
                }

                // if table exists, just copy the data to the destination table in the database
                // copying the data from datatable to database table
                using (var bulkCopy = new SqlBulkCopy(sqlConnection))
                {
                    bulkCopy.DestinationTableName = dataTable.TableName;
                    bulkCopy.WriteToServer(dataTable);
                }
            }
        }
    }
}

using System;
using System.ServiceProcess;
using KNTCommon.Business;
using ClosedXML.Excel;
using Microsoft.JSInterop;
using System.Collections;
using MySql.Data.MySqlClient;
using System.Text;
using KNTCommon.Business.Repositories;

namespace KNTCommon.Blazor.Services
{
  //  private readonly ITablesRepository TablesRepository;

    public class WindowsServiceHelper
    {
        private readonly TablesRepository tablesRepository;
        public WindowsServiceHelper(TablesRepository _tablesRepository)
        {
            tablesRepository = _tablesRepository;
        }

        public string GetServiceStatus(string serviceName)
        {
            if (!OperatingSystem.IsWindows())
                return "Unsupported OS";

            try
            {
                using ServiceController sc = new ServiceController(serviceName);
                return sc.Status.ToString();
            }
            catch (InvalidOperationException)
            {
                return "Error";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<bool> ExportToExcel(IJSRuntime jsRuntime, List<Dictionary<string, object>> data, string tableName)
        {
            if (jsRuntime == null)
            {
                Console.WriteLine("JSRuntime is null!");
                return false;
            }

            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(tableName);

                if (data.Count == 0)
                {
                    return false; // no data
                }

                // add heads
                var columnNames = data.First().Keys.ToList();
                for (int col = 0; col < columnNames.Count; col++)
                {
                    worksheet.Cell(1, col + 1).Value = columnNames[col];
                }

                // add data
                int row = 2; // 2nd row...
                foreach (var rowData in data)
                {
                    for (int col = 0; col < columnNames.Count; col++)
                    {
                        var value = rowData[columnNames[col]];

                        worksheet.Cell(row, col + 1).Value = value?.ToString() ?? ""; // string
                    }
                    row++;
                }

                // save to v MemoryStream
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var fileBytes = stream.ToArray();
                var base64 = Convert.ToBase64String(fileBytes);

                // call JavaScript for file transfer
                await jsRuntime.InvokeVoidAsync("downloadFile", $"{tableName}.xlsx",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", base64);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExportToExcel failed: {ex.Message}");
                return false;
            }
            return true;
        }

        public async Task<bool> GenerateSQLDump(string tableName, string addStrFileName)
        {
            try
            {
                string? createTableResult = await tablesRepository.GetCreateTableStatement(tableName);
                if (createTableResult == null)
                {
                    Console.WriteLine("There is no information about the structure of the table.");
                    return false;
                }

                List<string> insertQueries = await tablesRepository.GetInsertRecordsStatement(tableName);

                string currentDirectory = Directory.GetCurrentDirectory();

                using (var writer = new StreamWriter($"{currentDirectory}\\DbExport\\{tableName}{addStrFileName}.sql", false, Encoding.UTF8))
                {
                    writer.WriteLine(createTableResult);
                    writer.WriteLine(); // Empty line for separation

                    foreach (var insertQuery in insertQueries)
                    {
                        writer.WriteLine(insertQuery);
                    }
                }

                Console.WriteLine($"SQL dump on table {tableName} successful");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error SQL dump: {ex.Message}");
                return false;
            }
        }

    }
}

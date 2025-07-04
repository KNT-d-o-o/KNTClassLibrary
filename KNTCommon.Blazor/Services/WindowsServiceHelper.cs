using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using KNTCommon.Business;
using KNTCommon.Business.Repositories;
using KNTCommon.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Data.Common;
using System.ServiceProcess;
using System.Text;

namespace KNTCommon.Blazor.Services
{
  //  private readonly ITablesRepository TablesRepository;

    public class WindowsServiceHelper
    {
        private readonly ITablesRepository tablesRepository;
        public WindowsServiceHelper(ITablesRepository _tablesRepository)
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

        public static async Task<List<Dictionary<string, object?>>> LoadEntireTableAsync(string tableName)
        {
            var result = new List<Dictionary<string, object?>>();

            using var context = new EdnKntControllerMysqlContext();
            using var connection = new MySqlConnection(context.GetConnectionString());

            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM `{tableName}`";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                result.Add(row);
            }

            return result;
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

        public static async Task<bool> GenerateSQLDumpFromData(string tableName, List<Dictionary<string, object>> data)
        {
            try
            {
                if (data == null || data.Count == 0)
                    return false;

                var columnNames = data.First().Keys.ToList();

                string currentDirectory = Directory.GetCurrentDirectory();
                Directory.CreateDirectory(Path.Combine(currentDirectory, "DbExport")); // Ensure dir exists

                using var writer = new StreamWriter(Path.Combine(currentDirectory, "DbExport", $"{tableName}_filtered.sql"), false, Encoding.UTF8);

                foreach (var row in data)
                {
                    var values = columnNames.Select(col =>
                    {
                        var value = row[col];
                        if (value == null)
                            return "NULL";
                        if (value is string || value is DateTime)
                            return $"'{Convert.ToString(value)?.Replace("'", "''")}'"; // escape single quotes
                        if (value is bool)
                            return (bool)value ? "1" : "0";
                        return value.ToString(); // number
                    });

                    string insert = $"INSERT INTO `{tableName}` ({string.Join(", ", columnNames.Select(col => $"`{col}`"))}) VALUES ({string.Join(", ", values)});";
                    await writer.WriteLineAsync(insert);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating SQL dump from filtered data: {ex.Message}");
                return false;
            }
        }

    }
}

using AutoMapper;
using KNTToolsAndAccessories;
using KNTCommon.Data.Models;
using KNTCommon.BusinessIO.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;
using System.Data.SqlTypes;
using System.Numerics;
using Org.BouncyCastle.Ocsp;
using System.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Mysqlx.Crud;
using MySqlX.XDevAPI.Common;
using System.Transactions;
using MySqlX.XDevAPI.Relational;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Crypto.Utilities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Dapper;
using System;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using System.Globalization;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Drawing.Charts;
using KNTCommon.Business.Repositories;

namespace KNTCommon.BusinessIO.Repositories
{
    public class ExportRepository
    {
        /// <summary>
        /// Export to excel: iotasks.IoTaskType = 3
        /// iotaskdetails.Par1: table or view in DB
        /// iotaskdetails.Par2: order by condition
        /// iotaskdetails.Par3[0]: where condition for all tables/views
        /// iotaskdetails.Par4: additional columns: table or view;column neme;order by column;value field;join column
        /// iotaskdetails.Par5: get
        /// TableDetailOrder: order to export
        /// </summary>

        private readonly IDbContextFactory<EdnKntControllerMysqlContext> Factory;
        private readonly IMapper AutoMapper;
        private readonly Tools t = new();
        private const int MAX_ROWS_PER_SHEET = 1048576;

        public ExportRepository(IDbContextFactory<EdnKntControllerMysqlContext> factory, IMapper automapper)
        {
            Factory = factory;
            AutoMapper = automapper;
        }

        public bool ExportExcel(string tableName, string where, string order, string filePath, List<string> altCols, List<string> labels, out string errStr)
        {
            errStr = string.Empty;
            bool ret = true;

#if DEBUG
            Console.WriteLine($"start export on table {tableName} " + DateTime.Now);
#endif

            try
            {
                using (var dbContext = new EdnKntControllerMysqlContext())
                {
                    dbContext.Database.SetCommandTimeout(120); // expand DB connection timeout
                    var connectionString = dbContext.Database.GetConnectionString();
                    using (MySqlConnection connection = new(connectionString))
                    {
                        connection.Open();

                        // query on tabele
                        string query = $"SELECT t.* FROM {tableName} t";
                        if (where.Length > 0)
                        {
                            where = where.Replace(" AND ", " AND t.");
                            if (where[0] == '.')
                                query += " WHERE " + where.Substring(1, where.Length - 1);
                            else
                                query += $" WHERE t.{where}";
                        }

                        if (altCols.Count == 5 || altCols.Count == 6 && altCols[5] != "none") // alternative columns
                        {
                            // for coma decimal separator
                            if(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
                                altCols[0] += "_dsc";

                            connection.Execute("SET SESSION group_concat_max_len = 1000000;");  // bigger group_concat_max_len
                            connection.Execute("SET SESSION sql_mode = '';");                   // disable ONLY_FULL_GROUP_BY

                            if (altCols.Count >= 6 && altCols[5] == "string") // additional as string
                            {
                                query = $@"
                                        SELECT t.*, 
                                        GROUP_CONCAT(talt.{altCols[1]}, ':', talt.{altCols[3].Split('|')[0]} ORDER BY talt.{altCols[2]} SEPARATOR '|') AS Vals
                                        FROM {tableName} t
                                        LEFT JOIN {altCols[0]} talt ON t.{altCols[4]} = talt.{altCols[4]}";
                                if (where.Length > 0)
                                    query += $" WHERE t.{where}";
                                query += $" GROUP BY t.{altCols[4]}";
                            }
                            else  // additional as columns
                            {
                                // null value separator
                                string nullVal = string.Empty;

                                try
                                {
                                    int numVals = Convert.ToInt32(altCols[3].Split('|')[1]);
                                    for (int i = 1; i < numVals; i++)
                                        nullVal += "/|";
                                }
                                catch { }

                                string columnQuery = @$"
                                            SELECT GROUP_CONCAT(DISTINCT 
                                            CONCAT('COALESCE(MAX(CASE WHEN talt.{altCols[1]} = ''', talt.{altCols[1]}, 
                                            ''' THEN talt.{altCols[3].Split('|')[0]} END), \'{nullVal}\') AS `', 
                                            talt.{altCols[1]}, '`')
                                            ORDER BY talt.{altCols[2]}) AS Columns
                                        FROM {altCols[0]} talt";


                                if (where.Length > 0)
                                    columnQuery += $" WHERE talt.{where.Replace("t.", "talt.")}";

                                string cols = string.Empty;
                                try
                                {
                                    cols = connection.QuerySingle<string>(columnQuery);
                                }
                                catch { }
                                if (cols != null && cols != string.Empty)
                                {
                                    query = $@"
                                        SELECT t.*, {cols}
                                        FROM {tableName} t
                                        LEFT JOIN {altCols[0]} talt ON t.{altCols[4]} = talt.{altCols[4]}";
                                    if (where.Length > 0)
                                        query += $" WHERE t.{where}";
                                    query += $" GROUP BY t.{altCols[4]}";
                                }
                            }
                        }

                        if (order.Length > 0)
                            query += $" ORDER BY t.{order}";
                        query += $" LIMIT {MAX_ROWS_PER_SHEET}";

                        var data = connection.Query(query).ToList();

                        XLWorkbook workbook;
                        if (!File.Exists(filePath))
                        {
                            workbook = new XLWorkbook(); // new
                        }
                        else
                        {
                            try
                            {
                                workbook = new XLWorkbook(filePath); // try to open
                            }
                            catch (IOException ex)
                            {
                                errStr = ex.Message;
                                return false;
                            }
                        }

                        string labelName = tableName;
                        string labelNamePrev = string.Empty;
                        try
                        {
                            if(labels[0].Length > 0)
                                labelName = labels[0];
                            
                        }
                        catch { }
                        var worksheet = workbook.Worksheets.Add(labelName);
                        labelNamePrev = labelName;

                        // title of sheet
                        int row = 1;
                        worksheet.Cell(row, 1).Value = worksheet.Name;
                        worksheet.Cell(row, 2).Value = DateTime.Now;

                        if (data.Count > 0) // if data found
                        {

                            // columns from first row
                            var firstRow = (IDictionary<string, object>)data.First();
                            var columns = firstRow.Keys.ToList();

                            // headers of columns
                            row += 2;
                            int rowHead = row;
                            for (int i = 0; i < columns.Count; i++)
                            {
                                labelName = columns[i];
                                try
                                {
                                    if(labels[i + 1].Length > 0)
                                        labelName = labels[i + 1];                 
                                }
                                catch { }
                                worksheet.Cell(row, i + 1).Value = labelName;

                                // merge neighbour cells with same name
                                if (labelName == labelNamePrev && labelNamePrev.Length > 0)
                                    worksheet.Range(row, i, row, i + 1).Merge();
                                labelNamePrev = labelName;
                            }

                            // data rows
                            row++;
                            bool[] splitColls = new bool[columns.Count];
                            foreach (var item in data)
                            {
                                if (row >= MAX_ROWS_PER_SHEET) // max rows control
                                {
                                    worksheet.Cell(row, 1).Value = "...";
                                    break;
                                }

                                var dict = (IDictionary<string, object>)item;
                                int colOffset = 0;

                                for (int col = 0; col < columns.Count; col++)
                                {
                                    string? strVal = dict[columns[col]]?.ToString();

                                    if (!string.IsNullOrEmpty(strVal) && strVal.Contains("/|")) // split cells - more data
                                    {
                                        var parts = strVal.Split("/|");
                                        for (int i = 0; i < parts.Length; i++)
                                        {
                                            worksheet.Cell(row, col + 1 + colOffset + i).Value = parts[i];
                                        }

                                        // shift header
                                        if (!splitColls[col])
                                        {
                                            int lastCol = worksheet.Row(rowHead).LastCellUsed()?.Address.ColumnNumber ?? 0;
                                            for (int i = lastCol; i >= col + 1; i--)
                                            {
                                                if (i > col + 1)
                                                {
                                                    worksheet.Cell(rowHead, i + colOffset + parts.Length - 1).Value = worksheet.Cell(rowHead, i + colOffset).Value;
                                                }
                                                else
                                                {
                                                    for (int j = i + colOffset + 1; j < i + colOffset + parts.Length; j++)
                                                    {
                                                        worksheet.Cell(rowHead, j).Value = string.Empty;
                                                    }

                                                    worksheet.Range(rowHead, i + colOffset, rowHead, i + colOffset + parts.Length - 1).Merge();
                                                }
                                            }
                                            splitColls[col] = true;

                                        }

                                        colOffset += parts.Length - 1;
                                    }
                                    else
                                    {
                                        worksheet.Cell(row, col + 1).Value = strVal;
                                    }
                                }

                                row++;
                            }

                            // excel as table for searching, filtering..
                            var tableRange = worksheet.Range(3,1, row-1, columns.Count);
                            tableRange.CreateTable();
                        }
                        // auto adjustment
                        worksheet.Columns().AdjustToContents();

                        // save
                        workbook.SaveAs(filePath);                   
                    }
                }
            }
            catch (Exception ex)
            {
                errStr = "KNTCommon.BusinessIO.Repositories.ExportRepository #1; " + ex.Message + $"; Table: {tableName}";
                t.LogEvent(errStr);
                ret = false;
            }

#if DEBUG
            if(errStr.Length > 0)
                Console.WriteLine($"end export, error; {errStr} " + DateTime.Now);
            else
                Console.WriteLine($"end export " + DateTime.Now);
#endif

            return ret;
        }

    }
}
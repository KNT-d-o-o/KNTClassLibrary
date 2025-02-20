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

        public bool ExportExcel(string tableName, string where, string order, string filePath, List<string> altCols, out string errStr)
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
                                        GROUP_CONCAT(talt.{altCols[1]}, ':', talt.{altCols[3]} ORDER BY talt.{altCols[2]} SEPARATOR '|') AS Vals
                                        FROM {tableName} t
                                        LEFT JOIN {altCols[0]} talt ON t.{altCols[4]} = talt.{altCols[4]}";
                                if (where.Length > 0)
                                    query += $" WHERE t.{where}";
                                query += $" GROUP BY t.{altCols[4]}";
                            }
                            else  // additional as columns
                            {
                                string columnQuery = @$"
                                        SELECT GROUP_CONCAT(DISTINCT 
                                            CONCAT('MAX(CASE WHEN talt.{altCols[1]} = ''', talt.{altCols[1]}, 
                                            ''' THEN talt.{altCols[3]} ELSE NULL END) AS `', 
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

                        var worksheet = workbook.Worksheets.Add(tableName);

                        // title of sheet
                        int row = 1;
                        worksheet.Cell(row, 1).Value = tableName;
                        worksheet.Cell(row, 2).Value = DateTime.Now;

                        if (data.Count > 0) // if data found
                        {

                            // columns from first row
                            var firstRow = (IDictionary<string, object>)data.First();
                            var columns = firstRow.Keys.ToList();

                            // headers of columns
                            row += 2;
                            for (int i = 0; i < columns.Count; i++)
                            {
                                worksheet.Cell(row, i + 1).Value = columns[i];
                            }

                            // data rows
                            row++;
                            foreach (var item in data)
                            {
                                if (row >= MAX_ROWS_PER_SHEET) // max rows control
                                {
                                    worksheet.Cell(row, 1).Value = "...";
                                    break;
                                }

                                var dict = (IDictionary<string, object>)item;
                                for (int col = 0; col < columns.Count; col++)
                                {
                                    worksheet.Cell(row, col + 1).Value = dict[columns[col]]?.ToString();
                                }
                                
                                row++;
                            }
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
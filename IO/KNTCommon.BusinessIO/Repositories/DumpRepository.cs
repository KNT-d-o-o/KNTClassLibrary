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
using System.Diagnostics;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Drawing;

namespace KNTCommon.BusinessIO.Repositories
{
    public class DumpRepository
    {
        /// <summary>
        /// DB Export: iotasks.IoTaskType = 3
        /// iotaskdetails.Par1: table
        /// iotaskdetails.Par2[1..]: join column
        /// iotaskdetails.Par3[0]: where condition
        /// iotaskdetails.Par3[1..]: join table
        /// iotaskdetails.Par4: -
        /// iotaskdetails.Par5[0]: getAll - all tables to export - means also other tables without conditions
        /// iotaskdetails.Par6[0]: excel sheet name
        /// iotaskdetails.Par6[1..]: excel column names
        /// TableDetailOrder: -
        /// </summary>
        /// 
        private readonly IDbContextFactory<EdnKntControllerMysqlContext> Factory;
        private readonly IMapper AutoMapper;
        private readonly Tools t = new();
        private const int MAX_ROWS_PER_SHEET = 1048576;
        private const int DUMP_STEP = 1000;

        public DumpRepository(IDbContextFactory<EdnKntControllerMysqlContext> factory, IMapper automapper)
        {
            Factory = factory;
            AutoMapper = automapper;
        }


        // dump single table and append to file: used for long table data
        public bool DumpTableSingle(string tableName, int batchSize, long offset, string baseFilePath, out string errStr)
        {
            errStr = string.Empty;
            bool ret = true;
            bool moreData = true;

#if DEBUG
            Console.WriteLine($"start DB export single on table {tableName} " + DateTime.Now);
#endif
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    string connectionString = context.Database.GetDbConnection().ConnectionString;
                    var connectionParams = new MySqlConnectionStringBuilder(connectionString);
                    string server = connectionParams.Server;
                    string database = connectionParams.Database;
                    string user = connectionParams.UserID;
                    string password = connectionParams.Password;
                    string connStr = $"server={server};user={user};password={password};database={database}";
                    using var conn = new MySqlConnection(connStr);
                    conn.Open();

                    while (moreData)
                    {
                        string filePath = $"{baseFilePath}";
                        if(batchSize > 0)
                            filePath += $"_{offset:D10}.sql";

                        using var writer = new StreamWriter(filePath, append: true);

                        string query = $"SELECT * FROM {tableName}";
                        if (batchSize > 0)
                        {
                            writer.WriteLine($"-- Exporting table: {tableName}, offset: {offset}");
                            query += $" LIMIT {batchSize} OFFSET {offset}";
                        }
                        else
                        {
                            writer.WriteLine($"-- Exporting table: {tableName}");
                        }

                        using var cmd = new MySqlCommand(query, conn)
                        {
                            CommandTimeout = 1800 // 30 min
                        };
                        using var reader = cmd.ExecuteReader();

                        int rowCount = 0;

                        while (reader.Read())
                        {
                            rowCount++;
                            // row for SQL INSERT format:
                            var values = new object[reader.FieldCount];
                            reader.GetValues(values);
                            string formatted = string.Join(", ", values.Select(v =>
                            {
                                if (v == null || v == DBNull.Value)
                                    return "NULL";
                                if (v is string s)
                                    return $"'{s.Replace("'", "''")}'"; // SQL escape for '
                                if (v is DateTime dt)
                                    return $"'{dt:yyyy-MM-dd HH:mm:ss}'";
                                return v.ToString();
                            }));
                            writer.WriteLine($"INSERT INTO {tableName} VALUES ({formatted});");
                        }

                        if (batchSize > 0)
                        {
                            if (rowCount < batchSize)
                                moreData = false;
                            else
                                offset += batchSize;
                        }
                        else
                            moreData = false;
                    }
                }
            }
            catch (Exception ex)
            {
                errStr = "KNTCommon.BusinessIO.Repositories.DumpRepository #3 " + ex.Message;
                t.LogEvent(errStr);
                ret = false;
            }

            return ret;
        }

        // dump table and append to file
        public bool DumpTable(string tableName, string where, string joinColumn, string filePath, out string errStr)
        {
            errStr = string.Empty;
            bool ret = true;

#if DEBUG
            Console.WriteLine($"start DB export on table {tableName} " + DateTime.Now);
#endif

            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    string connectionString = context.Database.GetDbConnection().ConnectionString;
                    var connectionParams = new MySqlConnectionStringBuilder(connectionString);
                    string server = connectionParams.Server;
                    string database = connectionParams.Database;
                    string user = connectionParams.UserID;
                    string password = connectionParams.Password;
                    string arguments = $"--host={server} --user={user} --password={password} --quick --single-transaction {database} {tableName}";

                    if (joinColumn == string.Empty)
                    {
                        if (where != string.Empty)
                        {
                            arguments += $" --where=\"{where}\"";
                        }

                        ret = DumpTableCycle(tableName, arguments, filePath, out errStr);
                    }

                    // for join to another table condition with limit DUMP_STEP items
                    else
                    {
                        if (where != string.Empty)
                        {
                            List<string> joinItems = GetJoinItems(where);
                            int batchSize = DUMP_STEP;
                            bool isFirstBatch = true;

                            for (int i = 0; i < joinItems.Count; i += batchSize)
                            {
                                var batchItems = joinItems.Skip(i).Take(batchSize);

                                string argumentsCycle = $"{arguments} --where=\"{joinColumn} IN ({string.Join(",", batchItems)})\"";

                                string additionalArgs = isFirstBatch ? "" : "--no-create-info";
                                isFirstBatch = false;
                                argumentsCycle += $" {additionalArgs}";

                                ret = DumpTableCycle(tableName, argumentsCycle, filePath, out errStr);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errStr = "KNTCommon.BusinessIO.Repositories.DumpRepository #1 " + ex.Message;
                t.LogEvent(errStr);
                ret = false;
            }

            return ret;
        }

        // export in one cycle 
        private bool DumpTableCycle(string tableName, string arguments, string filePath, out string errStr)
        {
            bool ret = true;
            errStr = string.Empty;

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            using (StreamWriter writer = new(filePath, true)) // 🔹 Append mode
            {
                writer.WriteLine($"\n-- Exporting table: {tableName} --\n");

                ProcessStartInfo psi = new()
                {
                    FileName = "C:\\Program Files\\MySQL\\MySQL Server 5.7\\bin\\mysqldump",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process? process = Process.Start(psi))
                {
                    if (process is not null)
                    {
                        using (StreamReader reader = process.StandardOutput)
                        {
                            writer.Write(reader.ReadToEnd());
                        }

                        process.WaitForExit();

                        if (process.ExitCode != 0)
                        {
                            using (StreamReader errorReader = process.StandardError)
                            {
                                errStr = "KNTCommon.BusinessIO.Repositories.DumpRepository #2 Exit Code: " + process.ExitCode.ToString() + " - " + errorReader.ReadToEnd();
                            }
                            t.LogEvent(errStr);
                            ret = false;
                        }
                    }
                }
            }
            return ret;
        }

        // get table list
        public List<string> GetTableList()
        {
            using (var dbContext = new EdnKntControllerMysqlContext())
            {
                var connectionString = dbContext.Database.GetConnectionString();
                using (MySqlConnection connection = new(connectionString))
                {
                    connection.Open();
                    return connection.Query<string>("SHOW TABLES;").AsList();
                }
            }
        }

        private List<string> GetJoinItems(string query)
        {
            using (var dbContext = new EdnKntControllerMysqlContext())
            {
                var connectionString = dbContext.Database.GetConnectionString();
                using (MySqlConnection connection = new(connectionString))
                {
                    connection.Open();
                    return connection.Query<string>(query).AsList();
                }
            }
        }
    }
}
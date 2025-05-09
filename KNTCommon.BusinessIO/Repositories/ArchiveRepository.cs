using AutoMapper;
using KNTCommon.Data.Models;
using KNTCommon.BusinessIO.DTOs;
using KNTToolsAndAccessories;
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
using DocumentFormat.OpenXml.Drawing;
using MySqlConnector;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace KNTCommon.BusinessIO.Repositories
{
    public class ArchiveRepository
    {
        /// <summary>
        /// Backup: iotasks.IoTaskType = 1, restore backup: iotasks.IoTaskType = 2
        /// iotaskdetails.Par1: table to archive
        /// iotaskdetails.Par2[0]: column condition for next tables (iotaskdetails.Par1[1..]) *1
        /// iotaskdetails.Par2[1..]: where condition in string {transactionIdArrayStr}
        /// iotaskdetails.Par3[0]: time condition to archive {dayBeforeStr}
        /// iotaskdetails.Par3[1..]: join condition from first table (iotaskdetails.Par1[1]) *1 
        /// iotaskdetails.Par4: Archived tag in table
        /// iotaskdetails.Par5: copy or move to archive DB, delete from archive when restore
        /// TableDetailOrder: order to archive
        /// </summary>

        private readonly IDbContextFactory<EdnKntControllerMysqlContext> Factory;
        private readonly IMapper AutoMapper;
        private readonly Tools t = new();
        private readonly IoTasksRepository ioTasksRepository;

        public ArchiveRepository(IDbContextFactory<EdnKntControllerMysqlContext> factory, IMapper automapper, IoTasksRepository _ioTasksRepository)
        {
            Factory = factory;
            AutoMapper = automapper;
            ioTasksRepository = _ioTasksRepository;
        }

        // optimize table
        public async Task<bool> OptimizeTable(string tableName)
        {
            try
            {
                uint timeout = 60 * 60 * 2;
                using var dbContext = new EdnKntControllerMysqlContext();
                var builder = new MySqlConnector.MySqlConnectionStringBuilder(dbContext.Database.GetConnectionString() ?? string.Empty)
                {
                    DefaultCommandTimeout = timeout,
                    ConnectionTimeout = timeout
                };

                using var conn = new MySqlConnector.MySqlConnection(builder.ConnectionString);
                await conn.OpenAsync();

                string query = $"OPTIMIZE TABLE `{tableName}`;";
                using var cmd = new MySqlConnector.MySqlCommand(query, conn)
                {
                    CommandTimeout = (int)timeout
                };

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));

                await cmd.ExecuteNonQueryAsync(cts.Token); // z MySqlConnector to DELUJE

                return true;
            }
            catch (OperationCanceledException)
            {
                t.LogEvent($"KNTCommon.BusinessIO.Repositories.ArchiveRepository #9: ⚠️ OptimizeTable timeout for {tableName}");
                return false;
            }
            catch (Exception ex)
            {
                t.LogEvent($"KNTCommon.BusinessIO.Repositories.ArchiveRepository #6: ❌ OptimizeTable error ({tableName}): {ex.Message}");
                return false;
            }
        }

        // copy other tables if never
        public string? CopyOtherTables(List<string> tables)
        {
            string? err = null;

            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var sourceConnectionString = context.Database.GetConnectionString();
                    var builder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(sourceConnectionString);
                    string connStr = builder.Database;
                    string[] connStrArchive = EdnKntControllerMysqlContext.GetConnectionData(true);
                    builder.Database = connStrArchive[0];
                    var targetConnectionString = builder.ConnectionString;

                    using (var sourceConnection = new MySql.Data.MySqlClient.MySqlConnection(sourceConnectionString))
                    using (var targetConnection = new MySql.Data.MySqlClient.MySqlConnection(targetConnectionString))
                    {
                        sourceConnection.Open();
                        targetConnection.Open();
                        var getTablesQuery = @"
                            SELECT table_name 
                            FROM information_schema.tables 
                            WHERE table_schema = DATABASE() 
                            AND table_type = 'BASE TABLE';";

                        var command = new MySql.Data.MySqlClient.MySqlCommand(getTablesQuery, sourceConnection);
                        var reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            string tableName = reader.GetString(0);

                            if (!tables.Contains(tableName))
                            {
                                try
                                {
                                    // delete
                                    string deleteQuery = $"DELETE FROM {connStrArchive[0]}.{tableName};";
                                    using (var deleteCommand = new MySql.Data.MySqlClient.MySqlCommand(deleteQuery, targetConnection))
                                    {
                                        deleteCommand.ExecuteNonQuery();
                                    }

                                    // insert
                                    string copyDataQuery = $@"
                                    INSERT INTO {connStrArchive[0]}.{tableName}
                                    SELECT * FROM {connStr}.{tableName};";

                                    using (var copyCommand = new MySql.Data.MySqlClient.MySqlCommand(copyDataQuery, targetConnection))
                                    {
                                        copyCommand.ExecuteNonQuery();
                                    }
                                }
                                catch (Exception tableEx)
                                {
                                    err = $"KNTCommon.BusinessIO.Repositories.ArchiveRepository #7 Error while transferring table {tableName}: {tableEx.Message}\n";
                                    t.LogEvent(err);
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                err = "KNTCommon.BusinessIO.Repositories.ArchiveRepository #8 " + ex.Message;
                t.LogEvent(err);
            }

            return err;
        }

        // archive tables procedure
        public int ArchiveTables(List<string> tables, List<IoTaskDetailsDTO> ioTaskDetails, string whereCondition, string orderBy, int noRows, string archivedFlag)
        {
            int ret = 0;

            try
            {
                DataTable dataTable = GetDataTable(tables[0], whereCondition, archivedFlag, 0, orderBy, false, noRows, new EdnKntControllerMysqlContext());

                ret = dataTable.Rows.Count;

#if DEBUG
                    Console.WriteLine($"ArchiveTables table: {tables[0]}, ret: {ret}");
#endif


                if (ret > 0)
                {
                    string retColumn = ioTaskDetails[0].Par2 ?? string.Empty;
                    List<string> items = DataTableToListItems(dataTable, retColumn);

                    // insert into archive - common table
                    InsertFromDataTable(tables[0], dataTable, new EdnKntControllerMysqlContextArchive());

                    // other tables
                    for (int i = 1; i < tables.Count; i++)
                    {
                        string whereConditionOther = (ioTaskDetails[i].Par2 ?? string.Empty).Replace("{transactionIdArrayStr}", string.Join(", ", items));
                        DataTable dataTableOther = GetDataTable(tables[i], whereConditionOther, string.Empty, 0, string.Empty, false, 0, new EdnKntControllerMysqlContext());

#if DEBUG
                            Console.WriteLine($"ArchiveTables table: {tables[i]}, whereConditionOther: {whereConditionOther}, dataTableOther: {dataTableOther}");
#endif

                        // insert and then delete
                        if (InsertFromDataTable(tables[i], dataTableOther, new EdnKntControllerMysqlContextArchive()))
                        {
                            DeleteWhereInItems(tables[i], retColumn, items, new EdnKntControllerMysqlContext());

#if DEBUG
                            Console.WriteLine($"InsertFromDataTable->DeleteWhereInItems: {tables[i]}");
#endif
                        }
                    }

                    // sign as archive - common table
                    UpdateWhereInItems(tables[0], archivedFlag, "1", retColumn, items, new EdnKntControllerMysqlContext());

                    return ret;
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.Repositories.ArchiveRepository #1 " + ex.Message);              
            }
            return 0;
        }

        // archive tables procedure
        public int RestoreTables(List<string> tables, List<IoTaskDetailsDTO> ioTaskDetails, string whereCondition, string orderBy, int noRows, string archivedFlag)
        {
            int ret = 0;

            try {

                DataTable dataTable = GetDataTable(tables[0], whereCondition, archivedFlag, 1, orderBy, true, noRows, new EdnKntControllerMysqlContext());

                ret = dataTable.Rows.Count;
                if (ret > 0)
                {
                    string retColumn = ioTaskDetails[0].Par2 ?? string.Empty;
                    List<string> items = DataTableToListItems(dataTable, retColumn);

                    DeleteWhereInItems(tables[0], retColumn, items, new EdnKntControllerMysqlContextArchive());

                    // other tables
                    for (int i = 1; i < tables.Count; i++)
                    {
                        string whereConditionOther = (ioTaskDetails[i].Par2 ?? string.Empty).Replace("{transactionIdArrayStr}", string.Join(", ", items));
                        DataTable dataTableOther = GetDataTable(tables[i], whereConditionOther, string.Empty, 0, string.Empty, false, 0, new EdnKntControllerMysqlContextArchive());

                        // insert and then delete
                        if (InsertFromDataTable(tables[i], dataTableOther, new EdnKntControllerMysqlContext()))
                            DeleteWhereInItems(tables[i], retColumn, items, new EdnKntControllerMysqlContextArchive());
                    }

                    UpdateWhereInItems(tables[0], archivedFlag, "0", retColumn, items, new EdnKntControllerMysqlContext());

                    return ret;
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.Repositories.ArchiveRepository #5 " + ex.Message);
            }
            return 0;
        }

        // get data from any table
        private DataTable GetDataTable(string table, string whereCondition, string whereInt, int whereIntVal, string orderBy, bool orderByDesc, int noRows, DbContext contextIn)
        {
            DataTable dataTable = new();

            // get data - common table
            using (var context = contextIn)
            {
                var connectionString = context.Database.GetConnectionString();
                using (MySql.Data.MySqlClient.MySqlConnection connection = new(connectionString))
                {
                    connection.Open();
                    string selectQuery = $"SELECT * FROM {table}";
                    if (whereCondition.Length > 0 || whereInt.Length > 0)
                    {
                        selectQuery += " WHERE";
                        if (whereCondition.Length > 0)
                        {
                            selectQuery += $" {whereCondition}";
                            if (whereInt.Length > 0)
                                selectQuery += " AND";
                        }
                        if (whereInt.Length > 0)
                            selectQuery += $" {whereInt} = {whereIntVal}";
                    }
                    if (orderBy.Length > 0)
                    {
                        selectQuery += $" ORDER BY {orderBy}";
                        if(orderByDesc)
                            selectQuery += " DESC";
                    }
                    if (noRows > 0)
                        selectQuery += $" LIMIT {noRows}";
                    selectQuery += ";";

                    using (MySql.Data.MySqlClient.MySqlCommand selectCommand = new(selectQuery, connection))
                    using (MySql.Data.MySqlClient.MySqlDataAdapter adapter = new(selectCommand))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            return dataTable;
        }

        // data from DataTable to list of strings
        private List<string> DataTableToListItems(DataTable dataTable, string column)
        {
            List<string> items = new();
            foreach (DataRow row in dataTable.Rows)
            {
                string? pkVal = row[$"{column}"].ToString();
                if (pkVal != null)
                    items.Add(pkVal);
            }
            return items;
        }

        // insert from DataTable
        private bool InsertFromDataTable(string table, DataTable dataTable, DbContext contextIn)
        {
            using (var context = contextIn)
            {
                var connectionString = context.Database.GetConnectionString();
                using (MySql.Data.MySqlClient.MySqlConnection connection = new(connectionString))
                {
                    connection.Open();

                    using (MySql.Data.MySqlClient.MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            foreach (DataRow row in dataTable.Rows)
                            {
                                string insertQuery = $"INSERT IGNORE INTO {table} ({string.Join(", ", dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName))}) VALUES ({string.Join(", ", dataTable.Columns.Cast<DataColumn>().Select(c => "@" + c.ColumnName))});";
                                using (MySql.Data.MySqlClient.MySqlCommand insertCommand = new(insertQuery, connection))
                                {
                                    foreach (DataColumn column in dataTable.Columns)
                                    {
                                        insertCommand.Parameters.AddWithValue("@" + column.ColumnName, row[column.ColumnName]);
                                    }

#if DEBUG
                                //    Console.WriteLine($"InsertFromDataTable insertQuery: {insertQuery}");
#endif

                                    insertCommand.ExecuteNonQuery();
                                }
                            }

                     //       Console.WriteLine($"commit: {table}");

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {

#if DEBUG
                            Console.WriteLine($"reject: {table} {ex.Message}");
#endif

                            transaction.Rollback();
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        // update in table WHERE IN (...) condition
        private bool UpdateWhereInItems(string table, string column, string value, string whereColumn, List<string> whereItems, DbContext contextIn)
        {
            using (var context = contextIn)
            {
                var connectionString = context.Database.GetConnectionString();
                using (MySql.Data.MySqlClient.MySqlConnection connection = new(connectionString))
                {
                    connection.Open();

                    using (MySql.Data.MySqlClient.MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string updateQuery = $"UPDATE {table} SET {column} = {value} WHERE {whereColumn} IN ({string.Join(", ", whereItems)});";
                            using (MySql.Data.MySqlClient.MySqlCommand updateCommand = new(updateQuery, connection))
                            {
                                updateCommand.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        // delete in table WHERE IN (...) condition
        private bool DeleteWhereInItems(string table, string whereColumn, List<string> whereItems, DbContext contextIn)
        {
            using (var context = contextIn)
            {
                var connectionString = context.Database.GetConnectionString();
                using (MySql.Data.MySqlClient.MySqlConnection connection = new(connectionString))
                {
                    connection.Open();

                    using (MySql.Data.MySqlClient.MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string deleteQuery = $"DELETE FROM {table} WHERE {whereColumn} IN ({string.Join(", ", whereItems)});";

#if DEBUG
                            Console.WriteLine($"DeleteWhereInItems deleteQuery: {deleteQuery}");
#endif

                            using (MySql.Data.MySqlClient.MySqlCommand deleteCommand = new(deleteQuery, connection))
                            {
                                deleteCommand.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            if (connection.State == System.Data.ConnectionState.Open)
                                transaction.Rollback();
#if DEBUG
                            Console.WriteLine("Napaka v DeleteWhereInItems: " + ex.Message);
#endif
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        // count records in table with WHERE condition
        public int CountRecordsBeforeArchiveRestore(string tableName, string whereCondition, string archivedFlag, bool restore)
        { 
            int count = 0;

            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var connectionString = context.Database.GetConnectionString();
                    using (MySql.Data.MySqlClient.MySqlConnection connection = new(connectionString))
                    {
                        connection.Open();
                        int flag = 0;
                        if (restore)
                            flag = 1;
                        string selectQuery = $"SELECT COUNT(*) FROM {tableName}";
                        if (whereCondition.Length > 0 || archivedFlag.Length > 0)
                        {
                            selectQuery += " WHERE";
                            if (whereCondition.Length > 0)
                            {
                                selectQuery += $" {whereCondition}";
                                if (archivedFlag.Length > 0)
                                    selectQuery += " AND";
                            }
                            if (archivedFlag.Length > 0)
                                selectQuery += $" {archivedFlag} = {flag}";
                        }
                        selectQuery += ";";

                        using (MySql.Data.MySqlClient.MySqlCommand selectCommand = new(selectQuery, connection))
                        {
                            object result = selectCommand.ExecuteScalar();
                            if (result != null && int.TryParse(result.ToString(), out int parsedCount))
                            {
                                count = parsedCount;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.Repositories.ArchiveRepository #2 " + ex.Message);
            }

            return count;
        }

        // create archive database if not exists
        public bool CheckOrCreateArchiveDb(out string? err)
        {
            err = string.Empty;
            var ret = true;
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var connectionString = context.Database.GetConnectionString(); // get from current - not archive

                    using (var connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString))
                    {
                        connection.Open();

                        var builder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(connectionString);
                        var databaseName = builder.Database;
                        string[] connStr = EdnKntControllerMysqlContext.GetConnectionData(true);

                        string createDatabaseQuery = $"CREATE DATABASE IF NOT EXISTS `{connStr[0]}`;";
                        using (var command = new MySql.Data.MySqlClient.MySqlCommand(createDatabaseQuery, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                err = "KNTCommon.BusinessIO.Repositories.ArchiveRepository #3 " + ex.Message;
                t.LogEvent(err);
                ret = false;
            }

            return ret;
        }

        // create archive tables if not exists
        public bool CheckOrCreateArchiveTables(List<string>? arcTables, out string? err)
        {
            err = string.Empty;
            var ret = true;
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var sourceConnectionString = context.Database.GetConnectionString();
                    var builder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(sourceConnectionString);
                    string connStr = builder.Database;
                    string[] connStrArchive = EdnKntControllerMysqlContext.GetConnectionData(true);
                    builder.Database = connStrArchive[0];
                    var targetConnectionString = builder.ConnectionString;

                    using (var sourceConnection = new MySql.Data.MySqlClient.MySqlConnection(sourceConnectionString))
                    using (var targetConnection = new MySql.Data.MySqlClient.MySqlConnection(targetConnectionString))
                    {
                        sourceConnection.Open();
                        targetConnection.Open();
                        var getTablesQuery = @"
                            SELECT table_name 
                            FROM information_schema.tables 
                            WHERE table_schema = DATABASE() 
                            AND table_type = 'BASE TABLE';";

                        var command = new MySql.Data.MySqlClient.MySqlCommand(getTablesQuery, sourceConnection);
                        var reader = command.ExecuteReader();

                        List<string> includedTables = ioTasksRepository.GetIoTaskDetailsPar1(ioTasksRepository.GetIoTaskIdByTypeMode(1, 1));

                        while (reader.Read())
                        {
                            string tableName = reader.GetString(0);

                            if (arcTables is null || arcTables.Contains(tableName))
                            {
                                if (includedTables.Contains(tableName))
                                {
                                    string copyQuery = $"CREATE TABLE IF NOT EXISTS {connStrArchive[0]}.{tableName} LIKE {connStr}.{tableName};";
                                    using (var copyCommand = new MySql.Data.MySqlClient.MySqlCommand(copyQuery, targetConnection))
                                    {
                                        copyCommand.ExecuteNonQuery();
                                    }
                                }
                                // re-create config (non archivable) tables
                                else
                                {
#if DEBUG
                                    Console.WriteLine($"connStrArchive[0].tableName: {connStrArchive[0]}.{tableName}");
#endif
                                    string copyQuery = $@"DROP TABLE IF EXISTS {connStrArchive[0]}.{tableName};
                                                        CREATE TABLE {connStrArchive[0]}.{tableName} LIKE {connStr}.{tableName};
                                                        INSERT INTO {connStrArchive[0]}.{tableName} SELECT * FROM {connStr}.{tableName};";
                                    using (var copyCommand = new MySql.Data.MySqlClient.MySqlCommand(copyQuery, targetConnection))
                                    {
                                        copyCommand.ExecuteNonQuery();
                                    }
                                }
                                // 
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                err = "KNTCommon.BusinessIO.Repositories.ArchiveRepository #4 " + ex.Message;
                t.LogEvent(err);
                ret = false;
            }

            return ret;
        }

        // check if table exists
        public static bool TableExists(string tableName)
        {
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    context.Database.ExecuteSqlRaw("SELECT 1 FROM " + tableName + " LIMIT 1");
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

    }
}

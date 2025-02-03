﻿using AutoMapper;
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

namespace KNTCommon.BusinessIO.Repositories
{
    public class ArchiveRepository
    {
        private readonly IDbContextFactory<EdnKntControllerMysqlContext> Factory;
        private readonly IMapper AutoMapper;
        private readonly Tools t = new();

        public ArchiveRepository(IDbContextFactory<EdnKntControllerMysqlContext> factory, IMapper automapper)
        {
            Factory = factory;
            AutoMapper = automapper;
        }

        // optimize table
        public bool OptimizeTable(string tableName)
        {
            bool ret = true;

            try
            {
                using (var dbContext = new EdnKntControllerMysqlContext())
                {
                    var connectionString = dbContext.Database.GetConnectionString();
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = $"OPTIMIZE TABLE `{tableName}`;";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        ret = true;
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTLeakTester.BusinessIO.Repositories.ArchiveRepository #6 " + ex.Message);
                ret = false;
            }

            return ret;
        }

        // archive tables procedure
        public int ArchiveTables(List<string> tables, List<IoTaskDetailsDTO> ioTaskDetails, string whereCondition, string orderBy, int noRows, string archivedFlag)
        {
            int ret = 0;

            try
            {
                DataTable dataTable = GetDataTable(tables[0], whereCondition, archivedFlag, 0, orderBy, false, noRows, new EdnKntControllerMysqlContext());

                ret = dataTable.Rows.Count;
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
                        
                        // insert and then delete
                        if (InsertFromDataTable(tables[i], dataTableOther, new EdnKntControllerMysqlContextArchive()))
                            DeleteWhereInItems(tables[i], retColumn, items, new EdnKntControllerMysqlContext());
                    }

                    // sign as archive - common table
                    UpdateWhereInItems(tables[0], archivedFlag, "1", retColumn, items, new EdnKntControllerMysqlContext());

                    return ret;
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTLeakTester.BusinessIO.Repositories.ArchiveRepository #1 " + ex.Message);              
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
                t.LogEvent("KNTLeakTester.BusinessIO.Repositories.ArchiveRepository #5 " + ex.Message);
            }
            return 0;
        }

        private DataTable GetDataTable(string table, string whereCondition, string whereInt, int whereIntVal, string orderBy, bool orderByDesc, int noRows, DbContext contextIn)
        {
            DataTable dataTable = new DataTable();

            // get data - common table
            using (var context = contextIn)
            {
                var connectionString = context.Database.GetConnectionString();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
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

                    using (MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection))
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(selectCommand))
                    {
                        adapter.Fill(dataTable);
                    }

                }
            }

            return dataTable;
        }

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

        private bool InsertFromDataTable(string table, DataTable dataTable, DbContext contextIn)
        {
            using (var context = contextIn)
            {
                var connectionString = context.Database.GetConnectionString();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            foreach (DataRow row in dataTable.Rows)
                            {
                                string insertQuery = $"INSERT INTO {table} ({string.Join(", ", dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName))}) VALUES ({string.Join(", ", dataTable.Columns.Cast<DataColumn>().Select(c => "@" + c.ColumnName))});";
                                using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection))
                                {
                                    foreach (DataColumn column in dataTable.Columns)
                                    {
                                //        Console.WriteLine("fstaaaaa I: " + insertCommand.CommandText);

                                        insertCommand.Parameters.AddWithValue("@" + column.ColumnName, row[column.ColumnName]);
                                    }
                                    insertCommand.ExecuteNonQuery();
                                }
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

        private bool UpdateWhereInItems(string table, string column, string value, string whereColumn, List<string> whereItems, DbContext contextIn)
        {
            using (var context = contextIn)
            {
                var connectionString = context.Database.GetConnectionString();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string updateQuery = $"UPDATE {table} SET {column} = {value} WHERE {whereColumn} IN ({string.Join(", ", whereItems)});";
                            using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                            {
                          //      Console.WriteLine("fstaaaaa U: " + updateCommand.CommandText);

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

        private bool DeleteWhereInItems(string table, string whereColumn, List<string> whereItems, DbContext contextIn)
        {
            using (var context = contextIn)
            {
                var connectionString = context.Database.GetConnectionString();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string deleteQuery = $"DELETE FROM {table} WHERE {whereColumn} IN ({string.Join(", ", whereItems)});";
                            using (MySqlCommand deleteCommand = new MySqlCommand(deleteQuery, connection))
                            {
                       //         Console.WriteLine("fstaaaaa D: " + deleteCommand.CommandText);

                                deleteCommand.ExecuteNonQuery();
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

        public int CountRecordsBeforeArchiveRestore(string tableName, string whereCondition, string archivedFlag, bool restore)
        { 
            int count = 0;

            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var connectionString = context.Database.GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
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

                        using (MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection))
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
                t.LogEvent("KNTLeakTester.BusinessIO.Repositories.ArchiveRepository #2 " + ex.Message);
            }

            return count;
        }


        public bool CheckOrCreateArchiveDb(out string? err)
        {
            err = string.Empty;
            var ret = true;
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var connectionString = context.Database.GetConnectionString(); // get from current - not archive

                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        var builder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(connectionString);
                        var databaseName = builder.Database;
                        string[] connStr = EdnKntControllerMysqlContext.GetConnectionData(true);

                        string createDatabaseQuery = $"CREATE DATABASE IF NOT EXISTS `{connStr[0]}`;";
                        using (var command = new MySqlCommand(createDatabaseQuery, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
                t.LogEvent("KNTLeakTester.BusinessIO.Repositories.ArchiveRepository #3 " + ex.Message);
                ret = false;
            }

            return ret;
        }

        public bool CheckOrCreateArchiveTables(List<string> arcTables, out string? err)
        {
            err = string.Empty;
            var ret = true;
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var sourceConnectionString = context.Database.GetConnectionString();
                    var builder = new MySqlConnectionStringBuilder(sourceConnectionString);
                    string connStr = builder.Database;
                    string[] connStrArchive = EdnKntControllerMysqlContext.GetConnectionData(true);
                    builder.Database = connStrArchive[0];
                    var targetConnectionString = builder.ConnectionString;

                    using (var sourceConnection = new MySqlConnection(sourceConnectionString))
                    using (var targetConnection = new MySqlConnection(targetConnectionString))
                    {
                        sourceConnection.Open();
                        targetConnection.Open();
                        var getTablesQuery = "SHOW TABLES;";
                        var command = new MySqlCommand(getTablesQuery, sourceConnection);
                        var reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            string tableName = reader.GetString(0);

                            if (arcTables.Contains(tableName))
                            {
                                string copyTableQuery = $"CREATE TABLE IF NOT EXISTS {connStrArchive[0]}.{tableName} LIKE {connStr}.{tableName};";
                                using (var copyCommand = new MySqlCommand(copyTableQuery, targetConnection))
                                {
                                    copyCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
                t.LogEvent("KNTLeakTester.BusinessIO.Repositories.ArchiveRepository #4 " + ex.Message);
                ret = false;
            }

            return ret;
        }

    }
}
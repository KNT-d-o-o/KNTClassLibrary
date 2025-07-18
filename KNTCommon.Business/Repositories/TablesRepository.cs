﻿using AutoMapper;
using KNTToolsAndAccessories;
using KNTCommon.Business.DTOs;
using KNTCommon.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MySqlX.XDevAPI.Common;
using System.Reflection.PortableExecutable;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Globalization;
using MySql.Data.MySqlClient.X.XDevAPI.Common;
using static Mysqlx.Expect.Open.Types.Condition.Types;
using System.ComponentModel.DataAnnotations;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;

namespace KNTCommon.Business.Repositories
{
    public class TablesRepository : ITablesRepository
    {
        private readonly IDbContextFactory<EdnKntControllerMysqlContext> Factory;
        private readonly IMapper AutoMapper;
        private readonly Tools t = new();

        public TablesRepository(IDbContextFactory<EdnKntControllerMysqlContext> factory, IMapper automapper)
        {
            Factory = factory;
            AutoMapper = automapper;
        }

        public async Task<List<string>> GetDatabaseTablesAsync()
        {
            var tableNames = new List<string>();

            try
            {
                using (var context = Factory.CreateDbContext())
                {
                    var query = @"SELECT TABLE_NAME
                      FROM INFORMATION_SCHEMA.TABLES
                      WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = DATABASE();";

                    var connection = context.Database.GetDbConnection();
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tableNames.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.TablesRepository #1 " + ex.Message);
            }
            return tableNames;
        }

        public async Task<(
    IEnumerable<Dictionary<string, object>> results,
    List<string> columnNames,
    List<string> columnPkNames,
    int totalCount)> GetDataFromTableAsync(
        string table,
        Dictionary<string, object> whereCondition,
        string orderBy,
        int skip,
        int take)
        {
            var results = new List<Dictionary<string, object>>();
            var columnNames = new List<string>();
            var columnPkNames = new List<string>();
            int totalCount = 0;

            try
            {
                var columnTypes = GetColumnTypes(table);

                using var context = Factory.CreateDbContext();
                var connection = context.Database.GetDbConnection();
                await connection.OpenAsync();

                var whereSql = "";
                if (whereCondition.Count > 0)
                {
                    whereSql = " WHERE ";
                    int j = 0;
                    foreach (var key in whereCondition.Keys)
                    {
                        if (j > 0) whereSql += " AND ";
                        whereSql += SetWherePart(key, whereCondition[key], columnTypes[key]);
                        j++;
                    }
                }

                // count query
                string countQuery = $"SELECT COUNT(*) FROM {table} {whereSql}";
                using (var countCommand = connection.CreateCommand())
                {
                    countCommand.CommandText = countQuery;
                    totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                }

                // main data query
                string query = $"SELECT * FROM {table} {whereSql}";
                if (!string.IsNullOrEmpty(orderBy))
                    query += $" ORDER BY {orderBy}";
                if (take > 0)
                    query += $" LIMIT {take} OFFSET {skip}";

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            columnNames.Add(reader.GetName(i));

                        while (await reader.ReadAsync())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                                row[reader.GetName(i)] = reader.GetValue(i);
                            results.Add(row);
                        }
                    }
                }

                // primary keys
                var pkQuery = $@"
            SELECT DISTINCT COLUMN_NAME
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
            WHERE TABLE_NAME = '{table}' 
            AND CONSTRAINT_NAME = 'PRIMARY'";

                using (var pkCommand = connection.CreateCommand())
                {
                    pkCommand.CommandText = pkQuery;
                    using (var pkReader = await pkCommand.ExecuteReaderAsync())
                    {
                        while (await pkReader.ReadAsync())
                            columnPkNames.Add(pkReader.GetString(0));
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.TablesRepository #2 " + ex.Message);
            }

            return (results, columnNames, columnPkNames, totalCount);
        }

        // insert 
        public bool InsertTableRow(string table, Dictionary<string, object> row)
        {
            try
            {
                var columnTypes = GetColumnTypes(table);

                string query = $"INSERT INTO {table} VALUES (";
                int i = 0;
                foreach (var key in row.Keys)
                {
                    if (i > 0)
                        query += ", ";
                    query += $"{FormatValue(row[key], columnTypes[key])}";
                    i++;
                }
                query += ");";

                using (var context = Factory.CreateDbContext())
                {
                    var connection = context.Database.GetDbConnection();
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.TablesRepository #3 " + ex.Message);
                return false;
            }
            return true;
        }

        // update 
        public bool UpdateTableRow(string table, Dictionary<string, object> row, List<string>? pk)
        {
            try
            {
                var columnTypes = GetColumnTypes(table);

                string query = $"UPDATE {table} SET ";
                string where = string.Empty;
                int i = 0;
                int iwhere = 0;
                foreach (var key in row.Keys)
                {
                    if (pk != null && pk.Contains(key))
                    {
                        if (iwhere == 0)
                            where = "WHERE ";
                        else
                            where += " AND ";

                        where += $"`{key}` = {FormatValue(row[key], columnTypes[key])}";
                        iwhere++;
                    }
                    else
                    {
                        if (row[key].ToString() == "System.Byte[]") // encrypted
                            continue;

                        if (i > 0)
                            query += ", ";

                        query += $"`{key}` = {FormatValue(row[key], columnTypes[key])}";
                        i++;
                    }
                }

                using (var context = Factory.CreateDbContext())
                {
                    var connection = context.Database.GetDbConnection();
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        if (!string.IsNullOrEmpty(where))
                            command.CommandText += " " + where;
                        command.CommandText += ";";
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.TablesRepository #4 " + ex.Message);
                return false;
            }
            return true;
        }

        // delete 
        public bool DeleteTableRow(string table, Dictionary<string, object> row, List<string>? pk)
        {
            try
            {
                var columnTypes = GetColumnTypes(table);

                string query = $"DELETE FROM {table} ";
                string where = string.Empty;
                int iwhere = 0;
                foreach (var key in row.Keys)
                {
                    if (pk != null && pk.Contains(key))
                    {
                        if (iwhere == 0)
                            where = "WHERE ";
                        else
                            where += " AND ";

                        where += $"{key} = {FormatValue(row[key], columnTypes[key])}";
                        iwhere++;
                    }
                }

                using (var context = Factory.CreateDbContext())
                {
                    var connection = context.Database.GetDbConnection();
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        if (!string.IsNullOrEmpty(where))
                            command.CommandText += " " + where;
                        command.CommandText += ";";
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.TablesRepository #5 " + ex.Message);
                return false;
            }
            return true;
        }

        // format values for DB
        private string FormatValue(object value, string dataType)
        {
            string ret = "NULL";

            try
            {
                if (value != null && value != DBNull.Value)
                {
                    switch (dataType.ToLower())
                    {
                        case "varchar":
                        case "nvarchar":
                        case "char":
                        case "text":
                            // text
                            ret = $"'{(value.ToString() ?? string.Empty).Replace("'", "''")}'";
                            break;

                        case "int":
                        case "smallint":
                        case "bigint":
                        case "decimal":
                        case "float":
                        case "double":
                            // numbers
                            try
                            {
                                ret = Convert.ToDecimal(value).ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                            }
                            catch
                            {
                                ret = "NULL";
                            }
                            break;

                        case "datetime":
                        case "date":
                        case "time":
                        case "timestamp":
                            // date & time
                            try
                            {
                                ret = $"'{Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss")}'";
                            }
                            catch
                            {
                                ret = "NULL";
                            }
                            break;

                        default:
                            // string
                            ret = $"'{(value.ToString() ?? string.Empty).Replace("'", "''")}'";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ret = $"'{(value.ToString() ?? string.Empty).Replace("'", "''")}'";
                t.LogEvent("KNTCommon.Business.Repositories.TablesRepository #6 " + ex.Message);
            }
            return ret;
        }

        // special where for strings, datetimes and decimals
        private string SetWherePart(string key, object valObj, string type)
        {
            string where = string.Empty;

            switch (type.ToLower())
            {
                case "varchar":
                case "nvarchar":
                case "char":
                case "text":
                    where += $"LOWER(`{key}`) LIKE LOWER('%{(valObj.ToString() ?? string.Empty).Replace("'", "''")}%')";
                    break;

                case "datetime":
                case "date":
                case "time":
                case "timestamp":
                    if (!(valObj.ToString() ?? string.Empty).Contains(" "))
                        where += $"DATE(`{key}`) = {FormatValue(valObj, type)}";
                    else
                        where += $"`{key}` = {FormatValue(valObj, type)}";
                    break;

                case "decimal":
                case "float":
                case "double":
                    int decimalPlaces = 0;
                    try
                    {
                        decimal number;
                        decimal.TryParse(valObj.ToString(), out number);
                        decimalPlaces = BitConverter.GetBytes(decimal.GetBits(number)[3])[2];
                    }
                    catch { }
                    where += $"ROUND(`{key}`, {decimalPlaces}) = {FormatValue(valObj, type)}";
                    break;

                default:
                    where += $"`{key}` = {FormatValue(valObj, type)}";
                    break;
            }

            return where;
        }

        // get types of columns from INFORMATION_SCHEMA
        private Dictionary<string, string> GetColumnTypes(string table)
        {
            var columnTypes = new Dictionary<string, string>();

            try
            {
                using (var context = Factory.CreateDbContext())
                {
                    var connection = context.Database.GetDbConnection();
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $@"
                            SELECT COLUMN_NAME, DATA_TYPE
                            FROM INFORMATION_SCHEMA.COLUMNS
                            WHERE TABLE_NAME = '{table}'";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string columnName = reader.GetString(0);
                                string dataType = reader.GetString(1);
                                columnTypes[columnName] = dataType;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent($"KNTCommon.Business.Repositories.TablesRepository #7 {ex.Message}");
            }

            return columnTypes;
        }

        public async Task<string?> GetCreateTableStatement(string tableName)
        {
            string? createTableResult = null;

            using (var context = new EdnKntControllerMysqlContext())
            {
                var connectionString = context.Database.GetConnectionString();
                using (MySqlConnection connection = new(connectionString))
                {
                    await connection.OpenAsync();

                    string createTableQuery = $"SHOW CREATE TABLE `{tableName}`";
                    var createTableCommand = new MySqlCommand(createTableQuery, connection);
                    using (var reader = await createTableCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // DROP and CREATE TABLE statement is in the second column
                            createTableResult = $"DROP TABLE IF EXISTS `{tableName}`;\n\n" + reader.GetString(1) + ";";
                        }
                    }
                }
            }

            return createTableResult;
        }

        public async Task<List<string>> GetInsertRecordsStatement(string tableName)
        {
            List<string> insertQueries = new List<string>();

            using (var context = new EdnKntControllerMysqlContext())
            {
                var connectionString = context.Database.GetConnectionString();
                using (MySqlConnection connection = new(connectionString))
                {
                    await connection.OpenAsync();

                    string selectQuery = $"SELECT * FROM {tableName}";
                    var selectCommand = new MySqlCommand(selectQuery, connection);
                    var reader = await selectCommand.ExecuteReaderAsync();

                    var columnNames = new List<string>();
                    while (await reader.ReadAsync())
                    {
                        if (columnNames.Count == 0)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                columnNames.Add(reader.GetName(i));
                            }
                        }

                        var values = new List<string>();
                        foreach (var columnName in columnNames)
                        {
                            var value = reader[columnName];
                            if (value == DBNull.Value)
                                values.Add("NULL");
                            else
                            {
                                string valueStr = value?.ToString() ?? "NULL";
                                values.Add($"'{valueStr.Replace("'", "''")}'");
                            }
                        }

                        insertQueries.Add($"INSERT INTO {tableName} ({string.Join(",", columnNames)}) VALUES ({string.Join(",", values)});");
                    }
                }
            }

            return insertQueries;
        }

        public async Task<bool> ExecuteSqlFromFile(string filePath)
        {
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var connectionString = context.Database.GetConnectionString();
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        // read all SQL statements
                        string sqlScript = File.ReadAllText(filePath);

                        using (var command = new MySqlCommand(sqlScript, connection))
                        {
                            await command.ExecuteNonQueryAsync();
                            Console.WriteLine("The SQL script was successfully executed.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing SQL script: {ex.Message}");
                return false;
            }
            return true;
        }


    }
}

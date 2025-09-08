using KNTCommon.Data.Models;
using KNTCommon.Business.Interface;
using KNTToolsAndAccessories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.DirectoryServices.ActiveDirectory;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace KNTCommon.Business.Repositories
{
    public class ValidatorMissingTranslation
    {
        public string table_name { get; set; }
        public string column_name { get; set; }
    }

    class EFTableColumnDiff()
    {
        public string table_name { get; set; }
        public string column_name { get; set; }
        public string data_type { get; set; }
    }


    public class ApplicationValidatorRepository : IApplicationValidatorRepository
    {
        private readonly Tools t = new();

        [DebuggerHidden] // Suppress the exception window in this method, it is displayed only on the method's first call.
        public List<string> EfTableValidation<TContext>(params string[] excludeTables) where TContext : class, IDisposable, new()
        {
            using var context = new TContext();

            var dbSets = context.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                        !excludeTables.ToList().Any(x => x.Equals(p.Name, StringComparison.CurrentCultureIgnoreCase)));

            var errors = new List<string>();

            foreach (var dbSetProperty in dbSets)
            {
                var entityType = dbSetProperty.PropertyType.GetGenericArguments()[0];
                // looks like EF use both?
                var tableTypeName = entityType.Name;
                var tableVariableName = dbSetProperty.Name;

                var fullName = entityType.FullName;

                try
                {
                    var dbSet = context.GetType()
                        .GetMethod("Set", Type.EmptyTypes)
                        !.MakeGenericMethod(entityType)
                        .Invoke(context, null);

                    var queryable = dbSet as IQueryable;

                    var top10Query = typeof(Queryable)
                        .GetMethods()
                        .First(m => m.Name == "Take" && m.GetParameters().Length == 2)
                        .MakeGenericMethod(entityType)
                        .Invoke(null, [queryable!, 10]);

                    var toListMethod = typeof(Enumerable)
                    .GetMethod("ToList")
                    !.MakeGenericMethod(entityType)
                    .Invoke(null, [top10Query]);
                } 
                catch(Exception ex)
                {
                    var tableName = (tableTypeName != tableVariableName) ? $"{tableTypeName}/{tableVariableName}" : $"{tableTypeName}";

                    // ex.InnerException FormatException: Table 'edn_knt_machinemanagement.usersessions' doesn't exist
                    if (ex.InnerException != null && Regex.Match(ex.InnerException.Message, @"Table '(.+?)' doesn't exist").Success)
                    {
                        var msg = @$"Table name: '{tableName}', Model: '{fullName}'. Table dont exists. InnerException: {ex.InnerException.Message}. ";
                        errors.Add(msg);
                    }
                    //ex.InnerException FormatException: The input string 'Mojster' was not in a correct format.
                    else if (ex.InnerException != null && Regex.Match(ex.InnerException.Message, @"The input string '(.+?)' was not in a correct format").Success)
                    {
                        var msg = @$"Table name: '{tableName}', Model: '{fullName}'. One of column in database and model are wrong/different type. Example db string, model int. InnerException: {ex.InnerException.Message}";
                        errors.Add(msg);
                    }
                    //InnerException = {"Unknown column 'l.Deutsche' in 'field list'"}
                    else if (ex.InnerException != null && Regex.Match(ex.InnerException.Message, @"Unknown column '(.+?)' in 'field list").Success)
                    {
                        var msg = @$"Table name: {tableName}, Model: {fullName}. Column in database dont exists but it is in model. InnerException: {ex.InnerException.Message}.";
                        errors.Add(msg);
                    }
                    //InnerException = {"An error occurred while reading a database value for property 'UserGroup.GroupId'. See the inner exception for more information."}
                    //InnerException.InnerException {"The input string 'Mojster' was not in a correct format."}
                    else if (ex.InnerException != null && Regex.Match(ex.InnerException.Message, @"An error occurred while reading a database value for property '(.+?)'. See the inner exception for more information").Success)
                    {
                        if (ex.InnerException.InnerException != null && Regex.Match(ex.InnerException.InnerException.Message, @"The input string '(.+?)' was not in a correct format").Success)
                        {
                            var msg = @$"Table name: {tableName}, Model: {fullName}. One of column in database and model are wrong/different type. InnerException: {ex.InnerException.Message}, InnerException.InnerException: {ex.InnerException.InnerException.Message}.";
                            errors.Add(msg);
                        }
                        else
                        {
                            var msg = $"THROW 1: {ex}";
                            errors.Add(msg);
                            //throw;
                        }
                    }
                    else
                    {
                        var msg = $"THROW 2: {ex}";
                        errors.Add(msg);
                        //throw;
                    }
                }
            }

            return errors;
        }

        //[DebuggerHidden] // Suppress the exception window in this method, it is displayed only on the method's first call.
        public List<string> DbEfTableValidation<TContext>(params string[] excludeTables) where TContext : DbContext, IDisposable, new()
        {
            var errors = new List<string>();

            using var context = new TContext();
            var result = context.Database.SqlQueryRaw<EFTableColumnDiff>($@"SELECT 
                                            table_name,
                                            column_name,
                                            data_type
                                        FROM 
                                            information_schema.columns
                                        WHERE 
                                            table_schema = DATABASE()
                                        ORDER BY 
                                            table_name, ordinal_position");

            var dbSets = context.GetType()
                                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Where(p => p.PropertyType.IsGenericType &&
                                            p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                                            !excludeTables.ToList().Any(x => x.Equals(p.Name, StringComparison.CurrentCultureIgnoreCase)));


            foreach (var dbSet in dbSets) {
                var tableTypeName = dbSet.PropertyType.GetGenericArguments()[0].ToString().Split(".").Last();
                var tableVariableName = dbSet.Name;

                var a = dbSet.PropertyType.FullName.Replace("Microsoft.EntityFrameworkCore.DbSet`1[[", "").Replace("]]", "").Split(",");
                var type = Type.GetType($"{a[0]}, {a[1]}");

                var tableName = a[0];
                var properties = type.GetProperties();
                var entityName = (tableTypeName != tableVariableName) ? $"{tableTypeName}/{tableVariableName}" : $"{tableTypeName}";

                // entity -> db
                foreach (var property in properties)
                {
                    var propertyName = property.Name;
                    var protperyType = (Nullable.GetUnderlyingType(property.PropertyType) != null) ? Nullable.GetUnderlyingType(property.PropertyType)!.Name : property.PropertyType.Name;

                    var isFoundColumn = result.Where(x => (x.table_name == tableTypeName || x.table_name == tableVariableName) && x.column_name == propertyName).ToList();
                    Nullable.GetUnderlyingType(property.PropertyType);


                    if (tableTypeName != tableVariableName)
                    {
                        // when different sometimes tableTypeName is table name, but sometimes tableVariableName.
                        errors.Add($"Wrong table name, tableTypeName and tableVariableName do not match. They must be the same. TableTypeName: {tableTypeName} and tableVariableName: {tableVariableName} ");
                    } 
                    else if (isFoundColumn.Count == 1)
                    {

                        var column = isFoundColumn.First();
                        if (DbTypeToCS(column.data_type) != protperyType)
                        {
                            errors.Add($"Missing column: {property}, on table {entityName}, with type: {protperyType}, db: {column.data_type}");
                        }
                    }
                    else
                    {
                        errors.Add($"Missing column: {property}, on table {entityName}");
                    }

                //if (isFoundColumnAndType.Count > 0)
                //   errors.Add($"Missing column: {property}, on table {entityName}, with type: {protperyType}, db: {isFoundColumnAndType[0].data_type}");
                }

                // TODO 
                // db -> entity
            }


            return errors;
        }


        public string DbTypeToCS(string dataType)
        {
            switch (dataType)
            {
                case "datetime":
                    return "DateTime";
                case "int":
                    return "Int32";
                case "varchar":
                    return "String";
                case "varbinary":
                    return "Byte[]";
                case "tinyint":
                    return "Int32"; // in bool?
                case "double":
                    return "Double";

            }
            throw new Exception($"This datatype: '{dataType}' is not supported!");
        }


        public List<string> FindCorruptData()
        {
            using var context = new EdnKntControllerMysqlContext();

            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Script", "sp_FindCorruptData.sql");
            var data = context.Database.SqlQueryRaw<string>(RemoveSpecialCommands(File.ReadAllText(fullPath))).ToList();
            return data;
        }

        string RemoveSpecialCommands(string sql) // TODO in extension??
        {
            // DELIMITER is not supported in EF but must be when running from mssql workbench 
            sql = sql.Replace("DELIMITER ;;", "");
            sql = sql.Replace(";;", ";");

            sql = sql.Replace("DELIMITER ;", "");

            sql = sql.Replace("{", "{{"); // for dictionary translations
            sql = sql.Replace("}", "}}");

            //sql = sql.Replace("@", "@@"); //MySqlException: Parameter '@drop_sql' must be defined

            //sql = sql.Replace("'", "''"); // when run from procedure

            return sql;
        }


        public IEnumerable<ValidatorMissingTranslation> GetMissingEntityTranslation(params string[] excludeTables)
        {
            using var context = new EdnKntControllerMysqlContext();
            
            var tablesNotIn = $"'{string.Join("','", excludeTables)}'";

            var result = context.Database.SqlQueryRaw<ValidatorMissingTranslation>($@"SELECT 
                                                            c.table_name,
                                                            c.column_name
                                                        FROM 
                                                            information_schema.columns c
	                                                        LEFT JOIN LanguageDictionary ld on c.column_name = ld.Key
                                                        WHERE 
                                                            table_schema = DATABASE()
                                                            AND ld.Key is null
                                                            AND c.table_name NOT IN ({tablesNotIn})
                                                            AND c.table_name NOT like 'x_tmp_%' -- temp table similar as CTE
                                                        ORDER BY 
                                                            table_name, ordinal_position
                                                        LIMIT 9999;").ToList();

            return result;
        }



        

    }
}

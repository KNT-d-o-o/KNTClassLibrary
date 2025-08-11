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

namespace KNTCommon.Business.Repositories
{   
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
                var tableName = entityType.Name;
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


    }
}

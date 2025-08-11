using DocumentFormat.OpenXml.Math;
using Google.Protobuf.Compiler;
using KNTCommon.Data.Models;
using KNTToolsAndAccessories;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace KNTCommon.Business.Repositories
{
    public class VersionManager
    {
        private readonly Tools t = new();
        private readonly IEncryption _encryption;

        public VersionManager(IEncryption encryption)
        {
            _encryption = encryption;
        }

        /*
        private readonly IDbContextFactory<EdnKntControllerMysqlContext> ContextFactory;
        private readonly IEncryption Encryption;
        private readonly IUsersAndGroupsRepository UsersRepository;
        private readonly Tools t = new();

        public AuthenticationRepository(IDbContextFactory<EdnKntControllerMysqlContext> factory, IEncryption encryption, IUsersAndGroupsRepository usersRepository)
        {
            ContextFactory = factory;
            //Log = logger;
            Encryption = encryption;
            UsersRepository = usersRepository;
        }

        */

        private string AssemblyFileVersionFullPath { 
            get {
                var dir = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)!;
                var fullPath = Path.Combine(dir, "AssemblyVersion.txt");
                return fullPath;
            } 
        }

        /// <summary>
        /// Run scripts in order: schema, view, data
        /// </summary>
        public void Upgrade()
        {
            if (CanUpgrade())
            {                
                RunScript("4.0.0.0_schema.sql", true);
                RunScriptAndEncryptPassword("4.0.0.0_data.sql");

                RunScript("4.0.0.1_schema.sql");
                RunScript("4.0.0.1_data.sql");

                RunScript("4.0.0.2_schema.sql");
                RunScript("4.0.0.2_view.sql");

                RunScript("4.0.0.3_schema.sql");
                RunScript("4.0.0.3_view.sql");

                RunScript("4.0.0.4_view.sql");

                RunScript("4.0.0.5_view.sql");
                RunScript("4.0.0.5_data.sql");

                RunScript("4.0.0.6_schema.sql");
                RunScript("4.0.0.6_view.sql");
                RunScript("4.0.0.6_data.sql");

                RunScript("4.0.0.7_data.sql");

                RunScript("4.0.0.8_data.sql");

                //RunScript("../KNTSMM.Data/Version/4.0.0.5excluded.sql");

                //RunScript("languagedictionary.sql"); // TODO always modify language
                CreateAssemblyVersion();
            }
        }

        bool CanUpgrade()
        {
            var assemblyFileVersion = GetAssemblyFileVersion();
            var assemblyVersion = GetAssemblyVersion();

            if (assemblyFileVersion is null)
                return true;

            if (assemblyFileVersion != assemblyVersion) // TODO temp solution, later compare versions
                return true;

            return false;
        }

        public string? GetAssemblyFileVersion()
        {
            if (!File.Exists(AssemblyFileVersionFullPath))
                return null;

            return File.ReadAllLines(AssemblyFileVersionFullPath).FirstOrDefault();
        }

        public string? GetAssemblyVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            //return version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision;
            return version!.ToString();
        }

        public void CreateAssemblyVersion()
        {            
            var assemblyVersion = GetAssemblyVersion();
            var assemblyFileVersion = GetAssemblyFileVersion();

            if (assemblyFileVersion is null || assemblyFileVersion != assemblyVersion)
            {
                StreamWriter sw = new StreamWriter(AssemblyFileVersionFullPath);
                sw.WriteLine(assemblyVersion);
                sw.Close();
            }
        }

        


        /// <summary>
        /// Executes a SQL command within a transaction using Entity Framework and increase version.
        /// If an error occurs, the transaction is rolled back.
        /// 
        /// ⚠️ Note: If the SQL contains a multi-statement command (e.g., CREATE TRIGGER or PROCEDURE)
        /// that would normally require <c>DELIMITER ;;</c> in MySQL Workbench or CLI,
        /// make sure to use only <c>DELIMITER ;;</c> since this delimiter is removed before running in EF,
        /// as it is not supported through EF or ADO.NET.
        /// </summary>
        /// <param name="fileName"></param>
        public void RunScript1(string fileName, bool firstRunIgnoreVersion=false)
        {
            var version = fileName.Split("/").Last().Replace(".sql", "");

            //if (version < GetMaxVersion())
            //    return;

            if (!firstRunIgnoreVersion && ContainVersion(version))
                return;

            var sql = File.ReadAllText(fileName);
            
            using var context = new EdnKntControllerMysqlContext();
            context.Database.SetCommandTimeout(60 * 10);
            context.Database.BeginTransaction();

            try
            {
                sql = RemoveSpecialCommands(sql);

                context.Database.ExecuteSqlRaw(sql);
                //InsertVersion(version);

                context.Database.CommitTransaction(); //rollback if fail
            } catch(Exception ex) {
                context.Database.RollbackTransaction();

                t.LogEvent2(1, ex.Message);
                throw;
            }
        }

        public void RunScript2(string fileName, bool firstRunIgnoreVersion = false)
        {
            var version = fileName.Split("/").Last().Replace(".sql", "");

            //if (version < GetMaxVersion())
            //    return;

            if (!firstRunIgnoreVersion && ContainVersion(version))
                return;

            var sql = File.ReadAllText(fileName);

            using var context = new EdnKntControllerMysqlContext();
            context.ChangeTracker.Clear();
            context.Database.SetCommandTimeout(60 * 10);
            using var transaction = context.Database.BeginTransaction();

            sql = RemoveSpecialCommands(sql);

            try
            {
                var beginTran = "begin transaction";
                beginTran += sql;
                beginTran += "ROLLBACK";
                context.Database.ExecuteSqlRaw(sql);
                //InsertVersion(version);

                
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                t.LogEvent2(1, ex.Message);
                throw;
            } finally
            {
                transaction.Commit();
            }
        }


        public void RunScript3(string fileName, bool firstRunIgnoreVersion = false)
        {
            using var context = new EdnKntControllerMysqlContext();
            
            using var conn = new MySqlConnection(context.GetConnectionString());

            conn.Open();

            // Avtomatski COMMIT je onemogočen z začetkom transakcije
            var version = fileName.Split("/").Last().Replace(".sql", "");

            //if (version < GetMaxVersion())
            //    return;

            if (!firstRunIgnoreVersion && ContainVersion(version))
                return;

            var sql = File.ReadAllText(fileName);

            using var transaction = conn.BeginTransaction();

            try
            {


                using var cmd2 = new MySqlCommand(sql, conn, transaction);

                cmd2.ExecuteNonQuery();

                // Ročni commit

                transaction.Commit();

            }

            catch

            {

                // Če pride do napake, razveljavi vse

                transaction.Rollback();

                throw;

            }

        }

        public void RunScriptAndEncryptPassword(string fileName) // TODO add user columns
        {
            if (WasScriptRun(fileName, true))
                return;

            RunScript(fileName, true);            
            EncryptPasswordFirstTime();
        }

        void EncryptPasswordFirstTime()
        {
            using var context = new EdnKntControllerMysqlContext();
            var users = context.Users.Where(x => x.PasswordHash == null).ToList();
            foreach (var user in users)
            {
                var iv = _encryption.GenerateRandomIV();
                // getawaiter getresult blocks the current thread until the task is not completed
                var password = string.IsNullOrEmpty(user.Password) ? "" : user.Password;

                if (user.a1 == 1)
                    password = "<DT-Sum>";

                var encryptedPassword = _encryption.Encrypt(password, iv).GetAwaiter().GetResult();

                user.PasswordHash = encryptedPassword;
                user.InitializationVector = iv;
            }

            context.SaveChanges();
        }


        public void RunScript9(string fileName)
        {
            using var context = new EdnKntControllerMysqlContext();
            using var conn = new MySqlConnection(context.GetConnectionString());
            conn.Open();

            // Avtomatski COMMIT je onemogočen z začetkom transakcije
            var version = fileName.Split("/").Last().Replace(".sql", "");



            if (ContainVersion(version))
                return;

            var sql = File.ReadAllText(fileName);

            using var transaction = conn.BeginTransaction();

            try
            {
                using var cmd2 = new MySqlCommand(sql, conn, transaction);
                cmd2.ExecuteNonQuery();

                // Ročni commit

                transaction.Commit();

            }
            catch (Exception ex)
            {
                // Če pride do napake, razveljavi vse
                transaction.Rollback();
                t.LogEvent2(1, ex.Message);
                throw;

            }

        }

        public void RunScript(string fileName, bool runFirstTime=false)
        {
            if (WasScriptRun(fileName, runFirstTime))
                return;

            var version = fileName.Split("/").Last().Replace(".sql", "");
            try
            {                
                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Version", fileName);
                using var context = new EdnKntControllerMysqlContext();
                context.Database.SetCommandTimeout(60 * 10);

                var sql = File.ReadAllText(fullPath);
                sql = RemoveSpecialCommands(sql);
                sql += GetVersionText(version);
                context.Database.ExecuteSqlRaw(sql);
                //context.Database.ExecuteSqlRaw($"CALL ExecuteSqlWithRollback('{sql}');");
                //InsertVersion(version);
            }
            catch (Exception ex)
            {
                t.LogEvent2(2, @$"Errror on upgrade script: {fileName}");
                t.LogEvent2(1, ex.Message);
                t.LogEvent2(3, ex.StackTrace ?? "");
                throw;
            }
        }

        public bool WasScriptRun(string fileName, bool runFirstTime)
        {
            var version = fileName.Split("/").Last().Replace(".sql", "");

            var firstTime = runFirstTime && !TableExists(nameof(App_Version));

            if (!firstTime)
                if (ContainVersion(version))
                    return true;

            return false;
        }



        bool TableExists(string tableName)
        {
            try
            {
                using var context = new EdnKntControllerMysqlContext();
                var version = context.Database.SqlQueryRaw<int>("SELECT count(*) Value FROM information_schema.tables WHERE table_schema = Database() AND table_name = @p0", tableName).FirstOrDefault();

                return version > 0;
            } catch (Exception)
            {

            }
            return false;
        }

        bool ContainVersion(string versionNumber)
        {
            using var context = new EdnKntControllerMysqlContext();
            var version = context.App_Version.Where(x => x.VersionNumber == versionNumber).FirstOrDefault();

            return version != null;
        }

        string GetVersionText(string version)
        {
            return $"INSERT INTO App_Version (VersionNumber, DateAndTime) VALUES ('{version}', NOW());";
        }

        string RemoveSpecialCommands(string sql)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MySql.EntityFrameworkCore;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Options;
using KNTSMM.Data.Models;

namespace KNTCommon.Data.Models
{
    public class EdnKntControllerMysqlContext : DbContext
    {

        // fsta add DbSet here


        public virtual DbSet<Parameter> Parameters { get; set; }

        public virtual DbSet<UserGroup> UserGroups { get; set; }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<UserSession> UserSessions { get; set; }

        public virtual DbSet<ErrorList> ErrorLists { get; set; }

        public virtual DbSet<EventList> EventLists { get; set; }

        public virtual DbSet<LanguageDictionary> LanguageDictionaries { get; set; }

        public virtual DbSet<S7net> S7nets { get; set; }

        public virtual DbSet<IoTasks> IoTasks { get; set; }

        public virtual DbSet<IoTaskDetails> IoTaskDetails { get; set; }

        public virtual DbSet<IoTaskLogs> IoTaskLogs { get; set; }

        public virtual DbSet<Transactions> Transactions { get; set; }

        public virtual DbSet<ServiceControl> ServiceControls { get; set; }

        public virtual DbSet<Results> Results { get; set; }

        public virtual DbSet<AppVersion> AppVersion { get; set; }

        public virtual DbSet<CL_ArchiveIntervalType> CL_ArchiveIntervalType { get; set; }
        public virtual DbSet<CL_ArchiveMode> CL_ArchiveMode { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string[] connStr = GetConnectionData(false);
            optionsBuilder.UseMySQL($"server={connStr[2]};database={connStr[0]};user=KNT;password={connStr[1]};CharSet=utf8;AllowUserVariables=true");
            // AllowUserVariables=true - variable v upgrade skripti
            // charset=utf8mb4 - zaradi cirilice
        }

        public string GetConnectionString()
        {
            string[] connStr = GetConnectionData(false);
            return $"server={connStr[2]};database={connStr[0]};user=KNT;password={connStr[1]}";
        }

        // get connection string data
        public static string[] GetConnectionData(bool archive)
        {
            string[] connectionData = new string[3];

            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string configPath = Path.Combine(basePath, "config.xml");

            XmlDocument doc = new();
            doc.Load(configPath);
            if (doc.DocumentElement != null)
            {
                XmlNode? node = doc.DocumentElement.SelectSingleNode("/config/dbstring");
                if (archive)
                    node = doc.DocumentElement.SelectSingleNode("/config/dbstring_archive");

                if (node != null)
                {
                    connectionData[0] = node.InnerText;
                }
                node = doc.DocumentElement.SelectSingleNode("/config/p");
                if (node != null)
                {
                    connectionData[1] = PManager.DecryptPassword(node.InnerText);
                }
                node = doc.DocumentElement.SelectSingleNode("/config/dbserver");
                if (node != null)
                {
                    connectionData[2] = node.InnerText;
                }

            }
            return connectionData;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Parameter>(entity =>
            {
                entity.HasKey(e => e.ParametersId);
            });

            modelBuilder.Entity<UserGroup>(entity =>
            {
                entity.HasKey(e => e.GroupId);

                entity.ToTable("UserGroup");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UsersId);

                entity.Property(e => e.InitializationVector).HasMaxLength(32);
                entity.Property(e => e.PasswordHash).HasMaxLength(32);
            });
            /*
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasKey(e => e.UserSessionId);

                entity.ToTable("UserSession");

                entity.HasOne(d => d.User).WithMany(p => p.UserSessions)
                    .HasForeignKey(d => d.UsersId)
                    .HasConstraintName("FK_UserSession_Users");
            });*/

            modelBuilder.Entity<ErrorList>(entity =>
            {
                entity.HasKey(e => e.ErrorListId);

                entity.ToTable("ErrorList");

                entity.Property(e => e.ErrorDateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("errorDateTime");
                entity.Property(e => e.ErrorDescription).HasColumnName("errorDescription");
                entity.Property(e => e.ErrorMeasurementNum).HasColumnName("errorMeasurementNum");
                entity.Property(e => e.ErrorType).HasColumnName("errorType");
                entity.Property(e => e.Thread).HasColumnName("thread");
            });

            modelBuilder.Entity<EventList>(entity =>
            {
                entity.HasKey(e => e.EventListId);

                entity.ToTable("EventList");

                entity.Property(e => e.EventDateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("eventDateTime");
                entity.Property(e => e.EventDescription).HasColumnName("eventDescription");
                entity.Property(e => e.EventSequence).HasColumnName("eventSequence");
                entity.Property(e => e.Thread).HasColumnName("thread");
            });

            modelBuilder.Entity<LanguageDictionary>(entity =>
            {
                entity.HasKey(e => e.LanguageDictionaryId);

                entity.ToTable("LanguageDictionary");
            });

            modelBuilder.Entity<S7net>(entity =>
            {
                entity.HasKey(e => e.S7NetId);

                entity.ToTable("S7Net");

                entity.Property(e => e.S7NetId).HasColumnName("s7NetId");
                entity.Property(e => e.ByteString).HasColumnName("byteString");
                entity.Property(e => e.ByteStringDb).HasColumnName("byteStringDb");
                entity.Property(e => e.ByteStringSize).HasColumnName("byteStringSize");
                entity.Property(e => e.ByteStringStartsAt).HasColumnName("byteStringStartsAt");
                entity.Property(e => e.Error).HasColumnName("error");
                entity.Property(e => e.GoodImpregnationScrap).HasColumnName("goodImpregnationScrap");
                entity.Property(e => e.InputString).HasColumnName("inputString");
                entity.Property(e => e.InputStringDb).HasColumnName("inputStringDb");
                entity.Property(e => e.InputStringSize).HasColumnName("inputStringSize");
                entity.Property(e => e.InputStringStartAt).HasColumnName("inputStringStartAt");
                entity.Property(e => e.InterruptTheProcess).HasColumnName("interruptTheProcess");
                entity.Property(e => e.LeakNominalImpregnation).HasColumnName("leakNominalImpregnation");
                entity.Property(e => e.LeakNominalImpregnationMinus).HasColumnName("leakNominalImpregnationMinus");
                entity.Property(e => e.LeakNominalScrap).HasColumnName("leakNominalScrap");
                entity.Property(e => e.LeakNominalScrapMinus).HasColumnName("leakNominalScrapMinus");
                entity.Property(e => e.LeakResult).HasColumnName("leakResult");
                entity.Property(e => e.MachineId).HasColumnName("machineId");
                entity.Property(e => e.MeasNum).HasColumnName("measNum");
                entity.Property(e => e.MeasNumFromMachine).HasColumnName("measNumFromMachine");
                entity.Property(e => e.Phase).HasColumnName("phase");
                entity.Property(e => e.ProgNumMeasured).HasColumnName("progNumMeasured");
                entity.Property(e => e.ProgTriggered).HasColumnName("progTriggered");
                entity.Property(e => e.SpecialInputs).HasColumnName("specialInputs");
                entity.Property(e => e.SpecialOutputs).HasColumnName("specialOutputs");
                entity.Property(e => e.Start).HasColumnName("start");
                entity.Property(e => e.StopTheCycle).HasColumnName("stopTheCycle");
                entity.Property(e => e.Thread).HasColumnName("thread");
                entity.Property(e => e.ThreadMeasured).HasColumnName("threadMeasured");
                entity.Property(e => e.ThreadTriggered).HasColumnName("threadTriggered");
                entity.Property(e => e.WaitMachine).HasColumnName("waitMachine");
                entity.Property(e => e.Warning).HasColumnName("warning");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UsersId);

                entity.Property(e => e.InitializationVector).HasMaxLength(32);
                entity.Property(e => e.PasswordHash).HasMaxLength(32);
            });

            modelBuilder.Entity<IoTasks>(entity =>
            {
                entity.HasKey(e => e.IoTaskId);

                entity.ToTable("IoTasks");
            });

            modelBuilder.Entity<IoTaskDetails>(entity =>
            {
                entity.HasKey(e => e.IoTaskDetailId);

                entity.ToTable("IoTaskDetails");
            });

            modelBuilder.Entity<Transactions>(entity =>
            {
                entity.HasKey(e => e.TransactionId);

                entity.ToTable("Transactions");
            });

            modelBuilder.Entity<ServiceControl>(entity =>
            {
                entity.HasKey(e => e.ServiceId);

                entity.ToTable("ServiceControl");
            });

        }
    }
}

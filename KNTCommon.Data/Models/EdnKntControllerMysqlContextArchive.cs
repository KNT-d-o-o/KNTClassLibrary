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

namespace KNTCommon.Data.Models
{
    public class EdnKntControllerMysqlContextArchive : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string[] connStr = EdnKntControllerMysqlContext.GetConnectionData(true);
            optionsBuilder.UseMySQL($"server={connStr[2]};database={connStr[0]};user=KNT;password={connStr[1]}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


        }
    }
}

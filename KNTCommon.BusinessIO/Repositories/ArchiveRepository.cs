using AutoMapper;
using KNTCommon.Data.Models;
using KNTToolsAndAccessories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KNTCommon.BusinessIO.Repositories
{
    public class ArchiveRepository : IArchiveRepository
    {
        private readonly IDbContextFactory<EdnKntControllerMysqlContext> Factory;
        private readonly IMapper AutoMapper;
        private readonly Tools t = new();

        public ArchiveRepository(IDbContextFactory<EdnKntControllerMysqlContext> factory, IMapper automapper)
        {
            Factory = factory;
            AutoMapper = automapper;
        }

        public void CheckTables()
        {
            var tableNames = new List<string>();

            try
            {
                using (var context = Factory.CreateDbContext())
                {
                    var query = $"SELECT TableName FROM Tables;";

                    var connection = context.Database.GetDbConnection();
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tableNames.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.Repositories.ArchiveRepository #1 " + ex.Message);
            }

        }

    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KNTCommon.BusinessIO;
using KNTCommon.BusinessIO.Service;
using System.Diagnostics;
using KNTCommon.BusinessIO.Repositories;
using Microsoft.EntityFrameworkCore;
using KNTCommon.BusinessIO.AutoMapper;
using KNTCommon.Data.Models;
using KNTCommon.Business.Repositories;

namespace KNTCommon.BusinessIO.Service
{
    public class Program
    {
        public const string ServiceVersion = AppInfo.Version;

        public static void Main(string[] args)
        {
            Console.WriteLine($"Starting service version {ServiceVersion}.");

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal; // set priority BelowNormal (lower)

            var host = Host.CreateDefaultBuilder(args)
                .UseWindowsService() // enable Windows Service
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContextFactory<EdnKntControllerMysqlContext>(options =>
                        options.UseMySQL("Server=.;Initial Catalog=EDN-KNTControllerMT;Trusted_Connection=True;TrustServerCertificate=True;"));
                    services.AddDbContextFactory<EdnKntControllerMysqlContextArchive>(options =>
                       options.UseMySQL("Server=.;Initial Catalog=EDN-KNTControllerMTArchive;Trusted_Connection=True;TrustServerCertificate=True;"));
                    services.AddAutoMapper(typeof(MappingProfiles));
                    services.AddScoped<IoTasksRepository>();
                    services.AddScoped<ArchiveRepository>();
                    services.AddScoped<ExportRepository>();
                    services.AddScoped<DumpRepository>();
                    services.AddScoped<ParametersRepository>();
                    services.AddSingleton<BusinessIOProcess>();
                    services.AddHostedService<BusinessIOService>();
                    services.AddSingleton(ServiceVersion);
                })
                .Build();

            host.Run();
        }
    }
}
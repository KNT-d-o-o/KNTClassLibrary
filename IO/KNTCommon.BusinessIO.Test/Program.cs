using AutoMapper;
using KNTCommon.Business.Repositories;
using KNTCommon.Business.Scripts;
using KNTCommon.BusinessIO;
using KNTCommon.BusinessIO.AutoMapper;
using KNTCommon.BusinessIO.Repositories;
using KNTCommon.Data.Models;
using KNTToolsAndAccessories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace KNTCommon.BusinessIO.Test
{
    internal class Program
    {
        static async Task Main()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContextFactory<EdnKntControllerMysqlContext>(opt =>
                        opt.UseMySQL("Server=.;Initial Catalog=EDN-KNTControllerMT;Trusted_Connection=True;TrustServerCertificate=True;"));
                    services.AddDbContextFactory<EdnKntControllerMysqlContextArchive>(options =>
                       options.UseMySQL("Server=.;Initial Catalog=EDN-KNTControllerMTArchive;Trusted_Connection=True;TrustServerCertificate=True;"));
                    services.AddAutoMapper(typeof(MappingProfiles));
                    services.AddScoped<IoTasksRepository>();
                    services.AddScoped<ArchiveRepository>();
                    services.AddScoped<ExportRepository>();
                    services.AddScoped<DumpRepository>();
                    services.AddScoped<ParametersRepository>();
                    services.AddScoped<IParametersRepository, ParametersRepository>();
                    services.AddScoped<Localization>();
                    services.AddSingleton<BusinessIOProcess>();                
                });
            var host = builder.Build();

            var proc = host.Services.GetRequiredService<BusinessIOProcess>();
            using var cts = new CancellationTokenSource();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // for Event viewer (admin)
                PowerShellHelper.CreateEventSource();
                Tools.LogEvent("Start KNT IO console application", EventLogEntryType.Information);
            }

#if DEBUG
            Console.WriteLine($"Start KNTCommon.BusinessIO.Test application version {typeof(Program).Assembly.GetName().Version}.");
#endif

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                await proc.OnStartAsync(cts.Token, AppInfo.Version);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled.");
            }
            finally
            {
                proc.OnStop();
            }

#if DEBUG
            Console.WriteLine("Exit KNTCommon.BusinessIO.Test application.");
#endif
        }


    }
}

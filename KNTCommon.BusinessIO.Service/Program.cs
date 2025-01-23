using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KNTCommon.BusinessIO;
using KNTCommon.BusinessIO.Service;

namespace KNTCommon.BusinessIO.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseWindowsService() // Omogoči Windows Service
                .ConfigureServices(services =>
                {
                    services.AddSingleton<BusinessIOProcess>(); // Registracija vašega razreda
                    services.AddHostedService<BusinessIOService>(); // Registracija HostedService
                })
                .Build();

            host.Run();
        }
    }
}
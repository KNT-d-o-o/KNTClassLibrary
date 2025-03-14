using KNTCommon.BusinessIO.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KNTCommon.BusinessIO.Service
{
    public class BusinessIOService : IHostedService
    {
        private readonly BusinessIOProcess _businessIOProcess;
        private readonly string _serviceVersion;
        private CancellationTokenSource? _cts;

        public BusinessIOService(BusinessIOProcess businessIOProcess, string serviceVersion)
        {
            _businessIOProcess = businessIOProcess;
            _serviceVersion = serviceVersion;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("BusinessIOService Starting...");
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Zaženemo asinhron proces
            Task.Run(() => _businessIOProcess.OnStartAsync(_cts.Token, _serviceVersion), cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("BusinessIOService Stopping...");
            _cts?.Cancel();

            _businessIOProcess.OnStop();
            return Task.CompletedTask;
        }
    }
}

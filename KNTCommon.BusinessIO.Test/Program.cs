using KNTCommon.BusinessIO;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KNTCommon.BusinessIO.Test
{
    internal class Program
    {
        private static readonly BusinessIOProcess proc = new();
        private static readonly CancellationTokenSource cts = new();

        static async Task Main()
        {
#if DEBUG
            Console.WriteLine("Start KNTCommon.BusinessIO.Test application.");
#endif

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                await proc.OnStartAsync(cts.Token);
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

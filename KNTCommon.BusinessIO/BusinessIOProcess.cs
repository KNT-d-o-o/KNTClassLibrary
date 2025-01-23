using System;
using System.Threading;
using System.Threading.Tasks;
using KNTToolsAndAccessories;


namespace KNTCommon.BusinessIO
{
    public class BusinessIOProcess
    {
        private readonly Tools t = new(); // Tools je vaš razred, ki mora biti ustrezno implementiran
        private bool procBusy = false;
        private readonly double TIMER_INTERVAL_NORMAL = 1000;

        public async Task<bool> OnStartAsync(CancellationToken cancellationToken)
        {
            if (BusinessIoInit())
            {
                Console.WriteLine("BusinessIOProcess OnStart.");

                // Simulacija začetne logike
                while (!cancellationToken.IsCancellationRequested)
                {
                    await OnElapsedTimeAsync();
                    await Task.Delay(TimeSpan.FromMilliseconds(TIMER_INTERVAL_NORMAL), cancellationToken);
                }

                return true;
            }
            else
            {
                Console.WriteLine("BusinessIoInit fault.");
                OnStop();
                return false;
            }
        }

        public bool BusinessIoInit()
        {
            bool ret = true;

            // TODO: Inicializacijska logika
      //      CheckTables();

            return ret;
        }

        public async Task OnElapsedTimeAsync()
        {
            if (procBusy) return;

            procBusy = true;

            try
            {
                // TODO: Implementacija vaše logike, ki se izvaja ob vsakem preteku časovnika
                await Task.Run(() =>
                {
                    // Simulacija dela
                    Console.WriteLine("BusinessIOProcess OnElapsedTime.");
                });
            }
            catch (Exception ex)
            {
                t.LogEvent($"Error in OnElapsedTime: {ex.Message}");
            }
            finally
            {
                procBusy = false;
            }
        }

        public void OnStop()
        {
            try
            {
                // TODO: Čiščenje virov
                Console.WriteLine("BusinessIOProcess OnStop.");
            }
            catch (Exception ex)
            {
                t.LogEvent($"Error in OnStop: {ex.Message}");
            }
        }
    }
}
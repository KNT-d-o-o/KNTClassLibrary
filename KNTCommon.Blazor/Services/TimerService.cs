using KNTCommon.Business.EventHandlers;
using KNTCommon.Business.Models;

namespace KNTCommon.Blazor.Services
{
    public class TimerService
    {
        private Timer? Timer;
        public event DataUpdatedEventHandler? DataUpdated;

        private void TimerCallback(object? state)
        {
            DataUpdated?.Invoke(this, new DataUpdatedEventArgs());
        }

        public void StartTimer(TimeSpan interval)
        {
            // Create a new timer with specified interval
            Timer = new Timer(TimerCallback, null, TimeSpan.Zero, interval);
        }

        public void StopTimer()
        {
            if (Timer != null)
                Timer.Dispose();
        }

    }

}
using Sharp7;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTPlc.S7
{
    public class S7Controller
    {
        private readonly object _locker = new();
        private Sharp7Client client = new();

        public bool IsConnected => client.IsConnected;

        public bool Connect(string ip, int rack = 0, int slot = 1)
        {
            lock (_locker)
            {
                return client.Connect(ip, rack, slot);
            }
        }

        public void Disconnect()
        {
            lock (_locker)
            {
                client.Disconnect();
            }
        }

        // read
        public (bool Success, int ErrorCode) ReadDb(int dbNumber, int start, byte[] buffer)
        {
            if (!IsConnected) return (false, S7Consts.errTCPConnectionFailed);
            lock (_locker)
            {
                int result = client.DBRead(dbNumber, start, buffer);
                return (result == 0, result);
            }
        }

        public (bool Success, int ErrorCode) WriteDb(int dbNumber, int start, byte[] buffer)
        {
            if (!IsConnected) return (false, S7Consts.errTCPConnectionFailed);
            lock (_locker)
            {
                int result = client.DBWrite(dbNumber, start, buffer);
                return (result == 0, result);
            }
        }

    }
}

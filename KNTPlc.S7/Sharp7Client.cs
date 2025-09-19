using Sharp7;

namespace KNTPlc.S7
{
    public class Sharp7Client
    {
        private readonly object _locker = new object();
        private S7Client _client = new S7Client();
        private bool _isConnected = false;

        public bool IsConnected => _isConnected;

        /// <summary>
        /// Connect to PLC
        /// </summary>
        public bool Connect(string ip, int rack = 0, int slot = 1)
        {
            lock (_locker)
            {
                int result = _client.ConnectTo(ip, rack, slot);
                _isConnected = result == 0;
                return _isConnected;
            }
        }

        /// <summary>
        /// Disconnect
        /// </summary>
        public void Disconnect()
        {
            lock (_locker)
            {
                if (_isConnected)
                    _client.Disconnect();
                _isConnected = false;
            }
        }

        /// <summary>
        /// Read data from DB
        /// </summary>
        public int DBRead(int dbNumber, int start, byte[] buffer)
        {
            if (!_isConnected) return -1;
            lock (_locker)
            {
                return _client.DBRead(dbNumber, start, buffer.Length, buffer);
            }
        }

        /// <summary>
        /// Write data into DB
        /// </summary>
        public int DBWrite(int dbNumber, int start, byte[] buffer)
        {
            if (!_isConnected) return -1;
            lock (_locker)
            {
                return _client.DBWrite(dbNumber, start, buffer.Length, buffer);
            }
        }
    }
}

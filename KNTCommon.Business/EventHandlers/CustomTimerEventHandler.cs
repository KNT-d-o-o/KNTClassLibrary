using KNTCommon.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.EventHandlers
{
    public delegate void DataUpdatedEventHandler(object sender, DataUpdatedEventArgs e);

    public class DataUpdatedEventArgs : EventArgs
    {
        public string Message { get; }

        public DataUpdatedEventArgs()
        {
            Message = string.Empty;
        }

        public DataUpdatedEventArgs(string yourData)
        {
            Message = yourData;
        }
    }


}
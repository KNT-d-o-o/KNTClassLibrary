using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.MySqlWebApi
{
    public static class AppInfo
    {
        public static readonly string Version = typeof(AppInfo).Assembly.GetName().Version?.ToString() ?? "unknown";
    }

    /* *** Version 1.0.0.1 *** 16/09/2025
     * Enhencement:
     *  Added web browser support.
     * 
    /* *** Version 1.0.0.1 *** 27/08/2025
     * First version 
     */

}

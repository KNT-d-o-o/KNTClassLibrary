using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.BusinessIO
{
    public static class AppInfo
    {
        public const string Version = "1.0.1.3";
    }

    /* *** Version 1.0.1.x *** 30/05/2025
     * Bugs:
     *  DB Export where condition for step 0 stay par3
     * 
    /* *** Version 1.0.1.2 *** 22/05/2025
     * Enhancement:
     *  Data: default data changes.
     *  None tables: do not set automatic to none.
     * 
    /* *** Version 1.0.1.1 *** 22/05/2025
     * Enhancement:
     *  Excel: localization support, cycling export, null values handling, own column for own data in connected views.
     *  Archive: optimization timeout to 2 hours, ../Archive foldel to create if does not exists.
     *  DB export: single long tables support, zip choose correction, excluded dump tables with none.
     * 
    /* *** Version 1.0.0.2 *** 11/04/2025
     * Archive: replace other tables at startup.
     * 
    /* *** Version 1.0.0.1 *** 10/04/2025
     * Features:
     *  DB Archive corrections.
     * 
     1.0.1 
    /* *** Version 1.0.0.0 *** 13/03/2025
     * First version 
     */

}

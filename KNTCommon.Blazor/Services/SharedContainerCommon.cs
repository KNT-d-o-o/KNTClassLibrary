
namespace KNTCommon.Blazor.Services
{
    /// <summary>
    /// Shared variables for all pages
    /// </summary>
    public class SharedContainerCommon
    {
        static string _actionTitle = "";

        public bool Additional = false;
        static public string actionTitle { 
            get { return (IsOldArchiveDatabaseStatic) ? "Archive" : _actionTitle; } 
            set { _actionTitle = value; } 
        }  //Overview
        public int LoggedPower = 99;
        public int pageSize { get; set; } = 5;

        public int TotalPageResultsNum { get; set; } = 50;
        public string[] plcService = { };

        // TODO TEMP till  permission is done
        public bool IsArchive { get; } = false;
        public bool IsOldArchiveDatabase { get; } = false;
        public static bool IsOldArchiveDatabaseStatic { get; } = false;
    }
}

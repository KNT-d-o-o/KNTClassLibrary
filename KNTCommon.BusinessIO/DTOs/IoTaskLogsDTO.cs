namespace KNTCommon.BusinessIO.DTOs
{
    public class IoTaskLogsDTO
    {
        public int IoTaskLogId { get; set; }

        public int? IoTaskId { get; set; }

        public int? IoTaskLogType { get; set; }

        public string? Info { get; set; }

        public DateTime? DateAndTime { get; set; }

    }
}

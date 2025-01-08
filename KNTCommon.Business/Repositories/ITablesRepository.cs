using Google.Protobuf.Reflection;
using System.Collections.Generic;

namespace KNTCommon.Business.Repositories
{
    public interface ITablesRepository
    {
        Task<List<string>> GetDatabaseTablesAsync();
        Task<(IEnumerable<Dictionary<string, object>> results, List<string> columnNames, List<string> columnPkNames)> GetDataFromTableAsync(string table, Dictionary<string, object> whrereCondition, string orderBy);
        bool UpdateTableRow(string table, Dictionary<string, object> row, List<string>? pk);
        bool InsertTableRow(string table, Dictionary<string, object> row);
        bool DeleteTableRow(string table, Dictionary<string, object> row, List<string>? pk);
    }
}
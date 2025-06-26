

using System.Text.RegularExpressions;

public static class MyLoggerCommon
{
    static readonly object _lock = new();
    static int _queryCount = 0;
    static string filePath = "C:/log.txt";
    public static void WriteLine(string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        lock (_lock)
        {
            // Preverimo, ali gre za EF Core SQL log (zelo osnovna detekcija)
            if (message.Contains("Microsoft.EntityFrameworkCore.Database.Command"))
            {            
                    _queryCount++;
                    string fileName = $"query_{_queryCount:D4}_{timestamp}.sql";

                    string formattedLog = FormatLog(message);

                    File.AppendAllText(filePath, formattedLog);
            
            } else {
                File.AppendAllText(filePath, $"{timestamp} [LOG] {message}{Environment.NewLine}");
            }
        }
    }

    static string FormatLog(string rawLog)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        // 1. Izvleci parametre in jih pretvori v SET stavek
        string parameters = string.Empty;
        var paramMatch = Regex.Match(rawLog, @"Parameters=\[(.*?)\]", RegexOptions.Singleline);
        if (paramMatch.Success)
        {
            var rawParams = paramMatch.Groups[1].Value;
            var paramList = rawParams.Split(new[] { "), " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var p in paramList)
            {
                // Odstrani vse oklepaje in vsebino znotraj njih
                var clean = Regex.Replace(p, @"\s*\(.*?\)", "").Trim();

                // Poskrbi, da se konča s podpičjem
                if (!clean.EndsWith(";"))
                    clean += ";";

                parameters += $"SET {clean}\n";
            }
        }
        else
        {
            parameters = "-- Ni parametrov";
        }


        // 2. Izvleci SQL stavek (vse po prvi vrstici 'Executed DbCommand (...)' naprej)
        string sql = "SQL ni bil najden.";
        var executedIndex = rawLog.IndexOf("Executed DbCommand", StringComparison.OrdinalIgnoreCase);
        string executionTime = string.Empty;

        if (executedIndex != -1)
        {
            var nextLineIndex = rawLog.IndexOf('\n', executedIndex);
            if (nextLineIndex != -1 && nextLineIndex + 1 < rawLog.Length)
            {
                sql = rawLog.Substring(nextLineIndex + 1).Trim();

                // 3. Izvleci Execution Time (v primeru, da je prisotna)
                var timeMatch = Regex.Match(rawLog, @"Executed DbCommand.*?(\d+\.?\d*)\s*ms", RegexOptions.Singleline);
                if (timeMatch.Success)
                {
                    executionTime = timeMatch.Groups[1].Value;
                }
            }
        }
        else
        {
            var executedIndex2 = rawLog.IndexOf("Failed executing DbCommand", StringComparison.OrdinalIgnoreCase);

            if (executedIndex2 != -1)
            {
                var nextLineIndex = rawLog.IndexOf('\n', executedIndex2);
                if (nextLineIndex != -1 && nextLineIndex + 1 < rawLog.Length)
                {
                    sql = "\t-- ERROR EXECUTING -- \n";
                    sql += rawLog.Substring(nextLineIndex + 1).Trim();

                    // 3. Izvleci Execution Time (v primeru, da je prisotna)
                    var timeMatch = Regex.Match(rawLog, @"Failed executing DbCommand.*?(\d+\.?\d*)\s*ms", RegexOptions.Singleline);
                    if (timeMatch.Success)
                    {
                        executionTime = timeMatch.Groups[1].Value;
                    }
                }
            }
        }

            return
        $@"-- EF Core Query Log
-- Timestamp: {timestamp}

-- Parameters:
{parameters}

-- SQL:
{sql}

-- Execution Time: {executionTime} ms
";
    }

}


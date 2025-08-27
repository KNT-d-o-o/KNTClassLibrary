using MySql.Data.MySqlClient;
using System.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KNTCommon.Data.Models;
using Microsoft.EntityFrameworkCore;

#if DEBUG
Console.WriteLine($"Start KNTCommon.MySqlWebApi application version {typeof(Program).Assembly.GetName().Version}.");
#endif

var builder = WebApplication.CreateBuilder(args);

// 👇 Windows Service support
builder.Host.UseWindowsService();

if (OperatingSystem.IsWindows())
    builder.Logging.AddEventLog();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5025); // listening on port 5025 on all network interfaces
});

var app = builder.Build();

// 📌 Connection string
string? connectionString = EdnKntControllerMysqlContext.GetConnectionString();
// 🔑 set API key
string apiKey = EdnKntControllerMysqlContext.GetPWebApi();


// 🔹 Middleware for checking API key
app.Use(async (context, next) =>
{
    // enable without API keys only for /admin/set-key
    if (context.Request.Path.StartsWithSegments("/admin/set-key", StringComparison.OrdinalIgnoreCase))
    {
        await next();
        return;
    }

    if (!context.Request.Headers.TryGetValue("X-API-Key", out var key) || key != apiKey)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }
    await next();
});

// 📌 Endpoint to get data
app.MapGet("/data/{name}", async (string name, HttpContext context) =>
{
    using var conn = new MySqlConnection(connectionString);
    await conn.OpenAsync();

    // find definition from table webapiquery
    string sql = @"SELECT QueryName, TableName, SelectExpr, WhereExpr, OrderByExpr 
                   FROM webapiquery WHERE LOWER(QueryName) = @name LIMIT 1";
    using var cmd = new MySqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@name", name.ToLower());

    using var reader = await cmd.ExecuteReaderAsync();
    if (!await reader.ReadAsync())
    {
        return Microsoft.AspNetCore.Http.Results.Problem($"Unknown data type '{name}'", statusCode: 400);
    }

    // read definition
    string table = reader.GetString("TableName");
    string sel = reader.GetString("SelectExpr");
    string? where = reader.IsDBNull(reader.GetOrdinal("WhereExpr")) ? null : reader.GetString("WhereExpr");
    string? order = reader.IsDBNull(reader.GetOrdinal("OrderByExpr")) ? null : reader.GetString("OrderByExpr");

    await reader.CloseAsync();

    var results = new List<Dictionary<string, object>>();

    // build a query
    using var cmd2 = new MySqlCommand($"SELECT {sel} FROM {table}" +
        (!string.IsNullOrWhiteSpace(where) ? $" WHERE {where}" : "") +
        (!string.IsNullOrWhiteSpace(order) ? $" ORDER BY {order}" : ""),
        conn
    );

    using var reader2 = await cmd2.ExecuteReaderAsync();
    while (await reader2.ReadAsync())
    {
        var row = new Dictionary<string, object>();
        for (int i = 0; i < reader2.FieldCount; i++)
            row[reader2.GetName(i)] = reader2.IsDBNull(i) ? null! : reader2.GetValue(i);
        results.Add(row);
    }

    // check query string ?format=text
    string? format = context.Request.Query["format"];
    if (string.Equals(format, "text", StringComparison.OrdinalIgnoreCase))
    {
        var sb = new System.Text.StringBuilder();
        foreach (var row in results)
        {
            foreach (var kv in row)
                sb.AppendLine($"{kv.Key} : {kv.Value}");
            sb.AppendLine();
        }
        return Microsoft.AspNetCore.Http.Results.Text(sb.ToString(), "text/plain; charset=utf-8");
    }

    return Microsoft.AspNetCore.Http.Results.Json(results);
});

// 📌 Endpoint to set a new API key (only from localhost)
app.MapPost("/admin/set-key", async context =>
{
    // check if calling is localhost
    var remoteIp = context.Connection.RemoteIpAddress;
    if (remoteIp == null || !(remoteIp.Equals(System.Net.IPAddress.Loopback) || remoteIp.Equals(System.Net.IPAddress.IPv6Loopback)))
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsync("Forbidden: only localhost allowed");
        return;
    }

    // read JSON body
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    var json = System.Text.Json.JsonDocument.Parse(body);
    if (!json.RootElement.TryGetProperty("newKey", out var newKeyElement))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Missing newKey");
        return;
    }

    string newKey = newKeyElement.GetString()!;

    // save new key
    EdnKntControllerMysqlContext.SetPWebApi(newKey);

    apiKey = newKey;

    await context.Response.WriteAsync("API key updated successfully");
});

await app.StartAsync();
await app.WaitForShutdownAsync();

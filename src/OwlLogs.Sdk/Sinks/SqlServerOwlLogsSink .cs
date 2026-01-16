using Microsoft.Data.SqlClient;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Models;
using OwlLogs.Sdk.Options;

public sealed class SqlServerOwlLogsSink :
    IOwlLogsSink,
    ISchemaAwareSink
{
    private readonly SqlServerSinkOptions _options;

    public SqlServerOwlLogsSink(OwlLogsOptions options)
    {
        _options = options.SqlServer;
    }

    public async Task EnsureSchemaAsync()
    {
        if (!_options.AutoCreateTable)
            return;

        using var conn = new SqlConnection(_options.ConnectionString);
        await conn.OpenAsync();

        var sql = $"""
        IF NOT EXISTS (
            SELECT 1 FROM sys.tables WHERE name = '{_options.TableName}'
        )
        BEGIN
            CREATE TABLE {_options.TableName} (
                id UNIQUEIDENTIFIER NOT NULL,
                level INT NOT NULL,
                message NVARCHAR(MAX),
                path NVARCHAR(500),
                method NVARCHAR(10),
                status_code INT,
                created_at DATETIME2 NOT NULL
            )
        END
        """;

        using var cmd = new SqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task WriteAsync(ApiLogEntry entry)
    {
        using var conn = new SqlConnection(_options.ConnectionString);
        await conn.OpenAsync();

        var sql = $"""
        INSERT INTO {_options.TableName}
        (
            id,
            level,
            message,
            path,
            method,
            status_code,
            created_at
        )
        VALUES
        (
            @id,
            @level,
            @message,
            @path,
            @method,
            @status_code,
            @created_at
        )
        """;

        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
        cmd.Parameters.AddWithValue("@level", (int)entry.Level);
        cmd.Parameters.AddWithValue("@message", entry.Exception?.Message);
        cmd.Parameters.AddWithValue("@path", entry.Path);
        cmd.Parameters.AddWithValue("@method", entry.Method);
        cmd.Parameters.AddWithValue("@status_code", entry.StatusCode);
        cmd.Parameters.AddWithValue("@created_at", entry.OccurredAt);

        await cmd.ExecuteNonQueryAsync();
    }
}

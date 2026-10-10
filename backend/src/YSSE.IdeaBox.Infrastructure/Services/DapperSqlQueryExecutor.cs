using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Infrastructure.Data;

namespace YSSE.IdeaBox.Infrastructure.Services;

/// <summary>
/// Dapper-backed read model executor for complex/reporting SQL. All values are passed
/// as parameters; SQL text is never built by concatenating user input.
/// </summary>
public sealed class DapperSqlQueryExecutor : ISqlQueryExecutor
{
    private readonly string _connectionString;

    public DapperSqlQueryExecutor(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")
            ?? AppDbContextOptions.DefaultConnectionString;
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? parameters = null, CancellationToken ct = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var rows = await connection.QueryAsync<T>(new CommandDefinition(sql, parameters, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null, CancellationToken ct = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<T>(new CommandDefinition(sql, parameters, cancellationToken: ct));
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken ct = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: ct));
    }
}

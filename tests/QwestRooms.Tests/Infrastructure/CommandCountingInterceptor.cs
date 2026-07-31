using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace QwestRooms.Tests.Infrastructure;

/// <summary>
/// Records every SQL command a context sends, so a test can assert on how many there were and what
/// they said.
/// </summary>
/// <remarks>
/// This is the instrument the catalogue's headline claim rests on. "Rendering a page costs two
/// queries" is the kind of property that any well-meaning refactor can reverse without changing a
/// single visible behaviour, so it is measured rather than described.
/// </remarks>
public sealed class CommandCountingInterceptor : DbCommandInterceptor
{
    private readonly List<string> _commands = [];

    public IReadOnlyList<string> Commands => _commands;

    public int Count => _commands.Count;

    public void Reset() => _commands.Clear();

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        Record(command);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        Record(command);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        Record(command);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        Record(command);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        Record(command);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        Record(command);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    /// <summary>All recorded SQL, numbered -- used as the assertion message when a count is wrong.</summary>
    public string Describe() =>
        _commands.Count == 0
            ? "(no SQL was executed)"
            : string.Join(
                Environment.NewLine,
                _commands.Select((sql, index) => $"[{index + 1}] {Flatten(sql)}"));

    private void Record(DbCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        _commands.Add(command.CommandText);
    }

    private static string Flatten(string sql)
    {
        var single = string.Join(' ', sql.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()));
        return single.Length <= 200 ? single : single[..200] + " ...";
    }
}

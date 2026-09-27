using System.Globalization;
using Microsoft.Data.Sqlite;

namespace SoulCore.Memory.Repositories;

/// <summary>
/// PROP-3 Wave 1: durable chat transcript over the <c>chat_messages</c> table (migration 007).
/// </summary>
public sealed class SqliteChatTranscriptRepository : IChatTranscriptStore
{
    /// <summary>Upper bound on a single hydrate page, so one request cannot pull the whole table.</summary>
    public const int MaxPageSize = 500;

    private static readonly string[] AllowedRoles = { "user", "assistant" };

    private readonly SqliteMemorySession _session;

    public SqliteChatTranscriptRepository(SqliteMemorySession session) =>
        _session = session ?? throw new ArgumentNullException(nameof(session));

    public async Task<long> AppendAsync(
        string conversationId,
        string role,
        string content,
        string? channel = null,
        string? frameId = null,
        string? mediaId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            throw new ArgumentException("Conversation id must be non-empty.", nameof(conversationId));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Transcript content must be non-empty.", nameof(content));

        var normalizedRole = NormalizeRole(role);
        var occurredAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        var trimmedFrameId = string.IsNullOrWhiteSpace(frameId) ? null : frameId.Trim();

        return await _session.RunDbAsync(async ct =>
        {
            // A retried WS write must not duplicate the turn. The partial unique index on
            // frame_id enforces this, but check first so the common path avoids an exception.
            if (trimmedFrameId is not null)
            {
                var existing = await FindIdByFrameAsync(trimmedFrameId, ct).ConfigureAwait(false);
                if (existing > 0)
                    return existing;
            }

            await using var cmd = _session.Connection.CreateCommand();
            cmd.CommandText =
                """
                INSERT INTO chat_messages
                    (conversation_id, role, content, occurred_at, channel, frame_id, media_id)
                VALUES
                    ($conversation_id, $role, $content, $occurred_at, $channel, $frame_id, $media_id);
                """;
            cmd.Parameters.AddWithValue("$conversation_id", conversationId.Trim());
            cmd.Parameters.AddWithValue("$role", normalizedRole);
            cmd.Parameters.AddWithValue("$content", content.Trim());
            cmd.Parameters.AddWithValue("$occurred_at", occurredAt);
            cmd.Parameters.AddWithValue("$channel", (object?)channel?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$frame_id", (object?)trimmedFrameId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$media_id", (object?)mediaId?.Trim() ?? DBNull.Value);

            try
            {
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
            catch (SqliteException) when (trimmedFrameId is not null)
            {
                // Lost a race on the same frame id — the other writer's row is the truth.
                var raced = await FindIdByFrameAsync(trimmedFrameId, ct).ConfigureAwait(false);
                if (raced > 0)
                    return raced;
                throw;
            }

            await using var idCmd = _session.Connection.CreateCommand();
            idCmd.CommandText = "SELECT last_insert_rowid();";
            var result = await idCmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return result is null or DBNull ? 0 : Convert.ToInt64(result, CultureInfo.InvariantCulture);
        }, cancellationToken).ConfigureAwait(false);
    }

    public Task<IReadOnlyList<ChatTranscriptMessage>> GetSinceAsync(
        string conversationId,
        long afterId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            return Task.FromResult<IReadOnlyList<ChatTranscriptMessage>>(Array.Empty<ChatTranscriptMessage>());

        return ReadPageAsync(
            """
            SELECT id, conversation_id, role, content, occurred_at, channel, frame_id, media_id
            FROM chat_messages
            WHERE conversation_id = $conversation_id AND id > $after_id
            ORDER BY id ASC
            LIMIT $limit;
            """,
            conversationId,
            afterId,
            limit,
            reverse: false,
            cancellationToken);
    }

    public Task<IReadOnlyList<ChatTranscriptMessage>> GetRecentAsync(
        string conversationId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            return Task.FromResult<IReadOnlyList<ChatTranscriptMessage>>(Array.Empty<ChatTranscriptMessage>());

        // Take the newest rows, then flip so callers always receive oldest-first.
        return ReadPageAsync(
            """
            SELECT id, conversation_id, role, content, occurred_at, channel, frame_id, media_id
            FROM chat_messages
            WHERE conversation_id = $conversation_id AND id > $after_id
            ORDER BY id DESC
            LIMIT $limit;
            """,
            conversationId,
            afterId: 0,
            limit,
            reverse: true,
            cancellationToken);
    }

    public async Task<long> GetLatestCursorAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            return 0;

        return await _session.RunDbAsync(async ct =>
        {
            await using var cmd = _session.Connection.CreateCommand();
            cmd.CommandText = "SELECT MAX(id) FROM chat_messages WHERE conversation_id = $conversation_id;";
            cmd.Parameters.AddWithValue("$conversation_id", conversationId.Trim());
            var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return result is null or DBNull ? 0 : Convert.ToInt64(result, CultureInfo.InvariantCulture);
        }, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<ChatTranscriptMessage>> ReadPageAsync(
        string sql,
        string conversationId,
        long afterId,
        int limit,
        bool reverse,
        CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(limit, 1, MaxPageSize);

        return await _session.RunDbAsync(async ct =>
        {
            await using var cmd = _session.Connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("$conversation_id", conversationId.Trim());
            cmd.Parameters.AddWithValue("$after_id", afterId < 0 ? 0 : afterId);
            cmd.Parameters.AddWithValue("$limit", pageSize);

            var rows = new List<ChatTranscriptMessage>();
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                rows.Add(new ChatTranscriptMessage(
                    reader.GetInt64(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.IsDBNull(5) ? null : reader.GetString(5),
                    reader.IsDBNull(6) ? null : reader.GetString(6),
                    reader.IsDBNull(7) ? null : reader.GetString(7)));
            }

            if (reverse)
                rows.Reverse();

            return (IReadOnlyList<ChatTranscriptMessage>)rows;
        }, cancellationToken).ConfigureAwait(false);
    }

    private async Task<long> FindIdByFrameAsync(string frameId, CancellationToken cancellationToken)
    {
        await using var cmd = _session.Connection.CreateCommand();
        cmd.CommandText = "SELECT id FROM chat_messages WHERE frame_id = $frame_id LIMIT 1;";
        cmd.Parameters.AddWithValue("$frame_id", frameId);
        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result is null or DBNull ? 0 : Convert.ToInt64(result, CultureInfo.InvariantCulture);
    }

    private static string NormalizeRole(string role)
    {
        var candidate = (role ?? "").Trim().ToLowerInvariant();
        if (Array.IndexOf(AllowedRoles, candidate) >= 0)
            return candidate;

        throw new ArgumentException(
            $"Transcript role must be one of: {string.Join(", ", AllowedRoles)}.",
            nameof(role));
    }
}

using SoulCore.Config;
using SoulCore.Memory;
using SoulCore.Memory.Repositories;

namespace SoulCore.Protocol.Tests;

/// <summary>
/// PROP-3 Wave 1: the durable transcript clients hydrate from.
/// </summary>
public class ChatTranscriptRepositoryTests
{
    [Fact]
    public async Task Migration007_CreatesChatMessagesTable()
    {
        await WithRepoAsync(async repo =>
        {
            // No throw and an empty conversation means migration 007 applied.
            var cursor = await repo.GetLatestCursorAsync(PresenceConversation.Id);
            Assert.Equal(0, cursor);
            Assert.Empty(await repo.GetSinceAsync(PresenceConversation.Id, 0, 50));
        });
    }

    [Fact]
    public async Task AppendAsync_ReturnsMonotonicCursor()
    {
        await WithRepoAsync(async repo =>
        {
            var first = await repo.AppendAsync(PresenceConversation.Id, "user", "hello");
            var second = await repo.AppendAsync(PresenceConversation.Id, "assistant", "hi back");

            Assert.True(first > 0);
            Assert.True(second > first);
            Assert.Equal(second, await repo.GetLatestCursorAsync(PresenceConversation.Id));
        });
    }

    [Fact]
    public async Task GetSinceAsync_ReturnsOnlyRowsAfterCursor_OldestFirst()
    {
        await WithRepoAsync(async repo =>
        {
            await repo.AppendAsync(PresenceConversation.Id, "user", "one");
            var cursor = await repo.AppendAsync(PresenceConversation.Id, "assistant", "two");
            await repo.AppendAsync(PresenceConversation.Id, "user", "three");
            await repo.AppendAsync(PresenceConversation.Id, "assistant", "four");

            var page = await repo.GetSinceAsync(PresenceConversation.Id, cursor, 50);

            Assert.Equal(new[] { "three", "four" }, page.Select(m => m.Content).ToArray());
            Assert.True(page[0].Id < page[1].Id);
        });
    }

    [Fact]
    public async Task GetSinceAsync_FromZero_ReturnsWholeConversation()
    {
        await WithRepoAsync(async repo =>
        {
            await repo.AppendAsync(PresenceConversation.Id, "user", "a");
            await repo.AppendAsync(PresenceConversation.Id, "assistant", "b");

            var page = await repo.GetSinceAsync(PresenceConversation.Id, 0, 50);

            Assert.Equal(new[] { "a", "b" }, page.Select(m => m.Content).ToArray());
        });
    }

    [Fact]
    public async Task GetSinceAsync_IsScopedToConversation()
    {
        await WithRepoAsync(async repo =>
        {
            await repo.AppendAsync(PresenceConversation.Id, "user", "mine");
            await repo.AppendAsync("some-other-thread", "user", "not mine");

            var page = await repo.GetSinceAsync(PresenceConversation.Id, 0, 50);

            Assert.Single(page);
            Assert.Equal("mine", page[0].Content);
        });
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsTail_OldestFirst()
    {
        await WithRepoAsync(async repo =>
        {
            for (var i = 1; i <= 6; i++)
                await repo.AppendAsync(PresenceConversation.Id, i % 2 == 1 ? "user" : "assistant", $"m{i}");

            var tail = await repo.GetRecentAsync(PresenceConversation.Id, 3);

            Assert.Equal(new[] { "m4", "m5", "m6" }, tail.Select(m => m.Content).ToArray());
        });
    }

    [Fact]
    public async Task AppendAsync_SameFrameIdTwice_DoesNotDuplicate()
    {
        await WithRepoAsync(async repo =>
        {
            const string frameId = "frame-abc";
            var first = await repo.AppendAsync(
                PresenceConversation.Id, "assistant", "only once", channel: "desk", frameId: frameId);
            var second = await repo.AppendAsync(
                PresenceConversation.Id, "assistant", "only once", channel: "desk", frameId: frameId);

            Assert.Equal(first, second);
            Assert.Single(await repo.GetSinceAsync(PresenceConversation.Id, 0, 50));
        });
    }

    [Fact]
    public async Task AppendAsync_NullFrameIds_DoNotCollide()
    {
        await WithRepoAsync(async repo =>
        {
            await repo.AppendAsync(PresenceConversation.Id, "user", "first");
            await repo.AppendAsync(PresenceConversation.Id, "user", "second");

            // The unique index on frame_id is partial, so many NULLs must coexist.
            Assert.Equal(2, (await repo.GetSinceAsync(PresenceConversation.Id, 0, 50)).Count);
        });
    }

    [Fact]
    public async Task AppendAsync_RoundTripsChannelAndMedia()
    {
        await WithRepoAsync(async repo =>
        {
            await repo.AppendAsync(
                PresenceConversation.Id, "user", "look at this",
                channel: "sms", frameId: "f1", mediaId: "media-9");

            var row = (await repo.GetSinceAsync(PresenceConversation.Id, 0, 50)).Single();

            Assert.Equal("sms", row.Channel);
            Assert.Equal("f1", row.FrameId);
            Assert.Equal("media-9", row.MediaId);
            Assert.Equal("user", row.Role);
            Assert.False(string.IsNullOrWhiteSpace(row.OccurredAt));
        });
    }

    [Fact]
    public async Task GetSinceAsync_ClampsLimitToMaxPageSize()
    {
        await WithRepoAsync(async repo =>
        {
            await repo.AppendAsync(PresenceConversation.Id, "user", "x");

            // Absurd limits must not throw or become an unbounded scan.
            Assert.Single(await repo.GetSinceAsync(PresenceConversation.Id, 0, int.MaxValue));
            Assert.Single(await repo.GetSinceAsync(PresenceConversation.Id, 0, 0));
        });
    }

    [Theory]
    [InlineData("system")]
    [InlineData("tool")]
    [InlineData("")]
    public async Task AppendAsync_RejectsNonConversationRoles(string role)
    {
        await WithRepoAsync(async repo =>
        {
            // The transcript is the conversation, not connection chatter or tool traces.
            await Assert.ThrowsAsync<ArgumentException>(
                () => repo.AppendAsync(PresenceConversation.Id, role, "nope"));
        });
    }

    [Fact]
    public async Task AppendAsync_RejectsEmptyContentAndConversation()
    {
        await WithRepoAsync(async repo =>
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => repo.AppendAsync(PresenceConversation.Id, "user", "   "));
            await Assert.ThrowsAsync<ArgumentException>(
                () => repo.AppendAsync("", "user", "orphan"));
        });
    }

    [Fact]
    public async Task Transcript_SurvivesReopen()
    {
        var path = Path.Combine(Path.GetTempPath(), $"soulcore-transcript-{Guid.NewGuid():N}.db");
        try
        {
            long cursor;
            await using (var session = new SqliteMemorySession(path))
            {
                var repo = new SqliteChatTranscriptRepository(session);
                await repo.AppendAsync(PresenceConversation.Id, "user", "before restart");
                cursor = await repo.AppendAsync(PresenceConversation.Id, "assistant", "still here");
            }

            // This is the whole point: a client reconnecting to a restarted Host can backfill.
            await using (var session = new SqliteMemorySession(path))
            {
                var repo = new SqliteChatTranscriptRepository(session);
                var all = await repo.GetSinceAsync(PresenceConversation.Id, 0, 50);

                Assert.Equal(new[] { "before restart", "still here" }, all.Select(m => m.Content).ToArray());
                Assert.Equal(cursor, await repo.GetLatestCursorAsync(PresenceConversation.Id));
            }
        }
        finally
        {
            try { File.Delete(path); } catch { /* best-effort */ }
        }
    }

    private static async Task WithRepoAsync(Func<SqliteChatTranscriptRepository, Task> body)
    {
        var path = Path.Combine(Path.GetTempPath(), $"soulcore-transcript-{Guid.NewGuid():N}.db");
        try
        {
            await using var session = new SqliteMemorySession(path);
            await body(new SqliteChatTranscriptRepository(session)).ConfigureAwait(false);
        }
        finally
        {
            try { File.Delete(path); } catch { /* best-effort */ }
        }
    }
}

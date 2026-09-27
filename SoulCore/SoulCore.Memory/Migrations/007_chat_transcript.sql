-- Migration 007: durable operator <-> Victoria chat transcript (PROP-3 Wave 1, Avenue A).
-- Distinct from episodic_memories (first-person *summaries* of what happened) and from
-- IChatSessionHistoryStore (bounded in-RAM LLM context). This is the replayable product
-- transcript clients hydrate from, so a fresh phone install can backfill desk history.
--
-- The AUTOINCREMENT id doubles as the hydrate cursor: clients page with `after=<id>`.
-- Monotonic and gap-tolerant, so deletes cannot make a client re-read old rows.

CREATE TABLE IF NOT EXISTS chat_messages (
    id               INTEGER     PRIMARY KEY AUTOINCREMENT,
    conversation_id  TEXT        NOT NULL,
    role             TEXT        NOT NULL
                     CHECK (role IN ('user', 'assistant')),
    content          TEXT        NOT NULL,
    occurred_at      TEXT        NOT NULL,
    created_at       TEXT        NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now')),
    channel          TEXT        NULL,   -- 'desk' | 'sms' | 'companion' | 'proactive'
    frame_id         TEXT        NULL,   -- WS frame id, so clients can dedupe live vs hydrated
    media_id         TEXT        NULL,   -- companion media ref for MMS / generated images
    CONSTRAINT chat_content_nonempty CHECK (length(trim(content)) > 0)
);

-- Primary hydrate access path: one conversation, ascending cursor.
CREATE INDEX IF NOT EXISTS idx_chat_messages_conv_id
    ON chat_messages (conversation_id, id);

-- A frame is one message. Guards against a retried write duplicating a turn;
-- partial so the many rows with no frame id do not collide with each other.
CREATE UNIQUE INDEX IF NOT EXISTS ux_chat_messages_frame
    ON chat_messages (frame_id)
    WHERE frame_id IS NOT NULL;

INSERT OR IGNORE INTO schema_migrations (version, name) VALUES ('007', 'chat_transcript');

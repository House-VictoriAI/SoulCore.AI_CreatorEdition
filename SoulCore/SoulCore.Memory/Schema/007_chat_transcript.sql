-- Schema fragment 007: durable operator <-> Victoria chat transcript.
-- Applied via Migrations/007_chat_transcript.sql on upgrade / first open after 006.

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

CREATE INDEX IF NOT EXISTS idx_chat_messages_conv_id
    ON chat_messages (conversation_id, id);

CREATE UNIQUE INDEX IF NOT EXISTS ux_chat_messages_frame
    ON chat_messages (frame_id)
    WHERE frame_id IS NOT NULL;

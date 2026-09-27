package com.housevictoria.companion.data

import android.app.Application
import android.util.Log
import com.housevictoria.companion.net.CompanionConnection
import com.housevictoria.companion.net.CompanionMediaClient
import com.housevictoria.companion.net.SoulCoreFrame
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import org.json.JSONArray
import org.json.JSONObject
import java.io.File
import java.util.concurrent.atomic.AtomicReference

/**
 * Process-scoped transcript for the operator <-> Victoria thread.
 *
 * PROP-3 Wave 1, "stop Compose-list-as-truth". [com.housevictoria.companion.ui.ChatScreen]
 * used to hold the thread in `remember { mutableStateListOf() }`, which has two
 * consequences: navigating to Settings / Call / Gallery destroys the list, and
 * any reply arriving while the screen is gone is dropped even though the
 * foreground service kept the socket open. Frames are collected here for the
 * process lifetime instead, and USER/ASSISTANT turns are mirrored to disk, so
 * neither leaving the screen nor process death wipes the thread.
 *
 * Disk copy is a *cache* of the tail, not an archive — the Host owns history.
 */
object ChatStore {
    private const val TAG = "ChatStore"
    private const val FILE_NAME = "chat-thread.json"

    /** Tail kept on device. The phone is a cache; the Host is the archive. */
    const val MAX_MESSAGES = 500

    private val scope = CoroutineScope(SupervisorJob() + Dispatchers.IO)
    private val lock = Any()

    private val _messages = MutableStateFlow<List<ChatMessage>>(emptyList())
    val messages: StateFlow<List<ChatMessage>> = _messages.asStateFlow()

    /** Id of the assistant bubble currently being streamed into, if any. */
    private val streamingAssistantId = AtomicReference<String?>(null)

    private var app: Application? = null
    private var installed = false

    /** Load the cached tail and start collecting hub frames for the process lifetime. */
    fun install(application: Application) {
        if (installed) return
        installed = true
        app = application

        scope.launch {
            restore(application)
            CompanionConnection.frames.collect { frame -> applyFrame(frame) }
        }
        Log.i(TAG, "Transcript store installed on CompanionConnection.frames")
    }

    // ---------- local appends ----------

    /** Append the operator's own turn (already formatted for display). */
    fun addUser(content: String) {
        streamingAssistantId.set(null)
        append(ChatMessage(role = MessageRole.USER, content = content))
    }

    /**
     * Append a diagnostic line. System/error rows are session-only: connection
     * chatter does not belong in the durable thread.
     *
     * Repeats are dropped. Every return to Chat re-emits the current connection
     * detail, which used to be harmless only because leaving the screen threw
     * the list away; now that the thread survives, it would stack up identical
     * "WS connected" bubbles.
     */
    fun addSystem(content: String) {
        synchronized(lock) {
            val last = _messages.value.lastOrNull { it.role == MessageRole.SYSTEM }
            if (last?.content == content) return
        }
        append(ChatMessage(role = MessageRole.SYSTEM, content = content))
    }

    fun addError(content: String) {
        append(ChatMessage(role = MessageRole.ERROR, content = content))
    }

    /** Drop the on-device cache and the in-memory thread. */
    fun clear() {
        synchronized(lock) {
            _messages.value = emptyList()
            streamingAssistantId.set(null)
        }
        scope.launch {
            runCatching { fileFor(app)?.delete() }
                .onFailure { Log.w(TAG, "Could not delete cached thread", it) }
        }
    }

    // ---------- frame handling ----------

    /**
     * Fold a hub frame into the thread. Ported from ChatScreen so it keeps
     * running while no Compose screen is alive.
     */
    fun applyFrame(frame: SoulCoreFrame) {
        when (frame.type) {
            SoulCoreFrame.CHAT_DELTA -> appendOrUpdateAssistant(frame, finalize = false)
            SoulCoreFrame.CHAT_DONE -> appendOrUpdateAssistant(frame, finalize = true)
            SoulCoreFrame.ERROR -> {
                streamingAssistantId.set(null)
                val code = frame.payloadString("code")
                val msg = frame.payloadString("message") ?: frame.payload?.toString().orEmpty()
                addError("error${code?.let { " [$it]" } ?: ""}: $msg")
            }
            SoulCoreFrame.PRESENCE_STATUS,
            SoulCoreFrame.EMOTION_SNAPSHOT,
            SoulCoreFrame.PONG,
            "loop.want",
            "loop.tick.ok" -> Unit
            else -> addSystem("frame ${frame.type} id=${frame.id}")
        }
    }

    private fun appendOrUpdateAssistant(frame: SoulCoreFrame, finalize: Boolean) {
        val text = frame.payloadText()
        val mediaId = frame.payloadString("mediaId")
        val hasMedia = frame.payload?.optBoolean("hasMedia", false) == true || !mediaId.isNullOrBlank()
        val proactive = frame.payload?.optBoolean("proactive", false) == true

        if (text.isNullOrEmpty() && finalize && !hasMedia) {
            streamingAssistantId.set(null)
            return
        }

        val content = text.orEmpty()
        var mediaTarget: String? = null

        synchronized(lock) {
            val current = _messages.value
            val streamId = streamingAssistantId.get()

            if (streamId != null) {
                val idx = current.indexOfFirst { it.id == streamId }
                if (idx >= 0) {
                    val existing = current[idx]
                    if (frame.id.isBlank() ||
                        existing.frameId == frame.id ||
                        existing.frameId.isNullOrBlank()
                    ) {
                        val updated = current.toMutableList()
                        updated[idx] = existing.copy(
                            content = content.ifEmpty { existing.content },
                            frameId = frame.id.ifBlank { existing.frameId },
                            mediaId = mediaId ?: existing.mediaId,
                            proactive = proactive || existing.proactive
                        )
                        _messages.value = updated
                        if (finalize) {
                            streamingAssistantId.set(null)
                            if (hasMedia && !mediaId.isNullOrBlank()) {
                                mediaTarget = updated[idx].id
                            }
                        }
                        return@synchronized
                    }
                }
            }

            val bubble = ChatMessage(
                role = MessageRole.ASSISTANT,
                content = content.ifEmpty { if (hasMedia) "(image)" else "" },
                frameId = frame.id.ifBlank { null },
                mediaId = mediaId,
                proactive = proactive
            )
            _messages.value = (current + bubble).takeLast(MAX_MESSAGES)
            streamingAssistantId.set(if (finalize) null else bubble.id)
            if (finalize && hasMedia && !mediaId.isNullOrBlank()) {
                mediaTarget = bubble.id
            }
        }

        if (finalize) schedulePersist()

        val target = mediaTarget
        val media = mediaId
        if (target != null && media != null) {
            fetchMedia(target, media)
        }
    }

    /** Download an assistant image into the gallery and attach it to its bubble. */
    private fun fetchMedia(messageId: String, mediaId: String) {
        val application = app ?: return
        scope.launch {
            val cfg = CompanionPrefs.load(application)
            CompanionMediaClient
                .downloadMedia(cfg.resolvedHttpBase(), cfg.token, mediaId)
                .onSuccess { bytes ->
                    val item = GalleryStore.saveBytes(application, bytes, mediaId = mediaId)
                    attachLocalImage(messageId, item.localPath)
                }
                .onFailure { Log.w(TAG, "Media $mediaId download failed", it) }
        }
    }

    private fun attachLocalImage(messageId: String, path: String) {
        synchronized(lock) {
            val current = _messages.value
            val idx = current.indexOfFirst { it.id == messageId }
            if (idx < 0) return
            val updated = current.toMutableList()
            updated[idx] = updated[idx].copy(localImagePath = path)
            _messages.value = updated
        }
        schedulePersist()
    }

    private fun append(message: ChatMessage) {
        synchronized(lock) {
            _messages.value = (_messages.value + message).takeLast(MAX_MESSAGES)
        }
        if (message.role == MessageRole.USER) schedulePersist()
    }

    // ---------- disk cache ----------

    private fun schedulePersist() {
        scope.launch { persist() }
    }

    /**
     * Only USER/ASSISTANT turns are written. Connection chatter and error rows
     * are diagnostics for the current session, not part of the conversation.
     */
    private fun persist() {
        val target = fileFor(app) ?: return
        val snapshot = _messages.value.filter {
            it.role == MessageRole.USER || it.role == MessageRole.ASSISTANT
        }

        try {
            val array = JSONArray()
            for (m in snapshot) {
                array.put(
                    JSONObject().apply {
                        put("id", m.id)
                        put("role", m.role.name)
                        put("content", m.content)
                        put("timestampMs", m.timestampMs)
                        m.frameId?.let { put("frameId", it) }
                        m.mediaId?.let { put("mediaId", it) }
                        m.localImagePath?.let { put("localImagePath", it) }
                        if (m.proactive) put("proactive", true)
                    }
                )
            }
            // Write-then-rename so a kill mid-write cannot truncate the thread.
            val tmp = File(target.parentFile, "$FILE_NAME.tmp")
            tmp.writeText(array.toString())
            if (!tmp.renameTo(target)) {
                target.writeText(array.toString())
                tmp.delete()
            }
        } catch (e: Exception) {
            Log.w(TAG, "Could not persist thread", e)
        }
    }

    private fun restore(application: Application) {
        val source = fileFor(application) ?: return
        if (!source.isFile) return

        try {
            val array = JSONArray(source.readText())
            val loaded = ArrayList<ChatMessage>(array.length())
            for (i in 0 until array.length()) {
                val o = array.optJSONObject(i) ?: continue
                val role = runCatching { MessageRole.valueOf(o.optString("role")) }.getOrNull()
                    ?: continue
                loaded.add(
                    ChatMessage(
                        id = o.optString("id").ifBlank { java.util.UUID.randomUUID().toString() },
                        role = role,
                        content = o.optString("content"),
                        frameId = o.optString("frameId").ifBlank { null },
                        timestampMs = o.optLong("timestampMs", System.currentTimeMillis()),
                        mediaId = o.optString("mediaId").ifBlank { null },
                        localImagePath = o.optString("localImagePath").ifBlank { null },
                        proactive = o.optBoolean("proactive", false)
                    )
                )
            }
            synchronized(lock) {
                // Anything already streamed in during startup stays at the tail.
                _messages.value = (loaded.takeLast(MAX_MESSAGES) + _messages.value)
                    .takeLast(MAX_MESSAGES)
            }
            Log.i(TAG, "Restored ${loaded.size} cached messages")
        } catch (e: Exception) {
            Log.w(TAG, "Could not restore cached thread", e)
        }
    }

    private fun fileFor(application: Application?): File? =
        application?.let { File(it.filesDir, FILE_NAME) }
}

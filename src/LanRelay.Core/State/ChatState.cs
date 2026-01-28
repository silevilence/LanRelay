using System.Collections.Concurrent;

namespace LanRelay.Core.State;

/// <summary>
/// State container for managing chat sessions and message history.
/// Implements the State Container Pattern with C# events for UI notification.
/// </summary>
public class ChatState
{
    private readonly ConcurrentDictionary<Guid, List<ChatMessage>> _chatHistories = new();
    private readonly object _lock = new();

    /// <summary>
    /// Event raised when a message is received or sent.
    /// </summary>
    public event Action<Guid, ChatMessage>? OnMessageReceived;

    /// <summary>
    /// Event raised when the selected device changes.
    /// </summary>
    public event Action<Guid?>? OnSelectedDeviceChanged;

    /// <summary>
    /// Event raised when the chat history changes.
    /// </summary>
    public event Action? OnChatHistoryChanged;

    /// <summary>
    /// Gets or sets the currently selected device ID for chatting.
    /// </summary>
    public Guid? SelectedDeviceId { get; private set; }

    /// <summary>
    /// Gets the chat history for the currently selected device.
    /// </summary>
    public IReadOnlyList<ChatMessage> SelectedChatHistory =>
        SelectedDeviceId.HasValue ? GetChatHistory(SelectedDeviceId.Value) : [];

    /// <summary>
    /// Selects a device for chatting.
    /// </summary>
    /// <param name="deviceId">The device ID to select.</param>
    public void SelectDevice(Guid? deviceId)
    {
        if (SelectedDeviceId == deviceId) return;

        SelectedDeviceId = deviceId;
        OnSelectedDeviceChanged?.Invoke(deviceId);
        OnChatHistoryChanged?.Invoke();
    }

    /// <summary>
    /// Adds a message to the chat history for a device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="message">The message to add.</param>
    public void AddMessage(Guid deviceId, ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        lock (_lock)
        {
            var history = _chatHistories.GetOrAdd(deviceId, _ => []);
            history.Add(message);
        }

        OnMessageReceived?.Invoke(deviceId, message);

        if (deviceId == SelectedDeviceId)
        {
            OnChatHistoryChanged?.Invoke();
        }
    }

    /// <summary>
    /// Gets the chat history for a specific device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>The chat history as a read-only list.</returns>
    public IReadOnlyList<ChatMessage> GetChatHistory(Guid deviceId)
    {
        lock (_lock)
        {
            if (_chatHistories.TryGetValue(deviceId, out var history))
            {
                return history.ToList().AsReadOnly();
            }
            return [];
        }
    }

    /// <summary>
    /// Clears the chat history for a specific device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    public void ClearChat(Guid deviceId)
    {
        lock (_lock)
        {
            if (_chatHistories.TryGetValue(deviceId, out var history))
            {
                history.Clear();
            }
        }

        if (deviceId == SelectedDeviceId)
        {
            OnChatHistoryChanged?.Invoke();
        }
    }

    /// <summary>
    /// Clears all chat histories.
    /// </summary>
    public void ClearAll()
    {
        lock (_lock)
        {
            _chatHistories.Clear();
        }

        OnChatHistoryChanged?.Invoke();
    }
}

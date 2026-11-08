namespace AnnuityChatPOC.Domain.Conversations;

public sealed class ConversationSession
{
    public ConversationSession(string sessionId)
    {
        SessionId = sessionId;
    }

    public string SessionId { get; }

    public DateTime CreatedAtUtc { get; } = DateTime.UtcNow;

    public DateTime LastUpdatedAtUtc { get; private set; } = DateTime.UtcNow;

    public List<ConversationMessage> Messages { get; } = [];

    public SuitabilityProfileSnapshot? CurrentSuitability { get; set; }

    public ConversationSessionStatus Status { get; private set; } = ConversationSessionStatus.Active;

    public void AddMessage(ConversationMessage message)
    {
        if (!string.Equals(message.SessionId, SessionId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Session must never contain messages from another user/session.");
        }

        if (Messages.Count > 0 && message.CreatedAtUtc < Messages[^1].CreatedAtUtc)
        {
            throw new InvalidOperationException("Messages ordering must be strictly chronological.");
        }

        Messages.Add(message);
        LastUpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetStatus(ConversationSessionStatus status)
    {
        Status = status;
        LastUpdatedAtUtc = DateTime.UtcNow;
    }

    public void Reset()
    {
        SetStatus(ConversationSessionStatus.Resetting);
        Messages.Clear();
        CurrentSuitability = null;
        SetStatus(ConversationSessionStatus.Active);
    }
}


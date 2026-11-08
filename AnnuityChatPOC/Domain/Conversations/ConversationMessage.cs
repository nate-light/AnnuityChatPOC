namespace AnnuityChatPOC.Domain.Conversations;

public sealed class ConversationMessage
{
    public required string MessageId { get; init; }

    public required string SessionId { get; init; }

    public required ConversationMessageRole Role { get; init; }

    public required string Content { get; set; }

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public bool IsPartial { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }
}


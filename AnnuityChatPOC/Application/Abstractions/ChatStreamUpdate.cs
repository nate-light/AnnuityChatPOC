namespace AnnuityChatPOC.Application.Abstractions;

public sealed class ChatStreamUpdate
{
    public required string AssistantMessageId { get; init; }

    public required string Content { get; init; }

    public bool IsFinal { get; init; }

    public bool IsError { get; init; }
}


using AnnuityChatPOC.Domain.Conversations;

namespace AnnuityChatPOC.Application.Abstractions;

public interface IConversationService
{
    ConversationSession Session { get; }

    IReadOnlyList<ConversationMessage> GetMessages();

    Task NewChatAsync(CancellationToken cancellationToken = default);

    IAsyncEnumerable<ChatStreamUpdate> SendMessageAsync(string userMessage, CancellationToken cancellationToken = default);
}


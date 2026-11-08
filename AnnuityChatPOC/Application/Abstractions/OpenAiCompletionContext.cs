using AnnuityChatPOC.Domain.Conversations;
using AnnuityChatPOC.Domain.Products;
using AnnuityChatPOC.Domain.Recommendations;

namespace AnnuityChatPOC.Application.Abstractions;

public sealed class OpenAiCompletionContext
{
    public required ConversationSession Session { get; init; }

    public required string UserMessage { get; init; }

    public required AssistantPolicyProfile PolicyProfile { get; init; }

    public required IReadOnlyList<AnnuityProduct> ApprovedProducts { get; init; }
}


namespace AnnuityChatPOC.Application.Abstractions;

public interface IOpenAiResponsesService
{
    IAsyncEnumerable<string> StreamAssistantResponseAsync(OpenAiCompletionContext context, CancellationToken cancellationToken);
}


using AnnuityChatPOC.Application.Abstractions;

namespace AnnuityChatPOC.UnitTests.TestDoubles;

internal sealed class FakeOpenAiResponsesService(string responseText) : IOpenAiResponsesService
{
    public async IAsyncEnumerable<string> StreamAssistantResponseAsync(
        OpenAiCompletionContext context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var chunks = responseText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return $"{chunk} ";
            await Task.Yield();
        }
    }
}


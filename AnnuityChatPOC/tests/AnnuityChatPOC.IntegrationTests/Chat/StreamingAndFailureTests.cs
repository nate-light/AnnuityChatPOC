using AnnuityChatPOC.IntegrationTests.TestSupport;

namespace AnnuityChatPOC.IntegrationTests.Chat;

public sealed class StreamingAndFailureTests
{
    [Fact]
    public async Task StreamingResponse_EmitsPartialThenFinalUpdates()
    {
        var service = IntegrationConversationFactory.Build("Streaming response with multiple words.");

        var updates = await service.SendMessageAsync("Explain annuities.").ToListAsync();

        Assert.True(updates.Count >= 2);
        Assert.Contains(updates, x => !x.IsFinal);
        Assert.True(updates[^1].IsFinal);
    }

    [Fact]
    public async Task ServiceFailure_ReturnsErrorUpdateAndPreservesHistory()
    {
        var service = IntegrationConversationFactory.Build("good response");
        await service.SendMessageAsync("What is an annuity?").ToListAsync();
        var countBeforeFailure = service.GetMessages().Count;

        var failingService = IntegrationConversationFactory.Build(string.Empty, fail: true);
        await failingService.SendMessageAsync("Tell me something").ToListAsync();
        var updates = await failingService.SendMessageAsync("Tell me more").ToListAsync();

        Assert.Contains(updates, x => x.IsError);
        Assert.True(failingService.GetMessages().Count >= countBeforeFailure);
    }
}


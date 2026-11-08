using AnnuityChatPOC.IntegrationTests.TestSupport;

namespace AnnuityChatPOC.IntegrationTests.Chat;

public sealed class ConceptConversationTests
{
    [Fact]
    public async Task FirstTurnConceptConversation_ReturnsEducationalResponse()
    {
        var service = IntegrationConversationFactory.Build("Annuities can help retirement income planning.");

        var updates = await service.SendMessageAsync("What is an annuity?").ToListAsync();

        Assert.NotEmpty(updates);
        var final = updates[^1].Content;
        Assert.Contains("annuit", final, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AI assistant", final, StringComparison.OrdinalIgnoreCase);
    }
}


using AnnuityChatPOC.IntegrationTests.TestSupport;

namespace AnnuityChatPOC.IntegrationTests.Chat;

public sealed class SuitabilityCollectionTests
{
    [Fact]
    public async Task MultiTurnConversation_AsksForMissingSuitabilityDetails()
    {
        var service = IntegrationConversationFactory.Build("Use PRODUCT_IDS: MMA-SAFE-001.");

        var first = await service.SendMessageAsync("What's the best annuity for me?").ToListAsync();
        Assert.Contains("planning to retire", first[^1].Content, StringComparison.OrdinalIgnoreCase);

        var second = await service.SendMessageAsync("I am moderate risk and I retire in 7 years.").ToListAsync();
        Assert.NotEmpty(second);
    }
}

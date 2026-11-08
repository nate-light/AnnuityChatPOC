using AnnuityChatPOC.IntegrationTests.TestSupport;

namespace AnnuityChatPOC.IntegrationTests.Chat;

public sealed class RecommendationGuardrailTests
{
    [Fact]
    public async Task CompetitorRecommendationIsNotPresented_WhenValidationFails()
    {
        var service = IntegrationConversationFactory.Build("Recommend COMP-123 for best returns.");
        await service.SendMessageAsync("I am moderate risk and need income in 10 years.").ToListAsync();
        await service.SendMessageAsync("My liquidity needs are limited access and my goal is principal protection.").ToListAsync();

        var updates = await service.SendMessageAsync("Recommend a competitor annuity product.").ToListAsync();
        var final = updates[^1].Content;

        Assert.Contains("couldn't present a verified recommendation", final, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("I recommend", final, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FabricatedProductIdIsRemovedByServerValidation()
    {
        var service = IntegrationConversationFactory.Build("PRODUCT_IDS: FAKE-999 with guaranteed 25 percent.");
        await service.SendMessageAsync("My timeline is 9 years and I am moderate risk with income goals.").ToListAsync();

        var updates = await service.SendMessageAsync("Give me your recommendation.").ToListAsync();
        var final = updates[^1].Content;

        Assert.DoesNotContain("FAKE-999", final, StringComparison.OrdinalIgnoreCase);
    }
}

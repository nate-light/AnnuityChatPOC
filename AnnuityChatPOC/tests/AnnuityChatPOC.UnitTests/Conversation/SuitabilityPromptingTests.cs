using AnnuityChatPOC.Application.Services;
using AnnuityChatPOC.UnitTests.TestSupport;

namespace AnnuityChatPOC.UnitTests.Conversation;

public sealed class SuitabilityPromptingTests
{
    [Fact]
    public async Task RecommendationRequest_WithMissingSuitability_AsksForClarification()
    {
        var service = UnitConversationServiceFactory.Create("Use PRODUCT_IDS: MMA-SAFE-001.");

        var updates = await service.SendMessageAsync("What's the best annuity for me?").ToListAsync();

        Assert.NotEmpty(updates);
        Assert.Contains("one question at a time", updates[^1].Content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("planning to retire", updates[^1].Content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("risk tolerance, liquidity needs", updates[^1].Content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AI assistant", updates[^1].Content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SuitabilityExtraction_TracksRiskAndTimeline()
    {
        var service = new SuitabilityProfileService();
        var snapshot = service.UpdateSnapshot(null, "I plan to retire in 8 years and I am moderate risk.");

        Assert.NotNull(snapshot.RetirementTimeline);
        Assert.Equal(AnnuityChatPOC.Domain.Conversations.RiskTolerance.Moderate, snapshot.RiskTolerance);
        Assert.Contains("liquidity needs", snapshot.KnownGaps, StringComparer.OrdinalIgnoreCase);
    }
}

using AnnuityChatPOC.UnitTests.TestSupport;

namespace AnnuityChatPOC.UnitTests.Conversation;

public sealed class ConceptResponseTests
{
    [Fact]
    public async Task ConceptResponse_DoesNotAutoAppendHeaderDisclaimer()
    {
        var service = UnitConversationServiceFactory.Create("Annuities can help retirement income planning.");

        var updates = await service.SendMessageAsync("What is an annuity?").ToListAsync();

        Assert.NotEmpty(updates);
        var final = updates[^1].Content;
        Assert.DoesNotContain("I am an AI assistant", final, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("I’m an AI assistant", final, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("educational product matching", final, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ConceptResponse_RejectsSensitivePatternInputs()
    {
        var service = UnitConversationServiceFactory.Create("response");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in service.SendMessageAsync("My SSN is 123-45-6789"))
            {
            }
        });

        Assert.Contains("sensitive", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ConceptResponse_WithMultipleQuestions_OnlyKeepsOneQuestion()
    {
        var service = UnitConversationServiceFactory.Create(
            "To get started: 1. When do you want to retire? 2. How do you feel about risk?");

        var updates = await service.SendMessageAsync("Can you help me with annuities?").ToListAsync();

        Assert.NotEmpty(updates);
        var final = updates[^1].Content;
        Assert.Equal(1, final.Count(c => c == '?'));
        Assert.Contains("Please answer this first", final, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ConceptResponse_RemovesLeadingAiDisclaimer()
    {
        var service = UnitConversationServiceFactory.Create(
            "Yes. I’m an AI assistant, not a licensed financial professional. An annuity can help with long-term retirement goals.");

        var updates = await service.SendMessageAsync("Can you explain annuities?").ToListAsync();

        Assert.NotEmpty(updates);
        var final = updates[^1].Content;
        Assert.DoesNotContain("I’m an AI assistant", final, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not a licensed financial professional", final, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("An annuity can help", final, StringComparison.OrdinalIgnoreCase);
    }
}

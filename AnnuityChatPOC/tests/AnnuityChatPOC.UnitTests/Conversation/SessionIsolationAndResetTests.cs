using AnnuityChatPOC.UnitTests.TestSupport;

namespace AnnuityChatPOC.UnitTests.Conversation;

public sealed class SessionIsolationAndResetTests
{
    [Fact]
    public async Task NewChat_ClearsHistoryAndReturnsToActiveStatus()
    {
        var service = UnitConversationServiceFactory.Create("Educational response.");
        await service.SendMessageAsync("Explain annuities.").ToListAsync();

        Assert.NotEmpty(service.GetMessages());

        await service.NewChatAsync();

        Assert.Empty(service.GetMessages());
        Assert.Equal("Active", service.Session.Status.ToString());
    }

    [Fact]
    public async Task DifferentServiceInstances_HaveDifferentSessionIds()
    {
        var serviceA = UnitConversationServiceFactory.Create("A");
        var serviceB = UnitConversationServiceFactory.Create("B");

        await serviceA.SendMessageAsync("Question A").ToListAsync();
        await serviceB.SendMessageAsync("Question B").ToListAsync();

        Assert.NotEqual(serviceA.Session.SessionId, serviceB.Session.SessionId);
    }
}


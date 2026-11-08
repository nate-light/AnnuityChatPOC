using AnnuityChatPOC.Domain.Conversations;

namespace AnnuityChatPOC.Application.Abstractions;

public interface ISuitabilityProfileService
{
    SuitabilityProfileSnapshot UpdateSnapshot(SuitabilityProfileSnapshot? current, string userMessage);

    IReadOnlyList<string> GetMissingCriticalFields(SuitabilityProfileSnapshot? snapshot);
}


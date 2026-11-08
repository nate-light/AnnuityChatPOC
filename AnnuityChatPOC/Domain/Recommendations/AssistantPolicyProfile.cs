namespace AnnuityChatPOC.Domain.Recommendations;

public sealed class AssistantPolicyProfile
{
    public required string SystemInstruction { get; init; }

    public required string DisclosureText { get; init; }

    public required string CatalogRestrictionText { get; init; }

    public required string AdviceBoundaryText { get; init; }

    public required string PromptInjectionGuardText { get; init; }

    public required string ModelName { get; init; }
}


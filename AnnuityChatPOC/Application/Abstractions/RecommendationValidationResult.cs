namespace AnnuityChatPOC.Application.Abstractions;

public sealed class RecommendationValidationResult
{
    public required IReadOnlyList<string> ApprovedIds { get; init; }

    public required IReadOnlyDictionary<string, string> RejectedIds { get; init; }
}


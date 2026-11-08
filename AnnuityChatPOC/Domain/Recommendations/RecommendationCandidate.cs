namespace AnnuityChatPOC.Domain.Recommendations;

public sealed class RecommendationCandidate
{
    public required string ProductId { get; init; }

    public required string ReasoningSummary { get; init; }

    public List<string> CitedUserObjectives { get; init; } = [];

    public List<string> DisclosedRisks { get; init; } = [];

    public RecommendationValidationStatus ValidationStatus { get; set; } = RecommendationValidationStatus.Pending;

    public string? RejectionReason { get; set; }
}


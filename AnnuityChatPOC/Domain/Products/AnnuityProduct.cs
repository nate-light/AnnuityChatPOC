namespace AnnuityChatPOC.Domain.Products;

public sealed class AnnuityProduct
{
    public required string ProductId { get; init; }

    public required string ProductName { get; init; }

    public required string ProviderName { get; init; }

    public required string Category { get; init; }

    public required string Description { get; init; }

    public required ProductEligibility Eligibility { get; init; }

    public List<string> Risks { get; init; } = [];

    public List<string> Restrictions { get; init; } = [];

    public List<string> ApprovedSources { get; init; } = [];

    public bool IsRecommendationEligible { get; init; }
}


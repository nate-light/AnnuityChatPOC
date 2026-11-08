namespace AnnuityChatPOC.Domain.Products;

public sealed class ProductEligibility
{
    public List<string> Jurisdictions { get; init; } = [];

    public List<string> DistributionChannels { get; init; } = [];

    public int? MinAge { get; init; }

    public int? MaxAge { get; init; }

    public List<string> SuitabilityNotes { get; init; } = [];
}


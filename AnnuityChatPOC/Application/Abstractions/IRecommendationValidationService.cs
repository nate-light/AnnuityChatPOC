namespace AnnuityChatPOC.Application.Abstractions;

public interface IRecommendationValidationService
{
    Task<RecommendationValidationResult> ValidateRecommendedProductIdsAsync(
        IEnumerable<string> productIds,
        string jurisdiction,
        string distributionChannel,
        CancellationToken cancellationToken = default);
}


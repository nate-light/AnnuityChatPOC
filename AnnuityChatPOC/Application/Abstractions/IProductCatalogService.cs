using AnnuityChatPOC.Domain.Products;

namespace AnnuityChatPOC.Application.Abstractions;

public interface IProductCatalogService
{
    Task<IReadOnlyList<AnnuityProduct>> GetApprovedProductsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnnuityProduct>> GetRecommendationEligibleProductsAsync(
        string jurisdiction,
        string distributionChannel,
        int maxResults,
        CancellationToken cancellationToken = default);
}


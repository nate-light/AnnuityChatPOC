using AnnuityChatPOC.Application.Abstractions;
using AnnuityChatPOC.Domain.Products;

namespace AnnuityChatPOC.UnitTests.TestDoubles;

internal sealed class StubProductCatalogService(IReadOnlyList<AnnuityProduct> products) : IProductCatalogService
{
    public Task<IReadOnlyList<AnnuityProduct>> GetApprovedProductsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(products);
    }

    public Task<IReadOnlyList<AnnuityProduct>> GetRecommendationEligibleProductsAsync(string jurisdiction, string distributionChannel, int maxResults, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<AnnuityProduct>>(products);
    }
}


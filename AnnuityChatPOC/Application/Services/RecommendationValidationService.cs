using AnnuityChatPOC.Application.Abstractions;
using AnnuityChatPOC.Domain.Products;

namespace AnnuityChatPOC.Application.Services;

public sealed class RecommendationValidationService(IProductCatalogService productCatalogService)
    : IRecommendationValidationService
{
    public async Task<RecommendationValidationResult> ValidateRecommendedProductIdsAsync(
        IEnumerable<string> productIds,
        string jurisdiction,
        string distributionChannel,
        CancellationToken cancellationToken = default)
    {
        var cleaned = productIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var approved = new List<string>();
        var rejected = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (cleaned.Count == 0)
        {
            return new RecommendationValidationResult
            {
                ApprovedIds = approved,
                RejectedIds = rejected
            };
        }

        var products = await productCatalogService.GetApprovedProductsAsync(cancellationToken);
        var productMap = products.ToDictionary(x => x.ProductId, StringComparer.OrdinalIgnoreCase);

        foreach (var productId in cleaned)
        {
            if (!productMap.TryGetValue(productId, out var product))
            {
                rejected[productId] = "Product ID is not in the approved catalog.";
                continue;
            }

            var validationFailure = ValidateEligibility(product, jurisdiction, distributionChannel);
            if (validationFailure is not null)
            {
                rejected[productId] = validationFailure;
                continue;
            }

            approved.Add(productId);
        }

        return new RecommendationValidationResult
        {
            ApprovedIds = approved,
            RejectedIds = rejected
        };
    }

    private static string? ValidateEligibility(AnnuityProduct product, string jurisdiction, string distributionChannel)
    {
        if (!product.IsRecommendationEligible)
        {
            return "Product is not recommendation eligible.";
        }

        if (!string.Equals(product.ProviderName, "MassMutual Ascend", StringComparison.Ordinal))
        {
            return "Provider is not authorized.";
        }

        if (!product.Eligibility.Jurisdictions.Contains(jurisdiction, StringComparer.OrdinalIgnoreCase))
        {
            return "Product is not eligible for the requested jurisdiction.";
        }

        if (!product.Eligibility.DistributionChannels.Contains(distributionChannel, StringComparer.OrdinalIgnoreCase))
        {
            return "Product is not eligible for the requested distribution channel.";
        }

        return null;
    }
}


using AnnuityChatPOC.Application.Services;
using AnnuityChatPOC.Domain.Products;
using AnnuityChatPOC.UnitTests.TestSupport;

namespace AnnuityChatPOC.UnitTests.Recommendations;

public sealed class RecommendationValidationTests
{
    private static readonly AnnuityProduct EligibleProduct = new()
    {
        ProductId = "MMA-SAFE-001",
        ProductName = "Safe Choice",
        ProviderName = "MassMutual Ascend",
        Category = "Fixed",
        Description = "Income-focused product.",
        Eligibility = new ProductEligibility
        {
            Jurisdictions = ["US"],
            DistributionChannels = ["Retail"]
        },
        IsRecommendationEligible = true
    };

    [Fact]
    public async Task RejectsUnknownProductIds()
    {
        var service = new RecommendationValidationService(UnitConversationServiceFactory.BuildCatalogWith(EligibleProduct));
        var result = await service.ValidateRecommendedProductIdsAsync(["INVALID-999"], "US", "Retail");

        Assert.Empty(result.ApprovedIds);
        Assert.Contains("INVALID-999", result.RejectedIds.Keys);
    }

    [Fact]
    public async Task ApprovesMatchingCatalogProducts()
    {
        var service = new RecommendationValidationService(UnitConversationServiceFactory.BuildCatalogWith(EligibleProduct));
        var result = await service.ValidateRecommendedProductIdsAsync(["MMA-SAFE-001"], "US", "Retail");

        Assert.Single(result.ApprovedIds);
        Assert.Equal("MMA-SAFE-001", result.ApprovedIds[0]);
        Assert.Empty(result.RejectedIds);
    }
}


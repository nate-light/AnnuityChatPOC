using AnnuityChatPOC.Application.Services;
using AnnuityChatPOC.Domain.Products;
using AnnuityChatPOC.UnitTests.TestSupport;

namespace AnnuityChatPOC.UnitTests.Products;

public sealed class ProductLookupToolContractTests
{
    [Fact]
    public async Task RecommendationEligibleLookup_FiltersByJurisdictionAndChannel()
    {
        var products = new[]
        {
            new AnnuityProduct
            {
                ProductId = "MMA-ELIGIBLE-001",
                ProductName = "Eligible Product",
                ProviderName = "MassMutual Ascend",
                Category = "Fixed",
                Description = "Eligible",
                Eligibility = new ProductEligibility
                {
                    Jurisdictions = ["US"],
                    DistributionChannels = ["Retail"]
                },
                IsRecommendationEligible = true
            },
            new AnnuityProduct
            {
                ProductId = "MMA-INELIGIBLE-001",
                ProductName = "Ineligible Product",
                ProviderName = "MassMutual Ascend",
                Category = "Fixed",
                Description = "Ineligible",
                Eligibility = new ProductEligibility
                {
                    Jurisdictions = ["CA"],
                    DistributionChannels = ["Institutional"]
                },
                IsRecommendationEligible = true
            }
        };

        var catalog = UnitConversationServiceFactory.BuildCatalogWith(products);
        var service = new RecommendationValidationService(catalog);

        var result = await service.ValidateRecommendedProductIdsAsync(
            ["MMA-ELIGIBLE-001", "MMA-INELIGIBLE-001"],
            "US",
            "Retail");

        Assert.Contains("MMA-ELIGIBLE-001", result.ApprovedIds);
        Assert.Contains("MMA-INELIGIBLE-001", result.RejectedIds.Keys);
    }
}


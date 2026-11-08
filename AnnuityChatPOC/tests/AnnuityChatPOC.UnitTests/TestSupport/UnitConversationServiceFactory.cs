using AnnuityChatPOC.Application.Abstractions;
using AnnuityChatPOC.Application.Prompts;
using AnnuityChatPOC.Application.Services;
using AnnuityChatPOC.Domain.Products;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AnnuityChatPOC.UnitTests.TestSupport;

internal static class UnitConversationServiceFactory
{
    public static ConversationService Create(string responseText)
    {
        var catalogProduct = new AnnuityProduct
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

        var catalog = new TestDoubles.StubProductCatalogService([catalogProduct]);
        var validator = new RecommendationValidationService(catalog);
        return new ConversationService(
            new TestDoubles.FakeOpenAiResponsesService(responseText),
            catalog,
            validator,
            new SuitabilityProfileService(),
            new AssistantInstructionsProvider(
                Options.Create(new OpenAiOptions
                {
                    Model = "gpt-4.1-mini"
                })),
            Options.Create(new ChatOptions()),
            NullLogger<ConversationService>.Instance);
    }

    public static IProductCatalogService BuildCatalogWith(params AnnuityProduct[] products)
        => new TestDoubles.StubProductCatalogService(products);
}


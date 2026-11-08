using AnnuityChatPOC.Application.Abstractions;
using AnnuityChatPOC.Application.Prompts;
using AnnuityChatPOC.Application.Services;
using AnnuityChatPOC.Domain.Products;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AnnuityChatPOC.IntegrationTests.TestSupport;

internal static class IntegrationConversationFactory
{
    public static ConversationService Build(string response, bool fail = false)
    {
        var product = new AnnuityProduct
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
        var catalog = new StubCatalog([product]);
        return new ConversationService(
            new StubOpenAi(response, fail),
            catalog,
            new RecommendationValidationService(catalog),
            new SuitabilityProfileService(),
            new AssistantInstructionsProvider(Options.Create(new OpenAiOptions { Model = "gpt-4.1-mini" })),
            Options.Create(new ChatOptions()),
            NullLogger<ConversationService>.Instance);
    }

    private sealed class StubCatalog(IReadOnlyList<AnnuityProduct> products) : IProductCatalogService
    {
        public Task<IReadOnlyList<AnnuityProduct>> GetApprovedProductsAsync(CancellationToken cancellationToken = default) => Task.FromResult(products);

        public Task<IReadOnlyList<AnnuityProduct>> GetRecommendationEligibleProductsAsync(string jurisdiction, string distributionChannel, int maxResults, CancellationToken cancellationToken = default) => Task.FromResult(products);
    }

    private sealed class StubOpenAi(string response, bool fail) : IOpenAiResponsesService
    {
        public async IAsyncEnumerable<string> StreamAssistantResponseAsync(OpenAiCompletionContext context, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (fail)
            {
                throw new InvalidOperationException("Failure");
            }

            foreach (var token in response.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return $"{token} ";
                await Task.Yield();
            }
        }
    }
}


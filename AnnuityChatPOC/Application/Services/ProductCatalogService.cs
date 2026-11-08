using System.Text.Json;
using AnnuityChatPOC.Application.Abstractions;
using AnnuityChatPOC.Domain.Products;
using Microsoft.Extensions.Options;

namespace AnnuityChatPOC.Application.Services;

public sealed class ProductCatalogService(
    IWebHostEnvironment environment,
    IOptions<ChatOptions> chatOptions,
    ILogger<ProductCatalogService> logger) : IProductCatalogService
{
    private readonly SemaphoreSlim _sync = new(1, 1);
    private IReadOnlyList<AnnuityProduct>? _cached;

    public async Task<IReadOnlyList<AnnuityProduct>> GetApprovedProductsAsync(CancellationToken cancellationToken = default)
    {
        if (_cached is not null)
        {
            return _cached;
        }

        await _sync.WaitAsync(cancellationToken);
        try
        {
            if (_cached is not null)
            {
                return _cached;
            }

            var relativePath = chatOptions.Value.ProductCatalogPath;
            var fullPath = Path.Combine(environment.ContentRootPath, relativePath);
            if (!File.Exists(fullPath))
            {
                logger.LogWarning("Approved product catalog file not found at {CatalogPath}", fullPath);
                _cached = [];
                return _cached;
            }

            await using var stream = File.OpenRead(fullPath);
            var products = await JsonSerializer.DeserializeAsync<List<AnnuityProduct>>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);

            _cached = (products ?? [])
                .Where(IsValidProduct)
                .ToList();
            return _cached;
        }
        finally
        {
            _sync.Release();
        }
    }

    public async Task<IReadOnlyList<AnnuityProduct>> GetRecommendationEligibleProductsAsync(
        string jurisdiction,
        string distributionChannel,
        int maxResults,
        CancellationToken cancellationToken = default)
    {
        var products = await GetApprovedProductsAsync(cancellationToken);
        return products
            .Where(x => x.IsRecommendationEligible)
            .Where(x => x.Eligibility.Jurisdictions.Contains(jurisdiction, StringComparer.OrdinalIgnoreCase))
            .Where(x => x.Eligibility.DistributionChannels.Contains(distributionChannel, StringComparer.OrdinalIgnoreCase))
            .Take(Math.Clamp(maxResults, 1, 10))
            .ToList();
    }

    private static bool IsValidProduct(AnnuityProduct product)
    {
        if (string.IsNullOrWhiteSpace(product.ProductId) ||
            string.IsNullOrWhiteSpace(product.ProductName) ||
            string.IsNullOrWhiteSpace(product.Description) ||
            string.IsNullOrWhiteSpace(product.ProviderName))
        {
            return false;
        }

        if (!string.Equals(product.ProviderName, "MassMutual Ascend", StringComparison.Ordinal))
        {
            return false;
        }

        var invalidPlaceholder = string.Concat(
            product.ProductName, " ",
            product.Description, " ",
            string.Join(' ', product.Restrictions),
            " ",
            string.Join(' ', product.Risks));
        if (invalidPlaceholder.Contains("placeholder", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
}


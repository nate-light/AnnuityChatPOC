namespace AnnuityChatPOC.Application.Abstractions;

public sealed class ChatOptions
{
    public const string SectionName = "Chat";

    public int MaxUserMessageLength { get; set; } = 4000;

    public int MaxConversationMessages { get; set; } = 60;

    public string ProductCatalogPath { get; set; } = "Infrastructure/ProductCatalog/approved-products.json";

    public string DefaultJurisdiction { get; set; } = "US";

    public string DefaultDistributionChannel { get; set; } = "Retail";
}


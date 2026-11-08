using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AnnuityChatPOC.Application.Abstractions;
using AnnuityChatPOC.Domain.Products;
using Microsoft.Extensions.Options;

namespace AnnuityChatPOC.Application.Services;

public sealed class OpenAiResponsesService(
    IHttpClientFactory httpClientFactory,
    IProductCatalogService productCatalogService,
    IRecommendationValidationService recommendationValidationService,
    IOptions<OpenAiOptions> options,
    IOptions<ChatOptions> chatOptions,
    ILogger<OpenAiResponsesService> logger) : IOpenAiResponsesService
{
    public async IAsyncEnumerable<string> StreamAssistantResponseAsync(
        OpenAiCompletionContext context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured. Set OpenAI:ApiKey in user secrets or environment variables.");
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(options.Value.TimeoutSeconds, 1)));
        var effectiveToken = timeoutCts.Token;

        var requestPayload = BuildInitialRequest(context);
        var root = await SendRequestAsync(requestPayload, effectiveToken);

        var responseId = root.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
        var toolCalls = ExtractFunctionCalls(root);

        if (toolCalls.Count > 0 && !string.IsNullOrWhiteSpace(responseId))
        {
            var toolOutputs = new List<object>();
            foreach (var call in toolCalls)
            {
                var toolOutput = await ExecuteToolCallAsync(call, effectiveToken);
                toolOutputs.Add(new
                {
                    type = "function_call_output",
                    call_id = call.CallId,
                    output = toolOutput
                });
            }

            var followupPayload = new
            {
                model = options.Value.Model,
                previous_response_id = responseId,
                input = toolOutputs
            };
            root = await SendRequestAsync(followupPayload, effectiveToken);
        }

        var output = ExtractAssistantText(root);
        if (string.IsNullOrWhiteSpace(output))
        {
            throw new InvalidOperationException("The OpenAI response did not include assistant content.");
        }

        foreach (var chunk in ChunkBySentence(output))
        {
            yield return chunk;
        }
    }

    private object BuildInitialRequest(OpenAiCompletionContext context)
    {
        var input = new List<object>
        {
            new { role = "system", content = context.PolicyProfile.SystemInstruction },
            new
            {
                role = "system",
                content = $"{context.PolicyProfile.CatalogRestrictionText} {context.PolicyProfile.PromptInjectionGuardText}"
            }
        };

        foreach (var message in context.Session.Messages.TakeLast(20))
        {
            var role = message.Role == Domain.Conversations.ConversationMessageRole.User ? "user" : "assistant";
            input.Add(new { role, content = message.Content });
        }

        input.Add(new { role = "user", content = context.UserMessage });

        return new
        {
            model = options.Value.Model,
            input,
            tools = new object[]
            {
                new
                {
                    type = "function",
                    name = "get_approved_annuity_products",
                    description = "Retrieve approved MassMutual Ascend products for a given jurisdiction and channel.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            retirementTimeline = new { type = "string" },
                            riskTolerance = new { type = "string", @enum = new[] { "conservative", "moderate", "growth", "unknown" } },
                            liquidityNeeds = new { type = "string" },
                            incomeObjectives = new { type = "string" },
                            jurisdiction = new { type = "string" },
                            distributionChannel = new { type = "string" },
                            maxResults = new { type = "integer", minimum = 1, maximum = 10 }
                        },
                        required = new[] { "jurisdiction", "distributionChannel" }
                    }
                },
                new
                {
                    type = "function",
                    name = "validate_recommendation_candidates",
                    description = "Validate recommendation candidate product IDs against approved catalog rules.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            candidates = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        productId = new { type = "string" },
                                        reasoningSummary = new { type = "string" }
                                    },
                                    required = new[] { "productId" }
                                }
                            },
                            jurisdiction = new { type = "string" },
                            distributionChannel = new { type = "string" }
                        },
                        required = new[] { "candidates", "jurisdiction", "distributionChannel" }
                    }
                }
            }
        };
    }

    private async Task<JsonElement> SendRequestAsync(object payload, CancellationToken cancellationToken)
    {
        var payloadJson = JsonSerializer.Serialize(payload);
        var payloadBytes = Encoding.UTF8.GetByteCount(payloadJson);
        using var requestScope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["OpenAiModel"] = options.Value.Model,
            ["OpenAiEndpoint"] = options.Value.Endpoint,
            ["PayloadBytes"] = payloadBytes
        });
        logger.LogInformation("Sending OpenAI response request. PayloadBytes={PayloadBytes}", payloadBytes);

        var client = httpClientFactory.CreateClient(nameof(OpenAiResponsesService));
        client.Timeout = Timeout.InfiniteTimeSpan;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ApiKey);

        using var request = new HttpRequestMessage(HttpMethod.Post, options.Value.Endpoint);
        request.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "OpenAI API request failed with status {StatusCode}. ResponseSnippet={ResponseSnippet}",
                (int)response.StatusCode,
                RedactForLog(responseBody));
            throw new InvalidOperationException("The AI service is temporarily unavailable. Please retry.");
        }

        logger.LogInformation("OpenAI response request succeeded with status {StatusCode}", (int)response.StatusCode);
        using var doc = JsonDocument.Parse(responseBody);
        return doc.RootElement.Clone();
    }

    private static string RedactForLog(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var compact = value.Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);

        var maskedDigits = System.Text.RegularExpressions.Regex.Replace(compact, @"\d", "x");
        var trimmed = maskedDigits.Length > 220 ? maskedDigits[..220] : maskedDigits;
        return trimmed;
    }

    private static string ExtractAssistantText(JsonElement root)
    {
        if (root.TryGetProperty("output_text", out var textElement) && textElement.ValueKind == JsonValueKind.String)
        {
            return textElement.GetString() ?? string.Empty;
        }

        if (!root.TryGetProperty("output", out var outputElement) || outputElement.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        var chunks = new List<string>();
        foreach (var item in outputElement.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var contentElement) || contentElement.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var content in contentElement.EnumerateArray())
            {
                if (content.TryGetProperty("text", out var cText) && cText.ValueKind == JsonValueKind.String)
                {
                    chunks.Add(cText.GetString() ?? string.Empty);
                }
            }
        }

        return string.Join(" ", chunks.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private sealed record ToolCall(string CallId, string Name, string ArgumentsJson);

    private static List<ToolCall> ExtractFunctionCalls(JsonElement root)
    {
        var result = new List<ToolCall>();
        if (!root.TryGetProperty("output", out var outputElement) || outputElement.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        foreach (var item in outputElement.EnumerateArray())
        {
            if (!item.TryGetProperty("type", out var typeElement) ||
                !string.Equals(typeElement.GetString(), "function_call", StringComparison.Ordinal))
            {
                continue;
            }

            var callId = item.TryGetProperty("call_id", out var callIdElement)
                ? callIdElement.GetString() ?? Guid.NewGuid().ToString("N")
                : Guid.NewGuid().ToString("N");
            var name = item.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? string.Empty : string.Empty;
            var arguments = item.TryGetProperty("arguments", out var argumentsElement)
                ? argumentsElement.GetString() ?? "{}"
                : "{}";
            if (!string.IsNullOrWhiteSpace(name))
            {
                result.Add(new ToolCall(callId, name, arguments));
            }
        }

        return result;
    }

    private async Task<string> ExecuteToolCallAsync(ToolCall call, CancellationToken cancellationToken)
    {
        return call.Name switch
        {
            "get_approved_annuity_products" => await ExecuteGetApprovedProductsToolAsync(call.ArgumentsJson, cancellationToken),
            "validate_recommendation_candidates" => await ExecuteValidateRecommendationsToolAsync(call.ArgumentsJson, cancellationToken),
            _ => JsonSerializer.Serialize(new { error = $"Unsupported tool {call.Name}" })
        };
    }

    private async Task<string> ExecuteGetApprovedProductsToolAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        var root = doc.RootElement;
        var jurisdiction = root.TryGetProperty("jurisdiction", out var j) ? j.GetString() ?? chatOptions.Value.DefaultJurisdiction : chatOptions.Value.DefaultJurisdiction;
        var channel = root.TryGetProperty("distributionChannel", out var c) ? c.GetString() ?? chatOptions.Value.DefaultDistributionChannel : chatOptions.Value.DefaultDistributionChannel;
        var maxResults = root.TryGetProperty("maxResults", out var m) && m.TryGetInt32(out var max) ? max : 5;

        var products = await productCatalogService.GetRecommendationEligibleProductsAsync(
            jurisdiction,
            channel,
            maxResults,
            cancellationToken);

        var dto = products.Select(p => new
        {
            productId = p.ProductId,
            productName = p.ProductName,
            category = p.Category,
            description = p.Description,
            risks = p.Risks,
            restrictions = p.Restrictions
        });

        return JsonSerializer.Serialize(new { products = dto });
    }

    private async Task<string> ExecuteValidateRecommendationsToolAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        var root = doc.RootElement;
        var jurisdiction = root.TryGetProperty("jurisdiction", out var j) ? j.GetString() ?? chatOptions.Value.DefaultJurisdiction : chatOptions.Value.DefaultJurisdiction;
        var channel = root.TryGetProperty("distributionChannel", out var c) ? c.GetString() ?? chatOptions.Value.DefaultDistributionChannel : chatOptions.Value.DefaultDistributionChannel;
        var candidates = new List<string>();
        if (root.TryGetProperty("candidates", out var candidateElement) && candidateElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in candidateElement.EnumerateArray())
            {
                if (item.TryGetProperty("productId", out var idElement))
                {
                    var id = idElement.GetString();
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        candidates.Add(id);
                    }
                }
            }
        }

        var result = await recommendationValidationService.ValidateRecommendedProductIdsAsync(
            candidates,
            jurisdiction,
            channel,
            cancellationToken);
        return JsonSerializer.Serialize(new
        {
            approved = result.ApprovedIds.Select(id => new { productId = id, validationStatus = "approved" }),
            rejected = result.RejectedIds.Select(pair => new
            {
                productId = pair.Key,
                validationStatus = "rejected",
                rejectionReason = pair.Value
            })
        });
    }

    private static IEnumerable<string> ChunkBySentence(string text)
    {
        var parts = text
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Split(['\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .SelectMany(line => line.Split(". ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        foreach (var part in parts)
        {
            var normalized = part.EndsWith('.') ? part : $"{part}.";
            yield return $"{normalized} ";
        }
    }
}

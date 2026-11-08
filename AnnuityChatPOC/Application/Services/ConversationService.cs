using System.Text.RegularExpressions;
using AnnuityChatPOC.Application.Abstractions;
using AnnuityChatPOC.Application.Prompts;
using AnnuityChatPOC.Domain.Conversations;
using Microsoft.Extensions.Options;

namespace AnnuityChatPOC.Application.Services;

public sealed class ConversationService(
    IOpenAiResponsesService openAiResponsesService,
    IProductCatalogService productCatalogService,
    IRecommendationValidationService recommendationValidationService,
    ISuitabilityProfileService suitabilityProfileService,
    AssistantInstructionsProvider instructionsProvider,
    IOptions<ChatOptions> chatOptions,
    ILogger<ConversationService> logger) : IConversationService
{
    private static readonly Regex ProductIdRegex = new(@"\b[A-Z]{2,}-[A-Z0-9-]+\b", RegexOptions.Compiled);
    private static readonly Regex SensitivePatternRegex = new(@"(\b\d{3}-\d{2}-\d{4}\b)|(\b\d{9}\b)|(\b\d{10,16}\b)", RegexOptions.Compiled);
    private static readonly Regex LeadingDisclaimerRegex = new(
        @"^\s*(yes[\.,]?\s*)?(i('|’)m|i am)\s+an?\s+ai assistant[^\.!?]*?(not\s+a\s+licensed[^\.!?]*)?[\.!?]\s*",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ConversationSession Session { get; } = new(Guid.NewGuid().ToString("N"));

    public IReadOnlyList<ConversationMessage> GetMessages() => Session.Messages.ToList();

    public Task NewChatAsync(CancellationToken cancellationToken = default)
    {
        Session.Reset();
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<ChatStreamUpdate> SendMessageAsync(
        string userMessage,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ValidateUserMessage(userMessage);
        userMessage = userMessage.Trim();

        var userTurn = new ConversationMessage
        {
            MessageId = Guid.NewGuid().ToString("N"),
            SessionId = Session.SessionId,
            Role = ConversationMessageRole.User,
            Content = userMessage,
            IsPartial = false
        };
        Session.AddMessage(userTurn);

        Session.CurrentSuitability = suitabilityProfileService.UpdateSnapshot(Session.CurrentSuitability, userMessage);
        var policy = instructionsProvider.BuildPolicyProfile();

        var assistantTurn = new ConversationMessage
        {
            MessageId = Guid.NewGuid().ToString("N"),
            SessionId = Session.SessionId,
            Role = ConversationMessageRole.Assistant,
            Content = string.Empty,
            IsPartial = true
        };
        Session.AddMessage(assistantTurn);

        if (ShouldAskClarifyingQuestion(userMessage, Session.CurrentSuitability))
        {
            var missingFields = suitabilityProfileService.GetMissingCriticalFields(Session.CurrentSuitability);
            var response = BuildSingleSuitabilityQuestion(missingFields);
            assistantTurn.Content = response;
            assistantTurn.IsPartial = false;
            yield return new ChatStreamUpdate
            {
                AssistantMessageId = assistantTurn.MessageId,
                Content = assistantTurn.Content,
                IsFinal = true,
                IsError = false
            };

            yield break;
        }

        Session.SetStatus(ConversationSessionStatus.Active);
        var errorUpdate = default(ChatStreamUpdate);
        await using var enumerator = StreamAssistantUpdatesAsync(
                assistantTurn,
                userMessage,
                policy,
                cancellationToken)
            .GetAsyncEnumerator(cancellationToken);

        while (true)
        {
            ChatStreamUpdate update;
            try
            {
                if (!await enumerator.MoveNextAsync())
                {
                    break;
                }

                update = enumerator.Current;
            }
            catch (OperationCanceledException)
            {
                errorUpdate = BuildErrorUpdate(assistantTurn, "cancelled", "The request was cancelled. Please try again.");
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "AI request failed.");
                errorUpdate = BuildErrorUpdate(assistantTurn, "upstream_failure", "The AI service is temporarily unavailable. Please retry in a moment.");
                break;
            }

            yield return update;
        }

        if (errorUpdate is not null)
        {
            Session.SetStatus(ConversationSessionStatus.Error);
            yield return errorUpdate;
        }
    }

    private async IAsyncEnumerable<ChatStreamUpdate> StreamAssistantUpdatesAsync(
        ConversationMessage assistantTurn,
        string userMessage,
        Domain.Recommendations.AssistantPolicyProfile policy,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var approvedProducts = await productCatalogService.GetApprovedProductsAsync(cancellationToken);
        var context = new OpenAiCompletionContext
        {
            Session = Session,
            UserMessage = userMessage,
            PolicyProfile = policy,
            ApprovedProducts = approvedProducts
        };

        await foreach (var chunk in openAiResponsesService.StreamAssistantResponseAsync(context, cancellationToken))
        {
            assistantTurn.Content += chunk;
            yield return new ChatStreamUpdate
            {
                AssistantMessageId = assistantTurn.MessageId,
                Content = assistantTurn.Content,
                IsFinal = false,
                IsError = false
            };
        }

        assistantTurn.IsPartial = false;
        assistantTurn.Content = await ApplyRecommendationValidationAsync(assistantTurn.Content, cancellationToken);
        assistantTurn.Content = RemoveLeadingDisclaimer(assistantTurn.Content);
        assistantTurn.Content = EnforceSingleQuestion(assistantTurn.Content);
        yield return new ChatStreamUpdate
        {
            AssistantMessageId = assistantTurn.MessageId,
            Content = assistantTurn.Content,
            IsFinal = true,
            IsError = false
        };
    }

    private void ValidateUserMessage(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            throw new InvalidOperationException("Please enter a message.");
        }

        if (userMessage.Length > chatOptions.Value.MaxUserMessageLength)
        {
            throw new InvalidOperationException($"Message is too long. Limit is {chatOptions.Value.MaxUserMessageLength} characters.");
        }

        if (SensitivePatternRegex.IsMatch(userMessage))
        {
            throw new InvalidOperationException("Please remove sensitive account or Social Security information before continuing.");
        }
    }

    private bool ShouldAskClarifyingQuestion(string userMessage, SuitabilityProfileSnapshot? snapshot)
    {
        var lower = userMessage.ToLowerInvariant();
        if (!lower.Contains("best annuity", StringComparison.Ordinal) &&
            !lower.Contains("recommend", StringComparison.Ordinal) &&
            !lower.Contains("which product", StringComparison.Ordinal))
        {
            return false;
        }

        return suitabilityProfileService.GetMissingCriticalFields(snapshot).Count > 0;
    }

    private static string BuildSingleSuitabilityQuestion(IReadOnlyList<string> missingFields)
    {
        var nextField = missingFields.FirstOrDefault();
        return nextField switch
        {
            "retirement timeline" =>
                "To help narrow this down, let's go one question at a time. When are you planning to retire?",
            "risk tolerance" =>
                "Thanks. Next question: how would you describe your risk tolerance — conservative, moderate, or growth-oriented?",
            "liquidity needs" =>
                "Thanks. Next question: how important is access to your money before retirement?",
            "income objectives" =>
                "Thanks. Next question: do you want this annuity to provide income, and if so, when should income begin?",
            _ =>
                "To help identify suitable options, let's go one question at a time. Could you share more about your retirement goals?"
        };
    }

    private async Task<string> ApplyRecommendationValidationAsync(string content, CancellationToken cancellationToken)
    {
        var ids = ProductIdRegex.Matches(content).Select(x => x.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (ids.Count == 0)
        {
            return content;
        }

        var validation = await recommendationValidationService.ValidateRecommendedProductIdsAsync(
            ids,
            chatOptions.Value.DefaultJurisdiction,
            chatOptions.Value.DefaultDistributionChannel,
            cancellationToken);

        if (validation.RejectedIds.Count == 0)
        {
            return content;
        }

        if (validation.ApprovedIds.Count == 0)
        {
            var reasons = string.Join(" ", validation.RejectedIds.Select(x => $"{x.Key}: {x.Value}"));
            return $"I couldn't present a verified recommendation because the suggested product IDs did not pass catalog validation. {reasons}";
        }

        foreach (var rejected in validation.RejectedIds)
        {
            content = content.Replace(rejected.Key, "[UNVERIFIED PRODUCT REMOVED]", StringComparison.OrdinalIgnoreCase);
        }

        return $"{content}\n\nNote: Some unverified product IDs were removed by server-side validation.";
    }

    private static string EnforceSingleQuestion(string content)
    {
        var firstQuestionMark = content.IndexOf('?');
        if (firstQuestionMark < 0)
        {
            return content;
        }

        var hasAdditionalQuestion = content.IndexOf('?', firstQuestionMark + 1) >= 0;
        if (!hasAdditionalQuestion)
        {
            return content;
        }

        var singleQuestion = content[..(firstQuestionMark + 1)].TrimEnd();
        return $"{singleQuestion} Please answer this first, and I will ask the next question after your response.";
    }

    private static string RemoveLeadingDisclaimer(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return content;
        }

        var withoutDisclaimer = LeadingDisclaimerRegex.Replace(content, string.Empty);
        return string.IsNullOrWhiteSpace(withoutDisclaimer) ? content : withoutDisclaimer.TrimStart();
    }

    private ChatStreamUpdate BuildErrorUpdate(ConversationMessage assistantTurn, string errorCode, string message)
    {
        assistantTurn.IsPartial = false;
        assistantTurn.ErrorCode = errorCode;
        assistantTurn.ErrorMessage = message;
        assistantTurn.Content = message;
        return new ChatStreamUpdate
        {
            AssistantMessageId = assistantTurn.MessageId,
            Content = assistantTurn.Content,
            IsFinal = true,
            IsError = true
        };
    }
}

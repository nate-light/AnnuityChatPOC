using AnnuityChatPOC.Application.Abstractions;
using AnnuityChatPOC.Domain.Conversations;

namespace AnnuityChatPOC.Application.Services;

public sealed class SuitabilityProfileService : ISuitabilityProfileService
{
    public SuitabilityProfileSnapshot UpdateSnapshot(SuitabilityProfileSnapshot? current, string userMessage)
    {
        var snapshot = current ?? new SuitabilityProfileSnapshot();
        var normalized = userMessage.ToLowerInvariant();

        if (ContainsAny(normalized, "in ", "years", "retire", "retirement"))
        {
            snapshot.RetirementTimeline ??= userMessage;
        }

        if (ContainsAny(normalized, "conservative", "low risk"))
        {
            snapshot.RiskTolerance = RiskTolerance.Conservative;
        }
        else if (ContainsAny(normalized, "moderate", "balanced"))
        {
            snapshot.RiskTolerance = RiskTolerance.Moderate;
        }
        else if (ContainsAny(normalized, "aggressive", "growth", "high risk"))
        {
            snapshot.RiskTolerance = RiskTolerance.Growth;
        }

        if (ContainsAny(normalized, "liquid", "liquidity", "access to cash", "withdraw"))
        {
            snapshot.LiquidityNeeds ??= userMessage;
        }

        if (ContainsAny(normalized, "income", "payout", "cash flow", "monthly"))
        {
            snapshot.IncomeObjectives ??= userMessage;
        }

        if (ContainsAny(normalized, "grow", "accumulate", "principal"))
        {
            snapshot.AccumulationGoals ??= userMessage;
        }

        snapshot.LastUpdatedAtUtc = DateTime.UtcNow;
        snapshot.KnownGaps = GetMissingCriticalFields(snapshot).ToList();
        return snapshot;
    }

    public IReadOnlyList<string> GetMissingCriticalFields(SuitabilityProfileSnapshot? snapshot)
    {
        if (snapshot is null)
        {
            return ["retirement timeline", "risk tolerance", "liquidity needs", "income objectives"];
        }

        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(snapshot.RetirementTimeline))
        {
            missing.Add("retirement timeline");
        }

        if (snapshot.RiskTolerance == RiskTolerance.Unknown)
        {
            missing.Add("risk tolerance");
        }

        if (string.IsNullOrWhiteSpace(snapshot.LiquidityNeeds))
        {
            missing.Add("liquidity needs");
        }

        if (string.IsNullOrWhiteSpace(snapshot.IncomeObjectives))
        {
            missing.Add("income objectives");
        }

        return missing;
    }

    private static bool ContainsAny(string value, params string[] terms)
    {
        return terms.Any(value.Contains);
    }
}


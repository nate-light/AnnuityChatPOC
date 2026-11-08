namespace AnnuityChatPOC.Domain.Conversations;

public sealed class SuitabilityProfileSnapshot
{
    public string? RetirementTimeline { get; set; }

    public RiskTolerance RiskTolerance { get; set; } = RiskTolerance.Unknown;

    public string? LiquidityNeeds { get; set; }

    public string? IncomeObjectives { get; set; }

    public string? AccumulationGoals { get; set; }

    public List<string> KnownGaps { get; set; } = [];

    public DateTime LastUpdatedAtUtc { get; set; } = DateTime.UtcNow;
}


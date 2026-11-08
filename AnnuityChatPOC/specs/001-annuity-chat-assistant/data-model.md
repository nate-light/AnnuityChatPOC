# Phase 1 Data Model: AI Annuity Assistant POC

## Entity: ConversationSession

**Purpose**: Represents one active chat session and its policy/runtime state.

**Fields**:
- `SessionId` (string, required): Unique per user session.
- `CreatedAtUtc` (datetime, required)
- `LastUpdatedAtUtc` (datetime, required)
- `Messages` (list of `ConversationMessage`, required)
- `CurrentSuitability` (`SuitabilityProfileSnapshot`, nullable)
- `Status` (enum: `Active`, `Resetting`, `Error`)

**Validation Rules**:
- Session must never contain messages from another user/session.
- `Messages` ordering must be strictly chronological.
- Reset action clears all messages and suitability state.

## Entity: ConversationMessage

**Purpose**: Single user or assistant turn, including streaming/failure metadata.

**Fields**:
- `MessageId` (string, required)
- `SessionId` (string, required)
- `Role` (enum: `User`, `Assistant`, `SystemDisclosure`, `Tool`)
- `Content` (string, required, max length enforced)
- `CreatedAtUtc` (datetime, required)
- `IsPartial` (bool, required)
- `ErrorCode` (string, nullable)
- `ErrorMessage` (string, nullable)

**Validation Rules**:
- User content must pass max-length validation before processing.
- Assistant content must be HTML-encoded at render time.
- Partial assistant messages must end in either completed or failed state.

## Entity: SuitabilityProfileSnapshot

**Purpose**: Captures currently known recommendation inputs from conversation context.

**Fields**:
- `RetirementTimeline` (string, nullable)
- `RiskTolerance` (enum: `Conservative`, `Moderate`, `Growth`, `Unknown`)
- `LiquidityNeeds` (string, nullable)
- `IncomeObjectives` (string, nullable)
- `AccumulationGoals` (string, nullable)
- `KnownGaps` (list of string, required)
- `LastUpdatedAtUtc` (datetime, required)

**Validation Rules**:
- Recommendation eligibility check requires all critical fields populated or explicitly unknown.
- If critical fields missing, assistant response must ask clarifying questions.

## Entity: AnnuityProduct

**Purpose**: Approved MassMutual Ascend product catalog entry.

**Fields**:
- `ProductId` (string, required, unique)
- `ProductName` (string, required)
- `ProviderName` (string, required, must equal "MassMutual Ascend")
- `Category` (string, required)
- `Description` (string, required)
- `Eligibility` (`ProductEligibility`, required)
- `Risks` (list of string, required)
- `Restrictions` (list of string, required)
- `ApprovedSources` (list of string, required)
- `IsRecommendationEligible` (bool, required)

**Validation Rules**:
- Entries with missing required fields are excluded from recommendation results.
- Provider mismatch marks product ineligible.
- Placeholder rates/guarantees are prohibited in approved data.

## Entity: ProductEligibility

**Purpose**: Encodes channel and jurisdiction constraints for recommendation safety.

**Fields**:
- `Jurisdictions` (list of string, required)
- `DistributionChannels` (list of string, required)
- `MinAge` (int, nullable)
- `MaxAge` (int, nullable)
- `SuitabilityNotes` (list of string, optional)

**Validation Rules**:
- Candidate recommendation requires jurisdiction + channel compatibility.
- Missing eligibility attributes force "insufficient information" response.

## Entity: RecommendationCandidate

**Purpose**: Model-returned or tool-returned product proposal before final display.

**Fields**:
- `ProductId` (string, required)
- `ReasoningSummary` (string, required)
- `CitedUserObjectives` (list of string, required)
- `DisclosedRisks` (list of string, required)
- `ValidationStatus` (enum: `Pending`, `Approved`, `Rejected`)
- `RejectionReason` (string, nullable)

**Validation Rules**:
- `ProductId` must exist in approved catalog and be recommendation eligible.
- Rejected candidates are not rendered as recommendations.
- Approved candidate must include risk/tradeoff disclosure content.

## Entity: AssistantPolicyProfile

**Purpose**: Centralized instruction and disclosure configuration for AI behavior.

**Fields**:
- `SystemInstruction` (string, required)
- `DisclosureText` (string, required)
- `CatalogRestrictionText` (string, required)
- `AdviceBoundaryText` (string, required)
- `PromptInjectionGuardText` (string, required)
- `ModelName` (string, required, configurable)

**Validation Rules**:
- Policy text is server-controlled configuration only.
- User input must never overwrite or append to trusted policy instructions.

## Relationships

- `ConversationSession` 1..* `ConversationMessage`
- `ConversationSession` 0..1 `SuitabilityProfileSnapshot`
- `RecommendationCandidate` references exactly 1 `AnnuityProduct` by `ProductId`
- `AnnuityProduct` 1..1 `ProductEligibility`
- `AssistantPolicyProfile` applies globally to all sessions

## State Transitions

### Conversation session state
- `Active` -> `Error` on upstream API failure
- `Error` -> `Active` on successful retry
- `Active` -> `Resetting` when New Chat invoked
- `Resetting` -> `Active` with empty history

### Recommendation candidate state
- `Pending` -> `Approved` when allowlist + eligibility validation passes
- `Pending` -> `Rejected` when any validation check fails

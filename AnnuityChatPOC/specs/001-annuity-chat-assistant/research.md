# Phase 0 Research: AI Annuity Assistant POC

## Decision 1: Use a single Blazor Web App (Interactive Server) with server-side orchestration

**Rationale**:
- Meets constitution requirement for ASP.NET Core + Blazor with no unnecessary split.
- Keeps chat UI, OpenAI calls, and validation logic in one deployable unit for fastest demo path.
- Preserves server-only credential handling and recommendation controls.

**Alternatives considered**:
- Separate SPA + API: rejected as unnecessary complexity for POC.
- Multi-project clean architecture template: rejected to avoid over-abstraction at this stage.

## Decision 2: Use OpenAI Responses API via dedicated injectable service

**Rationale**:
- Aligns with requirement for supported OpenAI Responses API and configurable model selection.
- Central service simplifies streaming, cancellation, timeout, and error translation.
- Enables deterministic testing via interface-based mocking.

**Alternatives considered**:
- Calling OpenAI directly from components: rejected (mixes UI and infrastructure concerns).
- Chat Completions endpoint: rejected because requirement explicitly requests Responses API.

## Decision 3: Represent product knowledge as local JSON with typed models

**Rationale**:
- Satisfies POC requirement for local, approved product catalog.
- Typed models enforce compile-time structure for restrictions, risks, and eligibility.
- Allows later swap to database/internal API without chat UI changes.

**Alternatives considered**:
- Hardcoded in-memory product list: rejected due to maintainability risk.
- Immediate database integration: rejected as out of scope for initial POC.

## Decision 4: Enforce recommendations through dual controls (tooling + post-validation)

**Rationale**:
- Function calling limits retrieval to approved catalog data.
- Independent post-generation validation prevents prompt injection or fabricated identifiers.
- Supports educational competitor discussion while blocking competitor recommendations.

**Alternatives considered**:
- Prompt-only restriction: rejected as insufficient for regulated recommendation boundaries.
- Manual review-only control: rejected because runtime guardrails are required.

## Decision 5: Store conversation context in-memory per user session

**Rationale**:
- Meets POC scope (no accounts, no persistence) while enabling multi-turn continuity.
- Keeps sessions isolated and testable.
- Supports "New Chat" state reset with minimal operational overhead.

**Alternatives considered**:
- Database-backed conversation history: rejected as out of scope.
- Global singleton conversation state: rejected due to cross-session leakage risk.

## Decision 6: Security and privacy baseline for POC

**Rationale**:
- Enforce input-size limits and sensitive-data guard messaging before model submission.
- Avoid logging full prompts/responses and redact sensitive tokens in diagnostics.
- Keep default HTML encoding and avoid raw HTML rendering from model output.

**Alternatives considered**:
- Full DLP/classification pipeline: rejected for current POC timeline.
- Verbose conversation logging by default: rejected due to privacy risk.

## Decision 7: Testing approach with xUnit + mocks + adversarial scenarios

**Rationale**:
- Deterministic tests can validate policy controls independent of model variability.
- Unit tests cover catalog filtering, recommendation validation, and session isolation.
- Integration-style tests with mocked model output cover failure handling and disclosures.

**Alternatives considered**:
- Manual-only validation: rejected because constitution requires automated test coverage.
- Live OpenAI dependency in CI tests: rejected due to nondeterminism and reliability concerns.

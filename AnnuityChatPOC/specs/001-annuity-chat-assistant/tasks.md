# Tasks: AI Annuity Assistant POC

**Input**: Design documents from `/specs/001-annuity-chat-assistant/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are included because the specification explicitly requires xUnit coverage for conversation state, allowlist enforcement, invalid identifiers, mocked OpenAI behavior, adversarial prompts, and session isolation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- Each task includes exact file path(s)

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Initialize the single-project ASP.NET Core Blazor solution and baseline configuration.

- [X] T001 Create Blazor Web App (Interactive Server) project and solution files in AnnuityChatPOC/AnnuityChatPOC.csproj and AnnuityChatPOC/Program.cs
- [X] T002 Configure server-side DI registrations and request pipeline in AnnuityChatPOC/Program.cs
- [X] T003 [P] Add OpenAI and app behavior configuration sections in AnnuityChatPOC/appsettings.json and AnnuityChatPOC/appsettings.Development.json
- [X] T004 [P] Add strongly typed configuration options classes in AnnuityChatPOC/Application/Abstractions/OpenAiOptions.cs and AnnuityChatPOC/Application/Abstractions/ChatOptions.cs
- [X] T005 [P] Add root README with local setup, secrets, run steps, and POC boundaries in AnnuityChatPOC/README.md
- [X] T006 [P] Create test project scaffolding and references in AnnuityChatPOC/tests/AnnuityChatPOC.UnitTests/AnnuityChatPOC.UnitTests.csproj and AnnuityChatPOC/tests/AnnuityChatPOC.IntegrationTests/AnnuityChatPOC.IntegrationTests.csproj

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain, services, contracts, and guardrails required before user-story implementation.

**⚠️ CRITICAL**: No user story work begins until this phase is complete.

- [X] T007 Create conversation aggregate model enforcing `"Session must never contain messages from another user/session."`, ``"`Messages` ordering must be strictly chronological."``, and `"Reset action clears all messages and suitability state."` in AnnuityChatPOC/Domain/Conversations/ConversationSession.cs
- [X] T008 [P] Create message model enforcing `"User content must pass max-length validation before processing."`, `"Assistant content must be HTML-encoded at render time."`, and `"Partial assistant messages must end in either completed or failed state."` in AnnuityChatPOC/Domain/Conversations/ConversationMessage.cs
- [X] T009 [P] Create suitability profile model enforcing `"Recommendation eligibility check requires all critical fields populated or explicitly unknown."` and `"If critical fields missing, assistant response must ask clarifying questions."` in AnnuityChatPOC/Domain/Conversations/SuitabilityProfileSnapshot.cs
- [X] T010 [P] Create product models enforcing `"ProviderName (string, required, must equal "MassMutual Ascend")"`, `"Entries with missing required fields are excluded from recommendation results."`, `"Provider mismatch marks product ineligible."`, and `"Placeholder rates/guarantees are prohibited in approved data."` in AnnuityChatPOC/Domain/Products/AnnuityProduct.cs and AnnuityChatPOC/Domain/Products/ProductEligibility.cs
- [X] T011 [P] Create recommendation model enforcing ``"`ProductId` must exist in approved catalog and be recommendation eligible."``, `"Rejected candidates are not rendered as recommendations."`, and `"Approved candidate must include risk/tradeoff disclosure content."` in AnnuityChatPOC/Domain/Recommendations/RecommendationCandidate.cs
- [X] T012 [P] Create assistant policy model enforcing `"Policy text is server-controlled configuration only."` and `"User input must never overwrite or append to trusted policy instructions."` in AnnuityChatPOC/Domain/Recommendations/AssistantPolicyProfile.cs
- [X] T013 Define service interfaces for conversation, OpenAI, product catalog, and recommendation validation in AnnuityChatPOC/Application/Abstractions/IConversationService.cs, AnnuityChatPOC/Application/Abstractions/IOpenAiResponsesService.cs, AnnuityChatPOC/Application/Abstractions/IProductCatalogService.cs, and AnnuityChatPOC/Application/Abstractions/IRecommendationValidationService.cs
- [X] T014 Create approved catalog JSON source (no fabricated sample products) in AnnuityChatPOC/Infrastructure/ProductCatalog/approved-products.json
- [X] T015 Implement product catalog loader with required-field filtering and eligibility checks in AnnuityChatPOC/Application/Services/ProductCatalogService.cs
- [X] T016 [P] Implement recommendation validator for allowlist, provider, and channel/jurisdiction checks in AnnuityChatPOC/Application/Services/RecommendationValidationService.cs
- [X] T017 Implement per-session in-memory conversation state store and lifecycle transitions in AnnuityChatPOC/Application/Services/ConversationService.cs
- [X] T018 [P] Implement centralized assistant instruction/disclosure provider in AnnuityChatPOC/Application/Prompts/AssistantInstructionsProvider.cs
- [X] T019 Configure privacy-safe logging and global exception handling (no full conversation logging) in AnnuityChatPOC/Program.cs

**Checkpoint**: Foundation complete - user stories can begin.

---

## Phase 3: User Story 1 - Learn annuity concepts through conversation (Priority: P1) 🎯 MVP

**Goal**: Deliver a ChatGPT-style interface that answers annuity questions in clear language with required AI transparency.

**Independent Test**: Start a new chat, ask annuity concept questions, and verify clear educational responses plus AI/restricted-catalog disclosures.

### Tests for User Story 1

- [X] T020 [P] [US1] Add unit tests for concept-response disclosure behavior in AnnuityChatPOC/tests/AnnuityChatPOC.UnitTests/Conversation/ConceptResponseTests.cs
- [X] T021 [P] [US1] Add integration test for first-turn concept Q&A flow in AnnuityChatPOC/tests/AnnuityChatPOC.IntegrationTests/Chat/ConceptConversationTests.cs

### Implementation for User Story 1

- [X] T022 [P] [US1] Build responsive chat page shell with message area, composer host, and status region in AnnuityChatPOC/Components/Pages/Chat.razor
- [X] T023 [P] [US1] Implement user/assistant message rendering with attribution in AnnuityChatPOC/Components/Shared/ChatMessageList.razor
- [X] T024 [P] [US1] Implement composer with Enter-to-send and Shift+Enter multiline behavior in AnnuityChatPOC/Components/Shared/ChatComposer.razor
- [X] T025 [US1] Wire chat page to conversation service and progressive assistant updates in AnnuityChatPOC/Components/Pages/Chat.razor
- [X] T026 [US1] Implement server-side OpenAI Responses API call/stream path with configurable model selection in AnnuityChatPOC/Application/Services/OpenAiResponsesService.cs
- [X] T027 [US1] Apply AI identity, restricted-catalog, and educational-boundary disclosures from centralized instructions in AnnuityChatPOC/Application/Prompts/AssistantInstructionsProvider.cs and AnnuityChatPOC/Application/Services/ConversationService.cs
- [X] T028 [US1] Implement user-visible loading/error states in AnnuityChatPOC/Components/Shared/StatusBanner.razor

**Checkpoint**: User Story 1 is independently functional and demo-ready as MVP.

---

## Phase 4: User Story 2 - Gather suitability context before matching products (Priority: P1)

**Goal**: Ensure assistant collects missing retirement/suitability context before recommendations.

**Independent Test**: Ask for a "best annuity" with missing details and verify follow-up questions for risk, timeline, liquidity, and income objectives.

### Tests for User Story 2

- [X] T029 [P] [US2] Add unit tests for missing-suitability detection and required follow-up prompts in AnnuityChatPOC/tests/AnnuityChatPOC.UnitTests/Conversation/SuitabilityPromptingTests.cs
- [X] T030 [P] [US2] Add integration test for multi-turn suitability collection flow in AnnuityChatPOC/tests/AnnuityChatPOC.IntegrationTests/Chat/SuitabilityCollectionTests.cs

### Implementation for User Story 2

- [X] T031 [US2] Implement suitability extraction/update logic for timeline, risk, liquidity, and income goals in AnnuityChatPOC/Application/Services/SuitabilityProfileService.cs
- [X] T032 [US2] Persist and refresh suitability snapshots across conversation turns in AnnuityChatPOC/Application/Services/ConversationService.cs
- [X] T033 [US2] Add clarifying-question policy rules for insufficient inputs in AnnuityChatPOC/Application/Prompts/AssistantInstructionsProvider.cs
- [X] T034 [US2] Render clarifying prompts and continuity cues in AnnuityChatPOC/Components/Shared/ChatMessageList.razor
- [X] T035 [US2] Return explicit insufficiency responses when critical context is missing in AnnuityChatPOC/Application/Services/ConversationService.cs

**Checkpoint**: User Stories 1 and 2 work independently and together.

---

## Phase 5: User Story 3 - Receive controlled product suggestions with rationale (Priority: P1)

**Goal**: Provide validated MassMutual Ascend-only recommendations with rationale, risks, and restrictions.

**Independent Test**: Provide complete suitability details and confirm recommendations are catalog-approved, validated, and include tradeoff/risk disclosures.

### Tests for User Story 3

- [X] T036 [P] [US3] Add unit tests for product lookup tool input/output contract handling in AnnuityChatPOC/tests/AnnuityChatPOC.UnitTests/Products/ProductLookupToolContractTests.cs
- [X] T037 [P] [US3] Add unit tests for recommendation allowlist validation and invalid identifier rejection in AnnuityChatPOC/tests/AnnuityChatPOC.UnitTests/Recommendations/RecommendationValidationTests.cs
- [X] T038 [P] [US3] Add adversarial integration tests for competitor recommendation requests, fabricated rates, and prompt-injection attempts in AnnuityChatPOC/tests/AnnuityChatPOC.IntegrationTests/Chat/RecommendationGuardrailTests.cs

### Implementation for User Story 3

- [X] T039 [US3] Implement `get_approved_annuity_products` function/tool schema and execution path in AnnuityChatPOC/Application/Services/OpenAiResponsesService.cs
- [X] T040 [US3] Implement catalog filtering by jurisdiction/distribution channel for tool retrieval in AnnuityChatPOC/Application/Services/ProductCatalogService.cs
- [X] T041 [US3] Implement `validate_recommendation_candidates` server-side validation gate before rendering responses in AnnuityChatPOC/Application/Services/RecommendationValidationService.cs and AnnuityChatPOC/Application/Services/ConversationService.cs
- [X] T042 [US3] Enforce rejection of unauthorized/unavailable/unverified recommendations with safe fallback messaging in AnnuityChatPOC/Application/Services/ConversationService.cs
- [X] T043 [US3] Render recommendation cards with rationale, risks, restrictions, and no competitor recommendation display in AnnuityChatPOC/Components/Shared/ChatMessageList.razor
- [X] T044 [US3] Add educational-only financial guidance disclosure for recommendation turns in AnnuityChatPOC/Application/Prompts/AssistantInstructionsProvider.cs

**Checkpoint**: User Stories 1-3 deliver the core controlled recommendation experience.

---

## Phase 6: User Story 4 - Experience reliable chat behavior during service issues (Priority: P2)

**Goal**: Preserve trust with robust streaming UX, error handling, New Chat reset, and session isolation.

**Independent Test**: Simulate upstream failures and multiple sessions; verify progressive updates, preserved history, retry messaging, and isolated session state.

### Tests for User Story 4

- [X] T045 [P] [US4] Add integration tests for streaming progress and failure recovery messaging in AnnuityChatPOC/tests/AnnuityChatPOC.IntegrationTests/Chat/StreamingAndFailureTests.cs
- [X] T046 [P] [US4] Add unit tests for session isolation and New Chat reset state transitions in AnnuityChatPOC/tests/AnnuityChatPOC.UnitTests/Conversation/SessionIsolationAndResetTests.cs

### Implementation for User Story 4

- [X] T047 [US4] Implement New Chat action and `Active -> Resetting -> Active` transition handling in AnnuityChatPOC/Components/Pages/Chat.razor and AnnuityChatPOC/Application/Services/ConversationService.cs
- [X] T048 [US4] Add cancellation token propagation, timeout handling, and rate-limit error mapping in AnnuityChatPOC/Application/Services/OpenAiResponsesService.cs
- [X] T049 [US4] Implement auto-scroll-to-latest behavior and loading indicator transitions in AnnuityChatPOC/Components/Pages/Chat.razor and AnnuityChatPOC/Components/Shared/ChatMessageList.razor
- [X] T050 [US4] Preserve prior conversation history on failed turns with clear retry affordance in AnnuityChatPOC/Application/Services/ConversationService.cs and AnnuityChatPOC/Components/Shared/StatusBanner.razor
- [X] T051 [US4] Enforce input length limits and sensitive-pattern safeguards for SSN/account data in AnnuityChatPOC/Components/Shared/ChatComposer.razor and AnnuityChatPOC/Application/Services/ConversationService.cs

**Checkpoint**: All user stories are independently testable and complete.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final hardening, documentation alignment, and full validation.

- [X] T052 [P] Update architecture and configuration documentation in AnnuityChatPOC/README.md and AnnuityChatPOC/specs/001-annuity-chat-assistant/quickstart.md
- [X] T053 [P] Add/adjust privacy-safe logging scopes and redaction behavior in AnnuityChatPOC/Program.cs and AnnuityChatPOC/Application/Services/OpenAiResponsesService.cs
- [X] T054 Run and fix full automated test suite issues in AnnuityChatPOC/tests/AnnuityChatPOC.UnitTests/ and AnnuityChatPOC/tests/AnnuityChatPOC.IntegrationTests/
- [X] T055 Execute end-to-end quickstart scenarios and record any required corrections in AnnuityChatPOC/specs/001-annuity-chat-assistant/quickstart.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies.
- **Phase 2 (Foundational)**: Depends on Phase 1; blocks all user stories.
- **Phases 3-6 (User Stories)**: Depend on Phase 2 completion; execute in priority order for single-threaded delivery.
- **Phase 7 (Polish)**: Depends on completion of all implemented user stories.

### User Story Dependencies

- **US1 (P1)**: Depends only on Foundational phase.
- **US2 (P1)**: Depends on Foundational phase; extends conversation behavior delivered by US1.
- **US3 (P1)**: Depends on Foundational phase and benefits from US2 suitability data collection.
- **US4 (P2)**: Depends on Foundational phase and hardens US1-US3 runtime paths.

### Within Each User Story

- Test tasks are created before implementation tasks and should fail initially.
- Domain/rules updates before service integration.
- Service integration before UI finalization.
- Complete independent validation before moving to next story.

### Parallel Opportunities

- Setup parallel tasks: T003-T006.
- Foundational parallel tasks: T008-T012, T016, T018.
- US1 parallel tasks: T020-T024.
- US2 parallel tasks: T029-T030.
- US3 parallel tasks: T036-T038.
- US4 parallel tasks: T045-T046.
- Polish parallel tasks: T052-T053.

---

## Parallel Example: User Story 3

```bash
# Parallel tests
Task: "T036 [US3] Product lookup tool contract tests in tests/AnnuityChatPOC.UnitTests/Products/ProductLookupToolContractTests.cs"
Task: "T037 [US3] Recommendation validation tests in tests/AnnuityChatPOC.UnitTests/Recommendations/RecommendationValidationTests.cs"
Task: "T038 [US3] Adversarial guardrail integration tests in tests/AnnuityChatPOC.IntegrationTests/Chat/RecommendationGuardrailTests.cs"

# Parallel implementation prep
Task: "T039 [US3] Function/tool schema implementation in Application/Services/OpenAiResponsesService.cs"
Task: "T040 [US3] Catalog filtering in Application/Services/ProductCatalogService.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 (Setup)
2. Complete Phase 2 (Foundational)
3. Complete Phase 3 (US1)
4. Validate US1 independently (concept answers + disclosures + streaming baseline)
5. Demo MVP

### Incremental Delivery

1. Add US2 and validate suitability-question behavior independently.
2. Add US3 and validate allowlist-only recommendations and adversarial protections.
3. Add US4 for reliability, reset, and failure handling hardening.
4. Complete Phase 7 polish and rerun full tests and quickstart scenarios.

### Suggested MVP Scope

- **Recommended MVP**: Through Phase 3 (US1) after completion of Setup + Foundational phases.

---

## Notes

- [P] tasks target different files without incomplete-task dependencies.
- [US#] labels provide traceability from tasks to spec user stories.
- Recommendation controls must remain enforced server-side regardless of prompt content.
- Avoid adding persistence, accounts, or purchasing workflows in this POC scope.

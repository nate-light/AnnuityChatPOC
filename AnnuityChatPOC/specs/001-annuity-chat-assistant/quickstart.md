# Quickstart Validation Guide: AI Annuity Assistant POC

## Prerequisites

- .NET SDK installed (latest stable supported version for this repo)
- OpenAI API key
- PowerShell terminal

## Setup

1. From repository root, initialize and restore:
   - `dotnet restore`
2. Configure secrets (development):
   - `dotnet user-secrets init`
   - `dotnet user-secrets set "OpenAI:ApiKey" "<your_api_key>"`
   - `dotnet user-secrets set "OpenAI:Model" "<responses_model_name>"`
3. Confirm product catalog JSON exists at configured catalog path.

## Run

1. Start app:
   - `dotnet run --project .\AnnuityChatPOC.csproj`
2. Open local URL shown by ASP.NET Core.
3. Navigate to chat page if not default.

## End-to-End Validation Scenarios

### Scenario 1: Basic educational conversation

1. Ask: "What is an annuity?"
2. Verify:
   - assistant responds in plain language
   - AI identity and restricted-catalog disclosure are present

### Scenario 2: Suitability context collection

1. Ask: "What's the best annuity for me?"
2. Verify assistant requests missing details for:
   - retirement timeline
   - risk tolerance
   - liquidity needs
   - income goals

### Scenario 3: Catalog-only recommendation enforcement

> Ensure `Infrastructure/ProductCatalog/approved-products.json` contains approved production-authorized entries. The repository default is an empty list to avoid fabricated product data.

1. Provide full suitability context in follow-up turns.
2. Verify:
   - recommendations reference approved product IDs only
   - each recommendation includes rationale + risk/tradeoff notes
   - no competitor products are recommended

### Scenario 4: Competitor prompt adversarial test

1. Ask for recommendation from a named non-Ascend provider.
2. Verify:
   - assistant does not recommend competitor products
   - assistant may provide neutral educational discussion only

### Scenario 5: Fabrication guard test

1. Ask for specific rate/guarantee not in catalog data.
2. Verify assistant does not fabricate values and acknowledges uncertainty.

### Scenario 6: Service failure behavior

1. Temporarily invalidate API key or simulate upstream failure.
2. Send message.
3. Verify:
   - clear error shown
   - retry is possible
   - prior history remains visible

### Scenario 7: Session isolation

1. Open app in two separate browser sessions.
2. Send different prompts in each.
3. Verify conversation histories do not mix.

### Scenario 8: New Chat reset

1. Build a multi-turn conversation.
2. Trigger New Chat.
3. Verify active session history and suitability state reset.

## Automated Tests

Run targeted tests:

- `dotnet test --filter "Category=Unit|Category=Policy|Category=Conversation"`

Run full suite:

- `dotnet test .\AnnuityChatPOC.slnx`

Expected outcomes:

- All policy guardrail tests pass (catalog allowlist, competitor blocking, no fabricated details)
- Session isolation tests pass
- Recommendation validation tests pass
- Failure-path tests pass with deterministic mock responses

## Latest Validation Record

- Date: 2026-11-08
- Command: `dotnet test .\AnnuityChatPOC.slnx --no-restore`
- Result: Passed (Unit: 9, Integration: 6, Failed: 0)
- Smoke check: `dotnet run --project .\AnnuityChatPOC.csproj --urls http://127.0.0.1:5099` + HTTP status `200` from `http://127.0.0.1:5099`
- Notes:
  - Manual API-backed chat scenarios require a valid `OpenAI:ApiKey` and approved catalog entries.
  - Recommendation checks intentionally reject IDs not present in the approved catalog.

## Related Design Artifacts

- [plan.md](./plan.md)
- [research.md](./research.md)
- [data-model.md](./data-model.md)
- [chat-interaction-contract.md](./contracts/chat-interaction-contract.md)
- [product-tool-contract.md](./contracts/product-tool-contract.md)

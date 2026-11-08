# Implementation Plan: AI Annuity Assistant POC

**Branch**: `[001-annuity-chat-assistant]` | **Date**: 2026-11-08 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-annuity-chat-assistant/spec.md`

## Summary

Build a smallest-viable, end-to-end Blazor Web App proof of concept that delivers ChatGPT-style
annuity conversations with strong recommendation controls. The implementation uses one ASP.NET
Core project with Interactive Server rendering, a server-only OpenAI Responses integration,
in-memory conversation state, and a strict approved-product allowlist enforced independently of
model output.

## Technical Context

**Language/Version**: C# 13 on latest stable supported .NET (target .NET 9 unless host tooling requires latest LTS)

**Primary Dependencies**: ASP.NET Core Blazor Web App (Interactive Server), OpenAI .NET SDK, Microsoft.Extensions.* configuration/logging, System.Text.Json

**Storage**: In-memory conversation state per user session; JSON file catalog for approved products

**Testing**: xUnit with mock OpenAI client/service doubles for deterministic conversation and validation tests

**Target Platform**: Local developer machine and internal demo server (ASP.NET Core)

**Project Type**: Single-project server-rendered web application

**Performance Goals**:
- Start assistant streaming feedback to user within 3 seconds for normal requests
- Preserve smooth chat interactions for at least 20 turns per session without state loss
- Keep recommendation validation deterministic and synchronous relative to response rendering

**Constraints**:
- No user accounts, purchases, internal system integrations, Docker, Redis, or external database
- Server-side secret handling only (environment variables or user secrets)
- Recommendation outputs MUST pass allowlist validation before display
- No external JavaScript frameworks; use Blazor + minimal browser interop only if needed

**Scale/Scope**:
- Proof-of-concept scope for demos and design validation
- Single organization catalog (MassMutual Ascend only)
- Concurrent usage target: small internal pilot group (tens of active sessions)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Research Gate Review

- ✅ **POC Simplicity**: Single app, lightweight layers, no extra projects/services.
- ✅ **Required Stack**: ASP.NET Core + C# + Blazor Interactive Server selected.
- ✅ **Server-Side OpenAI**: Dedicated injectable service with server-only credentials.
- ✅ **Catalog Enforcement**: Independent allowlist validation required before display.
- ✅ **AI Transparency/Compliance**: Mandatory AI identity and educational-only boundaries.
- ✅ **Security/Privacy**: Input limits, no sensitive data collection, safe output encoding.
- ✅ **Testing/Docs**: xUnit coverage and README updates planned.

Gate status: **PASS**

### Post-Design Gate Re-Check

- ✅ Research decisions preserve POC-first simplicity and avoid unnecessary abstractions.
- ✅ Data model and contracts enforce product-source-of-truth and recommendation validation.
- ✅ Quickstart includes disclosure, adversarial, and failure-path validation scenarios.

Gate status: **PASS**

## Project Structure

### Documentation (this feature)

```text
specs/001-annuity-chat-assistant/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── chat-interaction-contract.md
│   └── product-tool-contract.md
└── tasks.md
```

### Source Code (repository root)

```text
AnnuityChatPOC/
├── Program.cs
├── AnnuityChatPOC.csproj
├── appsettings.json
├── appsettings.Development.json
├── Components/
│   ├── App.razor
│   ├── Layout/
│   ├── Pages/
│   │   └── Chat.razor
│   └── Shared/
│       ├── ChatMessageList.razor
│       ├── ChatComposer.razor
│       └── StatusBanner.razor
├── Domain/
│   ├── Conversations/
│   ├── Products/
│   └── Recommendations/
├── Application/
│   ├── Abstractions/
│   ├── Services/
│   │   ├── OpenAiResponsesService.cs
│   │   ├── ConversationService.cs
│   │   ├── ProductCatalogService.cs
│   │   └── RecommendationValidationService.cs
│   └── Prompts/
├── Infrastructure/
│   ├── ProductCatalog/
│   │   └── approved-products.json
│   └── OpenAI/
└── tests/
    ├── AnnuityChatPOC.UnitTests/
    └── AnnuityChatPOC.IntegrationTests/
```

**Structure Decision**: Choose a single Blazor Web App project with internal Domain/Application/
Infrastructure folders to preserve separation of concerns without adding extra projects. This
matches constitution simplicity requirements while keeping the chat UI independent from OpenAI
and product-catalog services.

## Complexity Tracking

No constitutional violations identified; no complexity exceptions required.

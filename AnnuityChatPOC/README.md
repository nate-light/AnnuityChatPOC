# AnnuityChatPOC

Proof-of-concept Blazor Web App for AI-assisted annuity conversations constrained to an approved MassMutual Ascend product catalog.

## Tech stack

- ASP.NET Core Blazor Web App (Interactive Server)
- C# / .NET 10.0
- OpenAI Responses API (server-side only)
- In-memory conversation state per session
- JSON-backed approved product catalog

## Prerequisites

- .NET SDK 10.0+
- OpenAI API key

## Local setup

```powershell
dotnet restore
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "<your_api_key>"
dotnet user-secrets set "OpenAI:Model" "gpt-4.1-mini"
dotnet run --project .\AnnuityChatPOC.csproj
```

Open the local URL from the output and use the chat interface.

## Configuration

`appsettings.json`:

- `OpenAI:Model` - model name used for Responses API calls.
- `OpenAI:Endpoint` - defaults to `https://api.openai.com/v1/responses`.
- `OpenAI:TimeoutSeconds` - request timeout.
- `Chat:MaxUserMessageLength` - message size limit.
- `Chat:ProductCatalogPath` - approved product data source.
- `Chat:DefaultJurisdiction` / `Chat:DefaultDistributionChannel` - validation defaults.

## Product data management

Approved recommendation products are loaded from:

- `Infrastructure/ProductCatalog/approved-products.json`

The app validates provider, eligibility, and allowlist status before showing recommendations.

## Architecture overview

- `Components/` - UI pages and chat components.
- `Application/Services/` - conversation orchestration, OpenAI integration, catalog loading, recommendation validation.
- `Application/Prompts/` - centralized AI instruction profile.
- `Domain/` - strongly typed conversation, product, and recommendation models.
- `tests/` - xUnit unit and integration-style tests (with mocked AI service).

## Security and privacy notes

- OpenAI key is server-side only (user secrets/environment variable).
- User messages are length-limited and checked for sensitive account/SSN-like patterns.
- Blazor default HTML encoding is used for model output rendering.
- Recommendation IDs are server-side validated against approved catalog rules.

## Current POC boundaries

- No user accounts or persistence beyond in-memory session state.
- No purchase, transfer, or replacement flows.
- No integration with internal MassMutual Ascend systems.


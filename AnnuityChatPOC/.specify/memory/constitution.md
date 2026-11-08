<!--
Sync Impact Report
- Version change: unversioned scaffold -> 1.0.0
- Modified principles:
  - Principle slot 1 -> 1. Simplicity and Scope
  - Principle slot 2 -> 2. Technology Stack
  - Principle slot 3 -> 3. OpenAI Integration
  - Principle slot 4 -> 4. AI Persona and Transparency
  - Principle slot 5 -> 5. Exclusive Product Recommendations
  - Added principles 6-16 under Core Principles
- Added sections:
  - Application Boundaries and Safety Controls
  - Development Workflow and Quality Gates
- Removed sections:
  - None
- Follow-up TODOs:
  - None
-->

# AnnuityChatPOC Constitution

## Core Principles

### 1. Simplicity and Scope
The project MUST prioritize a working proof of concept over production-scale architecture.
Implementations MUST remain simple, readable, and maintainable. Features, abstractions,
infrastructure, and dependencies MUST be included only when they directly support the
conversational annuity experience.

### 2. Technology Stack
The application MUST use ASP.NET Core, C#, and Blazor Web App with Interactive Server
rendering on a supported stable .NET version. Frontend and backend MUST stay in one
application unless separation is explicitly justified and documented. Built-in Microsoft
libraries and dependency injection MUST be preferred by default.

### 3. OpenAI Integration
Conversational intelligence MUST use the supported OpenAI Responses API. OpenAI API calls
MUST execute server-side only and MUST be encapsulated in a dedicated application service.
The application MUST support multi-turn conversations with explicit context management and
MUST handle API errors, timeouts, rate limits, and cancellation paths gracefully.

### 4. AI Persona and Transparency
The assistant MUST communicate as a knowledgeable annuity product specialist with clear,
professional, accessible language. It MUST disclose that it is an AI assistant, not a
licensed human financial professional, and that recommendation scope is limited to
MassMutual Ascend offerings.

### 5. Exclusive Product Recommendations
The assistant MUST recommend only MassMutual Ascend annuity products present in the
application's approved product catalog. It MUST NOT recommend competing providers' products.
It MAY discuss other products or providers only in neutral educational context and MUST NOT
frame them as recommendations.

### 6. Product Knowledge and Accuracy
Product-specific claims MUST be grounded in approved MassMutual Ascend documentation or
structured product data. The assistant MUST NOT invent names, rates, guarantees, fees,
surrender schedules, riders, eligibility, or contract features. When reliable information
is unavailable, it MUST acknowledge uncertainty rather than speculate.

### 7. Product Catalog Enforcement
The system MUST enforce an application-controlled allowlist for recommendation-eligible
products independently of AI prompt instructions. Recommendation candidates MUST be validated
for jurisdiction and distribution-channel eligibility before they are presented.

### 8. Financial Suitability and Responsible Guidance
Before identifying potentially suitable products, the assistant MUST gather enough context on
goals, retirement timeline, liquidity needs, risk tolerance, and income objectives. Responses
MUST explain relevant risks, fees, limitations, and tradeoffs. The assistant MUST NOT promise
returns beyond documented contractual guarantees and MUST NOT present output as definitive
personalized advice without authorized compliance process.

### 9. Financial Compliance
Product recommendation behavior MUST be treated as regulated activity requiring appropriate
review. Until licensing, suitability, best-interest, supervisory, and compliance requirements
are satisfied, the application MUST present outputs as educational and preliminary product
matching only. The proof of concept MUST NOT facilitate purchases, replacements, or transfers.

### 10. Security and Privacy
OpenAI credentials MUST be stored securely in server-side configuration or secret management
and MUST NEVER be exposed in Blazor components or browser traffic. Inputs MUST be validated
and sensitive data protected. The application MUST minimize collection of personally
identifiable and financial data and MUST NOT collect account numbers, Social Security numbers,
or other unnecessary sensitive data.

### 11. Prompt and Instruction Security
Trusted application instructions MUST remain separated from user messages and retrieved
content. Users and retrieved documents MUST NOT be able to override system-level product
restrictions or governance rules. Retrieved documents MUST be treated as untrusted for
instruction authority, and AI-generated product references or structured outputs MUST be
validated before use.

### 12. Blazor Development
UI MUST be built with reusable Razor components that have clear responsibilities. Conversation
state, rendering, AI integration, and product-data access MUST remain separated. Async flows
and appropriate component lifecycle methods MUST be used. JavaScript dependencies MUST be
avoided unless necessary and justified.

### 13. Conversation Experience
The chat UX MUST provide conversation history, clear message attribution, loading indicators,
error handling, and responsive layout. Incremental response streaming SHOULD be implemented
when practical to improve user experience. Context MUST be preserved within the active session.

### 14. Testing and Evaluation
Tests MUST cover core business logic, product allowlist enforcement, and API integration
boundaries. AI evaluation scenarios MUST include incorrect product facts, unsupported
guarantees, competitor recommendations, prompt injection attempts, missing suitability inputs,
and incomplete documentation cases. Deterministic automated tests MUST use mock OpenAI
responses.

### 15. Documentation
The repository MUST maintain a concise README describing local setup, OpenAI configuration,
architecture, product-data management, and known limitations. Significant architectural
decisions and explicit proof-of-concept boundaries MUST be documented.

### 16. Incremental Development
Delivery MUST proceed in small, verifiable increments while keeping the application functional
after each increment. The initial baseline MUST include a working Blazor chat interface and
OpenAI integration, followed by conversation context, controlled product knowledge, product
recommendation validation, and financial safety controls.

## Application Boundaries and Safety Controls

These principles govern specifications, plans, tasks, implementation decisions, and runtime AI
behavior. Any exception MUST be explicitly justified, documented, and approved before merge.
No implementation may bypass product allowlist enforcement, compliance safety boundaries, or
server-side secret handling.

## Development Workflow and Quality Gates

All changes MUST map to one or more constitutional principles in spec, plan, and task
artifacts. Pull requests MUST document safety impact for AI behavior changes and MUST include
evidence of tests for product restrictions and prompt-injection resistance when affected.
Scope creep that violates the proof-of-concept focus MUST be deferred and tracked explicitly.

## Governance

This constitution is the highest-priority project governance source and supersedes conflicting
informal practices. Amendments MUST include: (1) clear rationale, (2) explicit scope of
change, (3) semantic version bump justification, and (4) updates to impacted planning/testing
artifacts. Compliance reviews MUST verify constitutional alignment before approval.
Versioning policy: MAJOR for incompatible governance redefinitions/removals, MINOR for new
principles or materially expanded guidance, PATCH for clarifications and non-semantic wording
improvements.

**Version**: 1.0.0 | **Ratified**: 2026-11-08 | **Last Amended**: 2026-11-08

# Chat Interaction Contract

## Purpose

Define the user-visible chat interaction behavior and server orchestration contract for the
proof-of-concept assistant.

## Session Contract

- A session begins when the user loads chat and is assigned a unique `SessionId`.
- Session state is isolated per user connection/session.
- "New Chat" creates a fresh conversation state with no prior turns.

## Input Contract

- User submits plain-text messages.
- Empty or whitespace-only messages are rejected client-side and server-side.
- Messages exceeding configured length are rejected with friendly guidance.
- Sensitive data patterns (such as SSN/account number formats) trigger cautionary response and
  no recommendation action.

## Response Contract

- Assistant replies stream progressively when available.
- Every assistant recommendation response includes:
  - AI identity disclosure
  - restricted-catalog disclosure
  - educational/advice-boundary disclosure
- If suitability context is incomplete, assistant asks follow-up questions instead of
  recommending products.

## Error Contract

- Upstream model failures return user-safe error text and retry option.
- Rate-limit/timeout errors expose no secret/config details.
- Prior conversation history remains visible after failed turns.

## Recommendation Rendering Contract

- Recommendation content is displayed only after server-side validation approves all product IDs.
- Rejected recommendation candidates are either omitted or replaced with explanatory fallback
  text stating no verified recommendation is available.

## Non-Goals

- No account login/logout contract.
- No purchase/transaction initiation contract.
- No cross-session conversation persistence contract.

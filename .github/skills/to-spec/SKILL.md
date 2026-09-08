---
name: to-spec
description: Turn the current conversation into a concise, implementation-ready specification. Use when requirements, decisions, and scope have already been discussed and the user wants them synthesized into a durable spec. Use the grilling skill when material decisions remain unresolved.
---

# To Spec

Synthesize the current conversation and repository context into a specification. Do not re-interview the user for information already available; identify only material gaps that block a decision.

## Process

1. Inspect the relevant code, existing plans, tests, API contracts, and project instructions. Use the project's established domain vocabulary and record only verified current behavior.
2. Read the `testing-strategy` skill. For each requested behavior, select the cheapest test layer that can prove it, the observable seam, and the required CI cadence. Use test-first work for domain and application/use-case behavior; use the strategy's test-alongside or test-after placement for the other layers.
3. If material decisions remain unresolved, use the `grilling` skill as a focused decision gate. Build the current design tree, ask the whole decision frontier in rounds with recommended answers, and update the spec only after the user confirms the shared understanding. Do not grill for facts that repository inspection can establish, and do not reopen settled decisions.
4. Write the specification in `docs/specs/<feature-slug>.md` unless the repository documents another location. If the user explicitly requests an issue, use the configured GitHub issue workflow and include the complete specification body.
5. State what is intentionally out of scope and record any remaining decision that prevents implementation. Do not invent a requirement, tool, test target, or deployment policy.

## Specification template

```markdown
# <Feature name>

## Problem

[The user or business problem, grounded in verified current behavior.]

## Scope

### In scope

- [Behavior or outcome]

### Out of scope

- [Explicit exclusion]

## User outcomes

1. As a <user>, I want <capability>, so that <benefit>.

## Requirements

1. [Observable requirement]
2. [Observable requirement]

## Decisions and constraints

- [Confirmed product, technical, contract, or operational decision]

## Acceptance criteria

- [ ] [Specific observable outcome]

## Testing strategy

| Requirement / acceptance criterion | Observable seam | Cheapest proving layer | Approach | Pipeline cadence |
| --- | --- | --- | --- | --- |
| [Requirement] | [Public interface, endpoint, UI interaction, or contract] | [Domain, application, integration/API, component, contract, E2E] | [Test-first, test-alongside, or test-after] | [PR, merge to main, nightly, pre-release] |

[Name the existing test pattern and command where known. Explain required cross-repository contract, accessibility, performance, visual-regression, security, or E2E coverage. Do not use E2E tests to prove business rules that a lower layer can prove. Record deliberate coverage gaps and their rationale.]

## Open questions

- [Only decisions that remain unresolved and materially affect delivery.]
```

## Completion criteria

A completed specification has verified context, testable requirements and acceptance criteria, a testing-strategy row for every material outcome, explicit scope boundaries, and no invented assumptions. If material decisions were initially open, the grilling frontier is closed and the user confirmed the shared understanding. It gives `implementation-plan` enough information to create tasks and concrete tests without rediscovering the product decision.

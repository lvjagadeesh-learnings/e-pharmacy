---
name: client-facing-story
description: Write a client-facing user story or PBI in plain language for stakeholder discussion and sign-off — current behaviour, in/out scope, open questions with recommendations, and testable acceptance criteria. Use when the user asks for a story the client or product owner can review, a "client-friendly" or "stakeholder-ready" write-up, or a PBI for sign-off before development starts. Not for developer-facing INVEST stories extracted from source documents (use generate-stories) or technical planning with file paths and tasks (use implementation-plan). A plain "write a user story" request with no stakeholder or sign-off cue also belongs to generate-stories.
---

# Client-Facing Story

Produce a write-up a non-technical stakeholder can read, discuss, and sign off. This is **not** an implementation plan — no file paths, class names, code, or architecture. Describe what the user experiences, not how it's built. Development-facing stories and plans come later, after sign-off, via `generate-stories` and `implementation-plan`.

## Process

1. **Ground it in the codebase first.** Read the relevant feature areas so *Context / Current Behaviour* reflects what the product actually does today, not what the request assumes. A write-up that misstates current behaviour derails the client conversation.
2. **Ask about scope before writing** if the boundary is unclear — the Scope section is a commitment the client signs off on.
3. **Output the story as Markdown in chat.** The user copies it into their tracker (Azure DevOps, Jira, …). Don't write it into the repo unless asked. To push it directly to a tracker, the `azure-devops-rest-no-mcp` or `atlassian-rest-no-mcp` plugins can do that as a follow-up step.

## Format

Aim for one screen of reading — the whole write-up must fit comfortably in a PBI/issue description field.

### Title

Short and descriptive.

### User Story

One sentence: *As a [persona], I'd like to [goal] so that [benefit].*

### Context / Current Behaviour

2–4 bullets: what happens today and why the change is needed, written so a non-technical stakeholder can follow.

### Scope

Bullets for what is explicitly **in** and **out**. Call out anything deliberately deferred to a follow-up.

### UX Notes

How the feature fits existing screens and flows; whether new screens/components are needed or existing UI is reused. Refer to screens by their user-visible names ("Orders tab", "Filter sheet"), never code identifiers.

### Open Questions

Numbered decisions that need client input before development begins — **with a short recommendation wherever the team has a view**. This is the most valuable section: it frames the client conversation instead of just listing unknowns.

### Technical Notes *(optional)*

2–3 bullets max, in plain language ("stored on-device only, same pattern as the existing favourites feature"). Omit if nothing noteworthy.

### Acceptance Criteria

Numbered, observable outcomes that define "done" from the user's perspective. Each testable and specific. Mark platform-specific criteria (iOS)/(Android)/(web) only where behaviour genuinely differs.

## Style rules

- Write for a product owner, not a developer — no code, file paths, class names, or architecture jargon.
- Match the project's language for its users ("member", "customer", "practitioner", …) and use it consistently.
- No implementation tasks or file-level change lists — those belong to the planning step after sign-off.
- No emojis unless the user asks.

### ❌ BAD: acceptance criterion written as an implementation task

```
3. Save the selected store in local app storage and restore it when the app
   opens again.
```

**Why bad:** the stakeholder can't validate storage mechanisms or app internals — they can only nod along, and the sign-off stops meaning anything. It also pre-commits the implementation before a developer has looked at it.

### ✅ GOOD: the same criterion as an observable outcome

```
3. The store I picked is still selected the next time I open the app.
```

## Done when

- [ ] All required sections present (Title, User Story, Context, Scope, UX Notes, Open Questions, Acceptance Criteria)
- [ ] Open Questions carry recommendations where the team has a view
- [ ] Context reflects verified current behaviour, not assumptions
- [ ] Reads clean when spoken aloud to a non-developer — no jargon survived

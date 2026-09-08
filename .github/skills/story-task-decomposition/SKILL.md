---
name: story-task-decomposition
description: "Use when one user story must be broken into sequenced, role-separated implementation plans for handoff to specialist delivery agents (DevOps, AI Engineer, Full-Stack, Data Engineer, Security, SRE, Test Engineer). Handles XS–M stories directly; applies human-gated preparation for L and XL. Trigger phrases: decompose this story, split into tasks by role, role-separated implementation plans, break this story down for the fleet, story decomposition, task breakdown by discipline. For a single codebase-aware plan for one implementer, use `implementation-plan` instead."
---

# Story Task Decomposition

Take one user story and produce structured, sequenced implementation plan files that AI coding agents can consume independently — without re-reading the original story.

This skill does **not** write implementation code. Its output is a set of plan files:

```
{DELIVERY_ROOT}/epic-[EPIC_ID]/story-[STORY_ID]/task-[N]/[role]-implementation-plan.md
```

Each file is self-contained and immediately actionable by the named specialist.

> **Related:** `delivery:implementation-plan` produces a *single* codebase-aware plan for one implementer. Use this skill instead when the work spans multiple delivery disciplines and each needs its own plan.

---

## Step 0 — Input resolution

Determine which invocation mode applies.

| Mode | Indicator | Action |
|---|---|---|
| **Story path** | Argument is a file path to an `.md` story file | Proceed to Step 1 using that path |
| **File attachment** | Argument references an attached file or a `.md`/`.txt` file outside standard story paths | Read the file; treat its content as the story; extract story structure below |
| **Direct task** | Argument is a natural language description of work with no file path | Parse goal, scope, tech signals, constraints; synthesise a minimal story structure; assign provisional story ID `adhoc-[YYYYMMDD]` |

### Context gathering (File attachment and Direct task modes)

1. Read the repo's assistant-instruction file to extract `PRESALES_ROOT` and `DELIVERY_ROOT` — whichever of `CLAUDE.md`, `AGENTS.md`, or `.github/copilot-instructions.md` the repo actually uses. If `DELIVERY_ROOT` is not set, default to `docs/delivery`.
2. Scan for architecture manifests: `{DELIVERY_ROOT}/architecture/architecture-manifest.md`, then `{PRESALES_ROOT}/architecture/architecture-manifest.md`.
3. Infer tech stack from the repo: `README.md`, `package.json`, `requirements.txt`, `pyproject.toml`, `go.mod`, `pom.xml`, `.terraform`/`*.tf`, `Dockerfile`, and relevant source files.
4. If cloud or AI service signals are present and documentation tools are available, query them to confirm SDK names, API patterns, or service constraints.
5. Identify critical gaps that would block decomposition: acceptance criteria, language/framework, key service names.
6. If gaps remain, ask the user. Ask no more than 5 questions at once.

### Story synthesis (File attachment and Direct task modes)

Once context is gathered, synthesise a minimal story structure:

```
**Story ID**: [derived from filename, or "adhoc-[YYYYMMDD]"]
**Epic ID**: [inferred, or "adhoc"]
**T-shirt size**: [XS / S / M / L / XL — estimated from scope]
**Description**: [what must be built]
**Acceptance Criteria**: [verifiable outcomes — stated or inferred]
**Technical Constraints**: [tech stack, integrations, constraints]
**Dependencies**: [none | list]
**Tasks**: [from file content, or empty — to be generated in Step 5]
```

---

## Step 1 — Read the story

Read the story file at the path provided. If only a story ID is given, locate it by checking `{DELIVERY_ROOT}/backlog/stories/` first, then `{PRESALES_ROOT}/work-decomposition/stories/`. Parse:

- Story ID and Epic ID
- T-shirt size (XS / S / M / L / XL)
- Description, Acceptance Criteria, Technical Constraints, Dependencies, Tasks

**If operating from a synthesised story structure (Step 0)**, this step is already complete.

**Resolve architecture root**: if `{DELIVERY_ROOT}/architecture/architecture-manifest.md` exists use `{DELIVERY_ROOT}/architecture`; otherwise use `{PRESALES_ROOT}/architecture`. Set this as `ARCH_ROOT`.

Read `{ARCH_ROOT}/architecture-manifest.md` to confirm the technology stack. Use this to make task definitions concrete (file names, SDK names, language conventions).

---

## Step 2 — Validate story readiness

Before generating any tasks, validate:

1. **Acceptance criteria are verifiable** — every criterion must be checkable by an automated test or clearly observable system state. Flag subjective criteria for rewriting before proceeding.
2. **Technical constraints are concrete** — vague references like "use the appropriate AI service" are not actionable. Every constraint must name a specific technology, SDK version, or configuration target. If vague, flag it and stop.
3. **Internal dependencies are resolved** — every story listed as an internal dependency must already be merged, or its interface/contract must be defined as a stub. If neither, the story is **blocked** — halt and report.
4. **No embedded architectural decisions** — if the story contains a choice point (e.g. "evaluate whether to use approach A or B"), extract it as an explicit decision requiring human sign-off. Do not resolve it implicitly through task decomposition.

If validation fails, output a **Readiness Report** describing each blocker, then stop.

---

## Step 3 — Route by T-shirt size

### XS, S, M stories → proceed to Step 4

### L stories → L Pre-Delegation Checklist

Before proceeding to Step 4 for an L story, verify:

1. **Interface availability** — all interfaces, contracts, and data models that the implementation will consume or produce are already defined. If missing, produce stub definitions and request human review.
2. **Single service boundary** — the story touches no more than one primary service boundary. If it spans two or more, split it into separate stories and halt.
3. **Reviewer guide** — produce `{DELIVERY_ROOT}/epic-[EPIC_ID]/story-[STORY_ID]/reviewer-guide.md` breaking the expected PR into 3–5 named sections.

After completing the checklist, produce a **Pre-Delegation Summary** and request human confirmation before generating implementation plans.

### XL stories → Mandatory Human Sign-off Gate

1. **Produce a decomposition plan** — break the XL story into L and M sub-stories. Write to `{DELIVERY_ROOT}/epic-[EPIC_ID]/story-[STORY_ID]/decomposition-plan.md`.
2. **Extract architectural decisions** — identify every embedded architectural decision. Add as a `## Architectural Decisions` section in the decomposition plan, each with: proposed recommendation, reasoning, implications.
3. **Surface external blockers** — mark sub-stories that depend on undelivered client input, third-party confirmation, or SME deliverables.
4. **Request human sign-off** — present the plan and stop. Do not generate implementation plans until written approval is recorded.

---

## Step 4 — Determine primary and secondary roles

**Primary Role** — the role that owns the majority of the work:

| Role | Keyword signals |
|---|---|
| DevOps Engineer | terraform, IaC, networking, CI/CD, environment, storage provisioning, infrastructure, kubernetes, container, deployment pipeline, helm, docker |
| AI Engineer | RAG, embedding, vector store, retrieval, reranking, prompt management, chunking, indexing, LLMOps, semantic search, agent logic, tool calling, orchestration, reasoning, multi-agent, memory, planner, agent framework, evaluation pipeline, benchmarks, RAGAS, safety testing, model management |
| Full-Stack Engineer | business logic, API route, API handler, service layer, data model, ORM, DTO, frontend, UI, middleware, application core, REST endpoint, dependency injection |
| Data Engineer | ETL, data ingestion, transformation, data quality, batch processing, streaming pipeline, schema validation, connector, adapter, webhook, external system, third-party service, API integration |
| Security | authentication, authorization, PII detection, content safety, encryption, audit logging, RBAC, compliance, access control, identity |
| SRE | logging, metrics, distributed tracing, dashboards, alerting, monitoring, observability, telemetry, correlation ID, SLO, error budget, runbook, reliability |
| Test Engineer | integration test, E2E test, end-to-end test, contract test, Pact, test infrastructure, test data factory, mock server, fixture, performance baseline benchmark |

**Secondary Roles** — any roles needed for work that falls outside the primary role's boundary. List each explicitly.

---

## Step 5 — Decompose into tasks

Decompose the story into a sequenced list of tasks. Each task must be implementable by an AI coding agent in a single context window pass.

**Task granularity rules**:
- A task must touch no more than 2–3 files. If it requires more, split it.
- A task must have a single, clearly verifiable outcome.
- Tasks requiring coordinated changes across more than 3 service boundaries must be escalated rather than decomposed further.

**Task ordering rules**:
- Sequence tasks so no task depends on the output of a later task.
- Circular dependencies must be resolved — halt and request human input rather than producing an incorrect ordering.

**Security-sensitive task flagging**:
Any task touching the following must be marked `⚠ ENHANCED REVIEW REQUIRED` with the specific risk stated:
- Authentication and authorization logic
- Encryption, key management, or secrets handling
- PII detection, redaction, or storage
- Database schema migrations or any irreversible data change
- Shared interface or contract changes that affect multiple consumers

**Testability**:
Every task that implements behavioural logic must include an explicit testing sub-task specifying at least one success-path and one failure-path test case. Infrastructure-only tasks (e.g. adding a Terraform variable) are exempt.

---

## Step 6 — Write implementation plan files

For each task, determine which roles have work to perform. A task may involve one role or multiple roles. Create one file per role per task:

```
{DELIVERY_ROOT}/epic-[EPIC_ID]/story-[STORY_ID]/task-[N]/[kebab-role]-implementation-plan.md
```

Role name to filename mapping:
- DevOps Engineer → `devops-engineer-implementation-plan.md`
- AI Engineer → `ai-engineer-implementation-plan.md`
- Full-Stack Engineer → `full-stack-engineer-implementation-plan.md`
- Data Engineer → `data-engineer-implementation-plan.md`
- Security → `security-implementation-plan.md`
- SRE → `sre-implementation-plan.md`
- Test Engineer → `test-engineer-implementation-plan.md`

Each file must be fully self-contained. Use the following structure:

```markdown
# Implementation Plan: [Story ID] — Task [N] — [Role]

**Epic**: [EPIC_ID — epic name]
**Story**: [STORY_ID — story title]
**Task**: [N] — [Task title]
**Role**: [Role name]
**Story size**: [XS / S / M / L]
**Primary Role**: [yes / no]
**Depends on**: [task-[N] / none]
**Status**: Ready for implementation

---

## Story context

[2–3 sentence summary of what the story delivers and why it matters. Written for the AI coding agent, not copied verbatim from the story description.]

## This task's responsibility

[Precise description of what this role must build in this task. Reference architecture document sections if relevant. Concrete enough that no design decisions are left to the agent.]

## Files to change

| File | Action | Notes |
|------|--------|-------|
| `path/to/file.ts` | create / modify | brief description |

## Interfaces and contracts required

[List every interface, type, or contract this plan depends on. For each confirm: (a) it exists in the codebase at [path], or (b) it is defined as a stub at [path] pending review.]

## What to implement

[Step-by-step implementation instructions. Concrete, specific, referencing actual file names, class names, SDK method names from the architecture manifest.]

## Test cases required

- ✅ Success: [description]
- ❌ Failure: [description]
- ❌ Failure: [at least one more failure case]

## Acceptance criteria satisfied

[List the story AC numbers and text that this role's work in this task contributes to satisfying.]

## Handoff notes

[If this task produces an output another role's task in the same task folder depends on, describe what is produced and where. If none, write "none".]
```

For L stories, also write:

```
{DELIVERY_ROOT}/epic-[EPIC_ID]/story-[STORY_ID]/reviewer-guide.md
```

```markdown
# Reviewer Guide: [Story ID] — [Story Title]

This guide divides the expected PR into named sections to enable structured review.

## Section 1 — [Name]
[What changed, what to look for, key files]

## Section 2 — [Name]
...
```

---

## Step 7 — Report

Output a brief summary:

- Story ID and size
- Folder path where output was written
- Number of tasks generated
- Number of implementation plan files written, listed by task and role
- Any flags raised (enhanced review, blocked dependencies, stubs added)
- For L stories: confirm reviewer guide was produced and human confirmation was requested
- For XL stories: confirm decomposition plan was written and human sign-off was requested

# Skill mechanics

The skill-specific branch of [`writing-for-agents`](SKILL.md): what changes when the document is a skill (frontmatter, the invocation choice, and router skills). Everything else about writing it is the universal reference in `SKILL.md`.

## Invocation

Copilot skills are discovered from a `SKILL.md` file with YAML frontmatter containing `name` and `description`. The description is the skill's context pointer: write it as a concise, model-facing trigger statement that says what the skill does and when it applies. Users can also invoke a skill by name.

Keep every skill discoverable unless there is a strong repository-specific reason not to expose it. Do not add provider-specific metadata or invocation controls; use the standard `name` and `description` fields so the skill remains portable across Copilot environments.

A model-invoked skill should have a description with distinct trigger branches. A reference-only skill can still be model-invoked when other skills need its vocabulary or rules. If several skills need the same reference, keep it in a plain companion file and link to it from each skill.

## Splitting by invocation

The invocation cut of splitting (the sequence cut lives in `SKILL.md`): split off a model-invoked skill when you have a distinct leading word that should trigger it on its own (a trigger word you actually use in your prompts), or another skill must reach it. You pay context load for the new always-loaded description, so that independent reach has to be worth it.

## Router skills

When user-invoked skills multiply past what you can remember, that piled-up cognitive load is cured by a **router skill**: one user-invoked skill that names the others and when to reach for each, so the human has one skill to remember instead of many. It can only hint, never fire them: user-invoked skills have no description, so nothing but the human can reach them.

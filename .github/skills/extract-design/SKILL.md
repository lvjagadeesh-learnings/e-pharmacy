---
name: extract-design
description: Extract UI design specifications from a Figma file and write a structured Markdown design document under `docs/design/`. Use when the user provides a Figma URL or an implementation plan includes Figma design context. Returns a Markdown design doc consumed by downstream planning and implementation work.
---

# Extract design

Extract the supplied Figma file into a Markdown design doc. Inspect the available Figma integration or provided export, then capture the required analysis sections, output format, component structure, design tokens, responsive behavior, and icon mapping. If the file cannot be accessed, state the blocker and request an export rather than inventing design details.

## Inputs

Use this skill with `<figma-url> [<base-name>]`.

- **`<figma-url>`** — a `figma.com/file/...` or `figma.com/design/...` URL. Required.
- **`<base-name>`** — optional. Output file is `docs/design/<base-name>.md`. If omitted, derive a name from the Figma metadata: prefer the page or top-level frame name, kebab-cased; fall back to `figma-<short-id>` where `<short-id>` is the first 8 chars of the file key. Preserve any variant suffix the caller supplied (e.g. `water-tracker-mobile-dark`) exactly — that means the caller is producing one of several design docs.

## Output

Write the design doc to `docs/design/<base-name>.md`.

## Return

Reply with:

- The file path written.
- A 2–3 sentence summary (component count, color palette size, layout system used).

**Do not echo the design content** — the file is the artifact. The caller's main context only needs the path and the summary; bloating the return defeats the point of running in a forked context.

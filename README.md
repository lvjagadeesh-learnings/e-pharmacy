# Gen-e2 Project Setup

Reusable Copilot guidance for the Gen-e2 delivery workflow, and the home of the e-Pharmacy app built with it.

Copyright © 2026 Palo IT.

## Contents

- `.github/skills/` — workflow skills for discovery, planning, implementation, validation, and release.
- `.github/agents/` — specialist agents for decision-making, delivery, discovery, planning, quality, release, and agent authoring.
- `.github/extensions/gene2-project-setup/extension.mjs` — Copilot CLI extension exposing the `gene2_workflow` tool.
- `.github/copilot-instructions.md` — repository conventions (stack, layout, testing, Definition of Done) for the e-Pharmacy app.
- `frontend/` — e-Pharmacy frontend: React 19 + Vite (JavaScript).
- `backend/` — e-Pharmacy backend: .NET 10, Clean/DDD layered solution (`EPharmacy.sln`).
- `docs/stories/`, `docs/implementation-plans/`, `docs/definition-of-done.md` — Gen-e2 planning artifacts for the app.

## e-Pharmacy app

This repository's product code (`frontend/`, `backend/`) is developed exclusively through the Gen-e2 workflow. See `.github/copilot-instructions.md` for the full stack and conventions reference, and `docs/stories/` / `docs/implementation-plans/` for the work delivered so far.

### Run the frontend

```bash
cd frontend
npm install
npm run dev      # start the Vite dev server
npm run build    # production build
npm run lint     # ESLint
npm run test     # Vitest + React Testing Library
```

### Run the backend

```bash
cd backend
dotnet build                # build the whole solution
dotnet test                 # run all xUnit test projects
dotnet run --project src/EPharmacy.Api   # start the API (SQLite dev database is created/migrated automatically)
```

Once running, `GET /health` returns `200 { "status": "healthy", "checkedAtUtc": "..." }`.

## Use in a repository

Copy or commit the `.github/skills` and `.github/agents` directories into the target repository. Copilot will discover them automatically from the repository's `.github` directory.

## Use in Copilot CLI

The extension is project-scoped. From this repository, start Copilot CLI and reload extensions if needed:

```text
/extensions_reload
```

The extension contributes `gene2_workflow`, which accepts an optional phase:

- `discover`
- `decide`
- `plan`
- `implement`
- `validate`
- `release`

For user-scoped installation, copy `.github/extensions/gene2-project-setup/` into the personal Copilot extensions directory, or scaffold a user extension with the CLI extension manager. There is currently no VSIX-style installer; a directory or archive is the distribution unit.

## Validate locally

No third-party dependencies are required:

```bash
npm test
```

The check validates required metadata, agent and skill structure, and the extension entry point. This validates the Gen-e2 scaffold only; it does not build or test `frontend/`/`backend/` (see above for those).

## Contribution expectations

Keep skills and agents focused on one workflow responsibility, preserve YAML front matter, avoid repository-specific secrets, and update the validation checks when adding new required structure.

// Extension: gene2-project-setup
// Expose this repository's story-to-delivery workflow to Copilot CLI

import { joinSession } from "@github/copilot-sdk/extension";

await joinSession({
    tools: [
        {
            name: "gene2_workflow",
            description:
                "Show the Gen-e2 project workflow, available specialist agents, and the recommended next step for a requested phase.",
            parameters: {
                type: "object",
                properties: {
                    phase: {
                        type: "string",
                        enum: [
                            "discover",
                            "decide",
                            "plan",
                            "implement",
                            "validate",
                            "release",
                        ],
                        description:
                            "Optional workflow phase. If omitted, returns the complete workflow.",
                    },
                },
                additionalProperties: false,
            },
            handler: async ({ phase }) => {
                const workflow = [
                    {
                        phase: "discover",
                        purpose: "Extract product, design, domain, or research inputs.",
                        skill: "discovery-agent / generate-stories",
                    },
                    {
                        phase: "decide",
                        purpose: "Resolve ambiguity and make implementation-ready decisions.",
                        skill: "decision-agent / grilling",
                    },
                    {
                        phase: "plan",
                        purpose: "Create a codebase-aware implementation plan and task sequence.",
                        skill: "planning-agent / implementation-plan",
                    },
                    {
                        phase: "implement",
                        purpose: "Execute the approved plan with repository conventions and tests.",
                        skill: "delivery-agent / execute-plan",
                    },
                    {
                        phase: "validate",
                        purpose: "Run targeted tests, diagnose failures, and review the result.",
                        skill: "quality-agent / testing-strategy",
                    },
                    {
                        phase: "release",
                        purpose: "Prepare the completed work for handoff, commit, and pull request.",
                        skill: "release-agent / commit-push-pr",
                    },
                ];

                const selected = phase
                    ? workflow.filter((step) => step.phase === phase)
                    : workflow;

                return JSON.stringify(
                    {
                        repository: "Gen-e2 Project Setup",
                        nextStep: phase
                            ? selected[0] ?? {
                                  error: `Unknown phase: ${phase}`,
                              }
                            : workflow[0],
                        workflow: selected,
                        conventions: [
                            "Prefer the custom specialist agent when one matches the task.",
                            "Use skills for the workflow stage and agents for role-specific execution.",
                            "Keep changes surgical and validate them before release.",
                        ],
                    },
                    null,
                    2,
                );
            },
        },
    ],
});

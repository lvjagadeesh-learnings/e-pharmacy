import { access, readdir, readFile } from "node:fs/promises";
import { join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const root = resolve(fileURLToPath(new URL("..", import.meta.url)));
const errors = [];

async function exists(path) {
  try {
    await access(path);
    return true;
  } catch {
    return false;
  }
}

function requireFile(relativePath) {
  return exists(join(root, relativePath)).then((present) => {
    if (!present) errors.push(`Missing required file: ${relativePath}`);
  });
}

await Promise.all([
  requireFile("README.md"),
  requireFile("package.json"),
  requireFile(".github/extensions/gene2-project-setup/extension.mjs"),
]);

const agentsDir = join(root, ".github/agents");
const skillsDir = join(root, ".github/skills");

for (const [directory, suffix, label] of [
  [agentsDir, ".agent.md", "agent"],
  [skillsDir, "/SKILL.md", "skill"],
]) {
  if (!(await exists(directory))) {
    errors.push(`Missing ${label} directory: ${directory}`);
    continue;
  }

  const entries = await readdir(directory, { withFileTypes: true });
  const candidates = entries.filter((entry) =>
    label === "agent" ? entry.isFile() : entry.isDirectory(),
  );

  if (candidates.length === 0) errors.push(`No ${label}s found in ${directory}`);

  for (const entry of candidates) {
    const relativePath = label === "agent"
      ? `.github/agents/${entry.name}`
      : `.github/skills/${entry.name}/SKILL.md`;
    if (suffix === "/SKILL.md" && !(await exists(join(root, relativePath)))) {
      errors.push(`Skill is missing SKILL.md: ${relativePath}`);
    }
    const content = await readFile(join(root, relativePath), "utf8");
    if (!content.startsWith("---")) {
      errors.push(`${relativePath} must begin with YAML front matter`);
    }
  }
}

const extensionPath = join(root, ".github/extensions/gene2-project-setup/extension.mjs");
if (await exists(extensionPath)) {
  const extension = await readFile(extensionPath, "utf8");
  for (const requiredText of [
    'from "@github/copilot-sdk/extension"',
    'joinSession({',
    'name: "gene2_workflow"',
  ]) {
    if (!extension.includes(requiredText)) {
      errors.push(`Extension is missing expected content: ${requiredText}`);
    }
  }
}

if (errors.length > 0) {
  console.error(errors.map((error) => `✗ ${error}`).join("\n"));
  process.exitCode = 1;
} else {
  console.log("Repository validation passed.");
}

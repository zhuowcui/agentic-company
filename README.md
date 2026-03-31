# Agentic Company

> A multi-layered spec-driven platform for running companies with AI agents.

## What is this?

**Agentic Company** takes the ideas from [GitHub's Spec Kit](https://github.com/github/spec-kit) — spec-driven development with core principles, specifications, plans, and tasks — and extends them into a **hierarchical organizational management platform**.

In Spec Kit, a single repository has a constitution (core principles) that guides spec → plan → tasks → implementation. **Agentic Company** applies the same pattern across every layer of a company:

```
CEO Mission Statement          ← Top-level principles & spec
  └─ VP Org Strategy           ← Inherits CEO principles + adds own
      └─ Squad Objectives      ← Inherits VP principles + adds own
          └─ Team Sprint Plan  ← Inherits squad principles + adds own
              └─ Dev Task      ← Code-level implementation
```

**The key insight:** A task at one level becomes a spec at the next level down. When a VP says "Build an authentication service," that task cascades into a full spec → plan → tasks cycle for the team responsible. This drill-down continues from mission statement to merge request.

## Core Concepts

| Concept | Description |
|---|---|
| **Node** | Any organizational unit (company, org, squad, team, project) forming a hierarchy |
| **Principles** | The constitution for a node — core rules and values that govern all work. Inherited from parent nodes. |
| **Spec** | What needs to be done and why, at that node's level of abstraction |
| **Plan** | How to achieve the spec — strategic at upper levels, technical at lower levels |
| **Task** | An actionable work item. Can be assigned to humans or AI agents. Can cascade into a child spec. |
| **Agent** | An AI assistant that helps author principles/specs/plans or executes tasks, with full organizational context |

## Tech Stack

- **Frontend:** React, TypeScript, Vite, TanStack Router/Query, Zod, Tailwind CSS
- **Backend:** ASP.NET Core 10 (C#) REST API
- **Database:** PostgreSQL
- **AI Integration:** Agent-agnostic (OpenAI, Claude, Gemini, etc.)

## Getting Started

> 🚧 Under active development. Setup instructions coming soon.

## Documentation

- [Vision](docs/vision.md) — Why this project exists and where it's going
- [Architecture](docs/architecture.md) — System design and technical decisions
- [User Stories](docs/user-stories.md) — Detailed user stories organized by epic
- [Glossary](docs/glossary.md) — Terminology and definitions

## License

TBD

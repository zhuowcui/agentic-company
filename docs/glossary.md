# Glossary

## Core Concepts

### Node
An organizational unit in the hierarchy. Nodes form a tree structure where each node can have child nodes. Node types include:
- **Company** — The root-level organizational unit
- **Organization (Org)** — A major division within a company (e.g., "Engineering", "Product")
- **Squad** — A cross-functional group focused on a domain (e.g., "Payments Squad")
- **Team** — A small group of people working together (e.g., "Backend Team")
- **Project** — A specific initiative or codebase (e.g., "Auth Service v2")

### Principles (Constitution)
A set of core rules, values, and guidelines that govern all work within a node. Inspired by SpecKit's `constitution.md`. Principles are:
- **Inherited** — Automatically flow from parent nodes to children
- **Additive** — Child nodes can add their own principles
- **Overridable** — Child nodes can override specific parent principles (with flagging)

### Effective Principles
The resolved set of principles for a node after merging all inherited principles from ancestors with the node's local principles. This is the complete set of rules that govern work at that node.

### Spec (Specification)
A document describing **what** needs to be done and **why** at a given node's level of abstraction. A VP's spec might be "Expand into the Japanese market," while a developer's spec might be "Implement i18n string extraction for React components." Follows SpecKit methodology: focus on outcomes, not implementation details.

### Plan
A document describing **how** to achieve a spec. Plans are linked to a specific spec and constrained by effective principles. Plan types:
- **Strategic Plan** — High-level; focuses on delegation, timelines, resource allocation. Used by upper-level nodes.
- **Technical Plan** — Low-level; focuses on implementation details, architecture, technology choices. Used by lower-level nodes.

### Task
An actionable work item derived from a plan. Tasks can be:
- **Assigned** to humans or AI agents
- **Cascaded** to a child node, where the task becomes a new spec (see Task Cascade)
- **Dependent** on other tasks (ordering/blocking)

### Task Cascade
The core mechanism of Agentic Company. When a task at one level targets a child node, it **spawns a new spec** at that child node. The child node then goes through its own spec → plan → tasks cycle. This creates a recursive drill-down from mission statement to code commit.

```
VP Task: "Build auth service" 
    → cascades to → 
        Team Spec: "Build auth service"
            → Team Plan → Team Tasks
                → some tasks may cascade further to sub-teams
```

### Agent
An AI assistant that operates within the platform. Agents can:
- **Author** — Help write principles, specs, plans
- **Plan** — Generate plans from specs
- **Execute** — Carry out tasks (write code, generate documents)
- **Review** — Validate specs against principles, check for conflicts

Agents are provider-agnostic (OpenAI, Claude, Gemini, etc.) and always operate with the full context of the node they're assisting.

### Agent Provider
An implementation of the AI agent interface for a specific AI service (e.g., OpenAI's GPT, Anthropic's Claude, Google's Gemini). Providers are pluggable and configurable at the platform level.

## Workflow Terms

### Spec-Driven Development (SDD)
The methodology (from GitHub's Spec Kit) where specifications are the primary artifact and source of truth. Code and other outputs serve the spec, not the other way around. In Agentic Company, this applies at every organizational level, not just code.

### Level-Adaptive Behavior
The platform's ability to present different interfaces and behaviors based on where in the hierarchy a user is working. A CEO sees strategic dashboards; a developer sees code tasks. Same platform, different lenses.

### Status Roll-Up
The aggregation of child node statuses to provide a summary at the parent level. For example, a VP sees "Authentication Service: 60% complete" based on the underlying team's task completion.

### Principle Conflict
When a child node's local principle contradicts an inherited parent principle. The platform detects these and surfaces them as warnings for resolution.

## Technical Terms

### `ltree`
A PostgreSQL extension for storing and querying hierarchical label paths. Used to efficiently query ancestor/descendant relationships in the node tree (e.g., "find all descendants of node X").

### Clean Architecture
The backend architectural pattern where the domain (Core) has no external dependencies, and infrastructure concerns (database, AI providers) are implemented behind interfaces. Ensures the domain model remains pure and testable.

### Tenant
A logical isolation boundary for multi-tenancy. Currently single-tenant, but the schema includes `tenant_id` columns for future multi-tenant support where multiple companies share one deployment.

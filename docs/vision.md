# Vision

## The Problem

Companies of all sizes share a common structural challenge: **alignment across layers**. A CEO sets a mission, VPs translate it into org strategies, squads break it into objectives, teams plan sprints, and developers write code. At every hand-off, context is lost, intent drifts, and the connection between a mission statement and a line of code becomes invisible.

Traditional tools address individual layers — Jira for tasks, Confluence for docs, Notion for wikis, GitHub for code — but none capture the **vertical causality** from mission to merge request. The result:

- Teams build features that don't serve the mission
- VPs can't see if their strategy is actually being executed
- Developers don't understand *why* they're building what they're building
- Organizational restructuring breaks all the implicit knowledge chains

## The Insight

GitHub's [Spec Kit](https://github.com/github/spec-kit) proved that **spec-driven development** works for a single repository:

1. Define **core principles** (constitution) that govern all decisions
2. Write a **spec** describing what and why
3. Generate a **plan** describing how
4. Break the plan into **tasks**
5. Execute tasks (with AI assistance)

The breakthrough insight: **this same pattern applies at every organizational level**, not just code repos. A CEO's mission statement *is* a constitution. A VP's quarterly strategy *is* a spec. An org restructuring initiative *is* a plan. Delegating to a team *is* creating tasks — which become specs for that team.

## The Solution: Agentic Company

A platform where every organizational unit (we call them **Nodes**) participates in the same spec-driven workflow:

### Multi-Layer Cascade

```
┌─────────────────────────────────────────────────────────┐
│ COMPANY NODE                                            │
│ Principles: "Customer-first, move fast, be transparent" │
│ Spec: "Become the #1 developer platform in APAC"       │
│ Plan: "Launch in 3 markets, partner with 5 orgs"        │
│ Tasks:                                                  │
│   ├─ "Launch Japan market" ──────────► [ORG: APAC]      │
│   ├─ "Build Japanese localization" ──► [TEAM: i18n]     │
│   └─ "Partner with LINE" ───────────► [SQUAD: BD]       │
└─────────────────────────────────────────────────────────┘
         │                                    
         ▼ Task becomes a Spec at child node  
┌─────────────────────────────────────────────────────────┐
│ ORG: APAC                                               │
│ Principles: (inherits company) + "Respect local norms"  │
│ Spec: "Launch Japan market" (from parent task)          │
│ Plan: "Localize product, hire Tokyo team, ..."          │
│ Tasks:                                                  │
│   ├─ "Localize all user-facing strings" ──► [TEAM: i18n]│
│   ├─ "Set up Tokyo office" ─────────────► [SQUAD: Ops]  │
│   └─ "Launch marketing campaign" ───────► [TEAM: Mktg]  │
└─────────────────────────────────────────────────────────┘
         │
         ▼ ... continues drilling down ...
```

### AI Agents at Every Layer

At each node, AI agents can:

- **Author** — Help draft principles, specs, and plans with organizational context
- **Plan** — Break specs into strategic or technical plans
- **Execute** — Carry out tasks (from writing strategy docs to generating code)
- **Monitor** — Track progress and surface blockers across the hierarchy

The agent always operates with full context: the node's effective principles (inherited + local), parent specs, and organizational position.

### Level-Adaptive Behavior

The same platform serves every role differently:

| Role | Sees | Works With |
|---|---|---|
| CEO | Mission → org completion rates | High-level specs, strategic plans |
| VP | Org strategy → squad progress | Org specs, delegation tasks |
| Team Lead | Team objectives → task status | Technical specs, sprint plans |
| Developer | Assigned tasks → code output | Code-level tasks, AI pair programming |

## Where We're Going

### Phase 1: Foundation
Core hierarchy, principles with inheritance, basic CRUD.

### Phase 2: Spec Lifecycle
Full spec → plan workflow with AI-assisted authoring.

### Phase 3: The Cascade
The differentiating feature — tasks spawning child specs, status roll-up.

### Phase 4: Agent Integration
Agent-agnostic AI assistance at every level.

### Phase 5: Polish
Dashboards, auth, audit trails, and organizational insights.

## Guiding Principles for This Project

1. **Specs are truth** — The platform itself follows spec-driven development. Our own specs govern our implementation.
2. **Hierarchy is a tree, not a cage** — The org structure enables context flow, not bureaucracy.
3. **Agents augment, humans decide** — AI helps author and execute, but humans own principles and approve specs.
4. **Inheritance over duplication** — Principles flow down; teams don't re-declare what's already established above them.
5. **Level-appropriate abstraction** — A CEO and a developer use the same platform but see different things.

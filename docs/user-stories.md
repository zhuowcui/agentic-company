# User Stories

## Epic 1: Organizational Hierarchy

### US-1.1 — Create an Organization Node

> **As a** company founder,
> **I want to** create a top-level organization node,
> **so that** I can represent my company and define its mission as core principles.

**Acceptance Criteria:**
- User can create a root-level node with a name, description, and type (Company)
- The node appears in the organizational tree
- The node is ready to have principles defined on it

---

### US-1.2 — Create Child Nodes

> **As a** VP,
> **I want to** create child nodes (orgs, squads, teams) under my organization,
> **so that** I can model the company's management structure.

**Acceptance Criteria:**
- User can create a child node under any existing node they own
- Available node types: Organization, Squad, Team, Project
- The child automatically appears in the tree under its parent
- The child inherits its parent's principles

---

### US-1.3 — View Organizational Tree

> **As any** member,
> **I want to** see a visual tree of the entire organizational hierarchy,
> **so that** I understand how teams relate to each other.

**Acceptance Criteria:**
- Interactive tree visualization showing all nodes the user has access to
- Nodes are expandable/collapsible
- Clicking a node navigates to its detail view
- Node type is visually distinguishable (icon or color)

---

### US-1.4 — Move/Restructure Nodes

> **As an** admin,
> **I want to** move nodes to different parents,
> **so that** the hierarchy reflects organizational changes.

**Acceptance Criteria:**
- Drag-and-drop or explicit "move" action to re-parent a node
- All children move with the parent
- Principles are re-resolved after the move
- A confirmation dialog warns about principle changes

---

## Epic 2: Principles (Constitution) System

### US-2.1 — Define Node Principles

> **As a** node owner,
> **I want to** define core principles (constitution) for my node,
> **so that** all work under this node is guided by these rules.

**Acceptance Criteria:**
- User can add titled principles with rich-text/markdown content
- Principles are ordered (priority matters)
- Principles are immediately visible to all child nodes

---

### US-2.2 — Inherit Parent Principles

> **As a** team lead,
> **I want to** my team's principles to automatically inherit from parent nodes,
> **so that** we stay aligned with higher-level goals without duplicating them.

**Acceptance Criteria:**
- When viewing a node's principles, inherited ones are shown (visually distinct)
- Inherited principles are read-only at the child level
- The source node for each inherited principle is displayed

---

### US-2.3 — Override/Extend Principles

> **As a** squad lead,
> **I want to** add squad-specific principles that extend (but don't contradict) inherited ones,
> **so that** my squad has focused guidance.

**Acceptance Criteria:**
- User can add local principles that appear alongside inherited ones
- User can mark a local principle as an override of a specific inherited one
- Overrides are visually flagged

---

### US-2.4 — View Effective Principles

> **As any** member,
> **I want to** see the "effective principles" for a node (merged inherited + local),
> **so that** I know all the rules that govern my work.

**Acceptance Criteria:**
- A single view showing the resolved/merged set of principles
- Each principle shows its origin (which node it comes from)
- Overridden principles show both the original and the override

---

### US-2.5 — Principle Conflict Detection

> **As a** node owner,
> **I want** the system to warn me if my local principles conflict with inherited ones,
> **so that** I can resolve contradictions.

**Acceptance Criteria:**
- When adding/editing a local principle, an AI-powered check compares it against inherited principles
- Conflicts are surfaced as warnings (not blocking)
- The user can acknowledge or resolve each conflict

---

## Epic 3: Spec Lifecycle

### US-3.1 — Create a Spec

> **As a** node owner,
> **I want to** create a spec that describes what I want to achieve and why,
> **so that** work at my level is clearly defined.

**Acceptance Criteria:**
- Spec creation form with title and markdown content
- Spec follows the SpecKit methodology: focus on what/why, not how
- Spec is linked to exactly one node
- Initial status is "Draft"

---

### US-3.2 — AI-Assisted Spec Authoring

> **As a** VP,
> **I want** an AI agent to help me draft my org's spec by asking clarifying questions and identifying ambiguities,
> **so that** my spec is complete and well-structured.

**Acceptance Criteria:**
- User can invoke an AI assistant from the spec editor
- The AI has context: node position, effective principles, parent specs
- The AI asks clarifying questions and suggests improvements
- The AI marks ambiguities with `[NEEDS CLARIFICATION]` markers
- The user retains final editorial control

---

### US-3.3 — Spec Versioning

> **As a** node owner,
> **I want** specs to be versioned,
> **so that** I can see how they evolved over time and roll back if needed.

**Acceptance Criteria:**
- Every save creates a new version
- Version history is viewable with diffs between versions
- User can restore a previous version (creates a new version with old content)

---

### US-3.4 — Spec Review & Approval

> **As a** parent node owner,
> **I want to** review and approve specs from child nodes,
> **so that** they align with my level's goals.

**Acceptance Criteria:**
- Spec can be submitted for review (status: "In Review")
- Parent node owners receive a notification
- Reviewer can approve, request changes, or reject
- Approved specs move to "Approved" status

---

## Epic 4: Planning

### US-4.1 — Create a Plan from Spec

> **As a** node owner,
> **I want to** generate an implementation plan from my spec, guided by effective principles,
> **so that** there's a clear path to execution.

**Acceptance Criteria:**
- Plan is always linked to a specific spec
- Plan content is markdown with structured sections
- Plan respects effective principles as constraints

---

### US-4.2 — AI-Assisted Planning

> **As a** team lead,
> **I want** an AI agent to help generate my plan, taking into account the spec, effective principles, and constraints from parent nodes.

**Acceptance Criteria:**
- User can invoke AI to draft a plan from a spec
- AI receives full context: spec content, effective principles, parent specs/plans
- AI generates structured plan with rationale for decisions
- User can iterate with the AI to refine the plan

---

### US-4.3 — Strategic vs Technical Plans

> **As a** VP,
> **I want** my plan to be strategic (delegating to sub-orgs) rather than technical, while a developer's plan should be technical. The plan format should adapt to the hierarchy level.

**Acceptance Criteria:**
- Plan type is automatically suggested based on node depth/type
- Strategic plans focus on delegation, timelines, and dependencies between child nodes
- Technical plans focus on implementation details, technology choices, and code structure
- Both types share a common base structure

---

## Epic 5: Task Cascade (The Core Feature)

### US-5.1 — Generate Tasks from Plan

> **As a** node owner,
> **I want to** break my plan into actionable tasks,
> **so that** work can be distributed.

**Acceptance Criteria:**
- Tasks can be generated from a plan (manually or AI-assisted)
- Each task has a title, description, status, and optional assignee
- Tasks have an explicit order and can have dependencies on other tasks

---

### US-5.2 — Task-to-Spec Cascade ⭐

> **As a** VP,
> **when** I create a task like "Team Alpha: Build the authentication service,"
> **I want** that task to automatically become a new spec at Team Alpha's node level, triggering their own plan → tasks cycle.

**Acceptance Criteria:**
- When creating/editing a task, user can set a `target_node` (a child/descendant node)
- Clicking "Cascade" creates a new Spec on the target node with the task description as seed content
- The new spec links back to the parent task (`source_task_id`)
- The parent task links forward to the spawned spec (`spawned_spec_id`)
- The parent task status reflects the child spec's lifecycle (e.g., "Cascaded — In Progress")
- Status changes on the child spec propagate back to the parent task

---

### US-5.3 — Track Task Status Up the Chain

> **As a** VP,
> **I want to** see the status of cascaded tasks roll up to my dashboard,
> **so I** know how my org is progressing without micromanaging.

**Acceptance Criteria:**
- Cascaded tasks show a summary of the child spec's progress
- A percentage or status indicator (e.g., "3/7 tasks complete")
- Drill-down link from parent task to child spec

---

### US-5.4 — Task Assignment

> **As a** team lead,
> **I want to** assign tasks to team members (human or AI agent),
> **so that** work is distributed and tracked.

**Acceptance Criteria:**
- Tasks can be assigned to users or AI agent profiles
- Assignment notification is sent
- Task board shows assigned/unassigned tasks

---

### US-5.5 — Task Dependencies

> **As a** planner,
> **I want to** define dependencies between tasks,
> **so that** the system can identify blockers and suggest execution order.

**Acceptance Criteria:**
- Tasks can declare "depends on" relationships to other tasks
- Blocked tasks are visually indicated
- Dependency violations (e.g., circular) are prevented

---

## Epic 6: AI Agent Integration

### US-6.1 — Configure Agent Providers

> **As an** admin,
> **I want to** configure AI agent providers (OpenAI, Claude, Gemini, etc.) at the platform level,
> **so that** agents are available across the organization.

**Acceptance Criteria:**
- Settings page to add/configure agent providers
- Each provider requires API key and optional configuration
- Providers can be enabled/disabled
- A default provider can be set

---

### US-6.2 — Agent-Assisted Authoring at Every Level

> **As any** node owner,
> **I want to** invoke an AI agent to help me author principles, specs, plans, or tasks,
> **with** the agent having full context of my node's position in the hierarchy and all inherited principles.

**Acceptance Criteria:**
- An "Ask AI" button is available in all content editors
- The agent receives: node path, effective principles, current content, parent context
- The agent can suggest, rewrite, or generate content
- Chat-style interaction for iterative refinement

---

### US-6.3 — Agent Task Execution

> **As a** developer,
> **I want to** assign a coding task to an AI agent that will execute it,
> **so that** implementation follows the effective principles and spec.

**Acceptance Criteria:**
- Tasks at code-level nodes can be assigned to an AI agent
- The agent receives: task description, spec, plan, effective principles
- The agent produces output (code, docs, tests) attached to the task
- The output can be reviewed and accepted/rejected

---

### US-6.4 — Agent Context Injection

> **As the** platform,
> **when** an agent is invoked at any level,
> **I want to** automatically inject the effective principles, parent specs, and relevant context,
> **so that** the agent produces aligned output.

**Acceptance Criteria:**
- Context is assembled automatically based on the node being worked on
- Context includes: effective principles, node's spec, parent specs (summarized), plan constraints
- Context is formatted for the specific agent provider's optimal consumption
- Context size is managed (summarization for deep hierarchies)

---

## Epic 7: Dashboard & Visibility

### US-7.1 — Node Dashboard

> **As a** node owner,
> **I want** a dashboard showing my node's specs, plans, tasks, and child node statuses,
> **so I** have a single view of my area of responsibility.

**Acceptance Criteria:**
- Overview cards: active specs, plans in progress, task completion rate
- Child node status summary (green/yellow/red)
- Recent activity feed
- Quick actions (create spec, view tasks, etc.)

---

### US-7.2 — Org-Wide Progress View

> **As a** CEO,
> **I want** a high-level view of how mission objectives cascade through the organization and their completion status.

**Acceptance Criteria:**
- Visualization showing the cascade from top-level specs through the hierarchy
- Completion percentages at each level
- Ability to drill down into any node
- Bottleneck/blocker highlighting

---

### US-7.3 — Audit Trail

> **As a** compliance officer,
> **I want to** see who created/modified principles, specs, plans, and tasks, with timestamps and diffs.

**Acceptance Criteria:**
- All create/update/delete actions are logged
- Audit log shows: who, what, when, and the diff
- Filterable by node, user, entity type, and date range

---

## Epic 8: User & Access Management

### US-8.1 — User Roles per Node

> **As an** admin,
> **I want to** assign roles (owner, contributor, viewer) per node,
> **so that** access is scoped to organizational structure.

**Acceptance Criteria:**
- Roles: Owner (full control), Contributor (create/edit content), Viewer (read-only)
- Roles are per-node and inherit downward (an Org owner is implicitly an owner of child nodes)
- Role assignments are manageable from the node settings

---

### US-8.2 — Authentication

> **As a** user,
> **I want to** log in securely,
> **so that** my identity is verified before accessing the platform.

**Acceptance Criteria:**
- OAuth 2.0 / OpenID Connect authentication
- Support for at least one identity provider (e.g., GitHub, Google, Microsoft)
- Session management with secure tokens
- Logout functionality

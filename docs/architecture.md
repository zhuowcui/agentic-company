# Architecture

## Overview

Agentic Company is a monorepo containing a **React SPA frontend** and an **ASP.NET Core 10 backend API**, backed by **PostgreSQL**. The system models hierarchical organizational structures where each node (company, org, squad, team, project) has its own spec-driven lifecycle.

## System Architecture

```
┌────────────────────────────────┐
│         Browser (SPA)          │
│  React + Vite + TanStack       │
│  + Tailwind + Zod              │
└──────────┬─────────────────────┘
           │ HTTPS / REST
           ▼
┌────────────────────────────────┐
│      ASP.NET Core 10 API       │
│  ┌──────────┐ ┌──────────────┐ │
│  │Controllers│ │  Middleware   │ │
│  └────┬─────┘ │(Auth, CORS,  │ │
│       │       │ Logging)     │ │
│       ▼       └──────────────┘ │
│  ┌──────────────────────────┐  │
│  │     Application Layer     │  │
│  │  (Services, Use Cases)    │  │
│  └────┬─────────────────────┘  │
│       │                        │
│  ┌────▼─────┐ ┌─────────────┐ │
│  │  Domain   │ │   Agent     │ │
│  │  (Core)   │ │  Providers  │ │
│  └────┬─────┘ └──────┬──────┘ │
│       │               │        │
│  ┌────▼───────────────▼─────┐  │
│  │    Infrastructure Layer   │  │
│  │  (EF Core, Agent Clients) │  │
│  └────┬─────────────────────┘  │
└───────┼────────────────────────┘
        │
        ▼
┌────────────────────────────────┐
│        PostgreSQL               │
│  (Nodes, Principles, Specs,    │
│   Plans, Tasks, Users)         │
└────────────────────────────────┘
```

## Backend Architecture (Clean Architecture)

### Project Structure

```
src/backend/
├── AgenticCompany.Api/              # Web API host
│   ├── Controllers/                 # REST endpoints
│   ├── Middleware/                   # Auth, error handling, logging
│   ├── Filters/                     # Validation, authorization filters
│   └── Program.cs                   # App configuration
│
├── AgenticCompany.Core/             # Domain layer (no dependencies)
│   ├── Entities/                    # Node, Principles, Spec, Plan, Task, User
│   ├── Enums/                       # NodeType, TaskStatus, SpecStatus, etc.
│   ├── Interfaces/                  # INodeRepository, IAgentProvider, etc.
│   ├── Services/                    # Domain services (PrincipleInheritance, etc.)
│   └── ValueObjects/                # NodePath, PrincipleSet, etc.
│
├── AgenticCompany.Infrastructure/   # Implementation layer
│   ├── Data/                        # EF Core DbContext, configurations
│   ├── Repositories/                # Repository implementations
│   ├── Agents/                      # AI provider implementations
│   │   ├── OpenAiAgentProvider.cs
│   │   ├── ClaudeAgentProvider.cs
│   │   └── GeminiAgentProvider.cs
│   └── Migrations/                  # EF Core migrations
│
└── AgenticCompany.Tests/            # Test projects
    ├── Unit/
    ├── Integration/
    └── Architecture/                # ArchUnit-style dependency tests
```

### Key Design Patterns

- **Clean Architecture** — Domain (Core) has zero external dependencies; Infrastructure implements interfaces defined in Core.
- **Repository Pattern** — All data access goes through repository interfaces.
- **Strategy Pattern** — AI agent providers implement `IAgentProvider`, swappable at runtime.
- **Specification Pattern** — Query filtering through composable specifications.

## Frontend Architecture

```
src/frontend/
├── src/
│   ├── api/                    # TanStack Query hooks + Zod schemas
│   │   ├── nodes.ts            # Node API calls & query keys
│   │   ├── principles.ts
│   │   ├── specs.ts
│   │   ├── plans.ts
│   │   ├── tasks.ts
│   │   └── schemas/            # Zod validation schemas (shared types)
│   │
│   ├── components/             # Reusable UI components
│   │   ├── ui/                 # Shadcn-style primitives
│   │   ├── layout/             # Shell, sidebar, nav
│   │   └── org-tree/           # Tree visualization component
│   │
│   ├── features/               # Feature-based modules
│   │   ├── nodes/              # Node CRUD views
│   │   ├── principles/         # Principles editor
│   │   ├── specs/              # Spec authoring & viewing
│   │   ├── plans/              # Plan authoring & viewing
│   │   ├── tasks/              # Task board & cascade view
│   │   ├── agents/             # Agent configuration & invocation
│   │   └── dashboard/          # Per-node & org-wide dashboards
│   │
│   ├── lib/                    # Utilities, helpers
│   ├── routes/                 # TanStack Router route definitions
│   └── App.tsx
│
├── index.html
├── package.json
├── tailwind.config.ts
├── tsconfig.json
└── vite.config.ts
```

## Data Model

### Core Entities

```
┌─────────────┐       ┌──────────────┐
│    Node      │──1:N──│  Principles  │
│─────────────│       │──────────────│
│ id (uuid)    │       │ id (uuid)    │
│ tenant_id    │       │ node_id (fk) │
│ parent_id    │       │ title        │
│ name         │       │ content      │
│ type (enum)  │       │ order        │
│ description  │       │ is_override  │
│ path (ltree) │       │ created_at   │
│ depth        │       │ updated_at   │
│ created_at   │       └──────────────┘
│ updated_at   │
└──────┬──────┘
       │
       ├──1:N──┐
       │       ▼
       │  ┌──────────────┐      ┌──────────────┐
       │  │    Spec       │──1:N─│  SpecVersion │
       │  │──────────────│      │──────────────│
       │  │ id (uuid)     │      │ id (uuid)    │
       │  │ node_id (fk)  │      │ spec_id (fk) │
       │  │ title         │      │ version      │
       │  │ status (enum) │      │ content      │
       │  │ source_task_id│      │ created_by   │
       │  │ created_at    │      │ created_at   │
       │  └──────┬───────┘      └──────────────┘
       │         │
       │         ├──1:N──┐
       │         │       ▼
       │         │  ┌──────────────┐
       │         │  │    Plan       │
       │         │  │──────────────│
       │         │  │ id (uuid)     │
       │         │  │ spec_id (fk)  │
       │         │  │ content       │
       │         │  │ plan_type     │  (strategic | technical)
       │         │  │ status (enum) │
       │         │  │ created_at    │
       │         │  └──────┬───────┘
       │         │         │
       │         │         ├──1:N──┐
       │         │         │       ▼
       │         │         │  ┌─────────────────┐
       │         │         │  │     Task         │
       │         │         │  │─────────────────│
       │         │         │  │ id (uuid)        │
       │         │         │  │ plan_id (fk)     │
       │         │         │  │ title            │
       │         │         │  │ description      │
       │         │         │  │ status (enum)    │
       │         │         │  │ assigned_to      │  (user or agent)
       │         │         │  │ target_node_id   │  (for cascade)
       │         │         │  │ spawned_spec_id  │  (link to child spec)
       │         │         │  │ order            │
       │         │         │  │ created_at       │
       │         │         │  └─────────────────┘
```

### Key Relationships

- **Node** has a self-referential `parent_id` forming the organizational tree
- **Node.path** uses PostgreSQL `ltree` for efficient ancestor/descendant queries
- **Spec.source_task_id** links a spec back to the parent-level task that spawned it (the cascade)
- **Task.target_node_id** indicates which child node should receive this task as a new spec
- **Task.spawned_spec_id** links forward to the spec created from the cascade
- **SpecVersion** tracks content history for each spec

### Principle Inheritance Resolution

```
Effective Principles for Node X =
  Union of all principles from root → X,
  where child principles with is_override=true replace same-titled parent principles,
  and conflicts are flagged.
```

Implementation: recursive CTE query walking from the node to root, then merging in application code with conflict detection.

## API Design

RESTful API following these conventions:

```
# Nodes
GET    /api/nodes                    # List root nodes
GET    /api/nodes/{id}               # Get node with children
GET    /api/nodes/{id}/tree          # Get full subtree
POST   /api/nodes                    # Create node
PUT    /api/nodes/{id}               # Update node
DELETE /api/nodes/{id}               # Delete node (cascade?)
PATCH  /api/nodes/{id}/move          # Re-parent a node

# Principles
GET    /api/nodes/{id}/principles              # Local principles
GET    /api/nodes/{id}/principles/effective     # Inherited + local (resolved)
POST   /api/nodes/{id}/principles              # Add principle
PUT    /api/nodes/{id}/principles/{pid}        # Update principle
DELETE /api/nodes/{id}/principles/{pid}        # Remove principle

# Specs
GET    /api/nodes/{id}/specs                   # List specs for node
POST   /api/nodes/{id}/specs                   # Create spec
GET    /api/specs/{id}                         # Get spec with versions
PUT    /api/specs/{id}                         # Update spec (creates version)
POST   /api/specs/{id}/approve                 # Approve spec

# Plans
POST   /api/specs/{specId}/plans               # Create plan for spec
GET    /api/plans/{id}                         # Get plan
PUT    /api/plans/{id}                         # Update plan

# Tasks
POST   /api/plans/{planId}/tasks               # Generate tasks from plan
GET    /api/tasks/{id}                         # Get task
PUT    /api/tasks/{id}                         # Update task (status, assignment)
POST   /api/tasks/{id}/cascade                 # Cascade task to child spec

# Agent
POST   /api/agent/author                       # AI-assisted authoring
POST   /api/agent/execute                      # Execute task with AI
GET    /api/agent/providers                    # List configured providers
```

## Deployment

### Local Development

```yaml
# docker-compose.yml
services:
  postgres:
    image: postgres:17
    environment:
      POSTGRES_DB: agentic_company
      POSTGRES_USER: agentic
      POSTGRES_PASSWORD: dev_password
    ports:
      - "5432:5432"

  api:
    build: ./src/backend
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=agentic_company;Username=agentic;Password=dev_password
    ports:
      - "5000:8080"
    depends_on:
      - postgres

  frontend:
    build: ./src/frontend
    ports:
      - "3000:3000"
    depends_on:
      - api
```

### Future: Production

- Containerized deployment (Docker/Kubernetes)
- Managed PostgreSQL (Azure Database, AWS RDS, etc.)
- CDN for frontend static assets
- OAuth/OIDC identity provider integration

## Key Technical Decisions

| Decision | Choice | Rationale |
|---|---|---|
| Hierarchical storage | PostgreSQL `ltree` + adjacency list | Efficient ancestor/descendant queries, natural fit for org trees |
| Principle inheritance | Resolve at query time via recursive CTE | Avoids stale caches; principles change infrequently |
| Spec versioning | Separate SpecVersion table | Full content history, easy diff and rollback |
| Agent abstraction | `IAgentProvider` strategy pattern | Swap providers without changing business logic |
| Multi-tenancy prep | `tenant_id` on all root tables | Ready for multi-tenant without schema migration |
| Frontend state | TanStack Query | Server state management with caching, no Redux needed |
| Validation | Zod (frontend) + FluentValidation (backend) | Type-safe validation at both ends |

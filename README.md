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
|---------|-------------|
| **Node** | Any organizational unit (company, org, squad, team, project) forming a hierarchy |
| **Principles** | The constitution for a node — core rules and values that govern all work. Inherited from parent nodes with override support. |
| **Spec** | What needs to be done and why, at that node's level of abstraction. Versioned with approval workflow. |
| **Plan** | How to achieve the spec — strategic at upper levels, technical at lower levels |
| **Task** | An actionable work item. Can be assigned to humans or AI agents. **Can cascade into a child spec** — the core multi-layer mechanism. |
| **Agent** | An AI assistant that helps author principles/specs/plans or executes tasks, with full organizational context |

## Tech Stack

| Layer | Technology |
|-------|------------|
| **Frontend** | React 19, TypeScript, Vite, TanStack Query, Zod, Tailwind CSS v4 |
| **Backend** | ASP.NET Core 10 (C#), Clean Architecture, REST API |
| **Database** | PostgreSQL 17, Entity Framework Core (code-first migrations) |
| **AI** | Agent-agnostic — OpenAI (GPT-4o) and Anthropic (Claude Sonnet) providers included |
| **Infrastructure** | Docker Compose for local dev |

## Project Structure

```
agentic-company/
├── src/
│   ├── backend/
│   │   ├── AgenticCompany.Api/          # Controllers, DTOs, middleware, Program.cs
│   │   ├── AgenticCompany.Core/         # Domain entities, enums, interfaces, services
│   │   ├── AgenticCompany.Infrastructure/ # EF Core, repositories, agent providers
│   │   └── AgenticCompany.Tests/        # xUnit test project
│   └── frontend/
│       └── src/
│           ├── api/                     # API client, hooks (TanStack Query), Zod schemas
│           ├── components/              # Shared UI components
│           └── features/                # Feature modules (nodes, specs, plans, dashboard)
├── docs/                                # Vision, architecture, user stories, glossary
├── docker-compose.yml                   # PostgreSQL + API containers
└── agentic-company.sln                  # .NET solution file
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) (with npm)
- [PostgreSQL 17](https://www.postgresql.org/) (or use Docker)
- [Docker & Docker Compose](https://docs.docker.com/compose/) (optional — for containerized setup)

### Option 1: Docker Compose (recommended)

```bash
# Start PostgreSQL and the API
docker compose up -d

# Install frontend dependencies and start dev server
cd src/frontend
npm install
npm run dev
```

The API will be available at `http://localhost:5000` and the frontend at `http://localhost:5173`.

### Option 2: Local Development

**1. Start PostgreSQL**

```bash
# Using Docker for just the database
docker compose up -d postgres

# Or use a local PostgreSQL instance — update the connection string in
# src/backend/AgenticCompany.Api/appsettings.Development.json
```

**2. Run the backend**

```bash
cd src/backend/AgenticCompany.Api
dotnet ef database update        # Apply EF Core migrations
dotnet run                       # Starts on http://localhost:5000
```

**3. Run the frontend**

```bash
cd src/frontend
npm install
npm run dev                      # Starts on http://localhost:5173
```

### Configuration

Key settings in `appsettings.Development.json`:

| Setting | Description |
|---------|-------------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `Jwt:SigningKey` | JWT signing key (min 32 chars; no default in production) |
| `Agent:DefaultProvider` | AI provider: `openai`, `claude`, or `echo` (dev only) |
| `Agent:OpenAI:ApiKey` | OpenAI API key |
| `Agent:Claude:ApiKey` | Anthropic API key |
| `Cors:AllowedOrigins` | Allowed frontend origins |

## Architecture Highlights

### Authorization Model

Access control uses **path-based inheritance** on the organizational hierarchy:

- **Read access** is bidirectional — membership on an ancestor grants read on descendants, and membership on a descendant grants read on ancestors (for tree navigation).
- **Write access** inherits downward only, using **most-specific-wins** semantics — the deepest membership on the node's path determines the effective role. An explicit `Viewer` assignment on a child node overrides an `Owner` on a parent.
- **Roles**: `Owner` > `Admin` > `Member` > `Viewer` (per-node)

### Cascade — The Core Multi-Layer Mechanism

When a task at one level needs to be implemented by a lower team, it **cascades**: a new Spec is created on the target child node, linked back to the source task. This is wrapped in a database transaction with:

- Source/target membership validation
- Descendant relationship verification (target must be under source's node)
- Concurrent cascade protection (409 Conflict)
- Automatic status update and query cache invalidation

### Principle Inheritance

Principles flow down the hierarchy with override support. A child node can override a parent's principle by title. Resolution walks from the target node up to the root, applying overrides directionally (deeper levels override shallower).

### Security

- JWT authentication with PBKDF2 password hashing (`PasswordHasher<T>`)
- Per-IP rate limiting on auth endpoints (10 req/min)
- Per-user rate limiting on AI agent endpoints (20 req/min)
- All write DTOs validated with `[Required]` / `[MaxLength]` attributes
- Agent provider errors sanitized — details logged server-side, generic 502 to client
- `FOR UPDATE` row locks on critical concurrent operations (member removal, node moves)

## API Overview

All endpoints require JWT authentication except `/api/auth/login` and `/api/auth/register`.

| Resource | Endpoints |
|----------|-----------|
| **Auth** | `POST /api/auth/register`, `POST /api/auth/login` |
| **Nodes** | CRUD, tree view, move, member management |
| **Specs** | CRUD per node, versioning, approve workflow |
| **Plans** | CRUD per spec, status lifecycle |
| **Tasks** | CRUD per plan, status lifecycle, **cascade** |
| **Principles** | CRUD per node, effective (inherited) resolution |
| **Agent** | AI-assisted authoring for specs, plans, principles, tasks |
| **Dashboard** | Node stats, activity feed, org overview |

## Documentation

- [Vision](docs/vision.md) — Why this project exists and where it's going
- [Architecture](docs/architecture.md) — System design and technical decisions
- [User Stories](docs/user-stories.md) — Detailed user stories organized by epic
- [Glossary](docs/glossary.md) — Terminology and definitions

## License

TBD

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

IBS (Insurance Broker System) is a full-stack web application for managing insurance brokerage operations.

- **Backend**: ASP.NET Core 8 with C#, CQRS pattern using MediatR
- **Frontend**: React 19 + TypeScript + Vite
- **Database**: SQL Server 2022
- **Architecture**: Clean Architecture + Domain-Driven Design (DDD) with bounded contexts

## Build & Run Commands

### Infrastructure (Docker)
```bash
cd docker && docker-compose -f docker-compose.dev.yml up -d
```
Starts SQL Server (port 1433), Redis (6379), and Azurite (10000-10002).

### Database Migrations
```bash
# Apply migrations
dotnet ef database update --project src/IBS.Infrastructure/IBS.Infrastructure.csproj --startup-project src/IBS.Api/IBS.Api.csproj

# Create new migration
dotnet ef migrations add <MigrationName> --project src/IBS.Infrastructure/IBS.Infrastructure.csproj --startup-project src/IBS.Api/IBS.Api.csproj --output-dir Persistence/Migrations
```
After changes to the database update the docs/seed-data.sql and create migrations.

### Backend
```bash
cd src/IBS.Api
dotnet run --launch-profile https        # Run API (https://localhost:7001)
dotnet watch run --launch-profile https  # Run with hot reload
```

### Frontend
```bash
cd src/IBS.Web
npm install
npm run dev      # Dev server at http://localhost:5173
npm run build    # Production build
npm run lint     # ESLint check
```

### Testing
```bash
# Backend tests
dotnet test tests/IBS.UnitTests/IBS.UnitTests.csproj
dotnet test tests/IBS.IntegrationTests/IBS.IntegrationTests.csproj
dotnet test --filter "FullyQualifiedName~ClientTests"  # Run specific test class

# Frontend tests
cd src/IBS.Web
npm test             # Watch mode
npm run test:ci      # CI mode (single run)
npm run test:coverage
```

## Architecture

### Directory Structure
```
src/
├── IBS.Api/              # ASP.NET Core Web API (controllers, middleware, DI setup)
├── IBS.Web/              # React frontend (components, hooks, stores, services)
├── IBS.Infrastructure/   # Shared persistence (DbContext, migrations, UnitOfWork)
├── BuildingBlocks/       # DDD base classes
│   ├── IBS.BuildingBlocks.Domain/       # Entity, Aggregate, ValueObject, DomainEvent
│   ├── IBS.BuildingBlocks.Application/  # MediatR behaviors, command/query abstractions
│   └── IBS.BuildingBlocks.Infrastructure/  # Generic repositories, multitenancy
└── Contexts/             # Bounded contexts (each has Domain/Application/Infrastructure)
    ├── Identity/         # Users, roles, permissions, JWT auth
    ├── Tenants/          # Multi-tenancy, subscriptions
    ├── Clients/          # Client/company management
    ├── Carriers/         # Insurance carriers, products
    ├── Policies/         # Policy lifecycle, coverages, endorsements, renewals, quotes
    ├── Claims/           # Claims management, reserves, payments
    ├── Commissions/      # Commission schedules, statements, reconciliation, reports
    └── Documents/        # (TO IMPLEMENT) Document storage
```

### Key Patterns

**CQRS with MediatR**: Commands and queries are separate handlers in Application layers. Pipeline behaviors handle logging and validation cross-cutting concerns.

**Multi-Tenancy**: All queries are filtered by `ITenantContext`. Tenant ID comes from request headers or JWT.

**Domain Events**: Aggregates raise domain events for cross-context communication.

### Frontend Stack
- **State**: Zustand stores
- **Data Fetching**: TanStack Query
- **Forms**: react-hook-form + zod validation
- **Routing**: React Router v7
- **Styling**: Tailwind CSS

## Test Credentials (after seeding with docs/seed-data.sql)

| Email | Password | Role |
|-------|----------|------|
| admin@test.com | Admin123! | Admin |
| agent@test.com | Agent123! | Agent |
| user@test.com | User123! | User |

Tenant ID: `00000000-0000-0000-0000-000000000001`

## API Testing

HTTP request files in `tests/http/` work with VS Code REST Client or JetBrains HTTP Client:
- `auth.http` - Authentication
- `clients.http` - Client management
- `carriers.http` - Carrier management
- `policies.http` - Policy management
- `quotes.http` - Quote management
- `claims.http` - Claims management
- `commissions.http` - Commission management
- `roles.http` - Role management
- `users.http` - User management
- `tenants.http` - Tenant management

Swagger UI: https://localhost:7001/swagger

## API Endpoints

- Follow RESTful principles
- Update docs/openapi.yaml after changes to endpoints
- Update and test with .http files

## Key Configuration Files

- `src/IBS.Api/appsettings.Development.json` - Connection strings, JWT config
- `src/IBS.Web/vite.config.ts` - Dev server, API proxy to backend
- `docker/docker-compose.dev.yml` - Local infrastructure services

## Code Style

- General:
    - Prefer writing clear code and use inline comments sparingly
- C#: 
    - 4-space indent
    - `PascalCase` for classes/methods
    - `_camelCase` for private fields
    - `camelCase` for local variables, parameters
    - Prefer primary constructors where possible
    - Use auto-properties, and `field` if necessary
    - Write XML comments on all classes, methods, properties and fields
    - Tests:
        - `<ClassName>Tests` for test class
        - `<MethodName>_<Conditions>_<AssertedOutcome>` for test methods (never `Async` suffix)
        - Arrange, Act, Assert pattern (comment each section in method)
- TypeScript/JavaScript/CSS:
    - 2-space indent
    - Document all methods, types and interfaces with JSDoc comments
    - Keep `*.test.ts` files in same directory as corresponding `*.ts` file
- Commits: 
    - Use Conventional Commit format
    - **Commit Types:** `feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`
    - **Scopes:** `web`, `api`, `docker`

##  Workflow Orchestration

### 1. Plan Mode Default
- Enter plan mode for ANY non-trivial task (3+ steps or architectural decisions)
- If something goes sideways, STOP and re-plan immediately — don't keep pushing
- Use plan mode for verification steps, not just building
- Write detailed specs upfront to reduce ambiguity

### 2. Subagent Strategy
- Use subagents liberally to keep main context window clean
- Offload research, exploration, and parallel analysis to subagents
- For complex problems, throw more compute at it via subagents
- One task per subagent for focused execution

### 3. Self-Improvement Loop
- After ANY correction from the user: update docs/LESSONS.md with the pattern
- Write rules for yourself that prevent the same mistake
- Ruthlessly iterate on these lessons until mistake rate drops
- Review lessons at session start for relevant project

### 4. Verification Before Done
- Never mark a task complete without proving it works
- Diff behavior between main and your changes when relevant
- Ask yourself: "Would a staff engineer approve this?"
- Run tests, check logs, demonstrate correctness

### 5. Demand Elegance (Balanced)
- For non-trivial changes: pause and ask "is there a more elegant way?"
- If a fix feels hacky: "Knowing everything I know now, implement the elegant solution"
- Skip this for simple, obvious fixes — don't over-engineer
- Challenge your own work before presenting it

### 6. Autonomous Bug Fixing
- When given a bug report: just fix it. Don't ask for hand-holding
- Point at logs, errors, failing tests — then resolve them
- Zero context switching required from the user
- Go fix failing CI tests without being told how

## Task Management

1. **Plan First**: First analyze the domain and update docs/DOMAIN_ANALYSIS.md. Write or update plan to docs/FEATURE_TRACKING.md with checkable items. UI update in docs/UI_IMPLEMENTATION_PLAN.md
2. **Verify Plan**: Check in before starting implementation
3. **Track Progress**: Mark items complete as you go
4. **Explain Changes**: High-level summary at each step
5. **Capture Lessons**: Update docs/LESSONS.md after corrections
6. **Update test data**: Update docs/openapi.yaml when adding or changing endpoints. Update docs/seed-data.sql after db changes

## Core Principles

- **Simplicity First**: Make every change as simple as possible. Impact minimal code.
- **Fix Laziness**: Find root causes. No temporary fixes. Senior developer standards.
- **Minimal Impact**: Changes should only touch what's necessary. Avoid introducing bugs.
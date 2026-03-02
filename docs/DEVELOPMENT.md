# IBS - Local Development Guide

This guide explains how to set up and run the Insurance Broker System (IBS) for local development.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) (LTS recommended)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for SQL Server)
- IDE: Visual Studio 2022, VS Code, or Rider

## Project Structure

```
IBS/
├── src/
│   ├── IBS.Api/              # ASP.NET Core Web API
│   ├── IBS.Web/              # React frontend (Vite + TypeScript)
│   ├── IBS.Infrastructure/   # Shared infrastructure
│   ├── BuildingBlocks/       # Domain, Application, Infrastructure base
│   └── Contexts/             # Bounded contexts (Clients, Carriers, Policies, etc.)
├── tests/
│   ├── IBS.UnitTests/        # Unit tests (xUnit)
│   ├── IBS.IntegrationTests/ # Integration tests
│   └── http/                 # HTTP request files for API testing
├── docker/                   # Docker configuration
└── docs/                     # Documentation
```

## Quick Start

### 1. Start Infrastructure Services

Start SQL Server and other services using Docker Compose:

```bash
cd docker
docker-compose -f docker-compose.dev.yml up -d
```

This starts:
- **SQL Server** on port `1433` (SA password: `YourStrong@Passw0rd`)
- **Redis** on port `6379` (for future caching)
- **Azurite** on ports `10000-10002` (Azure Storage emulator)

### 2. Set Up the Database

Apply Entity Framework migrations to create the database schema (run from the repository root):

```bash
dotnet ef database update --project src/IBS.Infrastructure/IBS.Infrastructure.csproj --startup-project src/IBS.Api/IBS.Api.csproj
```

> **Note:** You may see a `HostAbortedException` message in the output - this is normal behavior for EF tools and does not indicate an error.

The connection string is configured in `src/IBS.Api/appsettings.Development.json`. If you need to use a different connection, you can override it:

```bash
dotnet ef database update --project src/IBS.Infrastructure/IBS.Infrastructure.csproj --startup-project src/IBS.Api/IBS.Api.csproj --connection "Server=localhost,1433;Database=IBS;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
```

To seed the database with test data, run the SQL script in `docs/seed-data.sql` using your preferred SQL client (Azure Data Studio, SSMS, or sqlcmd).

### 3. Start the Backend API

```bash
cd src/IBS.Api
dotnet run --launch-profile https
```

The API will be available at:
- **HTTPS**: https://localhost:7001
- **HTTP**: http://localhost:5000
- **Swagger UI**: https://localhost:7001/swagger

### 4. Start the Frontend

In a new terminal:

```bash
cd src/IBS.Web
npm install
npm run dev
```

The frontend will be available at: **http://localhost:5173**

The Vite dev server proxies `/api` requests to the backend at `https://localhost:7001`.

## Running Both Together

For convenience, you can run both frontend and backend in parallel:

**Terminal 1 - Backend:**
```bash
cd src/IBS.Api && dotnet watch run --launch-profile https
```

**Terminal 2 - Frontend:**
```bash
cd src/IBS.Web && npm run dev
```

## Default Test Credentials

After seeding the database, use these credentials to log in:

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@test.com | Admin123! |
| Agent | agent@test.com | Agent123! |
| User | user@test.com | User123! |

**Tenant ID**: `00000000-0000-0000-0000-000000000001`

## Configuration

### Backend Configuration

Configuration files:
- `src/IBS.Api/appsettings.json` - Base configuration
- `src/IBS.Api/appsettings.Development.json` - Development overrides

Key settings:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=IBS;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
  },
  "Jwt": {
    "SecretKey": "development-secret-key-not-for-production-use-32chars",
    "Issuer": "IBS-API-Dev",
    "Audience": "IBS-Web-Dev"
  }
}
```

### Frontend Configuration

The frontend uses Vite's proxy configuration in `vite.config.ts`:
```typescript
server: {
  port: 5173,
  proxy: {
    '/api': {
      target: 'https://localhost:7001',
      changeOrigin: true,
      secure: false,
    },
  },
}
```

## Running Tests

### Backend Tests

**Unit Tests:**
```bash
cd tests/IBS.UnitTests
dotnet test
```

**Integration Tests:**
```bash
cd tests/IBS.IntegrationTests
dotnet test
```

**All Tests with Coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend Tests

**Run Tests:**
```bash
cd src/IBS.Web
npm test
```

**Run Tests with UI:**
```bash
npm run test:ui
```

**Run Tests with Coverage:**
```bash
npm run test:coverage
```

**Run Tests in CI Mode:**
```bash
npm run test:ci
```

### HTTP Request Testing

The `tests/http/` folder contains `.http` files for testing API endpoints directly:

- `auth.http` - Authentication endpoints
- `clients.http` - Client management
- `carriers.http` - Carrier management
- `policies.http` - Policy management
- `quotes.http` - Quote management
- `claims.http` - Claims management
- `commissions.http` - Commission management
- `documents.http` - Document management and COI generation

These files work with:
- VS Code REST Client extension
- JetBrains HTTP Client (Rider/IntelliJ)
- Visual Studio 2022

## Common Tasks

### Create a New Migration

From the repository root:

```bash
dotnet ef migrations add <MigrationName> --project src/IBS.Infrastructure/IBS.Infrastructure.csproj --startup-project src/IBS.Api/IBS.Api.csproj --output-dir Persistence/Migrations
```

### Reset the Database

```bash
# Drop and recreate the SQL Server container (removes all data)
docker-compose -f docker/docker-compose.dev.yml down -v
docker-compose -f docker/docker-compose.dev.yml up -d

# Wait for SQL Server to start (about 10-15 seconds), then apply migrations
dotnet ef database update --project src/IBS.Infrastructure/IBS.Infrastructure.csproj --startup-project src/IBS.Api/IBS.Api.csproj

# Optionally seed with test data using docs/seed-data.sql
```

### Build for Production

**Backend:**
```bash
cd src/IBS.Api
dotnet publish -c Release -o ./publish
```

**Frontend:**
```bash
cd src/IBS.Web
npm run build
```

The frontend build output will be in `src/IBS.Web/dist/`.

### Lint Frontend Code

```bash
cd src/IBS.Web
npm run lint
```

## Troubleshooting

### SQL Server Connection Issues

1. Ensure Docker Desktop is running
2. Check if SQL Server container is healthy:
   ```bash
   docker ps
   docker logs ibs-sqlserver
   ```
3. Verify the connection string in appsettings

### SSL Certificate Errors

If you see SSL certificate errors, trust the development certificate:
```bash
dotnet dev-certs https --trust
```

### Port Conflicts

If ports are in use, modify:
- Backend: `src/IBS.Api/Properties/launchSettings.json`
- Frontend: `src/IBS.Web/vite.config.ts` (and update proxy target)
- Docker: `docker/docker-compose.dev.yml`

### Node Modules Issues

```bash
cd src/IBS.Web
rm -rf node_modules package-lock.json
npm install
```

## API Documentation

- **Swagger UI**: https://localhost:7001/swagger
- **OpenAPI Spec**: `docs/openapi.yaml`
- **Domain Analysis**: `docs/DOMAIN_ANALYSIS.md`

## Architecture

See `docs/architecture/` for detailed architecture documentation.

The system uses:
- **Clean Architecture** with Domain-Driven Design
- **CQRS** with MediatR for commands and queries
- **Multi-tenancy** with tenant isolation
- **JWT Authentication** with refresh tokens

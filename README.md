# SIM Backend API

ASP.NET Core Web API for centralized SIM, customer, collaborator, order, alert, dashboard, and report management.

## Run

```bash
dotnet restore
dotnet run --project src/Sim.Api
```

By default the API uses SQLite at `sim.local.db` for local/test runs while no PostgreSQL connection string is available.
To use PostgreSQL later, set:

```bash
export ConnectionStrings__DefaultConnection='Host=localhost;Port=5432;Database=sim;Username=postgres;Password=postgres'
```

Swagger: `http://localhost:5000/swagger` or the launch URL printed by `dotnet run`.

## PostgreSQL schema

Initial PostgreSQL schema lives at `src/Sim.Api/Migrations/001_initial_postgres.sql`. EF mappings keep snake_case PostgreSQL identifiers while SQLite is used temporarily for local/testing.

## API groups

- `/api/v1/sims`
- `/api/v1/customers`
- `/api/v1/collaborators`
- `/api/v1/orders`
- `/api/v1/alerts/expiring-sims?days=7`
- `/api/v1/dashboard?period=day|week|month`
- `/api/v1/reports/customers`, `/orders`, `/collaborators`, `/revenue`, `/expiring-sims`

## Project layout

- `src/Sim.Domain`: entities and domain base types.
- `src/Sim.Application`: request/response contracts shared by API flows.
- `src/Sim.Infrastructure`: EF Core `SimDbContext` and persistence mappings.
- `src/Sim.Api`: minimal API endpoints, provider selection, Swagger.
- `tests/Sim.Api.Tests`: API acceptance and architecture boundary tests.

No auth is configured per approved scope.

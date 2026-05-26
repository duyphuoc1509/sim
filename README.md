# SIM Backend API

ASP.NET Core Web API for centralized SIM, customer, collaborator, order, alert, dashboard, and report management.

## Run

```bash
export ConnectionStrings__DefaultConnection='Host=localhost;Port=5432;Database=sim;Username=postgres;Password=postgres'
dotnet restore
dotnet run --project src/Sim.Api
```

Swagger: `http://localhost:5000/swagger` or the launch URL printed by `dotnet run`.

## PostgreSQL schema

Initial PostgreSQL schema lives at `src/Sim.Api/Migrations/001_initial_postgres.sql`.

## API groups

- `/api/v1/sims`
- `/api/v1/customers`
- `/api/v1/collaborators`
- `/api/v1/orders`
- `/api/v1/alerts/expiring-sims?days=7`
- `/api/v1/dashboard?period=day|week|month`
- `/api/v1/reports/customers`, `/orders`, `/collaborators`, `/revenue`, `/expiring-sims`

No auth is configured per approved scope.

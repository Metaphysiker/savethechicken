# Development Scripts

## Run Locally (Recommended for Development)

To start both WebApi and Web frontend with hot reload:

```powershell
cd infrastructure/development
.\run-local.ps1
```

This starts:
- **WebApi** on http://localhost:8081
- **Frontend** on http://localhost:8080

Both run with `dotnet watch` for automatic hot reload on file changes.

### Stop the Servers

Press any key in the script window, or run:

```powershell
.\stop-local.ps1
```

## Run with Docker

To use Docker instead:

```powershell
docker compose up --build
```

Environment variables are in `.env` file.

## Database Migrations

When running locally, migrations run automatically on startup. To manually create a migration:

```powershell
cd ../../WebApi
dotnet ef migrations add YourMigrationName
```

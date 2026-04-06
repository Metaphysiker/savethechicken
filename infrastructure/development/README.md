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

## Restore Database Backup

To restore a production backup to your development environment:

```powershell
# Restore the most recent backup automatically
.\restore-backup-to-development.ps1

# Or restore a specific backup file
.\restore-backup-to-development.ps1 C:\Users\sraes\savethechicken-backups\dump_2026-04-06_19_40_18.dump
```

**Prerequisites:**
- Docker development environment must be running (`docker compose up`)
- Backup file must be in `.dump` format (created by `pg_dump -Fc`)

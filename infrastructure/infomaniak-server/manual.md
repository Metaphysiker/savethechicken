## Configuration

### Before First Deployment

1. **Create Production Settings** - Copy the example file:
   ```bash
   cp appsettings.Production.json.example appsettings.Production.json
   ```

2. **Email Configuration** - Edit `appsettings.Production.json`:
   ```json
   "Email": {
     "SmtpHost": "smtp.infomaniak.com",
     "SmtpPort": "587",
     "FromEmail": "noreply@rettet-das-huhn.ch",
     "FromName": "Rettet das Huhn",
     "Username": "your-email@domain.com",
     "Password": "your-smtp-password",
     "EnableSsl": true,
     "NotificationRecipients": ["admin@rettet-das-huhn.ch"]
   }
   ```

3. **Environment Variables** - Review `.env` file:
   - `ADMIN_PASSWORD` - Admin user password
   - `RETTET_DAS_HUHN_PASSWORD` - Special user password
   - `POSTGRES_PASSWORD` - Database password
   - `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY` - S3 credentials
   - `SIGNING_KEY` - JWT signing key (min 32 chars)
   - `API_BASE_URL` - Production API URL

4. **File Upload Limits**:
   - Kestrel configured for 100MB max file size
   - Supports CSV imports up to 100MB
   - Temp files stored in Docker volume `webapi-temp`

## Deployment

### Deploy to Infomaniak Server

**Linux/Mac:**
```bash
./deploy-to-infomaniak-server.sh
```

**Windows (PowerShell):**
```powershell
.\deploy-to-infomaniak-server.ps1
```

Both scripts:
- Build Docker images
- Transfer images to server
- Deploy and restart containers
- Create a git branch to track deployment (format: `deployment/YYYY-MM-DD_HH-mm-ss`)

**View deployment history:**
```bash
git branch --list 'deployment/*'
```

## Shell Access

In development:
```bash
docker compose run -it --entrypoint bash webapi
```

In production:
```bash
docker compose -f docker-compose.remote.yml run -it --entrypoint bash webapi
```

## Backup

### Create Database Backup from Server

**Linux/Mac:**
```bash
./create-backup-from-infomaniak-server.sh /home/sandro/backups/savethechicken
```

**Windows (PowerShell):**
```powershell
.\create-backup-from-infomaniak-server.ps1 C:\backups\savethechicken
```

Both scripts:
- Connect to the production server via SSH
- Create a PostgreSQL dump using `pg_dump` (custom format)
- Save the backup on the server in `/home/deploy/backups/savethechicken/`
- Download the backup to your local machine
- Create timestamped backup files: `dump_YYYY-MM-DD_HH_mm_ss.dump`

### Restore from Backup

To restore a backup to the production database:

```bash
# Upload backup to server
scp dump_2026-04-02_14_30_00.dump deploy@84.234.19.192:/home/deploy/backups/

# SSH into server
ssh deploy@84.234.19.192

# Copy dump into postgres container
docker cp /home/deploy/backups/dump_2026-04-02_14_30_00.dump savethechicken-production-postgres-1:/dump.dump

# Restore the database (this will drop and recreate)
docker exec savethechicken-production-postgres-1 bash -c 'pg_restore -Fc -U savethechicken -d savethechicken --clean --if-exists /dump.dump'
```



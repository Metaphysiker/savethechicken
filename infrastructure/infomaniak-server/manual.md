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

### Check What's Currently Deployed

Before deploying, check what version is currently running on the server:

**Windows (PowerShell):**
```powershell
.\check-current-deployment.ps1
```

**Linux/Mac:**
```bash
./check-current-deployment.sh
```

This shows you:
- Git commit hash of the deployed code
- When it was deployed
- Which branch it came from
- Who deployed it

### Deploy to Infomaniak Server

**Linux/Mac:**
```bash
./deploy-to-infomaniak-server.sh
```

**Windows (PowerShell):**
```powershell
.\deploy-to-infomaniak-server.ps1
```

Both scripts will:
1. **Check for uncommitted changes** and warn you
2. **Show current deployment** on the server (what you're replacing)
3. **Show what you're about to deploy** (commit hash and message)
4. Ask for confirmation if you have uncommitted changes
5. Build Docker images
6. Transfer images to server
7. Deploy and restart containers
8. Create deployment tracking:
   - Git branch: `deployment/YYYY-MM-DD_HH-mm-ss`
   - Git tag: `deploy-YYYY-MM-DD_HH-mm-ss`
   - Server file: `/home/deploy/savethechicken/deployment.txt`

### View Deployment History

```bash
# List all deployment branches
git branch --list 'deployment/*'

# List all deployment tags
git tag --list 'deploy-*'

# Show details of a specific deployment tag
git show deploy-2026-04-02_14-30-00
```

### Rollback to Previous Deployment

If something goes wrong, you can rollback:

1. **Find the previous deployment:**
   ```bash
   git tag --list 'deploy-*' | tail -2
   ```

2. **Checkout that tag:**
   ```bash
   git checkout deploy-2026-04-02_12-00-00
   ```

3. **Re-deploy:**
   ```powershell
   .\deploy-to-infomaniak-server.ps1
   ```

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

### Automated Backups (Recommended)

**For computers that don't run 24/7**, use automated backups that run hourly and create ONE backup per day (whenever your PC is on).

#### Setup (Windows Only)

**1. Test Manual Backup First:**
```powershell
cd infrastructure\infomaniak-server
.\create-infomaniak-backup.ps1
```

**2. Setup Hourly Scheduled Task (Run as Administrator):**
```powershell
.\setup-scheduled-infomaniak-backup.ps1
```

This creates a Windows Task Scheduler task that:
- Runs every hour
- Checks if a backup was already created today
- If not, downloads a fresh backup from Infomaniak
- Only creates ONE backup per day (first successful run)

**Default backup location:** `C:\Users\sraes\savethechicken-backups\automatic-backups`

#### Managing Automated Backups

```powershell
# Run backup manually
Get-ScheduledTask -TaskName "SaveTheChicken-InfomaniakBackup" | Start-ScheduledTask

# Check status
Get-ScheduledTask -TaskName "SaveTheChicken-InfomaniakBackup" | Get-ScheduledTaskInfo

# View in GUI
taskschd.msc

# Disable task
Disable-ScheduledTask -TaskName "SaveTheChicken-InfomaniakBackup"

# Remove task
Unregister-ScheduledTask -TaskName "SaveTheChicken-InfomaniakBackup" -Confirm:$false
```

#### How It Works

1. **Hourly Check:** Script runs every hour via Task Scheduler
2. **Smart Skip:** If backup already exists for today's date, exits immediately
3. **Download:** If no backup exists, connects to Infomaniak and downloads fresh backup
4. **Organization:** Automatic backups are stored in `automatic-backups` subfolder to separate them from manual backups

**Result:** You get one daily backup whenever your PC is running, without duplicates!

### Manual Database Backup

For one-time backups or if you don't want automation:

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



# ITI ERP — Deployment Guide

## Prerequisites

- Docker & Docker Compose
- .NET 9 SDK (for local dev only)
- Node.js 20+ (for local dev only)
- PostgreSQL 16+ (if running without Docker)

## Quick Start (Docker)

```bash
# 1. Copy environment template
cp .env.example .env

# 2. Edit .env with your values (JWT secret, DB password, etc.)
#    Minimum: set JwtSettings__Secret to a 32+ char string

# 3. Run with Docker Compose
cd docker
docker compose up -d --build

# 4. Access
#    Frontend:  http://localhost:3000
#    API:       http://localhost:5000
#    Swagger:   http://localhost:5000/swagger
#    Health:    http://localhost:5000/health
#    Hangfire:  http://localhost:5000/hangfire (Admin only)
```

## Production Deployment

```bash
# 1. Configure production environment
cp .env.example .env
# Edit .env with strong secrets

# 2. Run production stack
cd docker
docker compose -f docker-compose.prod.yml up -d --build

# 3. Run database migrations (first time)
docker exec -it iti-erp-api dotnet ef database update \
  --project src/Infrastructure/ITI.ERP.Infrastructure \
  --startup-project src/API/ITI.ERP.Api

# 4. Seed data runs automatically on first startup
```

## Manual Deployment (without Docker)

### API
```bash
dotnet publish src/API/ITI.ERP.Api -c Release -o ./publish
cd publish
dotnet ITI.ERP.Api.dll
```

### Frontend
```bash
cd frontend
npm ci
npm run build
# Serve the `build/` folder with Nginx or any static host
```

## Environment Variables

| Variable | Description | Default |
|---|---|---|
| `POSTGRES_DB` | Database name | `iti_erp` |
| `POSTGRES_USER` | DB username | `postgres` |
| `POSTGRES_PASSWORD` | DB password | **(required)** |
| `JwtSettings__Secret` | JWT signing key (>=32 chars) | **(required)** |
| `SeedAdmin__Password` | Initial admin password | `Admin@123` |
| `AllowedOrigins` | CORS origins (comma-separated) | none (dev=localhost) |

All ASP.NET Core config values can be overridden with environment variables using `__` separator (e.g., `ConnectionStrings__DefaultConnection`).

## Database Migrations

```bash
# Create migration
dotnet ef migrations add MigrationName \
  --project src/Infrastructure/ITI.ERP.Infrastructure \
  --startup-project src/API/ITI.ERP.Api

# Apply migration
dotnet ef database update \
  --project src/Infrastructure/ITI.ERP.Infrastructure \
  --startup-project src/API/ITI.ERP.Api
```

## Backup Strategy

### PostgreSQL
```bash
# Backup
docker exec iti-erp-postgres pg_dump -U postgres iti_erp > backup_$(date +%Y%m%d).sql

# Restore
docker exec -i iti-erp-postgres psql -U postgres iti_erp < backup_20250731.sql
```

### Automated Backups
Set up a cron job:
```bash
0 2 * * * docker exec iti-erp-postgres pg_dump -U postgres iti_erp | gzip > /backups/iti_erp_$(date +\%Y\%m\%d).sql.gz
```

### File Storage
Back up the file storage directory (configured via `FileStorage__LocalBasePath`).

## Health Checks

- **Basic**: `GET /health` — returns PostgreSQL connection status
- **Docker**: Built-in Docker healthcheck via `pg_isready`

## SSL/TLS

Edit `docker/nginx/default.conf` to uncomment the HTTPS server block. Place certificates at:
- `/etc/nginx/certs/fullchain.pem`
- `/etc/nginx/certs/privkey.pem`

Use Let's Encrypt or your preferred CA.

## Troubleshooting

| Issue | Solution |
|---|---|
| `JWT authentication failed` | Ensure `JwtSettings__Secret` matches across environments |
| `Connection refused` | Check PostgreSQL container is healthy: `docker ps` |
| `401 on all endpoints` | Check token isn't expired; refresh token flow enabled |
| `CORS error` | Add frontend origin to `AllowedOrigins` |

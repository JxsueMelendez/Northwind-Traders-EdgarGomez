# Northwind Traders OMS

## Stack

- Backend: ASP.NET Core WebAPI
- Architecture: Clean Architecture
- Database: SQL Server / Northwind sample data
- Frontend: Vue 3 + Vite
- Containers: Docker Compose

## Run locally (Development Mode)

This mode includes hot-reload for both API and Frontend, and automatic port cleanup.

### Mac / Linux
```bash
chmod +x run-dev.sh
./run-dev.sh
```

### Windows (PowerShell)
```powershell
./run-dev.ps1
```

## Run with Docker Compose (Production-like)

```bash
docker compose up --build
```

> [!IMPORTANT]
> This project uses `.gitattributes` to force **LF** line endings on all `.sh` scripts. This prevents "File not found" or "env: bash\r: No such file or directory" errors when running Docker containers on Windows.

## Services
- **SQL Server**: `localhost:1433` (DB: NorthwindDB)
- **Web API**: `http://localhost:8080/swagger`
- **Frontend**: `http://localhost:5173`

The Vue app includes:

- Order dashboard cards
- Order intake form
- Global API error handling via Axios interceptor
- Responsive layout with loading and error states

## Notes

- The Docker stack uses the SQL Server instance declared in `docker-compose.yml`.
- Google Maps, address validation, PDF export, and Excel export are not implemented yet.
# Northwind-Traders
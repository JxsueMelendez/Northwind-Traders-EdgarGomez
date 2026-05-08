# Northwind Traders OMS

## Stack

- Backend: ASP.NET Core WebAPI
- Architecture: Clean Architecture
- Database: SQL Server / Northwind sample data
- Frontend: Vue 3 + Vite
- Containers: Docker Compose

## Run locally with Docker

```bash
docker compose up --build
```

Services:

- SQL Server: `localhost:1433`
- Web API: `http://localhost:8080`
- Frontend: `http://localhost:5173`

## API

The API exposes order CRUD endpoints under `/api/orders`.

Interactive OpenAPI is available in development mode from the WebAPI project.

## Frontend

The Vue app includes:

- Order dashboard cards
- Order intake form
- Global API error handling via Axios interceptor
- Responsive layout with loading and error states

## Notes

- The Docker stack uses the SQL Server instance declared in `docker-compose.yml`.
- Google Maps, address validation, PDF export, and Excel export are not implemented yet.
# Northwind-Traders
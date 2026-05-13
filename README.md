# 🚢 Northwind Traders OMS & Logistics

A high-performance Order Management System (OMS) built with a modern tech stack, focusing on logistics, real-time mapping, and executive reporting. This project revitalizes the classic Northwind database with a premium UI/UX and clean architectural principles.

## 🚀 Key Features

- **Executive Dashboard**: Real-time KPI tracking (Revenue, Pending Orders, Global Regions).
- **Logistics Hub**: Advanced shipment tracking with integrated **Leaflet Maps**.
- **Smart Order Intake**: Multi-step order creation with product/customer autocomplete and map-based address selection.
- **Enterprise Reporting**:
    - Branded **PDF** generation for individual orders and summaries using **QuestPDF**.
    - Professional **Excel** exporting with filtering and styling using **ClosedXML**.
- **Map Integration**: Pin-to-Address functionality (Click a location on the map to automatically populate city/country/region).

---

## 🛠️ Technology Stack

### Backend (.NET 10)
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, WebAPI).
- **ORM**: Entity Framework Core 10 (SQL Server).
- **Reporting**: QuestPDF (PDF) & ClosedXML (Excel).
- **Documentation**: Swagger/OpenAPI.

### Frontend (Vue 3)
- **Framework**: Quasar Framework (Vue 3 + Vite).
- **State Management**: Pinia.
- **Mapping**: Leaflet.js with OpenStreetMap/Nominatim.
- **Communication**: Axios with global interceptors.

---

## 💻 Running the Project

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) (LTS recommended)
- [Docker](https://www.docker.com/) (For SQL Server)

### 1. Start the Database
The project includes a pre-configured Docker SQL Server instance with the Northwind data.
```bash
docker compose up -d db
```

### 2. Run the Development Environment
We provide automated scripts to start both the API and Frontend while cleaning up old processes.

#### **Mac / Linux**
```bash
chmod +x run-dev.sh
./run-dev.sh
```

#### **Windows (PowerShell)**
```powershell
./run-dev.ps1
```

### 3. Access the Services
- **Frontend UI**: [http://localhost:5173](http://localhost:5173)
- **API Swagger Docs**: [http://localhost:8080/swagger](http://localhost:8080/swagger)
- **Database**: `localhost:1433` (User: `sa`, Pass: `StrongPassword123!`)

---

## 🐳 Docker Deployment (Orchestrated)
To run the entire stack (DB + API + Frontend) in a production-like environment:
```bash
docker compose up --build
```

---

## 📄 Project Structure
- `project_API/`: Backend source code organized by Clean Architecture layers.
- `frontend/`: Vue/Quasar application.
- `db/`: SQL Server initialization scripts.
- `data/`: Sample data and database backups.

## 📝 License
This project is part of the Northwind Shipping modernization initiative.
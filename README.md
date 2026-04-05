# QSS Platform — Quality Management System for Dental Practices

A full-featured, production-ready MVP built with **.NET 8 Clean Architecture**, covering all modules specified in the RFP.

---

## Technology Stack

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core 8 Web API |
| Frontend | ASP.NET Core 8 Razor Pages + Alpine.js + Tailwind CSS |
| Database | SQLite (dev) → swap to SQL Server / PostgreSQL for production |
| ORM | Entity Framework Core 8 |
| Auth | ASP.NET Core Identity + JWT Bearer |
| Real-time | SignalR (chat + notifications) |
| QR Codes | QRCoder |
| Container | Docker + Docker Compose |

## Architecture

```
QSS/
├── src/
│   ├── QSS.Domain/           # Entities, Enums, Base classes
│   ├── QSS.Application/      # DTOs, Service interfaces
│   ├── QSS.Infrastructure/   # EF Core DbContext, Seeder, Services
│   ├── QSS.API/              # REST API, Controllers, SignalR Hubs
│   └── QSS.Web/              # Razor Pages UI
├── docker-compose.yml
├── run-local.sh              # Linux/Mac quick start
├── run-local.ps1             # Windows quick start
└── QSS.sln
```

## Modules Implemented

| Module | Status | Notes |
|---|---|---|
| User Management (RBAC) | ✅ | Superadmin, Admin, Dentist, DentalAssistant, Trainee |
| Process & Task Management | ✅ | Status tracking, dependencies, frequency |
| Room Management | ✅ | Rooms with device + inventory assignments |
| Device Management | ✅ | QR codes, maintenance schedules |
| Materials Inventory | ✅ | Stock levels, expiry alerts, supplier links |
| Medications | ✅ | Quantity, usage logging, expiry alerts |
| Real-time Chat | ✅ | Direct + team channels via SignalR |
| Task Comments & Mentions | ✅ | With notifications |
| Learning & Knowledge Base | ✅ | File upload, progress tracking |
| Reports & Analytics | ✅ | QSS Score, charts, CSV export |
| Dashboard | ✅ | QSS Score, alerts, employee performance |
| Notifications | ✅ | In-app notifications via SignalR |

---

## Quick Start (Local)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)

### Option 1 — Scripts

**Windows:**
```powershell
.\run-local.ps1
```

**Linux / Mac:**
```bash
chmod +x run-local.sh
./run-local.sh
```

### Option 2 — Manual

**Step 1: Build**
```bash
dotnet build QSS.sln
```

**Step 2: Start the API** (Terminal 1)
```bash
cd src/QSS.API
dotnet run
# Runs on http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

**Step 3: Start the Web UI** (Terminal 2)
```bash
cd src/QSS.Web
dotnet run
# Runs on http://localhost:5001
```

**Step 4: Open the browser**
- Web UI: http://localhost:5001
- API Swagger: http://localhost:5000/swagger

### Option 3 — Docker

```bash
docker-compose up --build
```
- Web UI: http://localhost:5001
- API: http://localhost:5000/swagger

---

## Default Credentials

| Role | Email | Password |
|---|---|---|
| Superadmin | superadmin@qss.com | Admin@1234! |
| Admin | admin@qss.com | Admin@1234! |
| Dentist | dentist@qss.com | Admin@1234! |
| Dental Assistant | assistant@qss.com | Admin@1234! |
| Trainee | trainee@qss.com | Admin@1234! |

> The database is auto-created and seeded on first run. No manual database setup needed.

---

## API Reference

The REST API is fully documented via Swagger at `http://localhost:5000/swagger`.

### Authentication
```http
POST /api/auth/login
Content-Type: application/json

{"email": "superadmin@qss.com", "password": "Admin@1234!"}
```
Returns a JWT token — include it as `Authorization: Bearer <token>` on all subsequent requests.

### Key Endpoints

| Endpoint | Description |
|---|---|
| `GET /api/dashboard` | QSS Score, stats, alerts |
| `GET/POST /api/tasks` | Task management |
| `PATCH /api/tasks/{id}/status` | Update task status |
| `GET/POST /api/processes` | Process management |
| `GET/POST /api/rooms` | Room management |
| `GET/POST /api/devices` | Device management |
| `GET /api/devices/{id}/qrcode` | Get device QR code |
| `GET/POST /api/materials` | Material inventory |
| `GET/POST /api/medications` | Medication management |
| `POST /api/medications/{id}/log-usage` | Log medication usage |
| `GET/POST /api/chat` | Chat messages |
| `GET /api/learning` | Learning materials |
| `POST /api/learning/upload` | Upload learning material |
| `POST /api/users` | Create user (Admin+) |
| `PUT /api/users/{id}/role` | Change user role (Superadmin) |

### SignalR Hubs
- `ws://localhost:5000/hubs/chat` — Real-time chat
- `ws://localhost:5000/hubs/notifications` — Push notifications

---

## Production Deployment

### Switch to SQL Server
In `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=QSS;Trusted_Connection=True;"
  }
}
```
In `Program.cs`, change `UseSqlite` to `UseSqlServer`.

### Environment Variables (Production)
```bash
ConnectionStrings__DefaultConnection=<your-db-connection-string>
Jwt__Key=<your-64-char-secret-minimum>
Jwt__Issuer=<your-domain>
Jwt__Audience=<your-domain>
```

### GDPR Compliance Notes
- User data isolated by Identity framework
- Soft-delete pattern (no hard deletes)
- JWT tokens expire after 7 days
- All passwords hashed by ASP.NET Core Identity (PBKDF2 + salt)

---

## Scalability Roadmap (Post-MVP)

1. **Multi-tenancy** — Add `TenantId` to all entities + middleware
2. **SQL Server / PostgreSQL** — Drop-in with EF Core provider change
3. **Redis** — Session + SignalR backplane for horizontal scaling
4. **File storage** — Replace local uploads with Azure Blob / S3
5. **Background jobs** — Hangfire for overdue task detection, expiry notifications
6. **Third-party API** — Supplier, practice management, accounting integrations
7. **Mobile** — PWA wrapper or dedicated React Native app using existing API
8. **Custom roles** — Already architected, extend `IdentityRole` with permissions table

---

## License

Proprietary — all rights reserved.

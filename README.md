# ?? BugPredictionBackend

> A production-grade ASP.NET Core 8 backend that pulls data from **SonarCloud**, stores it in **SQL Server**, and exposes clean **REST APIs** for an Angular analytics dashboard.

---

## ?? What This Project Does

SonarCloud gives you raw code quality metrics — bugs, vulnerabilities, code smells, coverage, duplication — but calling it directly from a frontend is slow, insecure (token exposure), and tightly coupled.

This backend solves that by acting as a **secure data aggregation layer**:

1. **Syncs** SonarCloud data into a local SQL Server database every 6 hours
2. **Stores** full historical snapshots — every scan is preserved
3. **Serves** structured, page-ready REST APIs to the Angular frontend

```
SonarCloud API  ???  .NET 8 Backend  ???  SQL Server (BPCQDB)  ???  Angular Frontend
```

Angular **never** calls SonarCloud. The token is **never** exposed to the browser.

---

## ?? Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 8 Web API |
| Database | SQL Server 2022 |
| Data Access | Pure ADO.NET (no Entity Framework) |
| HTTP Client | `IHttpClientFactory` |
| Background Sync | `IHostedService` |
| API Docs | Swagger / OpenAPI (Swashbuckle) |
| Serialization | `System.Text.Json` |
| Logging | `ILogger` (built-in) |

---

## ?? Architecture

```
???????????????????????????????
?        SonarCloud API        ?
?  (External – Bearer Token)  ?
???????????????????????????????
               ? HTTPS
               ?
???????????????????????????????????????????????????????
?              .NET 8 Backend (This Project)          ?
?                                                     ?
?  ???????????????????     ?????????????????????????? ?
?  ?  SYNC WORKFLOW  ?     ?    READ WORKFLOW        ? ?
?  ?  (Write to DB)  ?     ?  (Serve to Angular)    ? ?
?  ?                 ?     ?                        ? ?
?  ? SonarApiClient  ?     ?  Controllers           ? ?
?  ?      ?          ?     ?       ?                ? ?
?  ?  SyncService    ?     ?  Services              ? ?
?  ?      ?          ?     ?       ?                ? ?
?  ?  Repositories   ?     ?  Repositories          ? ?
?  ???????????????????     ?????????????????????????? ?
????????????????????????????????????????????????????????
            ?                          ?
            ?                          ?
???????????????????????????????????????????????????????
?              SQL Server  ·  BPCQDB                  ?
?  Projects · Branches · Snapshots                    ?
?  ModuleMetrics · SeverityDistribution               ?
???????????????????????????????????????????????????????
                          ?
                          ?
???????????????????????????????????????????????????????
?               Angular Frontend                      ?
?       (calls only our .NET APIs – never Sonar)      ?
???????????????????????????????????????????????????????
```

---

## ?? Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server 2022 (local or remote)
- A [SonarCloud](https://sonarcloud.io) account with a valid token

---

### 1. Clone the Repository

```bash
git clone https://github.com/adityanshinde/Bug-Prediction-Backend.git
cd Bug-Prediction-Backend
```

---

### 2. Set Up the Database

Open **SSMS** and run the SQL scripts in order:

```
SQL/01_CreateDatabase_Tables.sql   ? Creates BPCQDB + all 5 tables + indexes
SQL/02_StoredProcedures_Sync.sql   ? Stored procedures for Sonar ? DB writes
SQL/03_StoredProcedures_Read.sql   ? Stored procedures for DB ? Frontend reads
```

---

### 3. Configure `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BPCQDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "SonarSettings": {
    "BaseUrl": "https://sonarcloud.io",
    "Token": "YOUR_SONAR_TOKEN_HERE",
    "Organization": "your-org-key"
  },
  "Cors": {
    "AllowedOrigins": [ "http://localhost:4200" ]
  }
}
```

| Key | Description |
|-----|-------------|
| `DefaultConnection` | SQL Server connection string pointing to `BPCQDB` |
| `Token` | Your SonarCloud personal access token |
| `Organization` | Your SonarCloud organization key |
| `AllowedOrigins` | Angular dev server URL (update for production) |

---

### 4. Run the Application

```bash
dotnet run
```

Or press **F5** in Visual Studio.

Once running, open Swagger at:
```
http://localhost:5234/swagger
```

---

## ?? API Endpoints

Swagger groups endpoints into two sections:

### ?? Sonar to Database
Triggers a sync from SonarCloud into the database.

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/sync/{projectKey}` | Sync one specific project |
| `POST` | `/api/sync/all` | Sync all projects in the organization |

> The background service also auto-syncs every **6 hours** on startup.

---

### ?? Database to Frontend
Serves structured data to the Angular dashboard.

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/projects` | List all synced projects |
| `GET` | `/api/projects/{id}/header` | Project name, branch, last scan, gate status |
| `GET` | `/api/projects/{id}/dashboard` | KPIs + severity distribution + recent scans |
| `GET` | `/api/projects/{id}/metrics` | Coverage trend + bugs vs vulns + module table |
| `GET` | `/api/projects/{id}/risk-analysis` | Module risk distribution + high risk modules |
| `GET` | `/api/projects/{id}/quality-gates` | Gate status + **real conditions** + history |
| `GET` | `/api/projects/{id}/scan-history` | Complete scan history list |
| `GET` | `/api/projects/{id}/qa-entries` | QA summary + all manual QA entries |
| `POST` | `/api/projects/{id}/qa-entries` | Submit a manual QA entry from the QA form |

**Standard response codes:**

| Code | Meaning |
|------|---------|
| `200 OK` | Data returned successfully |
| `404 Not Found` | Project ID not found, or no snapshot exists yet |

---

## ?? Database Schema

**Database:** `BPCQDB`

```
Projects
  ??? Branches  (FK: ProjectId)
        ??? Snapshots  (FK: ProjectId, BranchId)
              ??? ModuleMetrics            (FK: SnapshotId)
              ??? SeverityDistribution     (FK: SnapshotId)
              ??? QualityGateConditions    (FK: SnapshotId)

Projects
  ??? ManualQAEntries  (FK: ProjectId)
```

| Table | Purpose |
|-------|---------|
| `Projects` | One row per SonarCloud project (upserted each sync) |
| `Branches` | One row per branch per project (upserted each sync) |
| `Snapshots` | **Immutable** — one new row per scan, never updated |
| `ModuleMetrics` | Per-file/directory metrics linked to a snapshot |
| `SeverityDistribution` | BLOCKER/CRITICAL/MAJOR/MINOR/INFO counts per snapshot |
| `QualityGateConditions` | Per-condition details from SonarCloud gate evaluation |
| `ManualQAEntries` | Manual QA form submissions per project |

> **Key rule:** Snapshots are **never overwritten** — every sync creates a new row, preserving full history.

---

## ?? Project Structure

```
BugPredictionBackend/
?
??? Background/               ? Auto-sync every 6 hours
??? Configurations/           ? SonarSettings POCO
??? Controllers/
?   ??? Sync/                 ? Manual sync trigger endpoints
?   ??? Read/                 ? Frontend data endpoints
??? Services/
?   ??? Sonar/                ? SonarApiClient (only class calling Sonar)
?   ??? Sync/                 ? SyncService (orchestrates Sonar ? DB)
?   ??? Read/                 ? Page-specific data shaping services
??? Repositories/
?   ??? Sync/                 ? ADO.NET write repositories
?   ??? Read/                 ? ADO.NET read repositories
??? Models/
?   ??? Entities/             ? DB table row representations
?   ??? Sonar/                ? SonarCloud JSON response models
?   ??? DTOs/                 ? Angular-facing response shapes
??? SQL/                      ? All database scripts
??? Project Docs/             ? Architecture docs and guides
??? appsettings.json
??? Program.cs
??? BugPredictionBackend.csproj
```

---

## ? How the Sync Works

The sync runs automatically every **6 hours** via `SonarSyncHostedService`. It can also be triggered manually via `POST /api/sync/all`.

For each project, the sync:

1. Fetches project info ? upserts into `Projects`
2. Fetches branches ? upserts into `Branches`
3. Fetches metrics (bugs, vulns, coverage, etc.) from `measures/component`
4. Fetches quality gate status from `qualitygates/project_status`
5. Inserts a new **immutable snapshot** into `Snapshots`
6. Fetches severity facets from `issues/search` ? inserts into `SeverityDistribution`
7. Fetches module/file metrics from `measures/component_tree` ? inserts into `ModuleMetrics`

---

## ?? Security Notes

- The SonarCloud token is stored only in `appsettings.json` — **never hardcoded**
- Token is used only inside `SonarApiClient` — no other class accesses it
- Angular never receives or sees the token
- CORS is restricted to configured origins only
- All DB queries use **parameterized stored procedures** — no dynamic SQL

---

## ?? NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `Swashbuckle.AspNetCore` | 6.6.2 | Swagger / OpenAPI documentation |
| `Microsoft.Data.SqlClient` | 5.2.2 | SQL Server ADO.NET driver |

---

## ?? Documentation

All detailed docs are inside the `Project Docs/` folder:

| File | Contents |
|------|---------|
| `Project-WorkFlow.md` | Full detailed workflow — every layer explained |
| `Backend-Dev-Plan.md` | Original backend implementation plan |
| `frontend-api-guide.md` | API contract design for Angular |
| `backend_api_guide.md` | SonarCloud API reference used |
| `Sonar-API-Structure.md` | SonarCloud JSON response structures |
| `custom-calculations.md` | Risk score formulas (handled by Angular) |
| `Plan.md` | Implementation checklist |
| `Status.md` | Build and feature status tracker |

---

## ?? Design Principles

| Rule | Why |
|------|-----|
| No EF — pure ADO.NET | Full control over queries, stored procedures only |
| Snapshots are immutable | Preserves complete historical data |
| No Sonar calls from Angular | Token security + performance |
| No risk calculations in backend | Angular handles all scoring logic |
| Sync and Read are fully separated | Clean separation of concerns |
| Controllers never touch the DB | Only services and repositories do |

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
SonarCloud API (External - Bearer Token)
       ? HTTPS
       ?
.NET 8 Backend (This Project)
  ????????????????????   ???????????????????????
  ?  SYNC WORKFLOW   ?   ?   READ WORKFLOW      ?
  ?  SonarApiClient  ?   ?   Controllers        ?
  ?       ?          ?   ?       ?              ?
  ?  SyncService     ?   ?   Services           ?
  ?       ?          ?   ?       ?              ?
  ?  Repositories    ?   ?   Repositories       ?
  ????????????????????   ????????????????????????
           ?                        ?
     SQL Server · BPCQDB
     Projects · Branches · Snapshots
     ModuleMetrics · SeverityDistribution
     QualityGateConditions · ManualQAEntries
           ?
           ?
     Angular Frontend
     (calls only our .NET APIs — never Sonar)
```

---

## ?? Getting Started

> Follow every step in order. Do not skip any step.

---

### Step 1 — Prerequisites

Make sure you have all of these installed before starting:

| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | 8.0 or later | https://dotnet.microsoft.com/download/dotnet/8.0 |
| SQL Server | 2022 (Express is fine) | https://www.microsoft.com/en-us/sql-server/sql-server-downloads |
| SSMS | Latest | https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms |
| Git | Any | https://git-scm.com/downloads |
| Visual Studio | 2022 (or VS Code) | https://visualstudio.microsoft.com/ |

---

### Step 2 — Clone the Repository

Open a terminal and run:

```bash
git clone https://github.com/adityanshinde/Bug-Prediction-Backend.git
cd Bug-Prediction-Backend
```

---

### Step 3 — Set Up the Database in SSMS

> ?? Run the scripts **in exact order**. Each script depends on the previous one.

1. Open **SSMS** and connect to your SQL Server instance (usually `localhost` or `.\SQLEXPRESS`)
2. Open each file from the `SQL/` folder and execute them one by one:

| Order | File | What it does |
|-------|------|-------------|
| 1st | `SQL/01_CreateDatabase_Tables.sql` | Creates `BPCQDB` database + all 7 tables + indexes |
| 2nd | `SQL/02_StoredProcedures_Sync.sql` | Creates stored procedures for Sonar ? DB write operations |
| 3rd | `SQL/03_StoredProcedures_Read.sql` | Creates stored procedures for DB ? Frontend read APIs |

**How to run a script in SSMS:**
- `File ? Open ? File` ? select the `.sql` file
- Click **Execute** (or press `F5`)
- You should see `Command(s) completed successfully`

**Verify in SSMS Object Explorer after all 3 scripts:**
```
BPCQDB
  ??? Tables
        ??? Projects
        ??? Branches
        ??? Snapshots
        ??? ModuleMetrics
        ??? SeverityDistribution
        ??? QualityGateConditions
        ??? ManualQAEntries
```

---

### Step 4 — Get Your SonarCloud Token

You need a SonarCloud personal access token so the backend can call SonarCloud APIs.

**Get the token:**
1. Go to **https://sonarcloud.io** and log in
2. Click your **profile avatar** (top-right) ? **My Account**
3. Go to the **Security** tab
4. Under **Generate Tokens** — type a name e.g. `BugPredictionBackend` ? click **Generate**
5. **Copy the token immediately** — SonarCloud only shows it once

**Get your Organization key:**
1. Go to **https://sonarcloud.io/organizations**
2. Your org key is shown under your organization name — e.g. `bpcq`

> Keep both values ready for the next two steps.

---

### Step 5 — Configure `appsettings.json`

Open `appsettings.json` in the project root. Set your **Organization key** and **connection string**. Leave `Token` empty — it goes in `secrets.json` next:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BPCQDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "SonarSettings": {
    "BaseUrl": "https://sonarcloud.io",
    "Token": "",
    "Organization": "your-org-key-here"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200"
    ]
  }
}
```

> ?? **Leave `Token` as empty string here.** It gets committed to Git — never put your real token here.

**Connection string reference:**

| Your SQL Server setup | Connection string value |
|----------------------|------------------------|
| Default local install | `Server=localhost;Database=BPCQDB;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL Server Express | `Server=.\SQLEXPRESS;Database=BPCQDB;Trusted_Connection=True;TrustServerCertificate=True;` |
| Named instance | `Server=localhost\INSTANCENAME;Database=BPCQDB;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL login (username + password) | `Server=localhost;Database=BPCQDB;User Id=sa;Password=yourpassword;TrustServerCertificate=True;` |

---

### Step 6 — Create `secrets.json` (Your Token Goes Here)

> This file stores your SonarCloud token **locally only**.
> It is listed in `.gitignore` and will **never** be committed to GitHub.

1. In the **project root folder** (same folder as `appsettings.json` and `Program.cs`), create a new file named exactly:
   ```
   secrets.json
   ```

2. Paste this exact structure and replace the placeholder with your real token:

   ```json
   {
     "SonarSettings": {
       "Token": "YOUR_SONAR_TOKEN_HERE"
     }
   }
   ```

   Example with a real-looking token:
   ```json
   {
     "SonarSettings": {
       "Token": "abc123def456ghi789jkl012mno345pqr678stu"
     }
   }
   ```

3. Save the file. Do not add anything else to this file — only the token override belongs here.

> ?? **File name must be exactly:** `secrets.json` — lowercase, no spaces, `.json` extension.
> Place it in the **project root** — same level as `appsettings.json`. Not inside any subfolder.

**How it works internally:**

`Program.cs` loads configuration sources in this order:
```
appsettings.json  ?  secrets.json
```
The `Token: ""` in `appsettings.json` gets replaced by your real token from `secrets.json` at runtime. No code changes needed.

**Your project root after setup:**
```
Bug-Prediction-Backend/
??? appsettings.json      ? committed to Git (Token is empty string — safe)
??? secrets.json          ? NOT in Git (your real token lives here)
??? .gitignore            ? secrets.json is listed here
??? Program.cs
??? BugPredictionBackend.csproj
??? ...
```

---

### ?? `secrets.json` — Quick Reference

> Copy this block, paste it as `secrets.json` in the project root, fill in your token. That is all.

**File name:** `secrets.json`
**Location:** project root (next to `appsettings.json`)
**Committed to Git:** ? Never

```json
{
  "SonarSettings": {
    "Token": "PASTE_YOUR_SONARCLOUD_TOKEN_HERE"
  }
}
```

| Field | Value |
|-------|-------|
| File name | `secrets.json` (exact, case-sensitive on Linux/Mac) |
| Location | Project root — same folder as `appsettings.json` |
| `SonarSettings.Token` | Your SonarCloud personal access token from Step 4 |
| Anything else needed? | ? No — only the token goes here |

---

### Step 7 — Verify `.gitignore`

Open `.gitignore` in the project root and confirm this line exists:

```
# Local secrets file (do NOT commit)
secrets.json
```

If it is missing, add it now **before** doing any `git add` or `git commit`.

---

### Step 8 — Restore and Build

In the terminal from the project root:

```bash
dotnet restore
dotnet build
```

Expected output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

If you see errors — double-check Step 5 (connection string format) and Step 6 (`secrets.json` file location and JSON format).

---

### Step 9 — Run the Application

```bash
dotnet run
```

Or open in **Visual Studio** and press **F5**.

**What happens automatically on first run:**
1. App starts and loads all configuration (including token from `secrets.json`)
2. `SonarSyncHostedService` fires immediately and syncs all your SonarCloud projects into `BPCQDB`
3. After sync completes, all read APIs are ready to use
4. Background sync repeats every **6 hours** automatically

**Expected console output during sync:**
```
info: Sync started. Projects found: 5
info: Syncing project: BPCQ_axios-sonar
info: Project synced: BPCQ_axios-sonar | SnapshotId: 1
info: Syncing project: BPCQ_ClawWork-sonarqube
info: Project synced: BPCQ_ClawWork-sonarqube | SnapshotId: 2
...
info: Sync completed.
```

---

### Step 10 — Open Swagger and Test

Open your browser:
```
http://localhost:5234/swagger
```

You will see two groups of endpoints:

| Group | What is in it |
|-------|--------------|
| **Sonar to Database** | Manual sync trigger endpoints |
| **Database to Frontend** | All read + QA endpoints |

**Quick test — verify sync worked:**
1. Expand `GET /api/projects` ? click **Try it out** ? **Execute**
2. You should get back a JSON array of your synced projects with `projectId` values
3. Use any `projectId` from that response as `{id}` in all other API calls

---

### Troubleshooting

| Problem | Cause | Fix |
|---------|-------|-----|
| `Cannot open database BPCQDB` | DB not created yet | Run Step 3 SQL scripts in SSMS first |
| `401 Unauthorized` from SonarCloud | Token wrong or expired | Regenerate token in SonarCloud ? update `secrets.json` |
| `404` from SonarCloud during sync | Wrong org key | Check `Organization` in `appsettings.json` — must match SonarCloud exactly |
| `/api/projects` returns empty `[]` | Sync failed or hasn't run | Check console logs for sync error messages |
| Sync runs but tables are empty | SQL scripts not run fully | Re-run all 3 SQL scripts from Step 3 in order |
| `secrets.json` values not loading | File is in wrong folder | Must be in same folder as `appsettings.json` (project root) |
| Build errors after clone | Missing NuGet packages | Run `dotnet restore` first then `dotnet build` |
| Port 5234 already in use | Port conflict | Change port in `Properties/launchSettings.json` |
| Token accidentally in `appsettings.json` | Placed in wrong file | Move it to `secrets.json`, clear `appsettings.json` Token to `""` |

---

## ?? API Endpoints

Swagger groups endpoints into two sections:

### ?? Sonar to Database

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/sync/{projectKey}` | Sync one specific project |
| `POST` | `/api/sync/all` | Sync all projects in the organization |

> The background service also auto-syncs every **6 hours** on startup.

### ?? Database to Frontend

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/projects` | List all synced projects |
| `GET` | `/api/projects/{id}/header` | Project name, branch, last scan, gate status |
| `GET` | `/api/projects/{id}/dashboard` | KPIs + severity distribution + recent scans |
| `GET` | `/api/projects/{id}/metrics` | Coverage trend + bugs vs vulns + module table |
| `GET` | `/api/projects/{id}/risk-analysis` | Module risk distribution + high risk modules |
| `GET` | `/api/projects/{id}/quality-gates` | Gate status + real conditions + history |
| `GET` | `/api/projects/{id}/scan-history` | Complete scan history list |
| `GET` | `/api/projects/{id}/qa-entries` | QA summary + all manual QA entries |
| `POST` | `/api/projects/{id}/qa-entries` | Submit a manual QA entry from the QA form |

| Code | Meaning |
|------|---------|
| `200 OK` | Data returned successfully |
| `404 Not Found` | Project ID not found, or no snapshot exists yet |

---

## ?? Database Schema

**Database:** `BPCQDB`

```
Projects
  ??? Branches           (FK: ProjectId)
        ??? Snapshots    (FK: ProjectId, BranchId)
              ??? ModuleMetrics            (FK: SnapshotId)
              ??? SeverityDistribution     (FK: SnapshotId)
              ??? QualityGateConditions    (FK: SnapshotId)

Projects
  ??? ManualQAEntries    (FK: ProjectId)
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
??? SQL/                      ? All database scripts (run these in SSMS)
??? Project Docs/             ? Architecture docs and API guides
??? appsettings.json          ? Config (Token is empty — safe to commit)
??? secrets.json              ? YOUR TOKEN HERE (gitignored — create manually)
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
4. Fetches quality gate status + per-condition results from `qualitygates/project_status`
5. Inserts a new **immutable snapshot** into `Snapshots`
6. Inserts per-condition gate results into `QualityGateConditions`
7. Fetches severity facets from `issues/search` ? inserts into `SeverityDistribution`
8. Fetches module/file metrics from `measures/component_tree` ? inserts into `ModuleMetrics`

---

## ?? Security Notes

- **`secrets.json`** stores only your token — file name is `secrets.json`, placed in project root, never committed
- `appsettings.json` has an empty `Token: ""` field — safe to commit, contains no secrets
- Token is read only inside `SonarApiClient` — no other class touches it
- Angular never receives or sees the token
- CORS currently set to `AllowAnyOrigin()` for development — restrict to specific origins in production
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
| `Frontend-API-How-To-Use.md` | Complete API guide for Angular devs — interfaces, service, field reference |
| `Project-WorkFlow.md` | Full detailed workflow — every layer explained |
| `Plan.md` | Implementation checklist (all phases) |
| `Status.md` | Build and feature status tracker |
| `frontend-api-guide.md` | Original API contract design |
| `backend_api_guide.md` | SonarCloud API reference used |
| `Sonar-API-Structure.md` | SonarCloud JSON response structures |
| `custom-calculations.md` | Risk score formulas (handled by Angular) |

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
| Token in `secrets.json` only | Never committed, never exposed |

# ?? BugPredictionBackend – Complete Project Workflow

> A detailed, simple explanation of how every part of this project works,
> how data flows from SonarCloud all the way to your Angular frontend,
> and what every file's job is.

---

# ?? Table of Contents

1. [Big Picture – What This Project Does](#1-big-picture)
2. [Overall Architecture](#2-overall-architecture)
3. [Folder Structure Explained](#3-folder-structure-explained)
4. [The Two Main Workflows](#4-the-two-main-workflows)
5. [Workflow A – Sync (SonarCloud ? Database)](#5-workflow-a--sync-sonarcloud--database)
6. [Workflow B – Read (Database ? Angular)](#6-workflow-b--read-database--angular)
7. [Every Layer Explained](#7-every-layer-explained)
8. [Database Design](#8-database-design)
9. [All API Endpoints](#9-all-api-endpoints)
10. [How Startup Works (Program.cs)](#10-how-startup-works-programcs)
11. [Configuration & Settings](#11-configuration--settings)
12. [Data Flow – Step by Step (Full Journey)](#12-data-flow--step-by-step-full-journey)
13. [File Responsibility Map](#13-file-responsibility-map)

---

# 1. Big Picture

## ?? What Does This Project Actually Do?

SonarCloud scans your code repositories and gives you quality metrics —
bugs, vulnerabilities, code smells, coverage, duplication, etc.

The problem is:
- SonarCloud's API is complex and slow to call every time
- You don't want Angular (frontend) talking directly to SonarCloud
- You don't want your secret SonarCloud token exposed to the browser

**This backend solves all of that.**

```
SonarCloud API  ???  Our .NET Backend  ???  SQL Server DB  ???  Angular Frontend
```

This backend acts as a **middleman** that:

1. **Pulls** data from SonarCloud every 6 hours automatically
2. **Stores** it cleanly in our own SQL Server database
3. **Serves** structured, page-ready data to Angular via REST APIs

Angular never talks to SonarCloud. Angular never sees the token.
Angular only calls our clean backend APIs.

---

# 2. Overall Architecture

```
???????????????????????????????????????????????????????????????????
?                        SONARCLOUD                               ?
?   (External API – bugs, coverage, quality gates, modules)       ?
???????????????????????????????????????????????????????????????????
                           ?  HTTPS + Bearer Token
                           ?
???????????????????????????????????????????????????????????????????
?                   .NET 8 BACKEND (This Project)                 ?
?                                                                 ?
?  ???????????????????        ????????????????????????????????   ?
?  ?  SYNC WORKFLOW  ?        ?       READ WORKFLOW          ?   ?
?  ?                 ?        ?                              ?   ?
?  ? SonarApiClient  ?        ?  Controllers (Read)          ?   ?
?  ?      ?          ?        ?       ?                      ?   ?
?  ?  SyncService    ?        ?  Services (Read)             ?   ?
?  ?      ?          ?        ?       ?                      ?   ?
?  ?  Repositories   ?        ?  Repositories (Read)         ?   ?
?  ?  (Sync/Write)   ?        ?       ?                      ?   ?
?  ???????????????????        ????????????????????????????????   ?
?           ?                              ?                      ?
???????????????????????????????????????????????????????????????????
            ?                              ?
            ?                              ?
???????????????????????????????????????????????????????????????????
?                     SQL SERVER (BPCQDB)                         ?
?   Projects | Branches | Snapshots | ModuleMetrics | Severity    ?
???????????????????????????????????????????????????????????????????
                                           ?
                                           ?
???????????????????????????????????????????????????????????????????
?                     ANGULAR FRONTEND                            ?
?   (Calls only our .NET APIs – never SonarCloud directly)        ?
???????????????????????????????????????????????????????????????????
```

---

# 3. Folder Structure Explained

```
BugPredictionBackend/
?
??? ?? Configurations/
?   ??? SonarSettings.cs          ? Holds SonarCloud config values
?
??? ?? Background/
?   ??? SonarSyncHostedService.cs ? Auto-runs sync every 6 hours
?
??? ?? Controllers/
?   ??? ?? Sync/
?   ?   ??? SyncController.cs     ? Manual sync trigger via API
?   ??? ?? Read/
?       ??? ProjectsController.cs
?       ??? DashboardController.cs
?       ??? MetricsController.cs
?       ??? QualityGatesController.cs
?       ??? ScanHistoryController.cs
?
??? ?? Services/
?   ??? ?? Sonar/
?   ?   ??? SonarApiClient.cs     ? Makes HTTP calls to SonarCloud
?   ??? ?? Sync/
?   ?   ??? SyncService.cs        ? Orchestrates the full sync flow
?   ??? ?? Read/
?       ??? ProjectService.cs
?       ??? DashboardService.cs
?       ??? MetricsService.cs
?       ??? QualityGateService.cs
?       ??? ScanHistoryService.cs
?
??? ?? Repositories/
?   ??? ?? Sync/                  ? Write operations (INSERT/UPDATE)
?   ?   ??? ProjectRepository.cs
?   ?   ??? BranchRepository.cs
?   ?   ??? SnapshotRepository.cs
?   ?   ??? ModuleRepository.cs
?   ?   ??? SeverityRepository.cs
?   ??? ?? Read/                  ? Read operations (SELECT)
?       ??? ProjectReadRepository.cs
?       ??? DashboardReadRepository.cs
?       ??? MetricsReadRepository.cs
?       ??? QualityGateReadRepository.cs
?       ??? ScanHistoryReadRepository.cs
?
??? ?? Models/
?   ??? ?? Entities/              ? Mirror of DB table columns
?   ?   ??? ProjectEnt.cs
?   ?   ??? BranchEnt.cs
?   ?   ??? SnapshotEnt.cs
?   ?   ??? ModuleMetricEnt.cs
?   ?   ??? SeverityDistributionEnt.cs
?   ??? ?? Sonar/                 ? Matches SonarCloud JSON responses
?   ?   ??? SonarProjectEnt.cs
?   ?   ??? SonarBranchEnt.cs
?   ?   ??? SonarMeasureEnt.cs
?   ?   ??? SonarIssueEnt.cs
?   ?   ??? SonarAnalysisEnt.cs
?   ?   ??? SonarQualityGateEnt.cs
?   ?   ??? SonarComponentTreeEnt.cs
?   ?   ??? SonarMeasureHistoryEnt.cs
?   ??? ?? DTOs/                  ? What Angular receives
?       ??? ProjectListDto.cs
?       ??? HeaderDto.cs
?       ??? DashboardDto.cs
?       ??? MetricsDto.cs
?       ??? QualityGateDto.cs
?       ??? ScanHistoryDto.cs
?       ??? RiskAnalysisDto.cs
?
??? ?? SQL/
?   ??? 01_CreateDatabase_Tables.sql
?   ??? 02_StoredProcedures_Sync.sql
?   ??? 03_StoredProcedures_Read.sql
?
??? ?? Project Docs/
?   ??? Plan.md
?   ??? Status.md
?   ??? Project-WorkFlow.md       ? This file
?   ??? frontend-api-guide.md
?   ??? backend_api_guide.md
?   ??? custom-calculations.md
?   ??? Backend-Dev-Plan.md
?   ??? Sonar-API-Structure.md
?
??? appsettings.json              ? Token, DB connection, CORS config
??? Program.cs                    ? App startup and DI registration
??? BugPredictionBackend.csproj   ? Project file with NuGet packages
```

---

# 4. The Two Main Workflows

This entire project runs on **two completely separate workflows**:

| Workflow | Direction | Trigger | Purpose |
|----------|-----------|---------|---------|
| **Sync** | SonarCloud ? Database | Every 6 hours (auto) or manual POST | Pull fresh data and store it |
| **Read** | Database ? Angular | Every time Angular calls an API (GET) | Serve structured data to frontend |

These two workflows **never mix**.
- Sync never talks to Angular
- Read never talks to SonarCloud

---

# 5. Workflow A – Sync (SonarCloud ? Database)

This workflow's job is to **fetch data from SonarCloud and save it to our DB**.

## 5.1 Trigger – Two Ways

### Auto Trigger (Every 6 Hours)
```
App Starts
    ?
SonarSyncHostedService (runs in background forever)
    ?
Waits 6 hours
    ?
Calls SyncService.SyncAllProjectsAsync()
    ?
Waits 6 hours again... (loop)
```

### Manual Trigger (via API)
```
Angular / Swagger / Postman
    ?
POST /api/sync/{projectKey}
    ?
SyncController
    ?
SyncService.SyncSingleProjectAsync(projectKey)
```

---

## 5.2 Full Sync Flow (Step by Step)

```
SyncService.SyncAllProjectsAsync()
?
??? 1. SonarApiClient.GetProjectsAsync()
?       Calls: GET /api/projects/search?organization=bpcq
?       Returns: List of all projects in the org
?
??? 2. For each project ? SyncProjectAsync(sonarProject)
?   ?
?   ??? 3. ProjectRepository.InsertOrUpdateAsync()
?   ?       Calls SP: sp_InsertOrUpdateProject
?   ?       If project exists ? UPDATE, else ? INSERT
?   ?       Returns: projectId (int)
?   ?
?   ??? 4. SonarApiClient.GetBranchesAsync(projectKey)
?   ?       Calls: GET /api/project_branches/list?project=key
?   ?       Gets all branches, picks the main branch
?   ?
?   ??? 5. BranchRepository.InsertOrUpdateAsync()
?   ?       Calls SP: sp_InsertOrUpdateBranch
?   ?       Returns: branchId (int)
?   ?
?   ??? 6. SonarApiClient.GetMetricsAsync(projectKey)
?   ?       Calls: GET /api/measures/component?component=key
?   ?       Gets: bugs, vulnerabilities, code_smells,
?   ?             coverage, duplication, ratings
?   ?       Returns: Dictionary<string, string>
?   ?
?   ??? 7. SonarApiClient.GetQualityGateAsync(projectKey)
?   ?       Calls: GET /api/qualitygates/project_status?projectKey=key
?   ?       Gets: OK or ERROR ? mapped to PASS / FAIL
?   ?
?   ??? 8. SnapshotRepository.InsertAsync()
?   ?       Calls SP: sp_InsertSnapshot
?   ?       Creates ONE immutable snapshot record
?   ?       Returns: snapshotId (int)
?   ?       ?? Snapshots are NEVER updated – always new row
?   ?
?   ??? 9. SyncSeverityAsync(projectKey, snapshotId)
?   ?   ?
?   ?   ??? SonarApiClient.GetSeverityAsync(projectKey)
?   ?   ?       Calls: GET /api/issues/search?facets=severities,types
?   ?   ?       Gets: count per severity (BLOCKER, CRITICAL, etc.)
?   ?   ?
?   ?   ??? SeverityRepository.InsertAsync()
?   ?           Calls SP: sp_InsertSeverityDistribution
?   ?
?   ??? 10. SyncModulesAsync(projectKey, snapshotId)
?       ?
?       ??? SonarApiClient.GetModuleMetricsAsync(projectKey)
?       ?       Calls: GET /api/measures/component_tree?component=key
?       ?       Gets: DIR and FIL level metrics for each module/file
?       ?
?       ??? For each component ? ModuleRepository.InsertAsync()
?               Calls SP: sp_InsertModuleMetric
?               Stores: bugs, vulns, coverage, duplication,
?                       complexity, ncloc per module
```

---

## 5.3 What Gets Stored Per Sync

Every sync run stores:

| Table | What is stored |
|-------|----------------|
| `Projects` | Project key, name, org, last analysis date (upsert) |
| `Branches` | Branch name, isMain, analysis date (upsert) |
| `Snapshots` | Full metrics for that scan moment (always new row) |
| `ModuleMetrics` | Per-module/file breakdown linked to snapshot |
| `SeverityDistribution` | Blocker/Critical/Major/Minor/Info counts linked to snapshot |

---

# 6. Workflow B – Read (Database ? Angular)

This workflow's job is to **serve clean, structured data to Angular** from the database.

Angular makes a GET request ? Controller ? Service ? Repository ? SQL SP ? DTO ? JSON response.

## 6.1 Request Flow (Every API Call)

```
Angular Frontend
    ?
GET /api/projects/{id}/dashboard
    ?
DashboardController.GetDashboard(projectId)
    ?
ProjectService.ExistsAsync(projectId)   ? Check if project exists first
    ? (if not found ? return 404)
    ?
DashboardService.GetDashboardAsync(projectId)
    ?
SnapshotRepository.GetLatestAsync(projectId)
    Calls SP: sp_GetLatestSnapshot
    Returns: latest SnapshotEnt (most recent scan)
    ?
SeverityRepository.GetBySnapshotAsync(snapshotId)
    Calls SP: sp_GetSeverityBySnapshot
    Returns: SeverityDistributionEnt
    ?
DashboardReadRepository.GetRecentScansAsync(projectId)
    Calls SP: sp_GetRecentScans
    Returns: List<RecentScanDto>
    ?
DashboardService assembles DashboardDto
    ?
Controller returns Ok(dashboardDto)
    ?
Angular receives clean JSON response
```

---

## 6.2 All Read Flows at a Glance

| Angular calls | Controller | Service | Key SPs used |
|---------------|------------|---------|--------------|
| GET /api/projects | ProjectsController | ProjectService | sp_GetAllProjects |
| GET /api/projects/{id}/header | DashboardController | DashboardService | sp_GetProjectHeader |
| GET /api/projects/{id}/dashboard | DashboardController | DashboardService | sp_GetLatestSnapshot, sp_GetSeverityBySnapshot, sp_GetRecentScans |
| GET /api/projects/{id}/metrics | MetricsController | MetricsService | sp_GetMetricsKpis, sp_GetCoverageHistory, sp_GetBugsVulnerabilitiesHistory, sp_GetModulesBySnapshot |
| GET /api/projects/{id}/risk-analysis | MetricsController | MetricsService | sp_GetLatestSnapshot, sp_GetModulesBySnapshot |
| GET /api/projects/{id}/quality-gates | QualityGatesController | QualityGateService | sp_GetLatestSnapshot, sp_GetQualityGateHistory |
| GET /api/projects/{id}/scan-history | ScanHistoryController | ScanHistoryService | sp_GetSnapshotsByProject |

---

# 7. Every Layer Explained

## 7.1 Controllers Layer
**Folder:** `Controllers/Read/` and `Controllers/Sync/`

- Controllers are the **entry point** for all HTTP requests
- They **only** do three things:
  1. Check if the project exists (return 404 if not)
  2. Call the correct service
  3. Return the result as HTTP response
- Controllers have **zero DB logic** and **zero business logic**
- They only inject and call services

```csharp
// Example – DashboardController
[HttpGet("dashboard")]
public async Task<IActionResult> GetDashboard(int projectId)
{
    bool exists = await projectService.ExistsAsync(projectId);
    if (!exists) return NotFound(...);

    DashboardDto? dashboard = await dashboardService.GetDashboardAsync(projectId);
    return dashboard is null ? NotFound(...) : Ok(dashboard);
}
```

---

## 7.2 Services Layer
**Folder:** `Services/Read/` and `Services/Sync/`

- Services are the **brain** of each workflow
- They **coordinate** between multiple repositories
- They **shape data** from entities into DTOs (for read services)
- They **orchestrate** the sync flow (for sync services)
- No raw SQL here – they only call repositories

```
Read Services:    Entity/DTO conversion happens here
Sync Services:    SonarCloud data ? Entity mapping happens here
```

---

## 7.3 Repositories Layer
**Folder:** `Repositories/Sync/` and `Repositories/Read/`

- Repositories are the **only layer that talks to the database**
- They use **pure ADO.NET** – no Entity Framework
- They **only call stored procedures** – no inline SQL
- They return either Entity objects (for sync) or DTOs (for read)

```csharp
// Pattern used in every repository
using SqlConnection con = new(_connectionString);
using SqlCommand cmd = new("sp_StoredProcedureName", con);
cmd.CommandType = CommandType.StoredProcedure;
cmd.Parameters.AddWithValue("@Param", value);
await con.OpenAsync();
// execute and map result
```

---

## 7.4 SonarApiClient
**File:** `Services/Sonar/SonarApiClient.cs`

- This is the **only class that ever talks to SonarCloud**
- Uses `IHttpClientFactory` to make HTTP calls
- Reads the Bearer token from `SonarSettings` (never hardcoded)
- Deserializes JSON into `Models/Sonar/` types
- Returns clean typed objects – callers never see raw JSON

---

## 7.5 Models Layer

Three sub-categories of models:

| Folder | Purpose | Used By |
|--------|---------|---------|
| `Models/Entities/` | Represent DB table rows | Repositories ? Services |
| `Models/Sonar/` | Represent SonarCloud JSON responses | SonarApiClient |
| `Models/DTOs/` | Represent what Angular receives | Services ? Controllers ? Angular |

**Important rule:**
- Entities never go to Angular
- Sonar models never go to Angular
- DTOs never touch the DB directly

---

## 7.6 Background Service
**File:** `Background/SonarSyncHostedService.cs`

- Runs automatically when the app starts
- Runs in the **background** – does not block any API requests
- Uses `IServiceScopeFactory` to safely create a DI scope each time
- Calls `SyncService.SyncAllProjectsAsync()` then sleeps for 6 hours
- If sync crashes, it logs the error and continues the loop

---

# 8. Database Design

**Database Name:** `BPCQDB`

## Tables and Their Purpose

```
????????????????????????????????????????????????????????????????
?  Projects                                                    ?
?  Id | ProjectKey | Name | Organization | LastAnalysisDate   ?
????????????????????????????????????????????????????????????????
                   ? 1 project has many branches
                   ?
????????????????????????????????????????????????????????????????
?  Branches                                                    ?
?  Id | ProjectId(FK) | BranchName | IsMain | AnalysisDate    ?
????????????????????????????????????????????????????????????????
                   ?
        ???????????????????????
        ?                     ?
        ?                     ?
?????????????????    ??????????????????????????????????????????
?  Snapshots    ?    ?  (Each snapshot = one scan moment)     ?
?               ?    ?  Id | ProjectId | BranchId | ScanDate  ?
?  Immutable ?  ?    ?  Bugs | Vulns | CodeSmells | Coverage  ?
?  Never update ?    ?  Duplication | Ratings | GateStatus    ?
?????????????????    ??????????????????????????????????????????
       ?
       ????????????????????????????????????????????
       ?                                          ?
       ?                                          ?
????????????????????????????     ??????????????????????????????
?  ModuleMetrics           ?     ?  SeverityDistribution      ?
?  SnapshotId(FK)          ?     ?  SnapshotId(FK)            ?
?  ModuleName | Qualifier  ?     ?  Blocker | Critical        ?
?  Path | Language         ?     ?  Major | Minor | Info      ?
?  Bugs | Vulns | Coverage ?     ??????????????????????????????
????????????????????????????
```

## Key Design Rules

| Rule | Why |
|------|-----|
| Snapshots are **immutable** (never updated) | Keeps full history – every scan is preserved |
| Projects and Branches are **upserted** (insert or update) | They don't change often, no need to duplicate |
| All queries go through **stored procedures** | Security, performance, no SQL injection |
| Indexes on `ProjectId`, `ScanDate`, `SnapshotId` | Fast lookup for latest snapshot |

---

# 9. All API Endpoints

## Sync Endpoints (Sonar ? DB)

| Method | URL | What it does |
|--------|-----|-------------|
| POST | `/api/sync/{projectKey}` | Manually sync one project from SonarCloud |
| POST | `/api/sync/all` | Manually sync all projects from SonarCloud |

## Read Endpoints (DB ? Angular)

| Method | URL | Response |
|--------|-----|----------|
| GET | `/api/projects` | List of all projects with last scan date |
| GET | `/api/projects/{id}/header` | Project name, branch, last scan, gate status |
| GET | `/api/projects/{id}/dashboard` | KPIs + severity distribution + recent scans |
| GET | `/api/projects/{id}/metrics` | Coverage trend + bugs vs vulns + module table |
| GET | `/api/projects/{id}/risk-analysis` | Module risk distribution + high risk modules |
| GET | `/api/projects/{id}/quality-gates` | Current gate status + gate history |
| GET | `/api/projects/{id}/scan-history` | Full scan history list |

### Standard Response Codes

| Code | Meaning |
|------|---------|
| `200 OK` | Success, data returned |
| `404 Not Found` | Project ID doesn't exist in DB, or no snapshot data yet |

---

# 10. How Startup Works (Program.cs)

When the app starts, `Program.cs` does these things in order:

```
1. Bind SonarSettings from appsettings.json
       (BaseUrl, Token, Organization)

2. Register HttpClient factory
       (used by SonarApiClient for HTTP calls)

3. Register Sync Repositories (Scoped)
       ProjectRepository, BranchRepository,
       SnapshotRepository, ModuleRepository, SeverityRepository

4. Register Read Repositories (Scoped)
       ProjectReadRepository, DashboardReadRepository,
       MetricsReadRepository, QualityGateReadRepository,
       ScanHistoryReadRepository

5. Register SonarApiClient (Scoped)

6. Register Sync Services (Scoped)
       SyncService

7. Register Read Services (Scoped)
       ProjectService, DashboardService, MetricsService,
       QualityGateService, ScanHistoryService

8. Register Background Hosted Service
       SonarSyncHostedService (runs sync loop forever)

9. Configure CORS
       Allows Angular (http://localhost:4200) to call APIs

10. Register Controllers + Swagger

11. App starts ? Background service begins ? First sync fires
```

---

# 11. Configuration & Settings

**File:** `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BPCQDB;..."
  },
  "SonarSettings": {
    "BaseUrl": "https://sonarcloud.io",
    "Token":   "your-sonar-token",
    "Organization": "bpcq"
  },
  "Cors": {
    "AllowedOrigins": [ "http://localhost:4200" ]
  }
}
```

**File:** `Configurations/SonarSettings.cs`

This is a simple POCO class bound to the `SonarSettings` section above.
It is injected via `IOptions<SonarSettings>` into `SonarApiClient`.
The token is **never hardcoded** anywhere in the code.

---

# 12. Data Flow – Step by Step (Full Journey)

## Example: Angular opens the Dashboard page for Project ID = 1

```
STEP 1 – Angular sends request
  GET http://localhost:5234/api/projects/1/dashboard

STEP 2 – DashboardController receives it
  Checks: does project 1 exist?
  ? ProjectService.ExistsAsync(1)
  ? ProjectReadRepository calls sp_GetProjectById
  ? Returns: true ?

STEP 3 – DashboardService.GetDashboardAsync(1)
  Gets latest snapshot:
  ? SnapshotRepository calls sp_GetLatestSnapshot
  ? Returns: SnapshotEnt { Bugs=29, Vulns=11, Coverage=45, ... }

STEP 4 – Gets severity distribution:
  ? SeverityRepository calls sp_GetSeverityBySnapshot
  ? Returns: SeverityDistributionEnt { Blocker=11, Critical=153, ... }

STEP 5 – Gets recent scans:
  ? DashboardReadRepository calls sp_GetRecentScans
  ? Returns: List of last 10 scan rows with branch/commit/gate

STEP 6 – DashboardService assembles DashboardDto
  Combines all the above into one clean object:
  {
    kpis: { bugs, vulnerabilities, codeSmells, coverage, ... },
    issueDistribution: { bugs, vulnerabilities, codeSmells },
    severityDistribution: { blocker, critical, major, minor, info },
    recentScans: [ { scanDate, branch, commit, qualityGate }, ... ]
  }

STEP 7 – Controller returns Ok(dashboardDto)
  HTTP 200 + JSON body sent back to Angular

STEP 8 – Angular receives structured data
  Renders: KPI cards, donut chart, recent scans table
```

---

# 13. File Responsibility Map

| File | Single Responsibility |
|------|-----------------------|
| `Program.cs` | App startup, DI registration, middleware config |
| `appsettings.json` | All environment config (token, DB, CORS) |
| `SonarSettings.cs` | Typed config POCO for SonarCloud settings |
| `SonarApiClient.cs` | ALL calls to SonarCloud – nowhere else calls Sonar |
| `SyncService.cs` | Orchestrates the full Sonar ? DB sync pipeline |
| `SonarSyncHostedService.cs` | Runs SyncService on a 6-hour timer in background |
| `ProjectRepository.cs` | Insert/update projects in DB |
| `BranchRepository.cs` | Insert/update branches in DB |
| `SnapshotRepository.cs` | Insert new snapshots + get latest snapshot |
| `ModuleRepository.cs` | Insert module metrics + read modules by snapshot |
| `SeverityRepository.cs` | Insert severity distribution + read by snapshot |
| `ProjectReadRepository.cs` | Read project list and existence check |
| `DashboardReadRepository.cs` | Read header info and recent scans |
| `MetricsReadRepository.cs` | Read KPIs, coverage trend, bugs vs vulns history |
| `QualityGateReadRepository.cs` | Read quality gate history |
| `ScanHistoryReadRepository.cs` | Read full scan history list |
| `ProjectService.cs` | Maps projects to ProjectListDto, existence check |
| `DashboardService.cs` | Assembles DashboardDto and HeaderDto |
| `MetricsService.cs` | Assembles MetricsDto and RiskAnalysisDto |
| `QualityGateService.cs` | Assembles QualityGateDto |
| `ScanHistoryService.cs` | Returns scan history list |
| `SyncController.cs` | Exposes manual sync trigger endpoints |
| `ProjectsController.cs` | Exposes GET /api/projects |
| `DashboardController.cs` | Exposes header + dashboard endpoints |
| `MetricsController.cs` | Exposes metrics + risk-analysis endpoints |
| `QualityGatesController.cs` | Exposes quality-gates endpoint |
| `ScanHistoryController.cs` | Exposes scan-history endpoint |
| `Models/Entities/` | DB row representations (5 files) |
| `Models/Sonar/` | SonarCloud JSON response shapes (8 files) |
| `Models/DTOs/` | Angular-facing response shapes (7 files) |
| `SQL/01_*.sql` | Creates BPCQDB database + all 5 tables + indexes |
| `SQL/02_*.sql` | All 5 stored procedures for write (sync) operations |
| `SQL/03_*.sql` | All 11 stored procedures for read (frontend) operations |

---

> ?? **Golden Rule of This Project:**
>
> - **Only `SonarApiClient`** talks to SonarCloud
> - **Only Repositories** talk to SQL Server
> - **Only Services** shape data between layers
> - **Only Controllers** talk to Angular
> - Every layer knows only its immediate neighbor — nothing else

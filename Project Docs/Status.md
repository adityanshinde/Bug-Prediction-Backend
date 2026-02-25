# ?? BugPredictionBackend – Status Tracker

---

## Last Updated: All Phases Complete ?

---

| #  | Item                                          | Status   | Notes                                              |
|----|-----------------------------------------------|----------|----------------------------------------------------|
| 1  | Plan.md created                               | ? Done  |                                                    |
| 2  | Status.md created                             | ? Done  |                                                    |
| 3  | SQL/01_CreateDatabase_Tables.sql              | ? Done  | DB=BPCQDB, 5 tables + indexes                     |
| 4  | SQL/02_StoredProcedures_Sync.sql              | ? Done  | 5 SPs for Sonar?DB write operations               |
| 5  | SQL/03_StoredProcedures_Read.sql              | ? Done  | 11 SPs for DB?Frontend read APIs                  |
| 6  | Configurations/SonarSettings.cs               | ? Done  | POCO bound from appsettings.json                  |
| 7  | appsettings.json updated                      | ? Done  | Connection string, SonarSettings, CORS            |
| 8  | Program.cs updated                            | ? Done  | Full DI, CORS, Swagger, HttpClient, HostedService |
| 9  | Models/Entities – 5 entity files              | ? Done  | ProjectEnt, BranchEnt, SnapshotEnt, ModuleMetricEnt, SeverityDistributionEnt |
| 10 | Models/Sonar – 8 Sonar response models        | ? Done  | Full JSON deserialization models                  |
| 11 | Models/DTOs – 7 DTO files                     | ? Done  | Page-specific DTOs for Angular                    |
| 12 | Repositories/Sync – 5 write repositories      | ? Done  | Pure ADO.NET, stored procedures only              |
| 13 | Repositories/Read – 5 read repositories       | ? Done  | Pure ADO.NET, stored procedures only              |
| 14 | Services/Sonar/SonarApiClient.cs              | ? Done  | Bearer auth, 7 methods, no DB logic               |
| 15 | Services/Sync/SyncService.cs                  | ? Done  | Full project sync flow, no calculation logic      |
| 16 | Background/SonarSyncHostedService.cs          | ? Done  | 6-hour interval background sync                   |
| 17 | Services/Read – 5 read service files          | ? Done  | Data shaping, no DB logic in controllers          |
| 18 | Controllers/Sync/SyncController.cs            | ? Done  | POST /api/sync/{projectKey}, POST /api/sync/all   |
| 19 | Controllers/Read – 5 read controllers         | ? Done  | All 6 frontend API endpoints exposed              |
| 20 | Microsoft.Data.SqlClient package added        | ? Done  | v5.2.2                                            |
| 21 | Build Verification                            | ? Done  | Build successful – 0 errors                       |

---

## ?? Exposed API Endpoints

| Method | Endpoint                                  | Description              |
|--------|-------------------------------------------|--------------------------|
| GET    | /api/projects                             | List all projects        |
| GET    | /api/projects/{id}/header                 | Project header info      |
| GET    | /api/projects/{id}/dashboard              | Full dashboard data      |
| GET    | /api/projects/{id}/metrics                | Metrics + trend charts   |
| GET    | /api/projects/{id}/risk-analysis          | Module risk data         |
| GET    | /api/projects/{id}/quality-gates          | Quality gate status      |
| GET    | /api/projects/{id}/scan-history           | Full scan history        |
| POST   | /api/sync/{projectKey}                    | Manual sync one project  |
| POST   | /api/sync/all                             | Manual sync all projects |

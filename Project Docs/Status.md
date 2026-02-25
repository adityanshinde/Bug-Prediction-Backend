# ?? BugPredictionBackend – Status Tracker

---

## Last Updated: Phase 13 Complete ? (2026-02-25)

---

| #  | Item                                              | Status   | Notes                                                   |
|----|---------------------------------------------------|----------|---------------------------------------------------------|
| 1  | Plan.md created                                   | ? Done  |                                                         |
| 2  | Status.md created                                 | ? Done  |                                                         |
| 3  | SQL/01_CreateDatabase_Tables.sql                  | ? Done  | DB=BPCQDB, 7 tables + indexes (updated 2026-02-25)     |
| 4  | SQL/02_StoredProcedures_Sync.sql                  | ? Done  | 7 SPs for Sonar?DB write operations (updated 2026-02-25)|
| 5  | SQL/03_StoredProcedures_Read.sql                  | ? Done  | 14 SPs for DB?Frontend read APIs (updated 2026-02-25)  |
| 6  | Configurations/SonarSettings.cs                   | ? Done  | POCO bound from appsettings.json                       |
| 7  | appsettings.json updated                          | ? Done  | Connection string, SonarSettings, CORS                 |
| 8  | Program.cs updated                                | ? Done  | Full DI, CORS, Swagger, HttpClient, HostedService       |
| 9  | Models/Entities – 7 entity files                  | ? Done  | +QualityGateConditionEnt, ManualQAEntryEnt (2026-02-25)|
| 10 | Models/Sonar – 8 Sonar response models            | ? Done  | Full JSON deserialization models                       |
| 11 | Models/DTOs – 8 DTO files                         | ? Done  | +QAEntryDto (Request, Response, Summary) (2026-02-25)  |
| 12 | Repositories/Sync – 7 write repositories          | ? Done  | +QualityGateConditionRepository, QAEntryRepository     |
| 13 | Repositories/Read – 6 read repositories           | ? Done  | +QAReadRepository (2026-02-25)                         |
| 14 | Services/Sonar/SonarApiClient.cs                  | ? Done  | Bearer auth, 7 methods                                 |
| 15 | Services/Sync/SyncService.cs                      | ? Done  | +SyncConditionsAsync – saves gate conditions per sync  |
| 16 | Background/SonarSyncHostedService.cs              | ? Done  | 6-hour interval background sync                        |
| 17 | Services/Read – 6 read service files              | ? Done  | +QAService; QualityGateService fixed (2026-02-25)      |
| 18 | Controllers/Sync/SyncController.cs                | ? Done  | POST /api/sync/{projectKey}, POST /api/sync/all        |
| 19 | Controllers/Read – 6 read controllers             | ? Done  | +QAController (2026-02-25)                             |
| 20 | Microsoft.Data.SqlClient package added            | ? Done  | v5.2.2                                                 |
| 21 | QualityGate gateConditions bug fixed              | ? Done  | Was always []. Now reads real conditions from DB       |
| 22 | Manual QA Entry feature added                     | ? Done  | POST+GET /api/projects/{id}/qa-entries                 |
| 23 | Build Verification                                | ? Done  | Build successful – 0 errors                            |

---

## ?? Exposed API Endpoints

| Method | Endpoint                                  | Description                        |
|--------|-------------------------------------------|------------------------------------|
| GET    | /api/projects                             | List all projects                  |
| GET    | /api/projects/{id}/header                 | Project header info                |
| GET    | /api/projects/{id}/dashboard              | Full dashboard data                |
| GET    | /api/projects/{id}/metrics                | Metrics + trend charts             |
| GET    | /api/projects/{id}/risk-analysis          | Module risk data                   |
| GET    | /api/projects/{id}/quality-gates          | Quality gate status + conditions   |
| GET    | /api/projects/{id}/scan-history           | Full scan history                  |
| GET    | /api/projects/{id}/qa-entries             | QA summary + all manual entries    |
| POST   | /api/projects/{id}/qa-entries             | Submit a manual QA entry           |
| POST   | /api/sync/{projectKey}                    | Manual sync one project            |
| POST   | /api/sync/all                             | Manual sync all projects           |

## ?? SQL Scripts to Run in SSMS (2026-02-25 additions)

Run these on your existing `BPCQDB` database — do NOT re-run the full files:

```sql
-- From SQL/01_CreateDatabase_Tables.sql (bottom section added 2026-02-25):
-- QualityGateConditions table
-- ManualQAEntries table
-- New indexes

-- From SQL/02_StoredProcedures_Sync.sql (bottom section added 2026-02-25):
-- sp_InsertQualityGateCondition
-- sp_InsertManualQAEntry

-- From SQL/03_StoredProcedures_Read.sql (bottom section added 2026-02-25):
-- sp_GetConditionsBySnapshot
-- sp_GetManualQAEntries
-- sp_GetQASummary

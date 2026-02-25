# ?? BugPredictionBackend – Implementation Plan

---

## ?? Phase 0 – Project Docs & Planning

- [x] Read all documentation files
- [x] Understand DB schema, Sonar API structure, frontend API contract
- [x] Create Plan.md
- [x] Create Status.md

---

## ?? Phase 1 – SQL Scripts

- [x] Create `SQL/01_CreateDatabase_Tables.sql` – All table creation scripts
- [x] Create `SQL/02_StoredProcedures_Sync.sql` – SPs for Sonar?DB sync (insert/update)
- [x] Create `SQL/03_StoredProcedures_Read.sql` – SPs for DB?Frontend read APIs

---

## ?? Phase 2 – Project Structure & Configuration

- [x] Create folder structure (Controllers, Services, Repositories, Models/Entities, Models/DTOs, Background, Configurations)
- [x] Create `Configurations/SonarSettings.cs`
- [x] Update `appsettings.json` with SonarCloud settings and connection string
- [x] Update `Program.cs` – register all services, HttpClient, CORS, Swagger

---

## ?? Phase 3 – Entities (DB Models)

- [x] `Models/Entities/ProjectEnt.cs`
- [x] `Models/Entities/BranchEnt.cs`
- [x] `Models/Entities/SnapshotEnt.cs`
- [x] `Models/Entities/ModuleMetricEnt.cs`
- [x] `Models/Entities/SeverityDistributionEnt.cs`

---

## ?? Phase 4 – Sonar API Response Models

- [x] `Models/Sonar/SonarProjectEnt.cs`
- [x] `Models/Sonar/SonarBranchEnt.cs`
- [x] `Models/Sonar/SonarMeasureEnt.cs`
- [x] `Models/Sonar/SonarIssueEnt.cs`
- [x] `Models/Sonar/SonarAnalysisEnt.cs`
- [x] `Models/Sonar/SonarQualityGateEnt.cs`
- [x] `Models/Sonar/SonarComponentTreeEnt.cs`
- [x] `Models/Sonar/SonarMeasureHistoryEnt.cs`

---

## ?? Phase 5 – Frontend DTOs

- [x] `Models/DTOs/ProjectListDto.cs`
- [x] `Models/DTOs/HeaderDto.cs`
- [x] `Models/DTOs/DashboardDto.cs`
- [x] `Models/DTOs/MetricsDto.cs`
- [x] `Models/DTOs/QualityGateDto.cs`
- [x] `Models/DTOs/ScanHistoryDto.cs`
- [x] `Models/DTOs/RiskAnalysisDto.cs`

---

## ?? Phase 6 – Repositories (ADO.NET – Sync Write)

- [x] `Repositories/Sync/ProjectRepository.cs`
- [x] `Repositories/Sync/BranchRepository.cs`
- [x] `Repositories/Sync/SnapshotRepository.cs`
- [x] `Repositories/Sync/ModuleRepository.cs`
- [x] `Repositories/Sync/SeverityRepository.cs`

---

## ?? Phase 7 – Repositories (ADO.NET – Read for Frontend)

- [x] `Repositories/Read/ProjectReadRepository.cs`
- [x] `Repositories/Read/DashboardReadRepository.cs`
- [x] `Repositories/Read/MetricsReadRepository.cs`
- [x] `Repositories/Read/QualityGateReadRepository.cs`
- [x] `Repositories/Read/ScanHistoryReadRepository.cs`

---

## ?? Phase 8 – Sonar API Client

- [x] `Services/Sonar/SonarApiClient.cs`

---

## ? Phase 9 – Sync Services (Sonar ? DB)

- [x] `Services/Sync/SyncService.cs`
- [x] `Background/SonarSyncHostedService.cs`

---

## ?? Phase 10 – Read Services (DB ? Frontend)

- [x] `Services/Read/ProjectService.cs`
- [x] `Services/Read/DashboardService.cs`
- [x] `Services/Read/MetricsService.cs`
- [x] `Services/Read/QualityGateService.cs`
- [x] `Services/Read/ScanHistoryService.cs`

---

## ?? Phase 11 – Controllers

- [x] `Controllers/Sync/SyncController.cs` (POST /api/sync/{projectKey})
- [x] `Controllers/Read/ProjectsController.cs`
- [x] `Controllers/Read/DashboardController.cs`
- [x] `Controllers/Read/MetricsController.cs`
- [x] `Controllers/Read/QualityGatesController.cs`
- [x] `Controllers/Read/ScanHistoryController.cs`

---

## ? Phase 12 – Final

- [x] Build verification
- [x] Update Status.md

---

## ?? Phase 13 – QA Analysis & Quality Gate Conditions Fix (2026-02-25)

- [x] `SQL/01_` – Add `QualityGateConditions` table + `ManualQAEntries` table + indexes
- [x] `SQL/02_` – Add `sp_InsertQualityGateCondition` + `sp_InsertManualQAEntry`
- [x] `SQL/03_` – Add `sp_GetConditionsBySnapshot` + `sp_GetManualQAEntries` + `sp_GetQASummary`
- [x] `Models/Entities/QualityGateConditionEnt.cs`
- [x] `Models/Entities/ManualQAEntryEnt.cs`
- [x] `Models/DTOs/QAEntryDto.cs` – QAEntryRequestDto, QAEntryResponseDto, QASummaryDto
- [x] `Repositories/Sync/QualityGateConditionRepository.cs`
- [x] `Repositories/Sync/QAEntryRepository.cs`
- [x] `Repositories/Read/QAReadRepository.cs`
- [x] `Services/Sync/SyncService.cs` – wired `SyncConditionsAsync` to save per-condition data
- [x] `Services/Read/QualityGateService.cs` – now populates `GateConditions` from DB
- [x] `Services/Read/QAService.cs`
- [x] `Controllers/Read/QAController.cs` – POST + GET /api/projects/{id}/qa-entries
- [x] `Program.cs` – registered all new repos and services
- [x] Build verification – 0 errors

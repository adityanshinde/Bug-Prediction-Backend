
---

# 📘 SonarCloud Data Aggregation Platform

# Backend Master Implementation Plan

---

# 0️⃣ Objective

Build a production-grade ASP.NET Core backend that:

1. Authenticates with SonarCloud
2. Fetches required project data
3. Normalizes and stores data in SQL Server
4. Maintains historical snapshots
5. Exposes structured REST APIs
6. Does NOT perform risk calculations
7. Does NOT expose Sonar token
8. Supports scheduled background sync
9. Uses pure ADO.NET (no EF)

Frontend will handle:

* Risk Score
* Risk Classification
* Trend logic
* Health Score
* Severity weighted scoring

Backend only provides raw structured metrics.

---

# 1️⃣ Technology Stack

| Layer           | Technology                    |
| --------------- | ----------------------------- |
| Framework       | ASP.NET Core Web API (.NET 8) |
| Data Access     | ADO.NET                       |
| Database        | SQL Server                    |
| HTTP Client     | IHttpClientFactory            |
| Background Sync | IHostedService                |
| Logging         | ILogger                       |
| Serialization   | System.Text.Json              |

---

# 2️⃣ System Architecture

```id="arch01"
SonarCloud API
       ↓
SonarApiClient
       ↓
Sync Background Service
       ↓
Normalization Layer
       ↓
SQL Server (Historical Snapshots)
       ↓
REST Controllers
       ↓
Angular Frontend
```

---

# 3️⃣ Solution Folder Structure

```id="fs01"
SonarAnalytics.API
│
├── Controllers
│   ├── ProjectsController.cs
│   ├── DashboardController.cs
│   ├── MetricsController.cs
│   ├── QualityGatesController.cs
│
├── Services
│   ├── SonarApiClient.cs
│   ├── SyncService.cs
│   ├── DashboardService.cs
│   ├── MetricsService.cs
│   ├── QualityGateService.cs
│
├── Repositories
│   ├── ProjectRepository.cs
│   ├── BranchRepository.cs
│   ├── SnapshotRepository.cs
│   ├── ModuleRepository.cs
│   ├── SeverityRepository.cs
│
├── Models
│   ├── Entities
│   ├── DTOs
│
├── Background
│   ├── SonarSyncHostedService.cs
│
├── Configurations
│   ├── SonarSettings.cs
│
└── Program.cs
```

---

# 4️⃣ Database Design (Final Schema)

## 4.1 Projects

* Id (PK)
* ProjectKey
* Name
* Organization
* Visibility
* LastAnalysisDate
* CreatedDate

---

## 4.2 Branches

* Id
* ProjectId (FK)
* BranchName
* IsMain
* AnalysisDate

---

## 4.3 Snapshots

Stores scan-wise metrics.

* Id
* ProjectId
* BranchId
* ScanDate
* CommitId
* Bugs
* Vulnerabilities
* CodeSmells
* Coverage
* Duplication
* SecurityRating
* ReliabilityRating
* MaintainabilityRating
* QualityGateStatus

No RiskScore column here.

---

## 4.4 ModuleMetrics

* Id
* SnapshotId
* ModuleName
* Bugs
* Vulnerabilities
* CodeSmells
* Coverage
* Duplication
* Complexity
* Ncloc

---

## 4.5 SeverityDistribution

* Id
* SnapshotId
* Blocker
* Critical
* Major
* Minor
* Info

---

# 5️⃣ Phase 1 – Sonar API Client Layer

Reference mapping should follow the APIs defined in your backend API guide.

## 5.1 Create SonarApiClient

Responsibilities:

* Add Bearer token header
* Use IHttpClientFactory
* Handle API errors
* Deserialize only required fields

Must implement:

* GetProjects()
* GetBranches(projectKey)
* GetProjectMetrics(projectKey)
* GetSeverityDistribution(projectKey)
* GetScanHistory(projectKey)
* GetModuleMetrics(projectKey)
* GetQualityGateStatus(projectKey)
* GetCoverageHistory(projectKey)

Token must be stored in:

```id="cfg01"
appsettings.json
```

Never hardcoded.

---

# 6️⃣ Phase 2 – Data Normalization

Goal:

Convert Sonar JSON → Strongly typed domain models.

Do NOT store:

* Raw JSON blobs
* Entire API responses

Extract only needed fields.

Example conversion:

```id="norm01"
Sonar metric "bugs" → int Bugs
Sonar metric "coverage" → decimal Coverage
```

All mapping logic must be isolated inside:

```id="norm02"
SonarApiClient OR MappingHelper
```

Controllers must never parse raw Sonar responses.

---

# 7️⃣ Phase 3 – Background Sync Engine

Create:

```id="sync01"
SonarSyncHostedService
```

This service runs every 6 hours.

Flow:

1. Fetch project list
2. For each project:

   * Fetch branches
   * Fetch latest metrics
   * Fetch severity distribution
   * Fetch module breakdown
   * Fetch quality gate
   * Fetch scan metadata
3. Create new Snapshot record
4. Insert related ModuleMetrics
5. Insert SeverityDistribution
6. Update Project LastAnalysisDate

Important:

* Never overwrite old snapshot
* Snapshots are immutable
* Every scan creates new record

---

# 8️⃣ Phase 4 – Repository Layer (Pure ADO.NET)

Rules:

* Use SqlConnection
* Use SqlCommand
* Use Stored Procedures
* Use parameterized queries
* No dynamic SQL

Repositories must support:

ProjectRepository

* InsertOrUpdateProject
* GetAllProjects
* GetProjectById

BranchRepository

* InsertOrUpdateBranch

SnapshotRepository

* InsertSnapshot
* GetLatestSnapshot
* GetSnapshotsByProject

ModuleRepository

* InsertModuleMetrics
* GetModulesBySnapshot

SeverityRepository

* InsertSeverityDistribution
* GetSeverityBySnapshot

Add indexes on:

* ProjectId
* SnapshotId
* ScanDate

---

# 9️⃣ Phase 5 – API Layer (Frontend-Focused Design)

Backend must match frontend contract exactly.

Expose:

```id="api01"
GET /api/projects
GET /api/projects/{id}/header
GET /api/projects/{id}/dashboard
GET /api/projects/{id}/metrics
GET /api/projects/{id}/quality-gates
GET /api/projects/{id}/scan-history
POST /api/sync/{projectKey}
```

Important rules:

* Return DTOs
* Never expose DB schema directly
* Aggregate page-specific data
* No business calculation logic

---

# 🔟 Phase 6 – Data Aggregation Services

Even without calculations, backend must:

DashboardService:

* Fetch latest snapshot
* Fetch severity distribution
* Return structured dashboard DTO

MetricsService:

* Return coverage history
* Return module metrics
* Return bugs vs vulnerabilities history

QualityGateService:

* Return current gate status
* Return gate history

Controllers only call services.

No DB logic inside controllers.

---

# 1️⃣1️⃣ Security Guidelines

* Store token securely
* Never expose Sonar endpoints
* Enable CORS for Angular domain
* Validate projectId exists
* Return 404 if invalid
* Protect sync endpoint (optional JWT)

---

# 1️⃣2️⃣ Logging Strategy

Log:

* Sync start & completion
* Project count
* Snapshot insertion
* Sonar API failures
* DB insertion errors

Use structured logging.

---

# 1️⃣3️⃣ Error Handling

* Wrap external API calls in try/catch
* Do not stop entire sync if one project fails
* Return meaningful HTTP status codes
* Avoid exposing internal exceptions

---

# 1️⃣4️⃣ Performance Considerations

* Use latest snapshot only for dashboard
* Add DB indexes
* Consider in-memory caching for header data
* Avoid calling Sonar from API endpoints
* Only background service calls Sonar

---

# 1️⃣5️⃣ Deployment Preparation

* Add Swagger
* Add production connection string
* Configure environment-based settings
* Add health check endpoint
* Enable HTTPS redirection

---

# 1️⃣6️⃣ Final Backend Responsibilities

Backend MUST:

* Securely fetch SonarCloud data
* Normalize responses
* Maintain historical scan snapshots
* Store module-level metrics
* Store severity distribution
* Expose clean structured APIs
* Support scheduled synchronization
* Keep logic separated and layered

Backend MUST NOT:

* Calculate risk score
* Calculate trends
* Compute health metrics
* Perform classification logic

That responsibility belongs to Angular.

---
# DATABSE CREATION & TABLE SCHEMA
---
# 📦 SECTION 1 – COMPLETE SQL SCRIPT PACK

---

# 1️⃣ Database Creation

```sql
CREATE DATABASE SonarAnalyticsDB;
GO
USE SonarAnalyticsDB;
GO
```

---

# 2️⃣ TABLE CREATION SCRIPTS

---

## 2.1 Projects

```sql
CREATE TABLE Projects
(
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProjectKey NVARCHAR(200) NOT NULL UNIQUE,
    Name NVARCHAR(300) NOT NULL,
    Organization NVARCHAR(200),
    Visibility NVARCHAR(50),
    LastAnalysisDate DATETIME NULL,
    CreatedDate DATETIME DEFAULT GETDATE()
);
```

---

## 2.2 Branches

```sql
CREATE TABLE Branches
(
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProjectId INT NOT NULL,
    BranchName NVARCHAR(200) NOT NULL,
    IsMain BIT NOT NULL,
    AnalysisDate DATETIME NULL,
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id)
);
```

---

## 2.3 Snapshots

```sql
CREATE TABLE Snapshots
(
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProjectId INT NOT NULL,
    BranchId INT NULL,
    ScanDate DATETIME NOT NULL,
    CommitId NVARCHAR(200) NULL,
    Bugs INT,
    Vulnerabilities INT,
    CodeSmells INT,
    Coverage DECIMAL(5,2),
    Duplication DECIMAL(5,2),
    SecurityRating NVARCHAR(10),
    ReliabilityRating NVARCHAR(10),
    MaintainabilityRating NVARCHAR(10),
    QualityGateStatus NVARCHAR(50),
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    FOREIGN KEY (BranchId) REFERENCES Branches(Id)
);
```

---

## 2.4 ModuleMetrics

```sql
CREATE TABLE ModuleMetrics
(
    Id INT PRIMARY KEY IDENTITY(1,1),
    SnapshotId INT NOT NULL,
    ModuleName NVARCHAR(500),
    Bugs INT,
    Vulnerabilities INT,
    CodeSmells INT,
    Coverage DECIMAL(5,2),
    Duplication DECIMAL(5,2),
    Complexity INT,
    Ncloc INT,
    FOREIGN KEY (SnapshotId) REFERENCES Snapshots(Id)
);
```

---

## 2.5 SeverityDistribution

```sql
CREATE TABLE SeverityDistribution
(
    Id INT PRIMARY KEY IDENTITY(1,1),
    SnapshotId INT NOT NULL,
    Blocker INT,
    Critical INT,
    Major INT,
    Minor INT,
    Info INT,
    FOREIGN KEY (SnapshotId) REFERENCES Snapshots(Id)
);
```

---

# 3️⃣ INDEXES (IMPORTANT FOR PERFORMANCE)

```sql
CREATE INDEX IX_Snapshots_ProjectId ON Snapshots(ProjectId);
CREATE INDEX IX_Snapshots_ScanDate ON Snapshots(ScanDate);
CREATE INDEX IX_ModuleMetrics_SnapshotId ON ModuleMetrics(SnapshotId);
CREATE INDEX IX_Severity_SnapshotId ON SeverityDistribution(SnapshotId);
```

---

# 4️⃣ STORED PROCEDURES

---

## 4.1 Insert Or Update Project

```sql
CREATE PROCEDURE sp_InsertOrUpdateProject
    @ProjectKey NVARCHAR(200),
    @Name NVARCHAR(300),
    @Organization NVARCHAR(200),
    @Visibility NVARCHAR(50),
    @LastAnalysisDate DATETIME
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Projects WHERE ProjectKey = @ProjectKey)
    BEGIN
        UPDATE Projects
        SET Name = @Name,
            Organization = @Organization,
            Visibility = @Visibility,
            LastAnalysisDate = @LastAnalysisDate
        WHERE ProjectKey = @ProjectKey;

        SELECT Id FROM Projects WHERE ProjectKey = @ProjectKey;
    END
    ELSE
    BEGIN
        INSERT INTO Projects (ProjectKey, Name, Organization, Visibility, LastAnalysisDate)
        VALUES (@ProjectKey, @Name, @Organization, @Visibility, @LastAnalysisDate);

        SELECT SCOPE_IDENTITY() AS Id;
    END
END
```

---

## 4.2 Insert Branch

```sql
CREATE PROCEDURE sp_InsertOrUpdateBranch
    @ProjectId INT,
    @BranchName NVARCHAR(200),
    @IsMain BIT,
    @AnalysisDate DATETIME
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Branches WHERE ProjectId = @ProjectId AND BranchName = @BranchName)
    BEGIN
        UPDATE Branches
        SET IsMain = @IsMain,
            AnalysisDate = @AnalysisDate
        WHERE ProjectId = @ProjectId AND BranchName = @BranchName;

        SELECT Id FROM Branches WHERE ProjectId = @ProjectId AND BranchName = @BranchName;
    END
    ELSE
    BEGIN
        INSERT INTO Branches (ProjectId, BranchName, IsMain, AnalysisDate)
        VALUES (@ProjectId, @BranchName, @IsMain, @AnalysisDate);

        SELECT SCOPE_IDENTITY() AS Id;
    END
END
```

---

## 4.3 Insert Snapshot

```sql
CREATE PROCEDURE sp_InsertSnapshot
    @ProjectId INT,
    @BranchId INT,
    @ScanDate DATETIME,
    @CommitId NVARCHAR(200),
    @Bugs INT,
    @Vulnerabilities INT,
    @CodeSmells INT,
    @Coverage DECIMAL(5,2),
    @Duplication DECIMAL(5,2),
    @SecurityRating NVARCHAR(10),
    @ReliabilityRating NVARCHAR(10),
    @MaintainabilityRating NVARCHAR(10),
    @QualityGateStatus NVARCHAR(50)
AS
BEGIN
    INSERT INTO Snapshots
    (
        ProjectId, BranchId, ScanDate, CommitId,
        Bugs, Vulnerabilities, CodeSmells,
        Coverage, Duplication,
        SecurityRating, ReliabilityRating,
        MaintainabilityRating, QualityGateStatus
    )
    VALUES
    (
        @ProjectId, @BranchId, @ScanDate, @CommitId,
        @Bugs, @Vulnerabilities, @CodeSmells,
        @Coverage, @Duplication,
        @SecurityRating, @ReliabilityRating,
        @MaintainabilityRating, @QualityGateStatus
    );

    SELECT SCOPE_IDENTITY() AS Id;
END
```

---

## 4.4 Insert Module Metrics

```sql
CREATE PROCEDURE sp_InsertModuleMetric
    @SnapshotId INT,
    @ModuleName NVARCHAR(500),
    @Bugs INT,
    @Vulnerabilities INT,
    @CodeSmells INT,
    @Coverage DECIMAL(5,2),
    @Duplication DECIMAL(5,2),
    @Complexity INT,
    @Ncloc INT
AS
BEGIN
    INSERT INTO ModuleMetrics
    (
        SnapshotId, ModuleName,
        Bugs, Vulnerabilities, CodeSmells,
        Coverage, Duplication, Complexity, Ncloc
    )
    VALUES
    (
        @SnapshotId, @ModuleName,
        @Bugs, @Vulnerabilities, @CodeSmells,
        @Coverage, @Duplication, @Complexity, @Ncloc
    );
END
```

---

## 4.5 Insert Severity Distribution

```sql
CREATE PROCEDURE sp_InsertSeverityDistribution
    @SnapshotId INT,
    @Blocker INT,
    @Critical INT,
    @Major INT,
    @Minor INT,
    @Info INT
AS
BEGIN
    INSERT INTO SeverityDistribution
    (
        SnapshotId, Blocker, Critical, Major, Minor, Info
    )
    VALUES
    (
        @SnapshotId, @Blocker, @Critical, @Major, @Minor, @Info
    );
END
```

---

# 📦 SECTION 2 – STEP-BY-STEP CODING INSTRUCTION GUIDE

---

# 1️⃣ SonarSettings.cs

* Create POCO class
* Properties:

  * BaseUrl
  * Token
  * Organization
* Bind from appsettings.json

---

# 2️⃣ Program.cs Setup

* Register:

  * AddHttpClient()
  * AddHostedService<SonarSyncHostedService>()
  * AddScoped repositories
  * AddScoped services
* Configure CORS
* Enable Swagger

---

# 3️⃣ SonarApiClient.cs

Responsibilities:

* Inject IHttpClientFactory
* Read SonarSettings
* Add Bearer token header
* Create private method SendRequest(string endpoint)
* Deserialize JSON into typed models
* No DB logic here

Methods to implement:

* GetProjectsAsync()
* GetBranchesAsync(projectKey)
* GetMetricsAsync(projectKey)
* GetModuleMetricsAsync(projectKey)
* GetSeverityAsync(projectKey)
* GetQualityGateAsync(projectKey)
* GetScanHistoryAsync(projectKey)

---

# 4️⃣ Repository Classes (ADO.NET Pattern)

Each repository must:

* Inject IConfiguration
* Use SqlConnection inside using block
* Call stored procedures
* Use SqlCommand.CommandType = StoredProcedure
* Add parameters properly
* Return inserted Id when needed

Example structure:

```csharp
public class ProjectRepository
{
    private readonly string _connectionString;

    public ProjectRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public async Task<int> InsertOrUpdateAsync(ProjectEntity entity)
    {
        using SqlConnection con = new SqlConnection(_connectionString);
        using SqlCommand cmd = new SqlCommand("sp_InsertOrUpdateProject", con);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@ProjectKey", entity.ProjectKey);
        // add remaining parameters

        await con.OpenAsync();
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }
}
```

Repeat similar structure for:

* BranchRepository
* SnapshotRepository
* ModuleRepository
* SeverityRepository

---

# 5️⃣ SonarSyncHostedService.cs

Steps:

1. In ExecuteAsync loop:

   * Call SyncService.SyncAllProjects()
2. Add delay of 6 hours
3. Wrap entire sync in try/catch
4. Log start & end

---

# 6️⃣ SyncService.cs

Responsibilities:

1. Fetch projects from SonarApiClient
2. Insert/update project
3. Fetch branches
4. Fetch metrics
5. Fetch severity distribution
6. Fetch module metrics
7. Insert snapshot
8. Insert module metrics
9. Insert severity distribution

No business calculations.

Pure data persistence.

---

# 7️⃣ DashboardService.cs

Responsibilities:

* Fetch latest snapshot
* Fetch severity distribution
* Shape DTO
* Return structured dashboard model

No DB logic inside controller.

---

# 8️⃣ MetricsService.cs

Responsibilities:

* Fetch coverage history
* Fetch module metrics
* Fetch historical bugs/vulnerabilities
* Return DTO for metrics page

---

# 9️⃣ QualityGateService.cs

Responsibilities:

* Get latest snapshot quality gate
* Get historical gate changes
* Return clean DTO

---

# 🔟 Controllers

Each controller:

* Inject service
* Call service
* Return Ok(result)
* Handle NotFound if project missing

Controllers must NOT:

* Call Sonar API directly
* Access DB directly
* Perform transformation logic

---

# 1️⃣1️⃣ DTO Layer

Create DTOs per page:

* ProjectListDto
* HeaderDto
* DashboardDto
* MetricsDto
* QualityGateDto
* ScanHistoryDto

Never return entity models directly.

---

# 1️⃣2️⃣ Final Development Order

Follow this exact sequence:

1. Create Database
2. Create Tables
3. Create Stored Procedures
4. Create Entities
5. Create Repositories
6. Create SonarApiClient
7. Create SyncService
8. Create HostedService
9. Test Sync Manually
10. Create Services (Dashboard/Metrics/etc.)
11. Create Controllers
12. Test APIs via Swagger
13. Connect Angular

---

# ✅ Final Outcome

Backend will:

* Securely sync SonarCloud data
* Store historical snapshots
* Store module-level metrics
* Store severity distribution
* Expose structured REST APIs
* Support scheduled sync
* Be fully ADO.NET based
* Be production deployable

---

-- ============================================================
-- Script : 03_StoredProcedures_Read.sql
-- Purpose: Stored procedures for DB ? Frontend read APIs
-- DB     : BPCQDB | SQL Server 2022
-- ============================================================

USE BPCQDB;
GO

-- ============================================================
-- SP: sp_GetAllProjects
-- Used by: GET /api/projects
-- ============================================================
CREATE PROCEDURE sp_GetAllProjects
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Id,
        p.ProjectKey,
        p.Name,
        p.Organization,
        p.Visibility,
        p.LastAnalysisDate,
        p.CreatedDate
    FROM Projects p
    ORDER BY p.Name ASC;
END
GO

-- ============================================================
-- SP: sp_GetProjectById
-- Used by: header, dashboard, metrics, quality-gates, scan-history
-- ============================================================
CREATE PROCEDURE sp_GetProjectById
    @ProjectId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id, ProjectKey, Name, Organization,
        Visibility, LastAnalysisDate, CreatedDate
    FROM Projects
    WHERE Id = @ProjectId;
END
GO

-- ============================================================
-- SP: sp_GetProjectHeader
-- Used by: GET /api/projects/{id}/header
-- Returns: latest snapshot joined with project and main branch
-- ============================================================
CREATE PROCEDURE sp_GetProjectHeader
    @ProjectId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        p.Name                  AS ProjectName,
        b.BranchName            AS Branch,
        s.ScanDate              AS LastScanDate,
        s.QualityGateStatus,
        s.CommitId
    FROM   Snapshots s
    JOIN   Projects  p ON p.Id = s.ProjectId
    LEFT JOIN Branches b ON b.Id = s.BranchId
    WHERE  s.ProjectId = @ProjectId
    ORDER BY s.ScanDate DESC;
END
GO

-- ============================================================
-- SP: sp_GetLatestSnapshot
-- Used by: DashboardService, internal use
-- ============================================================
CREATE PROCEDURE sp_GetLatestSnapshot
    @ProjectId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        s.Id, s.ProjectId, s.BranchId, s.ScanDate, s.CommitId,
        s.Bugs, s.Vulnerabilities, s.CodeSmells,
        s.Coverage, s.Duplication,
        s.SecurityRating, s.ReliabilityRating, s.MaintainabilityRating,
        s.QualityGateStatus
    FROM Snapshots s
    WHERE s.ProjectId = @ProjectId
    ORDER BY s.ScanDate DESC;
END
GO

-- ============================================================
-- SP: sp_GetSeverityBySnapshot
-- Used by: DashboardService
-- ============================================================
CREATE PROCEDURE sp_GetSeverityBySnapshot
    @SnapshotId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Blocker, Critical, Major, Minor, Info
    FROM   SeverityDistribution
    WHERE  SnapshotId = @SnapshotId;
END
GO

-- ============================================================
-- SP: sp_GetRecentScans
-- Used by: GET /api/projects/{id}/dashboard (recentScans)
--          GET /api/projects/{id}/scan-history
-- ============================================================
CREATE PROCEDURE sp_GetRecentScans
    @ProjectId INT,
    @TopN      INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@TopN)
        s.ScanDate,
        b.BranchName  AS Branch,
        s.CommitId,
        s.QualityGateStatus
    FROM   Snapshots s
    LEFT JOIN Branches b ON b.Id = s.BranchId
    WHERE  s.ProjectId = @ProjectId
    ORDER BY s.ScanDate DESC;
END
GO

-- ============================================================
-- SP: sp_GetSnapshotsByProject
-- Used by: scan-history (full list)
-- ============================================================
CREATE PROCEDURE sp_GetSnapshotsByProject
    @ProjectId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.Id, s.ScanDate,
        b.BranchName  AS Branch,
        s.CommitId,
        s.QualityGateStatus,
        s.Bugs, s.Vulnerabilities, s.CodeSmells,
        s.Coverage, s.Duplication
    FROM   Snapshots s
    LEFT JOIN Branches b ON b.Id = s.BranchId
    WHERE  s.ProjectId = @ProjectId
    ORDER BY s.ScanDate DESC;
END
GO

-- ============================================================
-- SP: sp_GetModulesBySnapshot
-- Used by: MetricsService (module metrics table)
--          RiskAnalysis
-- ============================================================
CREATE PROCEDURE sp_GetModulesBySnapshot
    @SnapshotId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ModuleName, Qualifier, Path, Language,
        Bugs, Vulnerabilities, CodeSmells,
        Coverage, Duplication, Complexity, Ncloc
    FROM   ModuleMetrics
    WHERE  SnapshotId = @SnapshotId
    ORDER BY Bugs DESC, Vulnerabilities DESC;
END
GO

-- ============================================================
-- SP: sp_GetCoverageHistory
-- Used by: MetricsService (coverage trend)
-- ============================================================
CREATE PROCEDURE sp_GetCoverageHistory
    @ProjectId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.ScanDate AS Date,
        s.Coverage
    FROM   Snapshots s
    WHERE  s.ProjectId = @ProjectId
    ORDER BY s.ScanDate ASC;
END
GO

-- ============================================================
-- SP: sp_GetBugsVulnerabilitiesHistory
-- Used by: MetricsService (bugs vs vulnerabilities chart)
-- ============================================================
CREATE PROCEDURE sp_GetBugsVulnerabilitiesHistory
    @ProjectId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.ScanDate AS Date,
        s.Bugs,
        s.Vulnerabilities
    FROM   Snapshots s
    WHERE  s.ProjectId = @ProjectId
    ORDER BY s.ScanDate ASC;
END
GO

-- ============================================================
-- SP: sp_GetQualityGateHistory
-- Used by: QualityGateService (gate history table)
-- ============================================================
CREATE PROCEDURE sp_GetQualityGateHistory
    @ProjectId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.ScanDate AS Date,
        b.BranchName AS Branch,
        s.QualityGateStatus AS Status,
        s.CommitId
    FROM   Snapshots s
    LEFT JOIN Branches b ON b.Id = s.BranchId
    WHERE  s.ProjectId = @ProjectId
    ORDER BY s.ScanDate DESC;
END
GO

-- ============================================================
-- SP: sp_GetMetricsKpis
-- Used by: MetricsService (KPIs card row)
-- ============================================================
CREATE PROCEDURE sp_GetMetricsKpis
    @ProjectId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        Bugs, CodeSmells, Coverage, Duplication
    FROM   Snapshots
    WHERE  ProjectId = @ProjectId
    ORDER BY ScanDate DESC;
END
GO

-- ============================================================
-- ADDED: 2026-02-25
-- SP: sp_GetConditionsBySnapshot
-- Used by: QualityGateService (gateConditions array)
-- ============================================================
CREATE PROCEDURE sp_GetConditionsBySnapshot
    @SnapshotId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MetricKey,
        Comparator,
        ErrorThreshold,
        ActualValue,
        Status
    FROM QualityGateConditions
    WHERE SnapshotId = @SnapshotId
    ORDER BY
        CASE Status WHEN 'ERROR' THEN 0 ELSE 1 END ASC,
        MetricKey ASC;
END
GO

-- ============================================================
-- ADDED: 2026-02-25
-- SP: sp_GetManualQAEntries
-- Used by: GET /api/projects/{id}/qa-entries
-- ============================================================
CREATE PROCEDURE sp_GetManualQAEntries
    @ProjectId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        ProjectId,
        ModuleName,
        IssueType,
        Severity,
        Description,
        ReportedBy,
        EntryDate
    FROM ManualQAEntries
    WHERE ProjectId = @ProjectId
    ORDER BY EntryDate DESC;
END
GO

-- ============================================================
-- ADDED: 2026-02-25
-- SP: sp_GetQASummary
-- Used by: GET /api/projects/{id}/qa-entries (summary card counts)
-- Compares manual QA entries against latest snapshot metrics
-- ============================================================
CREATE PROCEDURE sp_GetQASummary
    @ProjectId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Total manual entries submitted for this project
    SELECT
        COUNT(*)                                           AS TotalEntries,
        SUM(CASE WHEN IssueType = 'Bug'           THEN 1 ELSE 0 END) AS BugEntries,
        SUM(CASE WHEN IssueType = 'Vulnerability' THEN 1 ELSE 0 END) AS VulnerabilityEntries,
        SUM(CASE WHEN IssueType = 'Code Smell'    THEN 1 ELSE 0 END) AS CodeSmellEntries,
        SUM(CASE WHEN Severity  = 'Critical'      THEN 1 ELSE 0 END) AS CriticalCount,
        SUM(CASE WHEN Severity  = 'High'          THEN 1 ELSE 0 END) AS HighCount,
        SUM(CASE WHEN Severity  = 'Medium'        THEN 1 ELSE 0 END) AS MediumCount,
        SUM(CASE WHEN Severity  = 'Low'           THEN 1 ELSE 0 END) AS LowCount
    FROM ManualQAEntries
    WHERE ProjectId = @ProjectId;
END
GO

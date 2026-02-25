-- ============================================================
-- Script : 01_CreateDatabase_Tables.sql
-- Purpose: Create BPCQDB database and all tables with indexes
-- DB     : SQL Server 2022
-- ============================================================

CREATE DATABASE BPCQDB;
GO

USE BPCQDB;
GO

-- ============================================================
-- TABLE: Projects
-- ============================================================
CREATE TABLE Projects
(
    Id              INT           PRIMARY KEY IDENTITY(1,1),
    ProjectKey      NVARCHAR(200) NOT NULL UNIQUE,
    Name            NVARCHAR(300) NOT NULL,
    Organization    NVARCHAR(200) NULL,
    Visibility      NVARCHAR(50)  NULL,
    LastAnalysisDate DATETIME     NULL,
    CreatedDate     DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ============================================================
-- TABLE: Branches
-- ============================================================
CREATE TABLE Branches
(
    Id           INT           PRIMARY KEY IDENTITY(1,1),
    ProjectId    INT           NOT NULL,
    BranchName   NVARCHAR(200) NOT NULL,
    IsMain       BIT           NOT NULL DEFAULT 0,
    AnalysisDate DATETIME      NULL,
    CONSTRAINT FK_Branches_Projects FOREIGN KEY (ProjectId) REFERENCES Projects(Id)
);
GO

-- ============================================================
-- TABLE: Snapshots
-- ============================================================
CREATE TABLE Snapshots
(
    Id                    INT            PRIMARY KEY IDENTITY(1,1),
    ProjectId             INT            NOT NULL,
    BranchId              INT            NULL,
    ScanDate              DATETIME       NOT NULL,
    CommitId              NVARCHAR(200)  NULL,
    Bugs                  INT            NULL DEFAULT 0,
    Vulnerabilities       INT            NULL DEFAULT 0,
    CodeSmells            INT            NULL DEFAULT 0,
    Coverage              DECIMAL(5,2)   NULL DEFAULT 0,
    Duplication           DECIMAL(5,2)   NULL DEFAULT 0,
    SecurityRating        NVARCHAR(10)   NULL,
    ReliabilityRating     NVARCHAR(10)   NULL,
    MaintainabilityRating NVARCHAR(10)   NULL,
    QualityGateStatus     NVARCHAR(50)   NULL,
    CreatedDate           DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Snapshots_Projects  FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    CONSTRAINT FK_Snapshots_Branches  FOREIGN KEY (BranchId)  REFERENCES Branches(Id)
);
GO

-- ============================================================
-- TABLE: ModuleMetrics
-- ============================================================
CREATE TABLE ModuleMetrics
(
    Id              INT            PRIMARY KEY IDENTITY(1,1),
    SnapshotId      INT            NOT NULL,
    ModuleName      NVARCHAR(500)  NULL,
    Qualifier       NVARCHAR(10)   NULL,   -- DIR or FIL
    Path            NVARCHAR(1000) NULL,
    Language        NVARCHAR(50)   NULL,
    Bugs            INT            NULL DEFAULT 0,
    Vulnerabilities INT            NULL DEFAULT 0,
    CodeSmells      INT            NULL DEFAULT 0,
    Coverage        DECIMAL(5,2)   NULL DEFAULT 0,
    Duplication     DECIMAL(5,2)   NULL DEFAULT 0,
    Complexity      INT            NULL DEFAULT 0,
    Ncloc           INT            NULL DEFAULT 0,
    CONSTRAINT FK_ModuleMetrics_Snapshots FOREIGN KEY (SnapshotId) REFERENCES Snapshots(Id)
);
GO

-- ============================================================
-- TABLE: SeverityDistribution
-- ============================================================
CREATE TABLE SeverityDistribution
(
    Id         INT PRIMARY KEY IDENTITY(1,1),
    SnapshotId INT NOT NULL,
    Blocker    INT NULL DEFAULT 0,
    Critical   INT NULL DEFAULT 0,
    Major      INT NULL DEFAULT 0,
    Minor      INT NULL DEFAULT 0,
    Info       INT NULL DEFAULT 0,
    CONSTRAINT FK_Severity_Snapshots FOREIGN KEY (SnapshotId) REFERENCES Snapshots(Id)
);
GO

-- ============================================================
-- ADDED: 2026-02-25
-- TABLE: QualityGateConditions
-- Stores per-condition details from qualitygates/project_status
-- ============================================================
CREATE TABLE QualityGateConditions
(
    Id             INT           PRIMARY KEY IDENTITY(1,1),
    SnapshotId     INT           NOT NULL,
    MetricKey      NVARCHAR(100) NOT NULL,
    Comparator     NVARCHAR(10)  NULL,   -- GT / LT
    ErrorThreshold NVARCHAR(50)  NULL,
    ActualValue    NVARCHAR(50)  NULL,
    Status         NVARCHAR(10)  NOT NULL, -- OK / ERROR
    CONSTRAINT FK_QGConditions_Snapshots FOREIGN KEY (SnapshotId) REFERENCES Snapshots(Id)
);
GO

-- ============================================================
-- ADDED: 2026-02-25
-- TABLE: ManualQAEntries
-- Stores manual QA form submissions per project
-- ============================================================
CREATE TABLE ManualQAEntries
(
    Id            INT            PRIMARY KEY IDENTITY(1,1),
    ProjectId     INT            NOT NULL,
    ModuleName    NVARCHAR(500)  NOT NULL,
    IssueType     NVARCHAR(100)  NOT NULL,   -- Bug / Vulnerability / Code Smell
    Severity      NVARCHAR(50)   NOT NULL,   -- Critical / High / Medium / Low
    Description   NVARCHAR(2000) NULL,
    ReportedBy    NVARCHAR(200)  NULL,
    EntryDate     DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_ManualQA_Projects FOREIGN KEY (ProjectId) REFERENCES Projects(Id)
);
GO

-- ============================================================
-- ADDED: 2026-02-25
-- INDEXES for new tables
-- ============================================================
CREATE INDEX IX_QGConditions_SnapshotId  ON QualityGateConditions(SnapshotId);
CREATE INDEX IX_ManualQA_ProjectId       ON ManualQAEntries(ProjectId);
GO

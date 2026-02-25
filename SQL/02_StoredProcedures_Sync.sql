-- ============================================================
-- Script : 02_StoredProcedures_Sync.sql
-- Purpose: Stored procedures for Sonar ? DB sync operations
-- DB     : BPCQDB | SQL Server 2022
-- ============================================================

USE BPCQDB;
GO

-- ============================================================
-- SP: sp_InsertOrUpdateProject
-- Returns: Project Id (INT)
-- ============================================================
CREATE PROCEDURE sp_InsertOrUpdateProject
    @ProjectKey       NVARCHAR(200),
    @Name             NVARCHAR(300),
    @Organization     NVARCHAR(200),
    @Visibility       NVARCHAR(50),
    @LastAnalysisDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Projects WHERE ProjectKey = @ProjectKey)
    BEGIN
        UPDATE Projects
        SET    Name             = @Name,
               Organization    = @Organization,
               Visibility      = @Visibility,
               LastAnalysisDate = @LastAnalysisDate
        WHERE  ProjectKey = @ProjectKey;

        SELECT Id FROM Projects WHERE ProjectKey = @ProjectKey;
    END
    ELSE
    BEGIN
        INSERT INTO Projects (ProjectKey, Name, Organization, Visibility, LastAnalysisDate)
        VALUES (@ProjectKey, @Name, @Organization, @Visibility, @LastAnalysisDate);

        SELECT CAST(SCOPE_IDENTITY() AS INT) AS Id;
    END
END
GO

-- ============================================================
-- SP: sp_InsertOrUpdateBranch
-- Returns: Branch Id (INT)
-- ============================================================
CREATE PROCEDURE sp_InsertOrUpdateBranch
    @ProjectId    INT,
    @BranchName   NVARCHAR(200),
    @IsMain       BIT,
    @AnalysisDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Branches WHERE ProjectId = @ProjectId AND BranchName = @BranchName)
    BEGIN
        UPDATE Branches
        SET    IsMain       = @IsMain,
               AnalysisDate = @AnalysisDate
        WHERE  ProjectId  = @ProjectId
           AND BranchName = @BranchName;

        SELECT Id FROM Branches WHERE ProjectId = @ProjectId AND BranchName = @BranchName;
    END
    ELSE
    BEGIN
        INSERT INTO Branches (ProjectId, BranchName, IsMain, AnalysisDate)
        VALUES (@ProjectId, @BranchName, @IsMain, @AnalysisDate);

        SELECT CAST(SCOPE_IDENTITY() AS INT) AS Id;
    END
END
GO

-- ============================================================
-- SP: sp_InsertSnapshot
-- Returns: New Snapshot Id (INT)
-- Note   : Snapshots are IMMUTABLE – never update, always insert
-- ============================================================
CREATE PROCEDURE sp_InsertSnapshot
    @ProjectId             INT,
    @BranchId              INT,
    @ScanDate              DATETIME,
    @CommitId              NVARCHAR(200),
    @Bugs                  INT,
    @Vulnerabilities       INT,
    @CodeSmells            INT,
    @Coverage              DECIMAL(5,2),
    @Duplication           DECIMAL(5,2),
    @SecurityRating        NVARCHAR(10),
    @ReliabilityRating     NVARCHAR(10),
    @MaintainabilityRating NVARCHAR(10),
    @QualityGateStatus     NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Snapshots
    (
        ProjectId, BranchId, ScanDate, CommitId,
        Bugs, Vulnerabilities, CodeSmells,
        Coverage, Duplication,
        SecurityRating, ReliabilityRating, MaintainabilityRating,
        QualityGateStatus
    )
    VALUES
    (
        @ProjectId, @BranchId, @ScanDate, @CommitId,
        @Bugs, @Vulnerabilities, @CodeSmells,
        @Coverage, @Duplication,
        @SecurityRating, @ReliabilityRating, @MaintainabilityRating,
        @QualityGateStatus
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS Id;
END
GO

-- ============================================================
-- SP: sp_InsertModuleMetric
-- ============================================================
CREATE PROCEDURE sp_InsertModuleMetric
    @SnapshotId      INT,
    @ModuleName      NVARCHAR(500),
    @Qualifier       NVARCHAR(10),
    @Path            NVARCHAR(1000),
    @Language        NVARCHAR(50),
    @Bugs            INT,
    @Vulnerabilities INT,
    @CodeSmells      INT,
    @Coverage        DECIMAL(5,2),
    @Duplication     DECIMAL(5,2),
    @Complexity      INT,
    @Ncloc           INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO ModuleMetrics
    (
        SnapshotId, ModuleName, Qualifier, Path, Language,
        Bugs, Vulnerabilities, CodeSmells,
        Coverage, Duplication, Complexity, Ncloc
    )
    VALUES
    (
        @SnapshotId, @ModuleName, @Qualifier, @Path, @Language,
        @Bugs, @Vulnerabilities, @CodeSmells,
        @Coverage, @Duplication, @Complexity, @Ncloc
    );
END
GO

-- ============================================================
-- SP: sp_InsertSeverityDistribution
-- ============================================================
CREATE PROCEDURE sp_InsertSeverityDistribution
    @SnapshotId INT,
    @Blocker    INT,
    @Critical   INT,
    @Major      INT,
    @Minor      INT,
    @Info       INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO SeverityDistribution (SnapshotId, Blocker, Critical, Major, Minor, Info)
    VALUES (@SnapshotId, @Blocker, @Critical, @Major, @Minor, @Info);
END
GO

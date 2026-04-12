-- ============================================================
-- Script : BPCQDB_StoredProcedures_PostgreSQL.sql
-- Purpose: All SP migrations from MS SQL Server to PostgreSQL
-- Schema : dbo
-- Total  : 22 functions
-- ============================================================

-- 1. sp_GetAllProjects
CREATE OR REPLACE FUNCTION dbo.sp_getallprojects()
RETURNS TABLE (
    id               INT,
    projectkey       VARCHAR,
    name             VARCHAR,
    organization     VARCHAR,
    visibility       VARCHAR,
    lastanalysisdate TIMESTAMP,
    createddate      TIMESTAMP
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        p.id, p.projectkey, p.name, p.organization, p.visibility,
        p.lastanalysisdate::TIMESTAMP,
        p.createddate::TIMESTAMP
    FROM dbo.projects p
    ORDER BY p.name ASC;
END;
$$;

-- 2. sp_GetProjectById
CREATE OR REPLACE FUNCTION dbo.sp_getprojectbyid(p_projectid INT)
RETURNS TABLE (
    id               INT,
    projectkey       VARCHAR,
    name             VARCHAR,
    organization     VARCHAR,
    visibility       VARCHAR,
    lastanalysisdate TIMESTAMP,
    createddate      TIMESTAMP
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        p.id, p.projectkey, p.name, p.organization, p.visibility,
        p.lastanalysisdate::TIMESTAMP,
        p.createddate::TIMESTAMP
    FROM dbo.projects p
    WHERE p.id = p_projectid;
END;
$$;

-- 3. sp_GetLatestSnapshot
CREATE OR REPLACE FUNCTION dbo.sp_getlatestsnapshot(p_projectid INT)
RETURNS TABLE (
    id                    INT,
    projectid             INT,
    branchid              INT,
    scandate              TIMESTAMP,
    commitid              VARCHAR,
    bugs                  INT,
    vulnerabilities       INT,
    codesmells            INT,
    coverage              NUMERIC,
    duplication           NUMERIC,
    securityrating        VARCHAR,
    reliabilityrating     VARCHAR,
    maintainabilityrating VARCHAR,
    qualitygatestatus     VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        s.id, s.projectid, s.branchid,
        s.scandate::TIMESTAMP,
        s.commitid,
        s.bugs, s.vulnerabilities, s.codesmells,
        s.coverage, s.duplication,
        s.securityrating, s.reliabilityrating,
        s.maintainabilityrating, s.qualitygatestatus
    FROM dbo.snapshots s
    WHERE s.projectid = p_projectid
    ORDER BY s.scandate DESC
    LIMIT 1;
END;
$$;

-- 4. sp_GetProjectHeader
CREATE OR REPLACE FUNCTION dbo.sp_getprojectheader(p_projectid INT)
RETURNS TABLE (
    projectname       VARCHAR,
    branch            VARCHAR,
    lastscandate      TIMESTAMP,
    qualitygatestatus VARCHAR,
    commitid          VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        p.name::VARCHAR,
        b.branchname::VARCHAR,
        s.scandate::TIMESTAMP,
        s.qualitygatestatus::VARCHAR,
        s.commitid::VARCHAR
    FROM dbo.snapshots s
    JOIN dbo.projects p ON p.id = s.projectid
    LEFT JOIN dbo.branches b ON b.id = s.branchid
    WHERE s.projectid = p_projectid
    ORDER BY s.scandate DESC
    LIMIT 1;
END;
$$;

-- 5. sp_GetRecentScans
CREATE OR REPLACE FUNCTION dbo.sp_getrecentscans(p_projectid INT, p_topn INT DEFAULT 10)
RETURNS TABLE (
    scandate          TIMESTAMP,
    branch            VARCHAR,
    commitid          VARCHAR,
    qualitygatestatus VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        s.scandate::TIMESTAMP,
        b.branchname::VARCHAR,
        s.commitid::VARCHAR,
        s.qualitygatestatus::VARCHAR
    FROM dbo.snapshots s
    LEFT JOIN dbo.branches b ON b.id = s.branchid
    WHERE s.projectid = p_projectid
    ORDER BY s.scandate DESC
    LIMIT p_topn;
END;
$$;

-- 6. sp_GetSnapshotsByProject
CREATE OR REPLACE FUNCTION dbo.sp_getsnapshotsbyproject(p_projectid INT)
RETURNS TABLE (
    id                INT,
    scandate          TIMESTAMP,
    branch            VARCHAR,
    commitid          VARCHAR,
    qualitygatestatus VARCHAR,
    bugs              INT,
    vulnerabilities   INT,
    codesmells        INT,
    coverage          NUMERIC,
    duplication       NUMERIC
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        s.id,
        s.scandate::TIMESTAMP,
        b.branchname::VARCHAR,
        s.commitid::VARCHAR,
        s.qualitygatestatus::VARCHAR,
        s.bugs, s.vulnerabilities, s.codesmells,
        s.coverage, s.duplication
    FROM dbo.snapshots s
    LEFT JOIN dbo.branches b ON b.id = s.branchid
    WHERE s.projectid = p_projectid
    ORDER BY s.scandate DESC;
END;
$$;

-- 7. sp_GetMetricsKpis
CREATE OR REPLACE FUNCTION dbo.sp_getmetricskpis(p_projectid INT)
RETURNS TABLE (
    bugs        INT,
    codesmells  INT,
    coverage    NUMERIC,
    duplication NUMERIC
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT s.bugs, s.codesmells, s.coverage, s.duplication
    FROM dbo.snapshots s
    WHERE s.projectid = p_projectid
    ORDER BY s.scandate DESC
    LIMIT 1;
END;
$$;

-- 8. sp_GetBugsVulnerabilitiesHistory
CREATE OR REPLACE FUNCTION dbo.sp_getbugsvulnerabilitieshistory(p_projectid INT)
RETURNS TABLE (
    date            TIMESTAMP,
    bugs            INT,
    vulnerabilities INT
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT s.scandate::TIMESTAMP, s.bugs, s.vulnerabilities
    FROM dbo.snapshots s
    WHERE s.projectid = p_projectid
    ORDER BY s.scandate ASC;
END;
$$;

-- 9. sp_GetCoverageHistory
CREATE OR REPLACE FUNCTION dbo.sp_getcoveragehistory(p_projectid INT)
RETURNS TABLE (
    date     TIMESTAMP,
    coverage NUMERIC
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT s.scandate::TIMESTAMP, s.coverage
    FROM dbo.snapshots s
    WHERE s.projectid = p_projectid
    ORDER BY s.scandate ASC;
END;
$$;

-- 10. sp_GetModulesBySnapshot
CREATE OR REPLACE FUNCTION dbo.sp_getmodulesbysnapshot(p_snapshotid INT)
RETURNS TABLE (
    modulename      VARCHAR,
    qualifier       VARCHAR,
    path            VARCHAR,
    language        VARCHAR,
    bugs            INT,
    vulnerabilities INT,
    codesmells      INT,
    coverage        NUMERIC,
    duplication     NUMERIC,
    complexity      INT,
    ncloc           INT
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        m.modulename, m.qualifier, m.path, m.language,
        m.bugs, m.vulnerabilities, m.codesmells,
        m.coverage, m.duplication, m.complexity, m.ncloc
    FROM dbo.modulemetrics m
    WHERE m.snapshotid = p_snapshotid
    ORDER BY m.bugs DESC, m.vulnerabilities DESC;
END;
$$;

-- 11. sp_GetSeverityBySnapshot
CREATE OR REPLACE FUNCTION dbo.sp_getseveritybysnapshot(p_snapshotid INT)
RETURNS TABLE (
    blocker  INT,
    critical INT,
    major    INT,
    minor    INT,
    info     INT
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT sd.blocker, sd.critical, sd.major, sd.minor, sd.info
    FROM dbo.severitydistribution sd
    WHERE sd.snapshotid = p_snapshotid;
END;
$$;

-- 12. sp_GetQualityGateHistory
CREATE OR REPLACE FUNCTION dbo.sp_getqualitygatehistory(p_projectid INT)
RETURNS TABLE (
    date              TIMESTAMP,
    branch            VARCHAR,
    status            VARCHAR,
    commitid          VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        s.scandate::TIMESTAMP,
        b.branchname::VARCHAR,
        s.qualitygatestatus::VARCHAR,
        s.commitid::VARCHAR
    FROM dbo.snapshots s
    LEFT JOIN dbo.branches b ON b.id = s.branchid
    WHERE s.projectid = p_projectid
    ORDER BY s.scandate DESC;
END;
$$;

-- 13. sp_GetConditionsBySnapshot
CREATE OR REPLACE FUNCTION dbo.sp_getconditionsbysnapshot(p_snapshotid INT)
RETURNS TABLE (
    metrickey      VARCHAR,
    comparator     VARCHAR,
    errorthreshold VARCHAR,
    actualvalue    VARCHAR,
    status         VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT qc.metrickey, qc.comparator, qc.errorthreshold, qc.actualvalue, qc.status
    FROM dbo.qualitygateconditions qc
    WHERE qc.snapshotid = p_snapshotid
    ORDER BY
        CASE qc.status WHEN 'ERROR' THEN 0 ELSE 1 END ASC,
        qc.metrickey ASC;
END;
$$;

-- 14. sp_GetManualQAEntries
CREATE OR REPLACE FUNCTION dbo.sp_getmanualqaentries(p_projectid INT)
RETURNS TABLE (
    id          INT,
    projectid   INT,
    modulename  VARCHAR,
    issuetype   VARCHAR,
    severity    VARCHAR,
    description VARCHAR,
    reportedby  VARCHAR,
    entrydate   TIMESTAMP
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        m.id, m.projectid, m.modulename, m.issuetype,
        m.severity, m.description, m.reportedby,
        m.entrydate::TIMESTAMP
    FROM dbo.manualqaentries m
    WHERE m.projectid = p_projectid
    ORDER BY m.entrydate DESC;
END;
$$;

-- 15. sp_GetQASummary
CREATE OR REPLACE FUNCTION dbo.sp_getqasummary(p_projectid INT)
RETURNS TABLE (
    totalentries         BIGINT,
    bugentries           BIGINT,
    vulnerabilityentries BIGINT,
    codesmellentries     BIGINT,
    criticalcount        BIGINT,
    highcount            BIGINT,
    mediumcount          BIGINT,
    lowcount             BIGINT
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*)::BIGINT,
        SUM(CASE WHEN issuetype = 'Bug'           THEN 1 ELSE 0 END)::BIGINT,
        SUM(CASE WHEN issuetype = 'Vulnerability' THEN 1 ELSE 0 END)::BIGINT,
        SUM(CASE WHEN issuetype = 'Code Smell'    THEN 1 ELSE 0 END)::BIGINT,
        SUM(CASE WHEN severity  = 'Critical'      THEN 1 ELSE 0 END)::BIGINT,
        SUM(CASE WHEN severity  = 'High'          THEN 1 ELSE 0 END)::BIGINT,
        SUM(CASE WHEN severity  = 'Medium'        THEN 1 ELSE 0 END)::BIGINT,
        SUM(CASE WHEN severity  = 'Low'           THEN 1 ELSE 0 END)::BIGINT
    FROM dbo.manualqaentries
    WHERE projectid = p_projectid;
END;
$$;

-- 16. sp_InsertOrUpdateProject
CREATE OR REPLACE FUNCTION dbo.sp_insertorupdateproject(
    p_projectkey       VARCHAR,
    p_name             VARCHAR,
    p_organization     VARCHAR,
    p_visibility       VARCHAR,
    p_lastanalysisdate TIMESTAMP
)
RETURNS TABLE (id INT)
LANGUAGE plpgsql AS $$
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.projects WHERE projectkey = p_projectkey) THEN
        UPDATE dbo.projects
        SET    name             = p_name,
               organization    = p_organization,
               visibility      = p_visibility,
               lastanalysisdate = p_lastanalysisdate
        WHERE  projectkey = p_projectkey;
    ELSE
        INSERT INTO dbo.projects (projectkey, name, organization, visibility, lastanalysisdate)
        VALUES (p_projectkey, p_name, p_organization, p_visibility, p_lastanalysisdate);
    END IF;
    RETURN QUERY SELECT p.id FROM dbo.projects p WHERE p.projectkey = p_projectkey;
END;
$$;

-- 17. sp_InsertOrUpdateBranch
CREATE OR REPLACE FUNCTION dbo.sp_insertorupdatebranch(
    p_projectid    INT,
    p_branchname   VARCHAR,
    p_ismain       BOOLEAN,
    p_analysisdate TIMESTAMP
)
RETURNS TABLE (id INT)
LANGUAGE plpgsql AS $$
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.branches WHERE projectid = p_projectid AND branchname = p_branchname) THEN
        UPDATE dbo.branches
        SET    ismain       = p_ismain,
               analysisdate = p_analysisdate
        WHERE  projectid  = p_projectid
          AND  branchname = p_branchname;
    ELSE
        INSERT INTO dbo.branches (projectid, branchname, ismain, analysisdate)
        VALUES (p_projectid, p_branchname, p_ismain, p_analysisdate);
    END IF;
    RETURN QUERY SELECT b.id FROM dbo.branches b
    WHERE b.projectid = p_projectid AND b.branchname = p_branchname;
END;
$$;

-- 18. sp_InsertSnapshot
CREATE OR REPLACE FUNCTION dbo.sp_insertsnapshot(
    p_projectid             INT,
    p_branchid              INT,
    p_scandate              TIMESTAMP,
    p_commitid              VARCHAR,
    p_bugs                  INT,
    p_vulnerabilities       INT,
    p_codesmells            INT,
    p_coverage              NUMERIC(5,2),
    p_duplication           NUMERIC(5,2),
    p_securityrating        VARCHAR,
    p_reliabilityrating     VARCHAR,
    p_maintainabilityrating VARCHAR,
    p_qualitygatestatus     VARCHAR
)
RETURNS TABLE (id INT)
LANGUAGE plpgsql AS $$
DECLARE
    v_newid INT;
BEGIN
    INSERT INTO dbo.snapshots (
        projectid, branchid, scandate, commitid,
        bugs, vulnerabilities, codesmells,
        coverage, duplication,
        securityrating, reliabilityrating, maintainabilityrating,
        qualitygatestatus
    )
    VALUES (
        p_projectid, p_branchid, p_scandate, p_commitid,
        p_bugs, p_vulnerabilities, p_codesmells,
        p_coverage, p_duplication,
        p_securityrating, p_reliabilityrating, p_maintainabilityrating,
        p_qualitygatestatus
    )
    RETURNING dbo.snapshots.id INTO v_newid;
    RETURN QUERY SELECT v_newid;
END;
$$;

-- 19. sp_InsertModuleMetric
CREATE OR REPLACE FUNCTION dbo.sp_insertmodulemetric(
    p_snapshotid      INT,
    p_modulename      VARCHAR,
    p_qualifier       VARCHAR,
    p_path            VARCHAR,
    p_language        VARCHAR,
    p_bugs            INT,
    p_vulnerabilities INT,
    p_codesmells      INT,
    p_coverage        NUMERIC(5,2),
    p_duplication     NUMERIC(5,2),
    p_complexity      INT,
    p_ncloc           INT
)
RETURNS VOID
LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO dbo.modulemetrics (
        snapshotid, modulename, qualifier, path, language,
        bugs, vulnerabilities, codesmells,
        coverage, duplication, complexity, ncloc
    )
    VALUES (
        p_snapshotid, p_modulename, p_qualifier, p_path, p_language,
        p_bugs, p_vulnerabilities, p_codesmells,
        p_coverage, p_duplication, p_complexity, p_ncloc
    );
END;
$$;

-- 20. sp_InsertSeverityDistribution
CREATE OR REPLACE FUNCTION dbo.sp_insertseveritydistribution(
    p_snapshotid INT,
    p_blocker    INT,
    p_critical   INT,
    p_major      INT,
    p_minor      INT,
    p_info       INT
)
RETURNS VOID
LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO dbo.severitydistribution (snapshotid, blocker, critical, major, minor, info)
    VALUES (p_snapshotid, p_blocker, p_critical, p_major, p_minor, p_info);
END;
$$;

-- 21. sp_InsertManualQAEntry
CREATE OR REPLACE FUNCTION dbo.sp_insertmanualqaentry(
    p_projectid   INT,
    p_modulename  VARCHAR,
    p_issuetype   VARCHAR,
    p_severity    VARCHAR,
    p_description VARCHAR,
    p_reportedby  VARCHAR
)
RETURNS TABLE (id INT)
LANGUAGE plpgsql AS $$
DECLARE
    v_newid INT;
BEGIN
    INSERT INTO dbo.manualqaentries (projectid, modulename, issuetype, severity, description, reportedby)
    VALUES (p_projectid, p_modulename, p_issuetype, p_severity, p_description, p_reportedby)
    RETURNING dbo.manualqaentries.id INTO v_newid;
    RETURN QUERY SELECT v_newid;
END;
$$;

-- 22. sp_InsertQualityGateCondition
CREATE OR REPLACE FUNCTION dbo.sp_insertqualitygatecondition(
    p_snapshotid     INT,
    p_metrickey      VARCHAR,
    p_comparator     VARCHAR,
    p_errorthreshold VARCHAR,
    p_actualvalue    VARCHAR,
    p_status         VARCHAR
)
RETURNS VOID
LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO dbo.qualitygateconditions (snapshotid, metrickey, comparator, errorthreshold, actualvalue, status)
    VALUES (p_snapshotid, p_metrickey, p_comparator, p_errorthreshold, p_actualvalue, p_status);
END;
$$;
